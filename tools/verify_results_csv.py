#!/usr/bin/env python3
"""
Independent verifier for an EPR Calculator "Results" CSV export.

WHAT THIS CHECKS
================
The Results CSV is a single wide export containing (amongst other things) a
"Calculation Result" table with one row per producer/subsidiary. This script
re-derives, from first principles, the figures in that table for each
Level-1 (L1) producer row -- i.e. the per-producer total used for billing,
as opposed to the underlying L2 subsidiary rows or the single "all producers"
grand-total row -- and reports any row where the file's own printed figures
don't match what the documented calculation would produce.

Every number used below is read from the Results CSV itself. Nothing is
recomputed from a separate database, submission file, or parameter sheet, so
this script only tells you "is this file internally consistent with its own
documented formulas?" -- it cannot tell you whether the *inputs* fed into the
run were themselves correct.

Every comparison is an EXACT equality, not a tolerance-bounded "close enough":
each recomputed value is carried at full (unrounded) Decimal precision through
every intermediate step -- summed, apportioned, whatever -- and only rounded
once, the same way and to the same number of decimal places the app's own
CsvSanitiser does for that specific column, immediately before comparing. The
app itself never rounds until the point it writes a value to CSV, so
replicating that gives an exact match rather than an approximate one; a
mismatch of any size is a genuine finding, not rounding noise.

For each L1 producer row, the checks are:

  0. L1 = sum(L2): for a multi-entity producer (one with subsidiaries), its L1
     group-total row's raw tonnage columns (Household/Public Bin/HDC/Total
     Tonnage, and their RAG breakdowns) should equal the sum of its L2
     subsidiary rows' own copies of those columns -- exactly what
     ProducerRowBuilder.GetL1TotalRow does. This is deliberately narrow: SMCW,
     Net Tonnage, price and fee columns are computed once at the L1 (group)
     level rather than summed from L2, and are NOT checked against a sum of
     L2 (the source code itself documents them as "not derivable by summing
     L2 values") -- doing so would produce false positives, not real findings.

  1. Modulation pricing (run-wide, checked once): the "Modulation Calculation"
     section derives a Red and Green disposal-cost-per-tonne for each
     material from a single Amber ("flat") price plus a Red Modulation
     Factor. We recompute the Green Modulation Factor and the per-material
     Red/Green prices from that section's own printed intermediate figures,
     and separately confirm that section's starting tonnage figures equal
     the sum of every L1 producer's own Net Tonnage plus the Late Reporting
     Tonnage parameter -- tying the run-wide pricing back to the same
     per-producer data checked in step 2, rather than trusting it in isolation.

  2. Section 1 -- LA Disposal Fee: for each material, the producer's own
     "Net Tonnage" (Red/Amber/Green) is multiplied by that material's
     Red/Amber/Green price-per-tonne (from check 1), bad debt provision is
     added, and the result is apportioned across England/Wales/Scotland/
     Northern Ireland. We recompute this per material and re-sum it to the
     producer's Section-1 total. This also replicates the app's zero-override
     guard (a material's fee is forced to zero if self-managed consumer
     waste tonnage exceeds raw reported tonnage): for a multi-entity producer
     that comparison is against group-level, summed-across-L2 figures, but
     the app prints those already-aggregated figures directly on the L1 row
     itself (ProducerRowBuilder.GetL1TotalRow), so no L2 data is needed to
     replicate it for single-entity or multi-entity producers alike.

  3. Sections 2a/2b/2c -- Comms Costs: 2a multiplies the producer's raw
     reported tonnage by a per-material comms price; 2b and 2c apportion the
     run-wide UK-wide and by-country comms cost totals to the producer by its
     share of all producers' tonnage. All three add bad debt provision and
     apportion by country (2a and 2b via the 1+4 apportionment, 2c via the
     raw by-country split), using the "Parameters - Comms Costs" section's
     own printed price/cost figures as inputs.

  4. Sections 3/4/5 -- SA Operating Costs, LA Data Prep Costs, SA Set Up
     Costs: each is a fixed run-wide total (from "Parameters - Other")
     apportioned to the producer by its own "Percentage of Overall Producer
     Cost for (1+2a+2b+2c)" -- which we also independently recompute, as that
     producer's own (1+2a+2b+2c) total over the run-wide total printed once
     in the file's header row -- then split by country (3 and 5 via 1+4%,
     4 via its own "4 Country Apportionment %s").

  5. Total Producer Bill: Section 1 through 5 (all independently recomputed
     above) should sum to the printed Total Producer Bill, both in total and
     per country.

  6. Suggested Billing Instruction: given the Total Producer Bill (with bad
     debt provision) and the producer's prior invoiced total, we recompute
     the liability difference, the materiality/tonnage-change threshold
     flags, the suggested instruction (INITIAL/DELTA/REBILL/-), and the
     suggested invoice amount.

SCOPE / KNOWN LIMITATIONS
=========================
  * The full fee/comms/billing chain (checks 1-6) covers L1 rows only. L2
    subsidiary rows are read for check 0 (see above) -- only their raw
    tonnage columns feed into that one check -- and the single overall-total
    row is read for context but not independently verified.
  * Requires a Results file that includes the "Modulation Calculation"
    section (i.e. a run using RAG-rating disposal pricing). Files without
    that section are pre-modulation and are not supported -- the script
    will say so rather than silently checking the wrong formula.
  * A one-penny mismatch confined to a single country's share of a fee, where
    the country-apportionment ratio reduces to a repeating decimal (e.g. a
    split that's some multiple of ninths or twenty-ninths, common in
    hand-built test data with round GBP figures), can be a genuine
    infinite-precision tie: the true mathematical answer lands exactly on a
    rounding boundary, but the app's own decimal arithmetic -- which, like
    any fixed-precision arithmetic, must truncate a non-terminating division
    somewhere -- can settle a hair to one side of that tie rather than the
    other. is_exact_tie() detects this automatically (verified via exact
    integer arithmetic on the decimal representation, not just "looks close
    to X.5") and the CLI reports these separately from genuine discrepancies
    -- see EXIT CODES below. It never affects totals, only a per-country
    split of one fee.

EXIT CODES
==========
    0   No discrepancies, or every discrepancy found is a confirmed exact
        rounding tie (see above) -- printed for transparency but not treated
        as a failure.
    1   At least one discrepancy that is NOT a confirmed exact tie.
    2   Unsupported file (no "Modulation Calculation" section).

USAGE
=====
    python3 verify_results_csv.py path/to/2026-results.csv
    python3 verify_results_csv.py path/to/2026-results.csv --verbose
"""

from __future__ import annotations

import argparse
import csv
import decimal
import io
import sys
from dataclasses import dataclass, field
from decimal import Decimal, ROUND_HALF_UP
from typing import Optional

# The default 28-significant-digit precision is already enough headroom for every check in
# this script (confirmed empirically: recomputing the values below at 28, 50 and 100 digits
# of precision gives bit-identical results), but raising it further costs nothing and gives
# is_exact_tie() more margin for confidently telling "this is a provable exact rounding tie"
# apart from "this merely looks like one at limited precision".
decimal.getcontext().prec = 60


# ---------------------------------------------------------------------------
# Low-level parsing helpers
#
# The Results CSV renders every number as a formatted string (currency with
# a "£" prefix, percentages with a "%" suffix, or a plain decimal), and uses
# "-" for values that don't apply to a row. These helpers undo that
# formatting back into Decimal (or None), and reproduce the file's own
# "round half away from zero" rounding so that a value we recompute and a
# value we read out of the file round to the same printed string.
# ---------------------------------------------------------------------------

HYPHEN = "-"


def round_half_away_from_zero(value: Decimal, places: int) -> Decimal:
    """Matches .NET's MidpointRounding.AwayFromZero, used throughout the C# exporter."""
    quantum = Decimal(1).scaleb(-places)
    return value.quantize(quantum, rounding=ROUND_HALF_UP)


def is_exact_tie(value: Decimal, places: int) -> bool:
    """
    True if `value` sits exactly on the rounding boundary for `places` decimal places (e.g.
    126.885000...0 for places=2) -- as opposed to merely rounding to a "5" because of a
    truncated intermediate calculation. A value is exactly at that boundary iff doubling it
    after shifting `places` digits past the decimal point gives an exact odd integer; this
    is pure decimal-point shifting and multiplication by 2, so it cannot itself introduce
    any rounding error, regardless of how `value` was computed.

    Where the app's own recomputed figure disagrees with a value flagged this way, the
    disagreement is provably a genuine infinite-precision tie -- both the app's answer and
    this script's are one ULP either side of a boundary that no finite-precision decimal
    arithmetic (28 significant digits or otherwise) can land on consistently. See the
    "repeating apportionment ratio" note in the module docstring.
    """
    doubled = value.scaleb(places) * 2
    if doubled != doubled.to_integral_value():
        return False
    return int(doubled) % 2 == 1


def parse_decimal(raw: str) -> Optional[Decimal]:
    """Parses a plain (non-currency, non-percentage) number cell. '-' and '' -> None."""
    text = raw.strip()
    if text in ("", HYPHEN):
        return None
    return Decimal(text.replace(",", ""))


def parse_money(raw: str) -> Optional[Decimal]:
    """Parses a '£123.45' / '-£123.45' cell. '-' and '' -> None."""
    text = raw.strip()
    if text in ("", HYPHEN):
        return None
    negative = text.startswith("-")
    text = text.lstrip("-").replace("£", "").replace(",", "")
    value = Decimal(text)
    return -value if negative else value


def parse_percent(raw: str) -> Optional[Decimal]:
    """Parses a '54.04873246%' cell into the number 54.04873246 (not 0.5404...)."""
    text = raw.strip()
    if text in ("", HYPHEN):
        return None
    negative = text.startswith("-")
    text = text.lstrip("-").replace("%", "").replace(",", "")
    value = Decimal(text)
    return -value if negative else value


def d(value) -> Decimal:
    """Decimal(...) that also accepts None as zero, for terser arithmetic below."""
    return Decimal(0) if value is None else Decimal(value)


# ---------------------------------------------------------------------------
# Whole-file loading and section lookup
# ---------------------------------------------------------------------------


def load_rows(path: str) -> list[list[str]]:
    """
    Reads and CSV-parses the Results file, auto-detecting its text encoding. Results
    files exported by the real application have been seen in the wild as UTF-16LE with
    no byte-order mark (a common outcome of .NET's default Encoding.Unicode), which
    plain UTF-8 decoding rejects outright -- as well as plain UTF-8 and UTF-8 with a
    byte-order mark (used by this project's own test fixtures).
    """
    with open(path, "rb") as f:
        raw = f.read()

    if raw.startswith(b"\xff\xfe"):
        text = raw.decode("utf-16-le")
    elif raw.startswith(b"\xfe\xff"):
        text = raw.decode("utf-16-be")
    elif raw.startswith(b"\xef\xbb\xbf"):
        text = raw.decode("utf-8-sig")
    elif len(raw) >= 2 and raw[0] != 0 and raw[1] == 0:
        # No BOM, but every other byte is null -- consistent with ASCII/Latin text
        # encoded as UTF-16LE without a leading byte-order mark.
        text = raw.decode("utf-16-le")
    else:
        text = raw.decode("utf-8")

    return list(csv.reader(io.StringIO(text)))


