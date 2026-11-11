#!/usr/bin/env python3
"""
Build a v_extract_recent_pom_org_data.csv-shaped CSV directly from a Profic
"Results File" export, instead of from the live database via
../v_extract_recent_pom_org_data.sql.

WHY THIS EXISTS
----------------
v_extract_recent_pom_org_data.sql reproduces one row per (org, submission
period) of household packaging tonnage, sourced from the real packaging_data
table. A Results File run already contains the same granularity of data
(per producer/subsidiary, per material, per RAG rating, per H1/H2 period) -
this script reads that back out, so the Profic scripts can be exercised
against a synthetic org-data extract without needing direct DB access.

WHAT THIS CANNOT POPULATE (left blank/'NULL' - never fabricated)
------------------------------------------------------------------
The Results File is a household-packaging fee-calculation run. It has no
concept of:
  - Genuine submission history: there is no "first submission" vs "latest
    submission" concept in a single run's output - each output row IS one
    submission period (H1 or H2). So the first_/latest_ pairs of metadata
    columns are populated identically from that same row's data (documented
    per-column below), not derived from real submission history.
  - Non-household-facing packaging categories: Non-household drinks
    containers, Total Non-Household packaging, Self-managed organisation
    waste, Reusable packaging, Small organisation packaging - all, and
    Transitional organisation packaging - all. This file only calculates
    the household-waste disposal-fee obligation; those categories belong to
    a different (organisation-level) reporting obligation not present here.
  - The Plastic-Rigid / Plastic-Flexible sub-split: the raw H1/H2 sections
    only carry a single undifferentiated "Plastic" tonnage.
  - "(No.Units)" unit-count columns for drinks containers: the file only
    ever reports weights (tonnes), never unit counts.

KEY DESIGN DECISIONS (agreed in conversation, 2026-07-30)
-----------------------------------------------------------
1. Two output rows per producer (Level 1 - see decision 11): one for the "H1 Packaging
   Data - Submitted & Projected" section, one for "H2 Packaging Data -
   Submitted & Projected". This is not "splitting" a combined figure - the
   Results File already keeps H1 and H2 as separate per-period tables; we
   just emit each as its own synthetic org-data row, tagged with that
   period's code.

2. RAG tonnage columns are taken AS REPORTED, from the plain (non-Projected,
   non-defaulted) columns of each period's own section:
     Total Household packaging-X          <- "Household Tonnage"
     Total Household packaging-X RAM R    <- "Household Red Material Tonnage"
     Total Household packaging-X RAM G    <- "Household Green Material Tonnage"
     Total Household packaging-X RAM A    <- "Household Amber Material Tonnage"
     (+ RAM-M / R-M / G-M / A-M from the "* Medical Material Tonnage" cols)
   RAM / RAM-M totals are computed as the sum of their R/G/A parts.
   We deliberately do NOT fold "Tonnage Without RAM(Defaulted to Red)" (H2)
   or use the "Projected <Material> Breakdown" block (H1) - those are the
   CALCULATOR's output after defaulting unrated tonnage to Red, not the
   input. Per the user: report the input; unrated tonnage is left as a
   genuine gap (RAM-sum < Total for that row), exactly like the real
   packaging_data extract's own RAM pivot (which is SUM(weight WHERE
   ram_rag_rating IS NOT NULL) - i.e. it excludes unrated rows too).

3. Fields that only exist as an ANNUAL (H1+H2 combined) figure in the
   Results File - Self-managed consumer waste, sourced from the
   "Calculation Result" section's per-material "Self Managed Consumer
   Waste Tonnage" - are attributed 100% to the H2 row and 0 to the H1 row.
   This is a deliberate simplification (agreed answer), not a derivation.

3b. The H1/H2 "Packaging Data - Submitted and Projected" sections are NOT a
    full per-producer dump - they only exist as an audit trail for
    producers who had at least one blank/unrated material in either
    period. A producer whose H1 and H2 submissions were both fully RAG-
    rated for every material never appears in either section at all (of
    ~4,300 producers in Calculation Result, only ~1,950 show up in H1/H2 -
    confirmed by set-difference against the Results File used to build
    this script). For that "calc-only" majority we have no per-period data
    - only the annual Calculation Result figures. Per the same "SMCW"
    principle as (3): the household/public-bin/drinks-container totals AND
    their RAM/RAM-M/R/G/A breakdown are taken from Calculation Result and
    attributed 100% to the H2 row, 0 to H1. Using Calculation Result's
    Red/Amber/Green here does not break the "report the input, not the
    defaulted output" rule of decision 2: these producers had no unrated
    tonnage (that's why they aren't in the H1/H2 sections), so nothing was
    defaulted into Red and the figures are the producer's own ratings.
    Leaving the breakdown blank instead (as an earlier version did) made
    downstream RAM-gap filling treat each producer's entire tonnage as
    unrated and default it all to Red.

4. Org_ID = Producer ID (only Level 1 rows are output - decision 11).
   Org_name is looked up from the "Calculation Result" section's
   "Producer / Subsidiary Name" column for that producer's Level 1 row.

5. organisation_size is assumed 'L' throughout: this Results File / the SQL
   it was built to match is explicitly Large-producer-only
   (v_extract_recent_pom_org_data.sql: "client only uses Large producer
   data"). Not derived from any per-row signal.

6. Reporting_Year = the period's calendar year + 1 (matches
   v_extract_recent_pom_org_data.sql's POM_With_Year: "submission year + 1
   is the obligation year").

7. Enrolment_status is assumed 'Approved' throughout: only producers whose
   registration has been approved get processed into this Results File at
   all (per user confirmation), so every producer we can output a row for
   qualifies. Not derived from any per-row signal, but a safe inference
   from the fact that the row exists in the file at all.

8. Organisation_soft_deleted is assumed '0' throughout, for the same
   reason as (7): a soft-deleted organisation would never have made it
   into a live calculation run, so its mere presence in the Results File
   implies it isn't soft-deleted. Not derived from any per-row signal, but
   the same "presence implies status" inference as Enrolment_status.

9. Nation_of_enrolment cannot be inferred from anything in the Results
   File. The only per-nation figures in the file (the "England/Wales/
   Scotland/Northern Ireland with Bad Debt Provision" columns in
   Calculation Result) are each producer's disposal fee split across all
   four nations using the SAME fixed LAPCAP apportionment percentage from
   the "1 Country Apportionment %" table at the top of the file (verified:
   e.g. two different producers' Aluminium England/Wales fee figures are
   both in exactly the 17.24%/27.59% ratio set globally for Aluminium,
   regardless of where either producer is actually registered) - i.e. it's
   a cost-recovery allocation, not a signal of which nation the producer
   itself is enrolled in. There is no registered-nation field anywhere in
   this export. See decision 10.

10. Organisation registration/submission-metadata fields that have no
    signal anywhere in the Results File - CH_number, Nation_of_enrolment,
    Enrolment_date_time, Nation_of_Compliance_Scheme_regulator, the
    Organisation_data_* first/latest submission metadata columns, the
    Packaging_data_* first/latest submission status, datetime and
    CS_or_Direct columns,
    Single_File_Submission_* flags, and the fps/lps/fos/los filename
    columns - are hard-coded to a single fixed default value per column
    (see the DEFAULT_* constants), not derived or fabricated per-row. Per
    the user: downstream tooling just needs these columns non-blank: the
    actual value doesn't matter, so a single unvarying constant is used
    rather than inventing plausible-looking per-row data.

11. Only Level 1 rows are output; Level 2 (subsidiary) rows are dropped.
    For a producer group, the H1/H2 and Calculation Result sections each
    carry a Level 1 row (blank Subsidiary ID) holding the sum of the
    group's Level 2 member rows; a standalone producer is a Level 1 row
    with nothing under it. Profic treats every org-data row as a Level 1
    producer, so sending the Level 2 rows instead would stop SMCW being
    shared across members of the same group. (An H1/H2 section always
    lists every member of a group it lists, so its Level 1 row is the
    full group total.)

12. Partial-obligation pro-rating is undone. For a producer that joined
    or left during the year, Calculation Result's tonnages (and its Self-
    managed consumer waste) have been pro-rated by the producer's obligated
    days. The "Partial Obligation Calculation" section carries the same
    annual figures before that pro-rating, so for any producer listed there
    the calc-only totals/RAM breakdown of decision 3b and the SMCW of
    decision 3 are taken from it instead - keeping every row of the output
    as reported, the same as the H1/H2 rows. That section lists individual
    producers/subsidiaries with no group subtotals, so for a group with
    partially-obligated members the Level 1 figures are rebuilt as the
    Level 1 Calculation Result row with each such member's pro-rated
    figures swapped for its Partial Obligation figures. Scaling-up of part-period
    submissions is NOT undone (it doesn't apply to 2026 runs), so for a run
    with scaling the calc-only figures of a scaled-up producer are still
    post-scaling.

Sections are located by marker text and header text (not hardcoded line
numbers), so this should keep working against other Results File runs of
the same export format.
"""
import csv
import io
import sys
from pathlib import Path