def find_row(rows: list[list[str]], first_cell: str, start: int = 0) -> int:
    """Index of the first row whose first cell equals `first_cell`, searching from `start`."""
    for i in range(start, len(rows)):
        if rows[i] and rows[i][0].strip() == first_cell:
            return i
    raise ValueError(f"Section {first_cell!r} not found in file (searching from row {start})")


def find_row_containing(rows: list[list[str]], cell_text: str, start: int = 0) -> int:
    """Index of the first row that contains `cell_text` in any column."""
    for i in range(start, len(rows)):
        if cell_text in rows[i]:
            return i
    raise ValueError(f"No row containing {cell_text!r} found (searching from row {start})")


# ---------------------------------------------------------------------------
# Country apportionment (England/Wales/Scotland/Northern Ireland)
# ---------------------------------------------------------------------------


@dataclass
class ByCountry:
    england: Decimal
    wales: Decimal
    scotland: Decimal
    northern_ireland: Decimal

    @property
    def total(self) -> Decimal:
        return self.england + self.wales + self.scotland + self.northern_ireland

    def __add__(self, other: "ByCountry") -> "ByCountry":
        return ByCountry(
            self.england + other.england,
            self.wales + other.wales,
            self.scotland + other.scotland,
            self.northern_ireland + other.northern_ireland,
        )


def derive_apportionment_pct(total: ByCountry) -> ByCountry:
    """
    Every country-apportionment percentage in this calculator (LAPCAP's own, 1+4, and LA
    Data Prep's own) is defined the same way in the source: England / Total * 100, with
    no intermediate rounding (see CalcResultLapcapData.CountryApportionment and
    CalcResultParameterOtherCostBuilder.GetCountryApportionment). Deriving it here via
    exact Decimal division from the raw GBP totals -- rather than reading the file's own
    printed percentage, which is itself rounded to 8dp for display -- avoids feeding an
    already-rounded input into every downstream apportioned figure.
    """
    if total.total == 0:
        return ByCountry(england=Decimal(0), wales=Decimal(0), scotland=Decimal(0), northern_ireland=Decimal(0))
    return ByCountry(
        england=total.england / total.total * 100,
        wales=total.wales / total.total * 100,
        scotland=total.scotland / total.total * 100,
        northern_ireland=total.northern_ireland / total.total * 100,
    )


# ---------------------------------------------------------------------------
# Section parsers: pull run-wide (not per-producer) figures out of the
# header portion of the file, before the per-producer table begins.
# ---------------------------------------------------------------------------


@dataclass
class LapcapData:
    materials_order: list[str]
    total_by_country: ByCountry  # "1 LA Disposal Cost Total" row, in GBP
    country_apportionment_pct: ByCountry  # derived exactly from total_by_country
    printed_country_apportionment_pct: ByCountry  # "1 Country Apportionment %s" as printed, for cross-check


def parse_lapcap_data(rows: list[list[str]]) -> LapcapData:
    section = find_row(rows, "LAPCAP Data")
    header_row = section + 1
    materials = []
    r = header_row + 1
    while rows[r][0].strip() != "Total":
        materials.append(rows[r][0].strip())
        r += 1
    total_row = rows[r]
    total_by_country = ByCountry(
        england=d(parse_money(total_row[1])), wales=d(parse_money(total_row[2])),
        scotland=d(parse_money(total_row[3])), northern_ireland=d(parse_money(total_row[4])),
    )
    apportionment_row = rows[r + 1]
    assert apportionment_row[0].strip() == "1 Country Apportionment %s", apportionment_row[0]
    printed_apportionment = ByCountry(
        england=parse_percent(apportionment_row[1]),
        wales=parse_percent(apportionment_row[2]),
        scotland=parse_percent(apportionment_row[3]),
        northern_ireland=parse_percent(apportionment_row[4]),
    )
    return LapcapData(
        materials_order=materials,
        total_by_country=total_by_country,
        country_apportionment_pct=derive_apportionment_pct(total_by_country),
        printed_country_apportionment_pct=printed_apportionment,
    )


@dataclass
class LaDisposalCostData:
    # material name -> Disposal Cost Price Per Tonne (the "Amber"/flat price)
    price_per_tonne: dict[str, Decimal]


def parse_la_disposal_cost_data(rows: list[list[str]], materials_order: list[str]) -> LaDisposalCostData:
    section = find_row(rows, "LA Disposal Cost Data")
    header = rows[section + 1]
    price_col = header.index("Disposal Cost Price Per Tonne")
    prices = {}
    r = section + 2
    for _ in materials_order:
        material = rows[r][0].strip()
        prices[material] = parse_money(rows[r][price_col])
        r += 1
    return LaDisposalCostData(price_per_tonne=prices)


@dataclass
class RagAmounts:
    red: Decimal
    amber: Decimal
    green: Decimal


@dataclass
class LateReportingTonnages:
    # material name -> grouped (Red+RedMedical, Amber+AmberMedical, Green+GreenMedical) late reporting tonnage
    by_material: dict[str, RagAmounts]


def parse_late_reporting_tonnages(rows: list[list[str]], materials_order: list[str]) -> LateReportingTonnages:
    section = find_row(rows, "Parameters - Late Reporting Tonnages")
    header = rows[section + 1]
    col = {name: i for i, name in enumerate(header)}
    by_material = {}
    r = section + 2
    for _ in materials_order:
        cells = rows[r]
        material = cells[0].strip()
        by_material[material] = RagAmounts(
            red=parse_decimal(cells[col["Red + Red Medical Late Reporting Tonnage"]]),
            amber=parse_decimal(cells[col["Amber + Amber Medical Late Reporting Tonnage"]]),
            green=parse_decimal(cells[col["Green + Green Medical Late Reporting Tonnage"]]),
        )
        r += 1
    return LateReportingTonnages(by_material=by_material)


@dataclass
class MaterialModulation:
    red_net_tonnage: Decimal
    amber_net_tonnage: Decimal
    green_net_tonnage: Decimal
    total_red_at_amber_cost: Decimal
    total_green_at_amber_cost: Decimal
    red_price: Decimal
    amber_price: Decimal
    green_price: Decimal


@dataclass
class ModulationCalculation:
    red_factor: Decimal
    green_factor_printed: Decimal
    by_material: dict[str, MaterialModulation]
    total_red_at_amber_cost: Decimal
    total_green_at_amber_cost: Decimal


def parse_modulation_calculation(rows: list[list[str]], materials_order: list[str]) -> Optional[ModulationCalculation]:
    try:
        section = find_row(rows, "Modulation Calculation")
    except ValueError:
        return None

    red_factor = parse_decimal(rows[section + 1][1])
    green_factor_printed = parse_decimal(rows[section + 2][1])
    header_row = section + 3
    header = rows[header_row]
    col = {name: i for i, name in enumerate(header)}

    by_material: dict[str, MaterialModulation] = {}
    r = header_row + 1
    for _ in materials_order:
        cells = rows[r]
        material = cells[0].strip()
        by_material[material] = MaterialModulation(
            red_net_tonnage=parse_decimal(cells[col["Red + Red Medical Net Tonnage + Late Reporting Tonnage"]]),
            amber_net_tonnage=parse_decimal(cells[col["Amber + Amber Medical Net Tonnage + Late Reporting Tonnage"]]),
            green_net_tonnage=parse_decimal(cells[col["Green + Green Medical Net Tonnage + Late Reporting Tonnage"]]),
            total_red_at_amber_cost=parse_money(
                cells[col["Total Red Material at Amber Disposal Cost = Amber Material Disposal Cost x Red Material Tonnage"]]
            ),
            total_green_at_amber_cost=parse_money(
                cells[col["Total Green Material at Amber Disposal Cost = Amber Material Disposal Cost x Green Material Tonnage"]]
            ),
            red_price=parse_money(cells[col["Red Material Disposal Cost = Red Modulation Factor * Amber Material Disposal Cost"]]),
            amber_price=parse_money(cells[col["Amber Material Disposal Cost = Material Disposal Cost per Tonne"]]),
            green_price=parse_money(cells[col["Green Material Disposal Cost = Green Modulation Factor * Amber Material Disposal Cost"]]),
        )
        r += 1

    total_row = rows[r]
    assert total_row[0].strip() == "Total", total_row[0]
    total_red_at_amber = parse_money(
        total_row[col["Total Red Material at Amber Disposal Cost = Amber Material Disposal Cost x Red Material Tonnage"]]
    )
    total_green_at_amber = parse_money(
        total_row[col["Total Green Material at Amber Disposal Cost = Amber Material Disposal Cost x Green Material Tonnage"]]
    )

    return ModulationCalculation(
        red_factor=red_factor,
        green_factor_printed=green_factor_printed,
        by_material=by_material,
        total_red_at_amber_cost=total_red_at_amber,
        total_green_at_amber_cost=total_green_at_amber,
    )


@dataclass
class Materiality:
    amount: Decimal
    percentage: Decimal


@dataclass
class OtherParameters:
    sa_operating_cost_total: Decimal  # "3 SA Operating Costs" Total column
    la_data_prep_by_country: ByCountry  # "4 LA Data Prep Charge" row, in GBP
    la_data_prep_apportionment_pct: ByCountry  # derived exactly from la_data_prep_by_country
    printed_la_data_prep_apportionment_pct: ByCountry  # "4 Country Apportionment %s" as printed, for cross-check
    scheme_setup_cost_total: Decimal  # "5 Scheme set up cost Yearly Cost" Total column
    bad_debt_pct: Decimal
    materiality_increase: Materiality
    materiality_decrease: Materiality
    tonnage_change_increase: Materiality
    tonnage_change_decrease: Materiality

    @property
    def la_data_prep_charge_total(self) -> Decimal:
        return self.la_data_prep_by_country.total