MATERIALS = [
    ("Aluminium", "Aluminium"),
    ("Fibre Composite", "Fibre Composite"),
    ("Glass", "Glass"),
    ("Paper or Card", "Paper / Card"),
    ("Plastic", "Plastic"),
    ("Steel", "Steel"),
    ("Wood", "Wood"),
    ("Other Materials", "Other"),
]

RAG_MAP = [
    ("Red Material Tonnage", "RAM R"),
    ("Green Material Tonnage", "RAM G"),
    ("Amber Material Tonnage", "RAM A"),
]
RAG_MEDICAL_MAP = [
    ("Red Medical Material Tonnage", "RAM-M R-M"),
    ("Green Medical Material Tonnage", "RAM-M G-M"),
    ("Amber Medical Material Tonnage", "RAM-M A-M"),
]

# Fixed placeholder values for columns with no signal anywhere in the
# Results File - see design decision 10. The exact value doesn't matter,
# only that the column is non-blank, so these are single unvarying
# constants rather than fabricated per-row data.
DEFAULT_CH_NUMBER = "00000000"
DEFAULT_NATION = "England"
DEFAULT_DATETIME = "1900-01-01 00:00:00.000"
DEFAULT_CS_OR_DIRECT = "DP"
DEFAULT_SUBMISSION_STATUS = "GRANTED"
DEFAULT_PACKAGING_SUBMISSION_STATUS = "ACCEPTED"
DEFAULT_SINGLE_FILE_SUBMISSION = "N"
DEFAULT_FILENAME = "N/A"

# Default file locations, used when the corresponding command-line argument
# (<results.csv> <out.csv> <header.csv>) is omitted. Relative paths are resolved against this script's directory.
DEFAULT_RESULTS_FILE = "78-R180smoketest_Results File_20260720.csv"
DEFAULT_OUTPUT_FILE = "v_extract_recent_pom_org_data_from_results.csv"
DEFAULT_HEADER_FILE = "v_extract_recent_pom_org_data_header.csv"


def load_rows(path):
    """
    Reads the Results file. Exported as UTF-8 with a byte-order mark since ECV-697;
    utf-8-sig tolerates that BOM if present and decodes fine without one.
    """
    with open(path, "rb") as f:
        raw = f.read()
    text = raw.decode("utf-8-sig")

    return text.splitlines(keepends=True)


def parse_csv_line(line):
    return next(csv.reader(io.StringIO(line)))


def find_marker(lines, marker_text, start=0):
    for i in range(start, len(lines)):
        row = parse_csv_line(lines[i])
        if row and row[0].strip() == marker_text:
            return i
    raise ValueError(f"Marker not found: {marker_text!r}")


def find_header_row(lines, after, first_cell="Producer ID"):
    for i in range(after, len(lines)):
        row = parse_csv_line(lines[i])
        if row and row[0].strip() == first_cell:
            return i
    raise ValueError(f"Header row (first cell {first_cell!r}) not found after line {after}")