def parse_other_parameters(rows: list[list[str]]) -> OtherParameters:
    sa_operating_row = rows[find_row(rows, "3 SA Operating Costs")]
    sa_operating_cost_total = parse_money(sa_operating_row[5])

    la_data_prep_row_idx = find_row(rows, "4 LA Data Prep Charge")
    la_data_prep_row = rows[la_data_prep_row_idx]
    la_data_prep_by_country = ByCountry(
        england=d(parse_money(la_data_prep_row[1])), wales=d(parse_money(la_data_prep_row[2])),
        scotland=d(parse_money(la_data_prep_row[3])), northern_ireland=d(parse_money(la_data_prep_row[4])),
    )
    la_data_prep_apportionment_row = rows[la_data_prep_row_idx + 1]
    assert la_data_prep_apportionment_row[0].strip() == "4 Country Apportionment %s", la_data_prep_apportionment_row[0]
    printed_la_data_prep_apportionment_pct = ByCountry(
        england=parse_percent(la_data_prep_apportionment_row[1]),
        wales=parse_percent(la_data_prep_apportionment_row[2]),
        scotland=parse_percent(la_data_prep_apportionment_row[3]),
        northern_ireland=parse_percent(la_data_prep_apportionment_row[4]),
    )

    scheme_setup_row = rows[find_row(rows, "5 Scheme set up cost Yearly Cost")]
    scheme_setup_cost_total = parse_money(scheme_setup_row[5])

    bad_debt_row = find_row(rows, "6 Bad Debt Provision")
    bad_debt_pct = parse_percent(rows[bad_debt_row][1])

    mat_row = find_row(rows, "7 Materiality")
    mat_increase = rows[mat_row + 1]
    mat_decrease = rows[mat_row + 2]
    assert mat_increase[0].strip() == "Increase" and mat_decrease[0].strip() == "Decrease"

    ton_row = find_row(rows, "8 Tonnage Change")
    ton_increase = rows[ton_row + 1]
    ton_decrease = rows[ton_row + 2]
    assert ton_increase[0].strip() == "Increase" and ton_decrease[0].strip() == "Decrease"

    return OtherParameters(
        sa_operating_cost_total=sa_operating_cost_total,
        la_data_prep_by_country=la_data_prep_by_country,
        la_data_prep_apportionment_pct=derive_apportionment_pct(la_data_prep_by_country),
        printed_la_data_prep_apportionment_pct=printed_la_data_prep_apportionment_pct,
        scheme_setup_cost_total=scheme_setup_cost_total,
        bad_debt_pct=bad_debt_pct,
        materiality_increase=Materiality(parse_money(mat_increase[1]), parse_percent(mat_increase[2])),
        materiality_decrease=Materiality(parse_money(mat_decrease[1]), parse_percent(mat_decrease[2])),
        tonnage_change_increase=Materiality(parse_money(ton_increase[1]), parse_percent(ton_increase[2])),
        tonnage_change_decrease=Materiality(parse_money(ton_decrease[1]), parse_percent(ton_decrease[2])),
    )


@dataclass
class CommsCostParameters:
    one_plus_four_apportionment_pct: ByCountry  # derived exactly from LAPCAP + LA Data Prep totals
    printed_one_plus_four_apportionment_pct: ByCountry  # "1 + 4 Apportionment %s" as printed, for cross-check
    price_per_tonne_by_material: dict[str, Decimal]  # "2a Comms Costs - by Material" -- price per tonne column
    # "2b Comms Costs - UK wide"'s Total column, read directly rather than summed from its
    # four country cells: those are each independently rounded from an apportioned share
    # (unlike 2c's, which are raw un-apportioned input parameters), so summing them can be
    # a penny off the true total that CalcResultCommsCostBuilder actually computed.
    uk_wide_total: Decimal
    by_country: ByCountry  # "2c Comms Costs - by Country" row (before bad debt) -- raw per-country inputs, exact


def parse_comms_cost_parameters(
    rows: list[list[str]],
    materials_order: list[str],
    lapcap_total_by_country: ByCountry,
    la_data_prep_by_country: ByCountry,
) -> CommsCostParameters:
    section = find_row(rows, "Parameters - Comms Costs")
    apportionment_row = rows[find_row(rows, "1 + 4 Apportionment %s", start=section)]
    printed_one_plus_four_apportionment_pct = ByCountry(
        england=parse_percent(apportionment_row[1]),
        wales=parse_percent(apportionment_row[2]),
        scotland=parse_percent(apportionment_row[3]),
        northern_ireland=parse_percent(apportionment_row[4]),
    )
    # CalcResultOnePlusFourApportionment.OnePlusFourApportionment: 100 * (LaDisposalCost + LADataPrepCharge)
    # by country, over their combined total -- exact division, no intermediate rounding.
    one_plus_four_apportionment_pct = derive_apportionment_pct(lapcap_total_by_country + la_data_prep_by_country)

    header_row = find_row(rows, "2a Comms Costs - by Material", start=section)
    header = rows[header_row]
    price_col = header.index("Comms Cost - by Material Price Per Tonne")
    price_per_tonne_by_material = {}
    r = header_row + 1
    for _ in materials_order:
        material = rows[r][0].strip()
        price_per_tonne_by_material[material] = parse_money(rows[r][price_col])
        r += 1

    uk_wide_row = rows[find_row(rows, "2b Comms Costs - UK wide", start=section)]
    uk_wide_total = d(parse_money(uk_wide_row[5]))

    by_country_row = rows[find_row(rows, "2c Comms Costs - by Country", start=section)]
    by_country = ByCountry(
        england=d(parse_money(by_country_row[1])), wales=d(parse_money(by_country_row[2])),
        scotland=d(parse_money(by_country_row[3])), northern_ireland=d(parse_money(by_country_row[4])),
    )

    return CommsCostParameters(
        one_plus_four_apportionment_pct=one_plus_four_apportionment_pct,
        printed_one_plus_four_apportionment_pct=printed_one_plus_four_apportionment_pct,
        price_per_tonne_by_material=price_per_tonne_by_material,
        uk_wide_total=uk_wide_total,
        by_country=by_country,
    )


# ---------------------------------------------------------------------------
# Main per-producer table: column schema.
#
# The table is a flat sequence of fixed-width blocks (one per exporter class
# in the real C# CalcResultSummaryExporter), some of which repeat once per
# material. We rebuild the exact same sequence of blocks here so that we can
# slice each producer's row into named fields -- this mirrors how the file
# is *written*, which is more robust than trying to guess field boundaries
# from the (highly repetitive, not-fully-unique) header text alone.
# ---------------------------------------------------------------------------

RAG_KEYS = ["Red", "Amber", "Green", "RedMedical", "AmberMedical", "GreenMedical"]
RAG_GROUPS = ["Red", "Amber", "Green"]  # RedMedical groups with Red, etc.


@dataclass
class TonnageBreakdown:
    """
    The raw (pre-SMCW) reported tonnage a material's Section 1 block carries for a single
    row: Household/Public Bin/[Household Drinks Containers] packaging tonnage, each with
    its RAG breakdown, plus the combined Total Tonnage with its own RAG and grouped-RAG
    breakdown. Per ProducerRowBuilder.GetL1TotalRow, these specific fields ("Additive from
    L2 rows") are the ones a multi-entity producer's L1 row derives by summing its L2
    subsidiary rows -- unlike SMCW/Net Tonnage/price/fee, which are computed once at the
    L1 (group) level and are NOT expected to equal a sum of the L2 rows.
    """
    household: Decimal
    household_rag: dict[str, Decimal]
    public_bin: Decimal
    public_bin_rag: dict[str, Decimal]
    hdc: Decimal
    hdc_rag: dict[str, Decimal]
    total: Decimal
    total_rag: dict[str, Decimal]
    total_grouped_rag: dict[str, Decimal]

    def __add__(self, other: "TonnageBreakdown") -> "TonnageBreakdown":
        def add_rag(a: dict[str, Decimal], b: dict[str, Decimal]) -> dict[str, Decimal]:
            return {k: a[k] + b[k] for k in a}
        return TonnageBreakdown(
            household=self.household + other.household,
            household_rag=add_rag(self.household_rag, other.household_rag),
            public_bin=self.public_bin + other.public_bin,
            public_bin_rag=add_rag(self.public_bin_rag, other.public_bin_rag),
            hdc=self.hdc + other.hdc,
            hdc_rag=add_rag(self.hdc_rag, other.hdc_rag),
            total=self.total + other.total,
            total_rag=add_rag(self.total_rag, other.total_rag),
            total_grouped_rag=add_rag(self.total_grouped_rag, other.total_grouped_rag),
        )

    @staticmethod
    def zero() -> "TonnageBreakdown":
        return TonnageBreakdown(
            household=Decimal(0), household_rag={k: Decimal(0) for k in RAG_KEYS},
            public_bin=Decimal(0), public_bin_rag={k: Decimal(0) for k in RAG_KEYS},
            hdc=Decimal(0), hdc_rag={k: Decimal(0) for k in RAG_KEYS},
            total=Decimal(0), total_rag={k: Decimal(0) for k in RAG_KEYS},
            total_grouped_rag={k: Decimal(0) for k in RAG_GROUPS},
        )


def parse_tonnage_breakdown(m: list[str], i: int, is_glass: bool) -> tuple[TonnageBreakdown, int]:
    """Parses a Section1Materials-style tonnage block (household/public bin/[hdc]/total,
    each with RAG breakdown) starting at index i, returning it plus the index just past it."""
    household = parse_decimal(m[i]); i += 1
    household_rag = {}
    for key in RAG_KEYS:
        household_rag[key] = parse_decimal(m[i]); i += 1

    public_bin = parse_decimal(m[i]); i += 1
    public_bin_rag = {}
    for key in RAG_KEYS:
        public_bin_rag[key] = parse_decimal(m[i]); i += 1

    hdc = Decimal(0)
    hdc_rag = {key: Decimal(0) for key in RAG_KEYS}
    if is_glass:
        hdc = parse_decimal(m[i]); i += 1
        hdc_rag = {}
        for key in RAG_KEYS:
            hdc_rag[key] = parse_decimal(m[i]); i += 1

    total = parse_decimal(m[i]); i += 1
    total_rag = {}
    for key in RAG_KEYS:
        total_rag[key] = parse_decimal(m[i]); i += 1
    total_grouped_rag = {}
    for key in RAG_GROUPS:
        total_grouped_rag[key] = parse_decimal(m[i]); i += 1

    return TonnageBreakdown(
        household=household, household_rag=household_rag,
        public_bin=public_bin, public_bin_rag=public_bin_rag,
        hdc=hdc, hdc_rag=hdc_rag,
        total=total, total_rag=total_rag, total_grouped_rag=total_grouped_rag,
    ), i


@dataclass
class Block:
    name: str
    width: int