def forward_fill_group(group_row, width):
    """
    The exporter writes a section's material-group row only as far as the
    cell naming its LAST group, so that row is shorter than the column-header
    row beneath it. Pad out to the header row's width so the last group's
    trailing columns (e.g. Other Materials' Public Bin columns) still get
    labelled with it.
    """
    filled = []
    last = ""
    for cell in list(group_row) + [""] * (width - len(group_row)):
        if cell.strip():
            last = cell.strip()
        filled.append(last)
    return filled


def build_section(lines, marker_text, id_cols=("Producer ID", "Subsidiary ID")):
    """
    Returns (rows, group, header) where rows is a list of raw row lists,
    group/header are the forward-filled group names and column names
    (same length, index-aligned to each row).
    """
    marker = find_marker(lines, marker_text)
    header_idx = find_header_row(lines, marker + 1)
    header = parse_csv_line(lines[header_idx])
    group = forward_fill_group(parse_csv_line(lines[header_idx - 1]), len(header))

    rows = []
    i = header_idx + 1
    while i < len(lines):
        row = parse_csv_line(lines[i])
        if not row or not row[0].strip():
            break
        rows.append(row)
        i += 1
    return rows, group, header


def col_index(group, header, group_name, col_name):
    for idx, (g, c) in enumerate(zip(group, header)):
        if g == group_name and c == col_name:
            return idx
    return None


def build_lookup(rows, group, header, key_cols=("Producer ID", "Subsidiary ID")):
    """Index rows by (Producer ID, Subsidiary ID) for O(1) access."""
    pid_idx = col_index(group, header, "", "Producer ID")
    sid_idx = col_index(group, header, "", "Subsidiary ID")
    lookup = {}
    for row in rows:
        pid = row[pid_idx].strip() if pid_idx is not None and pid_idx < len(row) else ""
        sid = row[sid_idx].strip() if sid_idx is not None and sid_idx < len(row) else ""
        lookup[(pid, sid)] = row
    return lookup


def num(row, idx):
    if idx is None or idx >= len(row):
        return None
    v = row[idx].strip()
    if v in ("", "-"):
        return None
    try:
        return float(v)
    except ValueError:
        return None


def fmt(value, decimals=3):
    if value is None:
        return ""
    return f"{value:.{decimals}f}"


def period_display(period_code, half):
    # period_code looks like '2025-H1' / '2025-H2'
    year = period_code.split("-")[0]
    if half == "H1":
        return f"Jan to June {year} - H1"
    return f"July to Dec {year} - H2"


def reporting_year(period_code):
    year = int(period_code.split("-")[0])
    return str(year + 1)


class MaterialCols:
    """Resolves column indices for one material within a raw H1/H2 section."""

    def __init__(self, group, header, src_material):
        self.group = group
        self.header = header
        # Sections disagree on material name casing (e.g. Calculation
        # Result's "Paper or card" vs Partial Obligation's "Paper or Card"),
        # so resolve to whichever spelling this section actually uses.
        wanted = f"{src_material} Breakdown"
        self.src_material = next((g for g in group if g.lower() == wanted.lower()), wanted)

    def idx(self, col_name):
        return col_index(self.group, self.header, self.src_material, col_name)

    def household_total(self):
        return self.idx("Household Tonnage")

    def public_bin_total(self):
        return self.idx("Public Bin Tonnage")

    def household_rag(self, suffix):
        return self.idx(f"Household {suffix}")

    def public_bin_rag(self, suffix):
        return self.idx(f"Public Bin {suffix}")

    def drinks_total(self):
        # Calculation Result names this column "... Tonnage - Glass"; the
        # H1/H2 sections name it without the suffix.
        idx = self.idx("Household Drinks Containers Tonnage")
        return idx if idx is not None else self.idx("Household Drinks Containers Tonnage - Glass")

    def drinks_rag(self, suffix):
        return self.idx(f"Household Drinks Containers {suffix}")


def populate_material_columns(row_out, row_src, mcols, target_name, materials_with_drinks=("Glass",)):
    hh_total = num(row_src, mcols.household_total())
    row_out[f"Total Household packaging-{target_name}"] = fmt(hh_total)

    rag_vals = {}
    for src_suffix, _ in RAG_MAP:
        rag_vals[src_suffix] = num(row_src, mcols.household_rag(src_suffix))
    medical_vals = {}
    for src_suffix, _ in RAG_MEDICAL_MAP:
        medical_vals[src_suffix] = num(row_src, mcols.household_rag(src_suffix))

    ram_total = sum(v for v in rag_vals.values() if v is not None) if any(v is not None for v in rag_vals.values()) else None
    ram_m_total = sum(v for v in medical_vals.values() if v is not None) if any(v is not None for v in medical_vals.values()) else None

    row_out[f"Total Household packaging-{target_name} RAM"] = fmt(ram_total)
    row_out[f"Total Household packaging-{target_name} RAM-M"] = fmt(ram_m_total)
    for src_suffix, target_suffix in RAG_MAP:
        row_out[f"Total Household packaging-{target_name} {target_suffix}"] = fmt(rag_vals[src_suffix])
    for src_suffix, target_suffix in RAG_MEDICAL_MAP:
        row_out[f"Total Household packaging-{target_name} {target_suffix}"] = fmt(medical_vals[src_suffix])

    pb_total = num(row_src, mcols.public_bin_total())
    row_out[f"Public binned-{target_name}"] = fmt(pb_total)

    pb_rag_vals = {}
    for src_suffix, _ in RAG_MAP:
        pb_rag_vals[src_suffix] = num(row_src, mcols.public_bin_rag(src_suffix))
    pb_medical_vals = {}
    for src_suffix, _ in RAG_MEDICAL_MAP:
        pb_medical_vals[src_suffix] = num(row_src, mcols.public_bin_rag(src_suffix))

    pb_ram_total = sum(v for v in pb_rag_vals.values() if v is not None) if any(v is not None for v in pb_rag_vals.values()) else None
    pb_ram_m_total = sum(v for v in pb_medical_vals.values() if v is not None) if any(v is not None for v in pb_medical_vals.values()) else None

    row_out[f"Public binned-{target_name} RAM"] = fmt(pb_ram_total)
    row_out[f"Public binned-{target_name} RAM-M"] = fmt(pb_ram_m_total)
    for src_suffix, target_suffix in RAG_MAP:
        row_out[f"Public binned-{target_name} {target_suffix}"] = fmt(pb_rag_vals[src_suffix])
    for src_suffix, target_suffix in RAG_MEDICAL_MAP:
        row_out[f"Public binned-{target_name} {target_suffix}"] = fmt(pb_medical_vals[src_suffix])

    # Household drinks containers: only Glass carries this data in the raw
    # H1/H2 sections. For every other material the target schema has a
    # column for it but the source has nothing - left blank, not 0.
    if target_name in materials_with_drinks:
        drinks_total = num(row_src, mcols.drinks_total())
        row_out[f"Household drinks containers-{target_name} (Kg)"] = fmt(drinks_total)

        drinks_rag = {s: num(row_src, mcols.drinks_rag(s)) for s, _ in RAG_MAP}
        drinks_medical = {s: num(row_src, mcols.drinks_rag(s)) for s, _ in RAG_MEDICAL_MAP}
        drinks_ram = sum(v for v in drinks_rag.values() if v is not None) if any(v is not None for v in drinks_rag.values()) else None
        drinks_ram_m = sum(v for v in drinks_medical.values() if v is not None) if any(v is not None for v in drinks_medical.values()) else None

        row_out[f"Household drinks containers-{target_name} RAM (Kg)"] = fmt(drinks_ram)
        row_out[f"Household drinks containers-{target_name} RAM-M (Kg)"] = fmt(drinks_ram_m)
        for src_suffix, target_suffix in RAG_MAP:
            row_out[f"Household drinks containers-{target_name} {target_suffix} (Kg)"] = fmt(drinks_rag[src_suffix])
        for src_suffix, target_suffix in RAG_MEDICAL_MAP:
            row_out[f"Household drinks containers-{target_name} {target_suffix} (Kg)"] = fmt(drinks_medical[src_suffix])
    # "(No.Units)" columns for every material, and non-Glass Kg/RAM columns,
    # are never populated: the source file has no unit-count data at all,
    # and no drinks-container weight for materials other than Glass.