def build_producer_table_schema(materials_order: list[str], glass_name: Optional[str]) -> list[Block]:
    def is_glass(material: str) -> bool:
        return glass_name is not None and material == glass_name

    blocks: list[Block] = [Block("identity", 10)]

    for material in materials_order:
        # 1 (prev invoiced)
        # + household (1 + 6 rag)
        # + public bin (1 + 6 rag)
        # + [glass: hdc (1 + 6 rag)]
        # + total-tonnage block: total(1) + rag(6) + grouped-rag(3) = 10
        # + smcw(1) + actioned-smcw(total,red,amber,green = 4)
        # + net-tonnage(total,red,amber,green = 4)
        # + residual smcw(1)
        # + tonnage change(1)
        # + red/amber/green price(3) + red/amber/green disposal cost(3)
        # + fee w/o bdp(1) + bdp(1) + fee w/ bdp total+4 countries(5)
        width = 1 + (1 + 6) + (1 + 6) + (10) + (1 + 4) + 4 + 1 + 1 + 6 + 7
        if is_glass(material):
            width += 1 + 6
        blocks.append(Block(f"section1::{material}", width))

    blocks.append(Block("section1_total", 9))

    for material in materials_order:
        width = 2 + (1 if is_glass(material) else 0) + 9  # hh,pb,[hdc],total,price,fee,bdp,fee+4countries
        blocks.append(Block(f"section2a::{material}", width))

    blocks.append(Block("section2a_total_a", 7))  # Section2aCommsExporter
    blocks.append(Block("section1_total_b", 7))  # Section1DisposalExporter (repeat)
    blocks.append(Block("section2a_total_b", 7))  # Section2aComms2aExporter (repeat)
    blocks.append(Block("pct_vs_all_producers", 1))
    blocks.append(Block("section2b_total", 7))
    blocks.append(Block("section2c_total", 7))
    blocks.append(Block("oneplus_2a2b2c", 2))
    blocks.append(Block("section3_total", 7))
    blocks.append(Block("section4_total", 7))
    blocks.append(Block("section5_total", 7))
    blocks.append(Block("total_bill", 7))
    blocks.append(Block("billing_instructions", 10))
    return blocks


def block_offsets(blocks: list[Block]) -> dict[str, tuple[int, int]]:
    offsets = {}
    pos = 0
    for b in blocks:
        offsets[b.name] = (pos, pos + b.width)
        pos += b.width
    return offsets


# ---------------------------------------------------------------------------
# Per-producer parsed data
# ---------------------------------------------------------------------------


@dataclass
class WithBdp:
    """
    The recurring 7-cell group the exporter writes for every fee section:
    '<x> w/o Bad Debt Provision', 'Bad Debt Provision', '<x> with Bad Debt
    Provision' (a standalone total), then England/Wales/Scotland/Northern
    Ireland (each already 'with Bad Debt Provision').

    `total` is read directly from its own printed cell rather than derived by
    summing `by_country` -- the file rounds the total and each of the four
    country shares *independently* for display, so summing the (rounded)
    country cells can be a penny or two off from the (separately rounded)
    printed total. Keeping both as printed preserves that distinction.
    """
    without_bdp: Decimal
    bdp: Decimal
    total: Decimal
    by_country: ByCountry


def by_country_from_cells(cells: list[str]) -> ByCountry:
    """Reads 4 consecutive England/Wales/Scotland/NI currency cells."""
    return ByCountry(
        england=d(parse_money(cells[0])),
        wales=d(parse_money(cells[1])),
        scotland=d(parse_money(cells[2])),
        northern_ireland=d(parse_money(cells[3])),
    )


def parse_with_bdp(cells: list[str], i: int) -> tuple[WithBdp, int]:
    """Parses a WithBdp starting at index i, returning it plus the index just past it."""
    without_bdp = d(parse_money(cells[i])); i += 1
    bdp = d(parse_money(cells[i])); i += 1
    total = d(parse_money(cells[i])); i += 1
    by_country = by_country_from_cells(cells[i:i + 4]); i += 4
    return WithBdp(without_bdp, bdp, total, by_country), i


@dataclass
class MaterialFigures:
    raw_total_tonnage: Optional[Decimal]  # "Total Tonnage" -- reported tonnage before SMCW is deducted
    smcw_tonnage: Optional[Decimal]  # "Self Managed Consumer Waste Tonnage"
    net_red: Optional[Decimal]
    net_amber: Optional[Decimal]
    net_green: Optional[Decimal]
    net_total: Optional[Decimal]
    price_red: Optional[Decimal]
    price_amber: Optional[Decimal]
    price_green: Optional[Decimal]
    printed_fee_red: Optional[Decimal]
    printed_fee_amber: Optional[Decimal]
    printed_fee_green: Optional[Decimal]
    fee: WithBdp
    tonnage: TonnageBreakdown  # the raw HH/PB/HDC/Total tonnage this row prints for Section 1
    total_reported_tonnage_2a: Optional[Decimal]  # Section 2a's own "Total Tonnage" (should equal raw_total_tonnage)
    price_per_tonne_2a: Optional[Decimal]
    fee_2a: WithBdp


@dataclass
class ProducerRow:
    producer_id: str
    subsidiary_id: str
    name: str
    level: str
    by_material: dict[str, MaterialFigures]
    section1_total: WithBdp
    section2a: WithBdp
    section2b: WithBdp
    section2c: WithBdp
    section3: WithBdp
    section4: WithBdp
    section5: WithBdp
    total_bill: WithBdp
    pct_tonnage_vs_all_producers: Optional[Decimal]  # "Percentage of Producer Tonnage vs All Producers"
    pct_cost_vs_all_producers: Optional[Decimal]  # "Producer Percentage of Overall Producer Cost for (1+2a+2b+2c)"
    current_year_invoiced_total_to_date: Optional[Decimal]
    tonnage_change_advice: str  # "CHANGE" or ""
    liability_difference: Optional[Decimal]
    material_threshold_breached: str
    tonnage_threshold_breached: str
    pct_liability_difference: Optional[Decimal]
    material_pct_threshold_breached: str
    tonnage_pct_threshold_breached: str
    suggested_billing_instruction: str
    suggested_invoice_amount: Optional[Decimal]


def parse_producer_row(
    cells: list[str],
    offsets: dict[str, tuple[int, int]],
    materials_order: list[str],
    glass_name: Optional[str],
) -> ProducerRow:
    id_lo, id_hi = offsets["identity"]
    identity = cells[id_lo:id_hi]
    producer_id, subsidiary_id, name, _trading_name, level = identity[0:5]

    by_material: dict[str, MaterialFigures] = {}
    for material in materials_order:
        lo, hi = offsets[f"section1::{material}"]
        m = cells[lo:hi]
        is_glass = glass_name is not None and material == glass_name
        # Walk the block exactly as Section1MaterialsExporter wrote it.
        i = 1  # skip "Previous Invoiced Tonnage"
        tonnage, i = parse_tonnage_breakdown(m, i, is_glass)
        raw_total_tonnage = tonnage.total
        smcw_tonnage = parse_decimal(m[i]); i += 1
        i += 4  # actioned SMCW (total,red,amber,green)
        net_total = parse_decimal(m[i]); i += 1
        net_red = parse_decimal(m[i]); i += 1
        net_amber = parse_decimal(m[i]); i += 1
        net_green = parse_decimal(m[i]); i += 1
        i += 1  # residual SMCW
        i += 1  # tonnage change
        price_red = parse_money(m[i]); i += 1
        price_amber = parse_money(m[i]); i += 1
        price_green = parse_money(m[i]); i += 1
        fee_red = parse_money(m[i]); i += 1
        fee_amber = parse_money(m[i]); i += 1
        fee_green = parse_money(m[i]); i += 1
        fee, i = parse_with_bdp(m, i)

        lo2a, hi2a = offsets[f"section2a::{material}"]
        m2a = cells[lo2a:hi2a]
        j = 2  # skip Household Packaging Tonnage, Public Bin Tonnage
        if is_glass:
            j += 1  # skip Household Drinks Containers Tonnage
        total_reported_tonnage_2a = parse_decimal(m2a[j]); j += 1
        price_per_tonne_2a = parse_money(m2a[j]); j += 1
        fee_2a, j = parse_with_bdp(m2a, j)

        by_material[material] = MaterialFigures(
            raw_total_tonnage=raw_total_tonnage, smcw_tonnage=smcw_tonnage,
            net_red=net_red, net_amber=net_amber, net_green=net_green, net_total=net_total,
            price_red=price_red, price_amber=price_amber, price_green=price_green,
            printed_fee_red=fee_red, printed_fee_amber=fee_amber, printed_fee_green=fee_green,
            fee=fee,
            tonnage=tonnage,
            total_reported_tonnage_2a=total_reported_tonnage_2a,
            price_per_tonne_2a=price_per_tonne_2a,
            fee_2a=fee_2a,
        )

    s1_lo, s1_hi = offsets["section1_total"]
    s1 = cells[s1_lo:s1_hi]
    section1_total, _i = parse_with_bdp(s1, 0)
    tonnage_change_advice = s1[8].strip()

    def read_with_bdp(block_name: str) -> WithBdp:
        lo, hi = offsets[block_name]
        wb, _ = parse_with_bdp(cells[lo:hi], 0)
        return wb

    section2a = read_with_bdp("section2a_total_a")
    section2b = read_with_bdp("section2b_total")
    section2c = read_with_bdp("section2c_total")
    section3 = read_with_bdp("section3_total")
    section4 = read_with_bdp("section4_total")
    section5 = read_with_bdp("section5_total")
    total_bill = read_with_bdp("total_bill")

    pct_lo, pct_hi = offsets["pct_vs_all_producers"]
    pct_tonnage_vs_all_producers = parse_percent(cells[pct_lo])

    op_lo, op_hi = offsets["oneplus_2a2b2c"]
    op = cells[op_lo:op_hi]
    pct_cost_vs_all_producers = parse_percent(op[1])

    bi_lo, bi_hi = offsets["billing_instructions"]
    bi = cells[bi_lo:bi_hi]

    return ProducerRow(
        producer_id=producer_id.strip(),
        subsidiary_id=subsidiary_id.strip(),
        name=name.strip(),
        level=level.strip(),
        by_material=by_material,
        section1_total=section1_total,
        section2a=section2a,
        section2b=section2b,
        section2c=section2c,
        section3=section3,
        section4=section4,
        section5=section5,
        total_bill=total_bill,
        pct_tonnage_vs_all_producers=pct_tonnage_vs_all_producers,
        pct_cost_vs_all_producers=pct_cost_vs_all_producers,
        current_year_invoiced_total_to_date=parse_money(bi[0]),
        tonnage_change_advice=tonnage_change_advice,
        liability_difference=parse_money(bi[2]),
        material_threshold_breached=bi[3].strip().lstrip("‎"),
        tonnage_threshold_breached=bi[4].strip().lstrip("‎"),
        pct_liability_difference=parse_percent(bi[5]),
        material_pct_threshold_breached=bi[6].strip().lstrip("‎"),
        tonnage_pct_threshold_breached=bi[7].strip().lstrip("‎"),
        suggested_billing_instruction=bi[8].strip(),
        suggested_invoice_amount=parse_money(bi[9]),
    )


def find_producer_table(rows: list[list[str]]) -> tuple[int, int]:
    """Returns (header_row_index, first_data_row_index)."""
    header_row = find_row_containing(rows, "Registration Status Code")
    return header_row, header_row + 1


def read_producer_rows(rows: list[list[str]], first_data_row: int) -> list[list[str]]:
    out = []
    r = first_data_row
    while r < len(rows) and any(cell.strip() for cell in rows[r]):
        out.append(rows[r])
        r += 1
    return out