def cell(row, idx):
    return row[idx].strip() if idx is not None and idx < len(row) else ""


def split_levels(rows, group, header):
    """
    Splits a section's rows into (level_1_rows, level_2_rows) - design
    decision 11. A section with no Level column is treated as all Level 1.
    """
    level_idx = col_index(group, header, "", "Level")
    if level_idx is None:
        return rows, []
    l1 = [row for row in rows if cell(row, level_idx) == "1"]
    l2 = [row for row in rows if cell(row, level_idx) == "2"]
    return l1, l2


# Every annual figure read from a Calculation Result / Partial Obligation row,
# as MaterialCols accessors - the columns adjusted when rebuilding a group's
# pre-pro-rating total (design decision 12).
ANNUAL_ACCESSORS = (
    [MaterialCols.household_total, MaterialCols.public_bin_total, MaterialCols.drinks_total,
     lambda m: m.idx("Self-managed Consumer Waste Tonnage")]
    + [
        (lambda s, f: (lambda m: f(m, s)))(s, f)
        for s, _ in RAG_MAP + RAG_MEDICAL_MAP
        for f in (MaterialCols.household_rag, MaterialCols.public_bin_rag, MaterialCols.drinks_rag)
    ]
)


def group_annual_row(l1_row, calc_group, calc_header, members, partial_group, partial_header):
    """
    A group's Level 1 Calculation Result row with each partially-obligated
    member's pro-rated figures swapped for its pre-pro-rating Partial
    Obligation figures, i.e.
        L1 + sum over partial members of (partial row - Calculation Result row)
    members is a list of (partial_row, calc_member_row). Returned in the
    Calculation Result layout.
    """
    out = list(l1_row)
    for src, _ in MATERIALS:
        cm = MaterialCols(calc_group, calc_header, src)
        pm = MaterialCols(partial_group, partial_header, src)
        for accessor in ANNUAL_ACCESSORS:
            ci, pi = accessor(cm), accessor(pm)
            if ci is None or pi is None:
                continue
            values = [num(l1_row, ci)]
            for partial_row, calc_row in members:
                p, c = num(partial_row, pi), num(calc_row, ci)
                values.append(p)
                values.append(-c if c is not None else None)
            if all(v is None for v in values):
                continue
            if ci >= len(out):
                out.extend([""] * (ci + 1 - len(out)))
            out[ci] = repr(sum(v for v in values if v is not None))
    return out