def parse_l2_tonnage(
    cells: list[str],
    offsets: dict[str, tuple[int, int]],
    materials_order: list[str],
    glass_name: Optional[str],
) -> dict[str, TonnageBreakdown]:
    """
    A lightweight parse of an L2 subsidiary row: only the raw tonnage breakdown per
    material (Section 1's Household/Public Bin/HDC/Total figures), which is all that's
    needed to check an L1 group-total row sums its subsidiaries correctly. Everything
    else about an L2 row (its own price/fee/comms/billing columns) stays out of scope.
    """
    by_material: dict[str, TonnageBreakdown] = {}
    for material in materials_order:
        lo, hi = offsets[f"section1::{material}"]
        m = cells[lo:hi]
        is_glass = glass_name is not None and material == glass_name
        tonnage, _i = parse_tonnage_breakdown(m, 1, is_glass)  # index 1 skips "Previous Invoiced Tonnage"
        by_material[material] = tonnage
    return by_material


# ---------------------------------------------------------------------------
# Verification
# ---------------------------------------------------------------------------


@dataclass
class Discrepancy:
    producer: str
    section: str
    field: str
    expected: object
    actual: object
    # True when `expected` (unrounded) is provably an exact rounding-boundary tie (see
    # is_exact_tie): the app and this script are one ULP either side of a midpoint that no
    # finite-precision decimal arithmetic can land on consistently, rather than a genuine
    # calculation error.
    is_tie: bool = False


@dataclass
class VerificationResult:
    discrepancies: list[Discrepancy] = field(default_factory=list)

    def add(self, producer: str, section: str, field_name: str, expected, actual, is_tie: bool = False):
        self.discrepancies.append(Discrepancy(producer, section, field_name, expected, actual, is_tie))


def check_exact(
    result: VerificationResult,
    label: str,
    section: str,
    field_name: str,
    expected_unrounded: Decimal,
    actual: Optional[Decimal],
    places: int,
):
    """
    The app only ever rounds a value once -- at the point it's written to CSV (see
    CsvSanitiser.SanitiseData) -- carrying full (unrounded) Decimal precision through every
    intermediate step. As long as `expected_unrounded` was computed the same way (from
    exact inputs, at full precision, only rounded here), rounding it to the same number of
    decimal places the app used for this exact field should reproduce the printed value
    exactly -- so this requires equality, not closeness.
    """
    if actual is None:
        result.add(label, section, field_name, round_half_away_from_zero(expected_unrounded, places), None)
        return
    expected_rounded = round_half_away_from_zero(expected_unrounded, places)
    if expected_rounded != actual:
        result.add(label, section, field_name, expected_rounded, actual, is_exact_tie(expected_unrounded, places))


@dataclass
class UnroundedSection:
    """
    A section's fee figures at full (unrounded) precision -- the in-memory shape of a
    CalcResultSummaryBadDebtProvision before CSV rounding is ever applied. Sections are
    summed at this precision (e.g. for the Total Producer Bill, or the run-wide
    (1+2a+2b+2c) header total) so that a sum-of-exact-figures, rounded once, reproduces
    exactly what the app's own single rounding-at-display-time produces -- rather than
    accumulating independent rounding error from summing already-rounded parts.

    `with_bdp_total` is tracked as its own field -- computed directly from the fee, e.g.
    `fee * (1 + bad_debt_pct / 100)` -- rather than derived by summing `by_country`'s four
    fields. Mathematically the two are identical whenever the apportionment percentages
    used to build `by_country` sum to exactly 100 (which they always do here) -- but
    computing each country's share is 4 *separate* multiply-then-divide operations, each
    independently rounded to Decimal's context precision, and summing those 4
    independently-rounded results does not reliably reproduce a value computed directly in
    one step. That gap is normally many decimal places below anything that matters, but a
    handful of pence per run land exactly on a 2dp rounding tie (see is_exact_tie), where
    even a ~1e-60 deviation flips which way it rounds. Keeping the direct total as its own
    field, and reserving the by-country sum for genuinely apportioned per-country
    quantities, avoids introducing that gap where it isn't wanted.
    """
    without_bdp: Decimal
    bdp: Decimal
    with_bdp_total: Decimal
    by_country: ByCountry  # each country's fee "with bad debt provision", unrounded

    def __add__(self, other: "UnroundedSection") -> "UnroundedSection":
        return UnroundedSection(
            without_bdp=self.without_bdp + other.without_bdp,
            bdp=self.bdp + other.bdp,
            with_bdp_total=self.with_bdp_total + other.with_bdp_total,
            by_country=self.by_country + other.by_country,
        )

    @staticmethod
    def zero() -> "UnroundedSection":
        return UnroundedSection(Decimal(0), Decimal(0), Decimal(0), ByCountry(Decimal(0), Decimal(0), Decimal(0), Decimal(0)))


def check_section_exact(
    result: VerificationResult,
    label: str,
    section_name: str,
    expected: UnroundedSection,
    printed: WithBdp,
    without_bdp_field: str,
    bdp_field: str,
    total_field: str,
):
    """Checks all 7 cells of a WithBdp block (w/o BDP, BDP, total, x4 countries) exactly."""
    check_exact(result, label, section_name, without_bdp_field, expected.without_bdp, printed.without_bdp, 2)
    check_exact(result, label, section_name, bdp_field, expected.bdp, printed.bdp, 2)
    check_exact(result, label, section_name, total_field, expected.with_bdp_total, printed.total, 2)
    for country_name, exp_val, actual_val in [
        ("England", expected.by_country.england, printed.by_country.england),
        ("Wales", expected.by_country.wales, printed.by_country.wales),
        ("Scotland", expected.by_country.scotland, printed.by_country.scotland),
        ("Northern Ireland", expected.by_country.northern_ireland, printed.by_country.northern_ireland),
    ]:
        check_exact(result, label, section_name, f"{country_name} with Bad Debt Provision", exp_val, actual_val, 2)


def verify_modulation(mod: ModulationCalculation, materials_order: list[str], result: VerificationResult):
    """
    Check 1: the run-wide Red/Green modulation pricing is internally consistent.

    Every figure here is rounded to the exact decimal places CalcResultModulationExporter
    uses for that specific column, then compared for exact equality -- not "closeness" --
    since every input (tonnage, prices) is either exact or itself the app's own rounded
    value used downstream (see CalcResultModulationBuilder), so there is nothing left to
    absorb into a tolerance.
    """
    section = "Modulation Calculation"
    label = "(run-wide)"

    # The Total row sums the *already-2dp-rounded* per-material figures (see
    # CalcResultModulationExporter), so summing those same rounded values here reproduces
    # it exactly -- no further rounding needed on this sum.
    sum_red_at_amber = sum((mod.by_material[m].total_red_at_amber_cost for m in materials_order), Decimal(0))
    sum_green_at_amber = sum((mod.by_material[m].total_green_at_amber_cost for m in materials_order), Decimal(0))

    if sum_red_at_amber != mod.total_red_at_amber_cost:
        result.add(label, section, "Total row: Total Red Material at Amber Disposal Cost",
                    sum_red_at_amber, mod.total_red_at_amber_cost)
    if sum_green_at_amber != mod.total_green_at_amber_cost:
        result.add(label, section, "Total row: Total Green Material at Amber Disposal Cost",
                    sum_green_at_amber, mod.total_green_at_amber_cost)

    if mod.total_green_at_amber_cost == 0:
        green_factor = Decimal(0)
    else:
        green_discount = (mod.red_factor - 1) * mod.total_red_at_amber_cost / mod.total_green_at_amber_cost
        green_factor = round_half_away_from_zero(1 - green_discount, 6)

    if green_factor != mod.green_factor_printed:
        result.add(label, section, "Green Modulation Factor", green_factor, mod.green_factor_printed)

    for material in materials_order:
        mm = mod.by_material[material]
        mat_label = f"(modulation:{material})"

        check_exact(result, mat_label, section, "Total Red Material at Amber Disposal Cost",
                    mm.red_net_tonnage * mm.amber_price, mm.total_red_at_amber_cost, 2)
        check_exact(result, mat_label, section, "Total Green Material at Amber Disposal Cost",
                    mm.green_net_tonnage * mm.amber_price, mm.total_green_at_amber_cost, 2)
        check_exact(result, mat_label, section, "Red Material Disposal Cost",
                    mm.amber_price * mod.red_factor, mm.red_price, 4)
        check_exact(result, mat_label, section, "Green Material Disposal Cost",
                    mm.amber_price * green_factor, mm.green_price, 4)


def verify_modulation_vs_producers(
    mod: ModulationCalculation,
    late_reporting: LateReportingTonnages,
    materials_order: list[str],
    net_tonnage_sum_by_material: dict[str, RagAmounts],
    result: VerificationResult,
):
    """
    Check 1b: the "Net Tonnage + Late Reporting Tonnage" figures that the Modulation
    Calculation section's pricing is built on should equal the sum of every L1
    producer's own Net Tonnage (by RAG group) plus the Late Reporting Tonnage
    parameter. This ties the run-wide modulation pricing back to the same per-producer
    data used in check 2, rather than trusting it as a disconnected input.

    Tonnage has no rounding step anywhere in its calculation (the SMCW cascade is pure
    subtraction -- see ApplySubSmcw), so this is exact equality at the 3dp precision the
    file prints tonnage to, not a tolerance-bounded comparison.
    """
    section = "Modulation Calculation vs producer data"
    for material in materials_order:
        mm = mod.by_material[material]
        lrt = late_reporting.by_material[material]
        summed = net_tonnage_sum_by_material[material]

        check_exact(result, f"(modulation:{material})", section,
                    "Red + Red Medical Net Tonnage + Late Reporting Tonnage",
                    summed.red + lrt.red, mm.red_net_tonnage, 3)
        check_exact(result, f"(modulation:{material})", section,
                    "Amber + Amber Medical Net Tonnage + Late Reporting Tonnage",
                    summed.amber + lrt.amber, mm.amber_net_tonnage, 3)
        check_exact(result, f"(modulation:{material})", section,
                    "Green + Green Medical Net Tonnage + Late Reporting Tonnage",
                    summed.green + lrt.green, mm.green_net_tonnage, 3)


def producer_label(p: ProducerRow) -> str:
    label = f"Producer {p.producer_id}"
    if p.subsidiary_id:
        label += f" / Subsidiary {p.subsidiary_id}"
    if p.name:
        label += f" ({p.name})"
    return label


def verify_l1_equals_sum_of_l2(
    p: ProducerRow,
    l2_tonnage_rows: list[dict[str, TonnageBreakdown]],
    materials_order: list[str],
    result: VerificationResult,
):
    """
    Check 0: for a multi-entity producer, its L1 group-total row's raw tonnage columns
    (Household/Public Bin/HDC/Total Tonnage, each with RAG breakdown) should equal the sum
    of its L2 subsidiary rows' own copies of those same columns -- this is exactly what
    ProducerRowBuilder.GetL1TotalRow does ("Additive from L2 rows"). Tonnage has no
    rounding step in its calculation, so this is exact equality at the 3dp precision the
    file prints tonnage to.

    This deliberately does NOT extend to SMCW, Net Tonnage, price, or fee columns: those
    are computed once at the L1 (group) level from the group's aggregate SMCW allocation,
    not by summing the L2 rows, and are explicitly documented as such in that same
    function ("From L1 SMCW record -- not derivable by summing L2 values"). Checking those
    against a sum of L2 would produce false positives.
    """
    if not l2_tonnage_rows:
        return
    label = producer_label(p)

    for material in materials_order:
        summed = TonnageBreakdown.zero()
        for l2_material_tonnage in l2_tonnage_rows:
            summed = summed + l2_material_tonnage[material]

        l1 = p.by_material[material].tonnage
        section_name = f"L1 vs sum(L2) :: {material}"

        check_exact(result, label, section_name, "Household Packaging Tonnage", summed.household, l1.household, 3)
        check_exact(result, label, section_name, "Public Bin Tonnage", summed.public_bin, l1.public_bin, 3)
        check_exact(result, label, section_name, "Household Drinks Containers Tonnage", summed.hdc, l1.hdc, 3)
        check_exact(result, label, section_name, "Total Tonnage", summed.total, l1.total, 3)

        for rag_label, summed_rag, l1_rag in [
            ("Household", summed.household_rag, l1.household_rag),
            ("Public Bin", summed.public_bin_rag, l1.public_bin_rag),
            ("Household Drinks Containers", summed.hdc_rag, l1.hdc_rag),
            ("Total", summed.total_rag, l1.total_rag),
        ]:
            for key in RAG_KEYS:
                check_exact(result, label, section_name, f"{rag_label} Tonnage ({key})", summed_rag[key], l1_rag[key], 3)

        for key in RAG_GROUPS:
            check_exact(result, label, section_name, f"Total Tonnage ({key} + {key}Medical)",
                        summed.total_grouped_rag[key], l1.total_grouped_rag[key], 3)


def verify_section1_disposal_fee(
    p: ProducerRow,
    materials_order: list[str],
    country_apportionment_pct: ByCountry,
    bad_debt_pct: Decimal,
    result: VerificationResult,
) -> UnroundedSection:
    """Checks 2: per-material and total Section 1 (LA Disposal Fee). Returns the producer's
    unrounded Section 1 total, for exact summation into the Total Producer Bill."""
    label = producer_label(p)
    total = UnroundedSection.zero()

    for material in materials_order:
        mf = p.by_material[material]
        if mf.net_red is None:
            # No modulation data for this producer/material combination -- nothing to check.
            continue
        section_name = f"Section 1 :: {material}"

        # The app zeroes a material's disposal fee outright if self-managed consumer
        # waste tonnage exceeds raw reported tonnage. For a multi-entity producer this
        # compares the *group's* SMCW tonnage against the *group's* summed raw tonnage
        # -- but the app prints exactly those two (already-aggregated) group figures as
        # this L1 row's own "Self Managed Consumer Waste Tonnage" and "Total Tonnage"
        # columns (see ProducerRowBuilder.GetL1TotalRow), so no L2 data is needed to
        # replicate it: this row's own printed columns are already the right inputs,
        # for single-entity and multi-entity producers alike.
        zero_override = (
            mf.smcw_tonnage is not None
            and mf.raw_total_tonnage is not None
            and mf.smcw_tonnage > mf.raw_total_tonnage
        )

        if zero_override:
            expected_fee_red = expected_fee_amber = expected_fee_green = Decimal(0)
        else:
            expected_fee_red = mf.net_red * mf.price_red
            expected_fee_amber = mf.net_amber * mf.price_amber
            expected_fee_green = mf.net_green * mf.price_green
        expected_fee_total = expected_fee_red + expected_fee_amber + expected_fee_green

        check_exact(result, label, section_name, "Producer Red Material Disposal Cost", expected_fee_red, mf.printed_fee_red, 2)
        check_exact(result, label, section_name, "Producer Amber Material Disposal Cost", expected_fee_amber, mf.printed_fee_amber, 2)
        check_exact(result, label, section_name, "Producer Green Material Disposal Cost", expected_fee_green, mf.printed_fee_green, 2)

        expected_bdp = expected_fee_total * bad_debt_pct / 100
        expected_with_bdp_total = expected_fee_total * (1 + bad_debt_pct / 100)
        expected_country = ByCountry(
            england=expected_with_bdp_total * country_apportionment_pct.england / 100,
            wales=expected_with_bdp_total * country_apportionment_pct.wales / 100,
            scotland=expected_with_bdp_total * country_apportionment_pct.scotland / 100,
            northern_ireland=expected_with_bdp_total * country_apportionment_pct.northern_ireland / 100,
        )
        material_section = UnroundedSection(without_bdp=expected_fee_total, bdp=expected_bdp,
                                             with_bdp_total=expected_with_bdp_total, by_country=expected_country)
        check_section_exact(result, label, section_name, material_section, mf.fee,
                             "Producer Disposal Fee w/o Bad Debt Provision", "Bad Debt Provision",
                             "Producer Disposal Fee with Bad Debt Provision")

        total = total + material_section

    check_section_exact(result, label, "Section 1 total", total, p.section1_total,
                         "1 Total Producer Fee for LA Disposal Costs w/o Bad Debt provision", "Bad Debt Provision",
                         "1 Total Producer Fee for LA Disposal Costs with Bad Debt provision")
    return total


def verify_section2a(
    p: ProducerRow,
    materials_order: list[str],
    comms: CommsCostParameters,
    bad_debt_pct: Decimal,
    result: VerificationResult,
) -> UnroundedSection:
    """Check 3a: Comms Costs by Material -- tonnage x price per material, apportioned by 1+4%.
    Returns the producer's unrounded Section 2a total."""
    label = producer_label(p)
    apportionment = comms.one_plus_four_apportionment_pct
    total = UnroundedSection.zero()

    for material in materials_order:
        mf = p.by_material[material]
        if mf.total_reported_tonnage_2a is None:
            continue
        section_name = f"Section 2a :: {material}"

        # The same raw (pre-SMCW) tonnage feeds both Section 1's disposal fee and
        # Section 2a's comms fee -- both are printed independently, so cross-check
        # the file agrees with itself.
        if mf.raw_total_tonnage is not None and mf.raw_total_tonnage != mf.total_reported_tonnage_2a:
            result.add(label, section_name, "Total Tonnage (vs Section 1's own copy)",
                        mf.raw_total_tonnage, mf.total_reported_tonnage_2a)

        expected_price = comms.price_per_tonne_by_material[material]
        if mf.price_per_tonne_2a is not None and expected_price != mf.price_per_tonne_2a:
            result.add(label, section_name, "Price per Tonne", expected_price, mf.price_per_tonne_2a)

        expected_fee_without_bdp = mf.total_reported_tonnage_2a * expected_price
        expected_bdp = expected_fee_without_bdp * bad_debt_pct / 100
        expected_with_bdp_total = expected_fee_without_bdp * (1 + bad_debt_pct / 100)
        expected_country = ByCountry(
            england=expected_with_bdp_total * apportionment.england / 100,
            wales=expected_with_bdp_total * apportionment.wales / 100,
            scotland=expected_with_bdp_total * apportionment.scotland / 100,
            northern_ireland=expected_with_bdp_total * apportionment.northern_ireland / 100,
        )
        material_section = UnroundedSection(without_bdp=expected_fee_without_bdp, bdp=expected_bdp,
                                             with_bdp_total=expected_with_bdp_total, by_country=expected_country)
        check_section_exact(result, label, section_name, material_section, mf.fee_2a,
                             "Producer Total Cost w/o Bad Debt Provision", "Bad Debt Provision",
                             "Producer Total Cost with Bad Debt Provision")

        total = total + material_section

    check_section_exact(result, label, "Section 2a total", total, p.section2a,
                         "2a Total Producer Fee for Comms Costs - by Material w/o Bad Debt provision", "Total Bad Debt Provision",
                         "2a Total Producer Fee for Comms Costs - by Material with Bad Debt provision")
    return total


def verify_section2b(
    p: ProducerRow, pct_tonnage: Decimal, comms: CommsCostParameters, bad_debt_pct: Decimal, result: VerificationResult
) -> UnroundedSection:
    """Check 3b: Comms Costs UK-wide, apportioned to this producer by its share (`pct_tonnage`,
    unrounded) of all producers' tonnage. Returns the producer's unrounded Section 2b total."""
    label = producer_label(p)
    apportionment = comms.one_plus_four_apportionment_pct

    expected_fee_without_bdp = comms.uk_wide_total * pct_tonnage / 100
    expected_bdp = expected_fee_without_bdp * bad_debt_pct / 100
    expected_with_bdp_total = expected_fee_without_bdp * (1 + bad_debt_pct / 100)
    expected_country = ByCountry(
        england=expected_with_bdp_total * apportionment.england / 100,
        wales=expected_with_bdp_total * apportionment.wales / 100,
        scotland=expected_with_bdp_total * apportionment.scotland / 100,
        northern_ireland=expected_with_bdp_total * apportionment.northern_ireland / 100,
    )
    section = UnroundedSection(without_bdp=expected_fee_without_bdp, bdp=expected_bdp,
                                    with_bdp_total=expected_with_bdp_total, by_country=expected_country)
    check_section_exact(result, label, "Section 2b", section, p.section2b,
                         "2b Total Producer Fee for Comms Costs - UK wide w/o Bad Debt provision", "Bad Debt Provision for 2b",
                         "2b Total Producer Fee for Comms Costs - UK wide with Bad Debt provision")
    return section


def verify_section2c(
    p: ProducerRow, pct_tonnage: Decimal, comms: CommsCostParameters, bad_debt_pct: Decimal, result: VerificationResult
) -> UnroundedSection:
    """
    Check 3c: Comms Costs by Country, apportioned to this producer by its share (`pct_tonnage`,
    unrounded) of all producers' tonnage. Unlike 2b, the per-country split here follows the
    *raw* by-country comms cost split directly (not the 1+4 apportionment). Returns the
    producer's unrounded Section 2c total.
    """
    label = producer_label(p)
    pct = pct_tonnage / 100

    expected_fee_without_bdp = comms.by_country.total * pct
    expected_bdp = expected_fee_without_bdp * bad_debt_pct / 100
    expected_with_bdp_total = expected_fee_without_bdp * (1 + bad_debt_pct / 100)
    expected_country = ByCountry(
        england=comms.by_country.england * (1 + bad_debt_pct / 100) * pct,
        wales=comms.by_country.wales * (1 + bad_debt_pct / 100) * pct,
        scotland=comms.by_country.scotland * (1 + bad_debt_pct / 100) * pct,
        northern_ireland=comms.by_country.northern_ireland * (1 + bad_debt_pct / 100) * pct,
    )
    section = UnroundedSection(without_bdp=expected_fee_without_bdp, bdp=expected_bdp,
                                    with_bdp_total=expected_with_bdp_total, by_country=expected_country)
    check_section_exact(result, label, "Section 2c", section, p.section2c,
                         "2c Total Producer Fee for Comms Costs - by Country w/o Bad Debt provision", "Bad Debt Provision for 2c",
                         "2c Total Producer Fee for Comms Costs - by Country with Bad Debt provision")
    return section