def main():
    base = Path(__file__).resolve().parent
    results_file = Path(sys.argv[1]) if len(sys.argv) > 1 else base / DEFAULT_RESULTS_FILE
    output_file = Path(sys.argv[2]) if len(sys.argv) > 2 else base / DEFAULT_OUTPUT_FILE
    header_file = Path(sys.argv[3]) if len(sys.argv) > 3 else base / DEFAULT_HEADER_FILE

    # The exact v_extract_recent_pom_org_data.csv column list/order, copied from the
    # outer SELECT of dbo.Views.v_extract_recent_pom_org_data.sql in epr-data-sqldb --
    # keep this file in sync if that view's columns change.
    with open(header_file, newline="", encoding="utf-8") as f:
        target_header = next(csv.reader(f))

    lines = load_rows(results_file)

    h2_rows, h2_group, h2_header = build_section(lines, "H2 Packaging Data - Submitted and Projected")
    h1_rows, h1_group, h1_header = build_section(lines, "H1 Packaging Data - Submitted and Projected")
    calc_rows, calc_group, calc_header = build_section(lines, "Calculation Result")

    # Design decision 11: only Level 1 rows are output.
    h2_rows, _ = split_levels(h2_rows, h2_group, h2_header)
    h1_rows, _ = split_levels(h1_rows, h1_group, h1_header)
    calc_rows, calc_member_rows = split_levels(calc_rows, calc_group, calc_header)

    # Design decision 12. Older Results Files have no such section.
    try:
        partial_rows, partial_group, partial_header = build_section(lines, "Partial Obligation Calculation")
    except ValueError:
        partial_rows, partial_group, partial_header = [], [], []

    calc_lookup = build_lookup(calc_rows, calc_group, calc_header)
    calc_member_lookup = build_lookup(calc_member_rows, calc_group, calc_header)
    name_idx = col_index(calc_group, calc_header, "", "Producer / Subsidiary Name")

    # The Partial Obligation section has one row per partially-obligated
    # producer/subsidiary and no group subtotals, so collect each group's
    # partial members (paired with their Calculation Result rows) for
    # group_annual_row. Rows for producers absent from Calculation Result
    # (e.g. the section's "None" placeholder) are ignored.
    grouped_pids = {pid for pid, _ in calc_member_lookup}
    p_pid_idx = col_index(partial_group, partial_header, "", "Producer ID")
    p_sid_idx = col_index(partial_group, partial_header, "", "Subsidiary ID")
    partial_standalone = {}
    partial_members = {}
    for prow in partial_rows:
        pid, sid = cell(prow, p_pid_idx), cell(prow, p_sid_idx)
        if (pid, "") not in calc_lookup:
            continue
        if pid in grouped_pids:
            partial_members.setdefault(pid, []).append((prow, calc_member_lookup.get((pid, sid), [])))
        else:
            partial_standalone[pid] = prow

    def annual_source(pid):
        """
        The annual (H1+H2) row to take calc-only tonnages and SMCW from, with
        its group/header, before partial-obligation pro-rating: a standalone
        producer's Partial Obligation row, a group's Level 1 row rebuilt
        from its members' Partial Obligation rows, else the Level 1
        Calculation Result row as-is.
        """
        if pid in partial_standalone:
            return partial_standalone[pid], partial_group, partial_header
        calc_row = calc_lookup.get((pid, ""))
        if pid in partial_members and calc_row is not None:
            return (
                group_annual_row(calc_row, calc_group, calc_header, partial_members[pid], partial_group, partial_header),
                calc_group,
                calc_header,
            )
        return calc_row, calc_group, calc_header

    def blank_row():
        return {h: "" for h in target_header}

    def base_row(pid, period_code, half):
        row = blank_row()
        row["Org_ID"] = pid
        calc_row = calc_lookup.get((pid, ""))
        if calc_row is not None and name_idx is not None:
            row["Org_name"] = calc_row[name_idx].strip()

        row["Packaging_data_submission_period"] = period_display(period_code, half)
        row["Packaging_data_first_submission_period_code"] = period_code
        row["Packaging_data_latest_submission_period_code"] = period_code
        row["Packaging_data_first_submission_organisation_size"] = "L"
        row["Packaging_data_latest_submission_organisation_size"] = "L"
        row["Organisation_data_submission_period"] = period_display(period_code, half)
        row["Enrolment_status"] = "Approved"
        row["Organisation_soft_deleted"] = "0"
        row["Reporting_Year"] = reporting_year(period_code)

        row["CH_number"] = DEFAULT_CH_NUMBER
        row["Nation_of_enrolment"] = DEFAULT_NATION
        row["Enrolment_date_time"] = DEFAULT_DATETIME
        row["Nation_of_Compliance_Scheme_regulator"] = DEFAULT_NATION

        row["Organisation_data_first_submission_datetime"] = DEFAULT_DATETIME
        row["Organisation_data_first_submitted_CS_or_Direct"] = DEFAULT_CS_OR_DIRECT
        row["Organisation_data_first_submitted_CS_Nation"] = DEFAULT_NATION
        row["Organisation_data_first_submission_status"] = DEFAULT_SUBMISSION_STATUS
        row["Organisation_data_first_submission_organisation_size"] = "L"
        row["Organisation_data_latest_submission_datetime"] = DEFAULT_DATETIME
        row["Organisation_data_latest_submitted_CS_or_Direct"] = DEFAULT_CS_OR_DIRECT
        row["Organisation_data_latest_submitted_CS_Nation"] = DEFAULT_NATION
        row["Organisation_data_latest_submission_status"] = DEFAULT_SUBMISSION_STATUS
        row["Organisation_data_latest_submission_organisation_size"] = "L"

        row["Single_File_Submission_Packaging"] = DEFAULT_SINGLE_FILE_SUBMISSION
        row["Single_File_Submission_Orgdata"] = DEFAULT_SINGLE_FILE_SUBMISSION
        row["fps_pm_filename"] = DEFAULT_FILENAME
        row["lps_pm_filename"] = DEFAULT_FILENAME
        row["fos_cd_filename"] = DEFAULT_FILENAME
        row["los_cd_filename"] = DEFAULT_FILENAME

        row["Packaging_data_first_submission_status"] = DEFAULT_PACKAGING_SUBMISSION_STATUS
        row["Packaging_data_latest_submission_status"] = DEFAULT_PACKAGING_SUBMISSION_STATUS
        row["Packaging_data_first_submission_datetime"] = DEFAULT_DATETIME
        row["Packaging_data_latest_submission_datetime"] = DEFAULT_DATETIME
        row["Packaging_data_latest_submitted_CS_or_Direct"] = DEFAULT_CS_OR_DIRECT

        if half == "H2":
            src_row, src_group, src_header = annual_source(pid)
            for src, disp in MATERIALS:
                idx = MaterialCols(src_group, src_header, src).idx("Self-managed Consumer Waste Tonnage")
                if src_row is not None and idx is not None:
                    row[f"Self-managed consumer waste-{disp}"] = fmt(num(src_row, idx))
        return row

    out_rows = []

    period_col_idx_h2 = col_index(h2_group, h2_header, "", "Submission period code")
    pid_idx_h2 = col_index(h2_group, h2_header, "", "Producer ID")
    h2_keys = set()
    for r in h2_rows:
        pid = r[pid_idx_h2].strip()
        period_code = r[period_col_idx_h2].strip()
        h2_keys.add(pid)
        row_out = base_row(pid, period_code, "H2")
        for src_material, disp in MATERIALS:
            mcols = MaterialCols(h2_group, h2_header, src_material)
            populate_material_columns(row_out, r, mcols, disp)
        out_rows.append(row_out)

    period_col_idx_h1 = col_index(h1_group, h1_header, "", "Submission period code")
    pid_idx_h1 = col_index(h1_group, h1_header, "", "Producer ID")
    h1_keys = set()
    for r in h1_rows:
        pid = r[pid_idx_h1].strip()
        period_code = r[period_col_idx_h1].strip()
        h1_keys.add(pid)
        row_out = base_row(pid, period_code, "H1")
        for src_material, disp in MATERIALS:
            mcols = MaterialCols(h1_group, h1_header, src_material)
            populate_material_columns(row_out, r, mcols, disp)
        out_rows.append(row_out)

    # Both sections cover a single H1/H2 pair per run - confirmed uniform
    # across every row in this Results File.
    period_code_h2 = h2_rows[0][period_col_idx_h2].strip()
    period_code_h1 = h1_rows[0][period_col_idx_h1].strip()

    # Design decision 3b: producers present in Calculation Result but never
    # audited in either H1 or H2 section - i.e. every material in both
    # periods was fully RAG-rated, so no "blank" needed inferring. Emit the
    # same two rows as everyone else, but sourced from the annual totals and
    # RAM breakdown (100% H2 / 0% H1) - see docstring.
    audited_keys = h1_keys | h2_keys
    calc_only_pids = sorted(pid for pid, _ in calc_lookup if pid not in audited_keys)
    for pid in calc_only_pids:
        src_row, src_group, src_header = annual_source(pid)

        h2_row_out = base_row(pid, period_code_h2, "H2")
        for src_material, disp in MATERIALS:
            mcols = MaterialCols(src_group, src_header, src_material)
            populate_material_columns(h2_row_out, src_row, mcols, disp)
        out_rows.append(h2_row_out)

        h1_row_out = base_row(pid, period_code_h1, "H1")
        out_rows.append(h1_row_out)

    with open(output_file, "w", newline="", encoding="utf-8") as f:
        writer = csv.DictWriter(f, fieldnames=target_header)
        writer.writeheader()
        writer.writerows(out_rows)

    print(f"Wrote {len(out_rows)} rows to {output_file}")


if __name__ == "__main__":
    sys.exit(main())