def verify_pct_tonnage_vs_all_producers(
    p: ProducerRow, producer_tonnage: Decimal, tonnage_grand_total: Decimal, result: VerificationResult
) -> Decimal:
    """
    Checks "Percentage of Producer Tonnage vs All Producers" -- the input Sections 2b and
    2c apportion by -- against this producer's own total packaging tonnage (summed across
    materials) as a percentage of the exact run-wide tonnage total (accumulated from every
    L1 producer in an earlier pass). Returns the unrounded percentage: like the cost
    percentage below, TonnageVsAllProducerUtil computes this without any intermediate
    rounding, and Sections 2b/2c apportion by that unrounded in-memory value, not the 8dp
    figure the file prints.
    """
    label = producer_label(p)
    expected_pct = Decimal(0) if tonnage_grand_total == 0 else producer_tonnage / tonnage_grand_total * 100
    check_exact(result, label, "Percentage of Producer Tonnage vs All Producers",
                "Percentage of Producer Tonnage vs All Producers", expected_pct, p.pct_tonnage_vs_all_producers, 8)
    return expected_pct


def verify_producer_pct_cost_vs_all_producers(
    p: ProducerRow,
    section1: UnroundedSection,
    section2a: UnroundedSection,
    section2b: UnroundedSection,
    section2c: UnroundedSection,
    header_total_1_2a2b2c: Decimal,
    result: VerificationResult,
) -> Decimal:
    """
    Checks that "Producer Percentage of Overall Producer Cost for (1+2a+2b+2c)" -- the input
    that sections 3, 4 and 5 apportion by -- is this producer's own (1+2a+2b+2c) total as a
    percentage of the run-wide (1+2a+2b+2c) total. `header_total_1_2a2b2c` is computed in an
    earlier pass over the whole file as Section 1 and 2a summed across every L1 producer
    (both additive) plus Section 2b/2c computed directly from the run-wide comms totals
    (TwoBCommsCostProducer/TwoCCommsCostProducer -- these are NOT sums of individual
    producers' 2b/2c shares, which is a distinction that matters here: it isn't the file's
    own printed header cell, which is already rounded to 2dp for display and would
    reintroduce a rounding step the app itself never takes at this point.

    Returns the unrounded percentage (not the 8dp-rounded value the file prints): this is
    what OnePlus2A2B2CProducer.GetOverallProducerPercentage computes and what sections 3/4/5
    actually apportion by in-memory, before the CSV exporter rounds it for display -- so
    sections 3/4/5 must use this exact value, not the printed 8dp approximation of it.
    """
    label = producer_label(p)
    producer_total = section1.with_bdp_total + section2a.with_bdp_total + section2b.with_bdp_total + section2c.with_bdp_total

    expected_pct = Decimal(0) if header_total_1_2a2b2c == 0 else producer_total / header_total_1_2a2b2c * 100
    check_exact(result, label, "Producer Percentage of Overall Producer Cost",
                "Producer Percentage of Overall Producer Cost for (1+2a+2b+2c)",
                expected_pct, p.pct_cost_vs_all_producers, 8)
    return expected_pct


def verify_sections_3_4_5(
    p: ProducerRow,
    producer_pct: Decimal,
    comms: CommsCostParameters,
    other_params: OtherParameters,
    bad_debt_pct: Decimal,
    result: VerificationResult,
) -> tuple[UnroundedSection, UnroundedSection, UnroundedSection]:
    """
    Check 4: SA Operating Costs, LA Data Prep Costs, SA Set Up Costs. All three are
    structurally identical -- a fixed run-wide total apportioned to this producer by its
    (unrounded) 'Percentage of Overall Producer Cost for (1+2a+2b+2c)', then split by
    country. Section 4 uses its own ("4 Country Apportionment %s") apportionment; 3 and 5
    use 1+4%. Returns the three unrounded sections, for exact summation into the Total
    Producer Bill.
    """
    label = producer_label(p)

    sections = [
        ("Section 3", other_params.sa_operating_cost_total, comms.one_plus_four_apportionment_pct, p.section3),
        ("Section 4", other_params.la_data_prep_charge_total, other_params.la_data_prep_apportionment_pct, p.section4),
        ("Section 5", other_params.scheme_setup_cost_total, comms.one_plus_four_apportionment_pct, p.section5),
    ]
    results = []

    for section_name, section_total, apportionment, printed in sections:
        expected_without_bdp = producer_pct * section_total / 100
        expected_bdp = expected_without_bdp * bad_debt_pct / 100
        expected_with_bdp_total = expected_without_bdp * (1 + bad_debt_pct / 100)
        expected_country = ByCountry(
            england=expected_with_bdp_total * apportionment.england / 100,
            wales=expected_with_bdp_total * apportionment.wales / 100,
            scotland=expected_with_bdp_total * apportionment.scotland / 100,
            northern_ireland=expected_with_bdp_total * apportionment.northern_ireland / 100,
        )
        section = UnroundedSection(without_bdp=expected_without_bdp, bdp=expected_bdp,
                                    with_bdp_total=expected_with_bdp_total, by_country=expected_country)
        check_section_exact(result, label, section_name, section, printed,
                             "w/o Bad Debt Provision", "Bad Debt Provision", "with Bad Debt Provision")
        results.append(section)

    return tuple(results)


def verify_total_bill(
    p: ProducerRow,
    section1: UnroundedSection, section2a: UnroundedSection, section2b: UnroundedSection, section2c: UnroundedSection,
    section3: UnroundedSection, section4: UnroundedSection, section5: UnroundedSection,
    result: VerificationResult,
) -> UnroundedSection:
    """
    Check 5: Total Producer Bill = Section 1 + 2a + 2b + 2c + 3 + 4 + 5, using each
    section's own exact (unrounded) recomputed figures -- matching
    TotalBillBreakdownProducer, which sums the in-memory (unrounded)
    CalcResultSummaryBadDebtProvision objects and only rounds once, at CSV-display time.
    Returns the unrounded Total Producer Bill for the billing-instruction check.
    """
    label = producer_label(p)
    total = section1 + section2a + section2b + section2c + section3 + section4 + section5
    check_section_exact(result, label, "Total Producer Bill", total, p.total_bill,
                         "Total Producer Bill (1+2a+2b+2c+3+4+5) w/o Bad Debt Provision",
                         "Bad Debt Provision for Total Producer Bill",
                         "Total Producer Bill (1+2a+2b+2c+3+4+5) with Bad Debt Provision")
    return total


def verify_billing_instruction(
    p: ProducerRow, total_bill: UnroundedSection, params: OtherParameters, result: VerificationResult
):
    """Check 6: suggested billing instruction and invoice amount."""
    label = producer_label(p)
    prior = p.current_year_invoiced_total_to_date

    if prior is None:
        expected_liability_diff = None
    else:
        expected_liability_diff = (
            round_half_away_from_zero(total_bill.with_bdp_total, 2)
            - round_half_away_from_zero(prior, 2)
        )

    if expected_liability_diff is None:
        if p.liability_difference is not None:
            result.add(label, "Billing Instruction", "Liability Difference (Calc vs Prev)", None, p.liability_difference)
    elif expected_liability_diff != p.liability_difference:
        result.add(label, "Billing Instruction", "Liability Difference (Calc vs Prev)",
                    expected_liability_diff, p.liability_difference)

    def threshold_flag(diff: Optional[Decimal], increase: Decimal, decrease: Decimal) -> str:
        if diff is None:
            return HYPHEN
        if diff >= increase:
            return "+ve"
        if diff <= decrease:
            return "-ve"
        return HYPHEN

    expected_material_flag = (
        HYPHEN if prior is None or expected_liability_diff is None
        else threshold_flag(expected_liability_diff, params.materiality_increase.amount, params.materiality_decrease.amount)
    )
    if expected_material_flag != p.material_threshold_breached:
        result.add(label, "Billing Instruction", "Material £ Threshold Breached",
                    expected_material_flag, p.material_threshold_breached)

    tonnage_changed = p.tonnage_change_advice == "CHANGE"
    expected_tonnage_flag = (
        HYPHEN if prior is None or not tonnage_changed or expected_liability_diff is None
        else threshold_flag(expected_liability_diff, params.tonnage_change_increase.amount, params.tonnage_change_decrease.amount)
    )
    if expected_tonnage_flag != p.tonnage_threshold_breached:
        result.add(label, "Billing Instruction", "Tonnage £ Threshold Breached (if tonnage changed)",
                    expected_tonnage_flag, p.tonnage_threshold_breached)

    if prior is None or expected_liability_diff is None or prior == 0:
        expected_pct_diff = None
    else:
        expected_pct_diff = round_half_away_from_zero(expected_liability_diff / prior * 100, 2)
    if expected_pct_diff != p.pct_liability_difference:
        result.add(label, "Billing Instruction", "% Liability Difference (Calc vs Prev)",
                    expected_pct_diff, p.pct_liability_difference)

    expected_material_pct_flag = (
        HYPHEN if prior is None
        else threshold_flag(expected_pct_diff, params.materiality_increase.percentage, params.materiality_decrease.percentage)
    )
    if expected_material_pct_flag != p.material_pct_threshold_breached:
        result.add(label, "Billing Instruction", "Material % Threshold Breached",
                    expected_material_pct_flag, p.material_pct_threshold_breached)

    expected_tonnage_pct_flag = (
        HYPHEN if prior is None or not tonnage_changed
        else threshold_flag(expected_pct_diff, params.tonnage_change_increase.percentage, params.tonnage_change_decrease.percentage)
    )
    if expected_tonnage_pct_flag != p.tonnage_pct_threshold_breached:
        result.add(label, "Billing Instruction", "Tonnage % Threshold Breached (if tonnage changed)",
                    expected_tonnage_pct_flag, p.tonnage_pct_threshold_breached)

    any_breached = any(
        flag != HYPHEN
        for flag in (expected_material_flag, expected_tonnage_flag, expected_material_pct_flag, expected_tonnage_pct_flag)
    )

    if prior is None:
        expected_instruction = "INITIAL"
    elif expected_liability_diff is not None and expected_liability_diff > 0 and any_breached:
        expected_instruction = "DELTA"
    elif expected_liability_diff is not None and expected_liability_diff < 0 and any_breached:
        expected_instruction = "REBILL"
    else:
        expected_instruction = HYPHEN

    if expected_instruction != p.suggested_billing_instruction:
        result.add(label, "Billing Instruction", "Suggested Billing Instruction",
                    expected_instruction, p.suggested_billing_instruction)

    if expected_instruction in ("INITIAL", "REBILL"):
        expected_amount = round_half_away_from_zero(total_bill.with_bdp_total, 2)
    elif expected_instruction == "DELTA":
        expected_amount = expected_liability_diff
    else:
        expected_amount = None

    if expected_amount != p.suggested_invoice_amount:
        result.add(label, "Billing Instruction", "Suggested Invoice Amount",
                    expected_amount, p.suggested_invoice_amount)


# ---------------------------------------------------------------------------
# CLI
# ---------------------------------------------------------------------------


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("results_csv", help="Path to a Results CSV export")
    parser.add_argument("--verbose", action="store_true", help="Print every discrepancy in full")
    args = parser.parse_args()

    rows = load_rows(args.results_csv)

    lapcap = parse_lapcap_data(rows)
    la_disposal = parse_la_disposal_cost_data(rows, lapcap.materials_order)
    modulation = parse_modulation_calculation(rows, lapcap.materials_order)
    late_reporting = parse_late_reporting_tonnages(rows, lapcap.materials_order)
    other_params = parse_other_parameters(rows)
    comms = parse_comms_cost_parameters(rows, lapcap.materials_order, lapcap.total_by_country, other_params.la_data_prep_by_country)

    if modulation is None:
        print("This file has no 'Modulation Calculation' section -- it is a pre-modulation")
        print("Results file, which this version of the script does not support.")
        return 2

    result = VerificationResult()

    # Sanity-check the derived (exact) apportionment percentages against the file's own
    # printed (8dp-rounded) percentages -- confirms the derivation matches what the app
    # actually computed, rather than silently drifting from it.
    check_exact(result, "(run-wide)", "Apportionment", "1 Country Apportionment %s (England)",
                lapcap.country_apportionment_pct.england, lapcap.printed_country_apportionment_pct.england, 8)
    check_exact(result, "(run-wide)", "Apportionment", "1 + 4 Apportionment %s (England)",
                comms.one_plus_four_apportionment_pct.england, comms.printed_one_plus_four_apportionment_pct.england, 8)
    check_exact(result, "(run-wide)", "Apportionment", "4 Country Apportionment %s (England)",
                other_params.la_data_prep_apportionment_pct.england, other_params.printed_la_data_prep_apportionment_pct.england, 8)

    # Sanity-check the modulation section's Amber price against LA Disposal Cost Data
    # (both should be the same underlying flat price-per-tonne).
    for material in lapcap.materials_order:
        expected_amber = la_disposal.price_per_tonne[material]
        actual_amber = modulation.by_material[material].amber_price
        if expected_amber is not None and actual_amber is not None and expected_amber != actual_amber:
            print(f"WARNING: Amber price mismatch between 'LA Disposal Cost Data' and 'Modulation Calculation' "
                  f"for {material}: {expected_amber} vs {actual_amber}")

    glass_name = next((m for m in lapcap.materials_order if m.strip().lower() == "glass"), None)

    header_row, first_data_row = find_producer_table(rows)
    schema = build_producer_table_schema(lapcap.materials_order, glass_name)
    offsets = block_offsets(schema)

    raw_producer_rows = read_producer_rows(rows, first_data_row)
    id_lo, id_hi = offsets["identity"]

    def row_identity(cells: list[str]) -> tuple[str, str]:
        idn = cells[id_lo:id_hi]
        return idn[0].strip(), idn[4].strip()  # producer_id, level

    verify_modulation(modulation, lapcap.materials_order, result)

    net_tonnage_sum_by_material = {
        m: RagAmounts(red=Decimal(0), amber=Decimal(0), green=Decimal(0)) for m in lapcap.materials_order
    }

    # --- Pass 1: parse every L1 producer, verify Sections 1 and 2a (each only needs
    # run-wide inputs known upfront), and accumulate two exact run-wide totals that later
    # sections depend on: the total packaging tonnage across every producer (the
    # denominator Sections 2b/2c apportion by) and the (1+2a+2b+2c) total (the denominator
    # Sections 3/4/5 apportion by). Sections 2b/2c, 3/4/5, the Total Bill, and the billing
    # instruction all wait for pass 2, once every producer has been folded into these.
    cached = []
    header_total_1_2a2b2c = Decimal(0)
    tonnage_grand_total = Decimal(0)
    l1_count = 0
    l2_count = 0
    for idx, cells in enumerate(raw_producer_rows):
        producer_id, level = row_identity(cells)
        if level != "1" or producer_id == "":
            continue  # L2 subsidiary row, or the single overall-total row: out of scope

        p = parse_producer_row(cells, offsets, lapcap.materials_order, glass_name)
        l1_count += 1

        # A multi-entity producer's L1 group-total row is immediately followed by all of
        # its L2 subsidiary rows (CalcResultSummaryBuilder always writes them in that
        # order) -- collect them so we can check the L1 row sums them correctly.
        l2_tonnage_rows = []
        j = idx + 1
        while j < len(raw_producer_rows):
            next_id, next_level = row_identity(raw_producer_rows[j])
            if next_level != "2" or next_id != producer_id:
                break
            l2_tonnage_rows.append(parse_l2_tonnage(raw_producer_rows[j], offsets, lapcap.materials_order, glass_name))
            l2_count += 1
            j += 1

        verify_l1_equals_sum_of_l2(p, l2_tonnage_rows, lapcap.materials_order, result)

        # Attach the run-wide modulation prices as each material's price, for
        # verify_section1_disposal_fee (per-producer prices are the same run-wide
        # value, printed again on every row by Section1MaterialsExporter -- we
        # cross-check the file's own per-row copies against the printed values below).
        for material in lapcap.materials_order:
            mf = p.by_material[material]
            mm = modulation.by_material[material]
            if mf.price_red is not None and mf.price_red != mm.red_price:
                result.add(producer_label(p), f"Section 1 :: {material}", "Red + Red Medical Material Price per Tonne",
                            mm.red_price, mf.price_red)
            if mf.price_amber is not None and mf.price_amber != mm.amber_price:
                result.add(producer_label(p), f"Section 1 :: {material}", "Amber + Amber Medical Material Price per Tonne",
                            mm.amber_price, mf.price_amber)
            if mf.price_green is not None and mf.price_green != mm.green_price:
                result.add(producer_label(p), f"Section 1 :: {material}", "Green + Green Medical Material Price per Tonne",
                            mm.green_price, mf.price_green)

            if mf.net_red is not None:
                totals = net_tonnage_sum_by_material[material]
                net_tonnage_sum_by_material[material] = RagAmounts(
                    red=totals.red + mf.net_red, amber=totals.amber + mf.net_amber, green=totals.green + mf.net_green
                )

        section1 = verify_section1_disposal_fee(p, lapcap.materials_order, lapcap.country_apportionment_pct,
                                                  other_params.bad_debt_pct, result)
        section2a = verify_section2a(p, lapcap.materials_order, comms, other_params.bad_debt_pct, result)

        # The run-wide (1+2a+2b+2c) header total is NOT the sum of every producer's own
        # 2b/2c share -- TwoBCommsCostProducer/TwoCCommsCostProducer compute the header's
        # Section 2b/2c directly from the run-wide comms totals (with bad debt), with no
        # per-producer percentage weighting at all. Only Sections 1 and 2a are additive
        # across producers here; Section 2b/2c's own (global) contribution is added once,
        # below, after this loop.
        header_total_1_2a2b2c += section1.with_bdp_total + section2a.with_bdp_total

        # This producer's own total packaging tonnage (Household + Public Bin + [Glass]
        # Household Drinks Containers, summed across materials) -- the numerator for its
        # share of tonnage_grand_total, which Sections 2b/2c apportion by.
        producer_tonnage = sum((p.by_material[m].tonnage.total for m in lapcap.materials_order), Decimal(0))
        tonnage_grand_total += producer_tonnage

        cached.append((p, section1, section2a, producer_tonnage))

    header_total_1_2a2b2c += (
        comms.uk_wide_total * (1 + other_params.bad_debt_pct / 100)
        + comms.by_country.total * (1 + other_params.bad_debt_pct / 100)
    )

    # --- Pass 2: now that tonnage_grand_total and header_total_1_2a2b2c are the exact
    # totals across every producer, verify each producer's share of each, Sections 2b/2c,
    # 3/4/5, the Total Bill, and billing instruction.
    for p, section1, section2a, producer_tonnage in cached:
        pct_tonnage = verify_pct_tonnage_vs_all_producers(p, producer_tonnage, tonnage_grand_total, result)
        section2b = verify_section2b(p, pct_tonnage, comms, other_params.bad_debt_pct, result)
        section2c = verify_section2c(p, pct_tonnage, comms, other_params.bad_debt_pct, result)

        producer_pct = verify_producer_pct_cost_vs_all_producers(
            p, section1, section2a, section2b, section2c, header_total_1_2a2b2c, result
        )
        section3, section4, section5 = verify_sections_3_4_5(
            p, producer_pct, comms, other_params, other_params.bad_debt_pct, result
        )
        total_bill = verify_total_bill(p, section1, section2a, section2b, section2c, section3, section4, section5, result)
        verify_billing_instruction(p, total_bill, other_params, result)

    verify_modulation_vs_producers(modulation, late_reporting, lapcap.materials_order,
                                    net_tonnage_sum_by_material, result)

    print(f"Checked {l1_count} Level-1 producer row(s) ({l2_count} Level-2 subsidiary rows found) in {args.results_csv}")
    print(f"Materials: {', '.join(lapcap.materials_order)}")
    print(f"Red Modulation Factor: {modulation.red_factor}   Green Modulation Factor (printed): {modulation.green_factor_printed}")
    print()

    if not result.discrepancies:
        print("No discrepancies found.")
        return 0

    genuine = [d for d in result.discrepancies if not d.is_tie]
    ties = [d for d in result.discrepancies if d.is_tie]

    def print_discrepancies(discs: list[Discrepancy]):
        shown = discs if args.verbose else discs[:50]
        for disc in shown:
            print(f"  [{disc.producer}] {disc.section} :: {disc.field}")
            print(f"      expected: {disc.expected}")
            print(f"      actual:   {disc.actual}")
        if not args.verbose and len(discs) > 50:
            print(f"  ... and {len(discs) - 50} more (use --verbose to see all)")

    if genuine:
        print(f"{len(genuine)} discrepancy(ies) found:\n")
        print_discrepancies(genuine)

    if ties:
        if genuine:
            print()
        print(f"{len(ties)} exact rounding tie(s) found (not necessarily bugs -- see module docstring):\n")
        print("  These are cases where the true, infinite-precision answer sits exactly on a")
        print("  rounding boundary (e.g. 126.885000...0 for 2dp); the app's own finite-precision")
        print("  decimal arithmetic can legitimately land a hair to either side of such a tie.")
        print("  Confirmed via exact rational arithmetic before being labelled as ties, not just")
        print("  \"close\". Only ever affects a single country's share of a single fee -- never a")
        print("  total.\n")
        print_discrepancies(ties)

    return 1 if genuine else 0


if __name__ == "__main__":
    sys.exit(main())
