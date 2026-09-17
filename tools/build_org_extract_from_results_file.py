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
1. Two output rows per (producer, subsidiary): one for the "H1 Packaging
   Data - Submitted & Projected" section, one for "H2 Packaging Data -
   Submitted & Projected". This is not "splitting" a combined figure - the
   Results File already keeps H1 and H2 as separate per-period tables; we
   just emit each as its own synthetic org-data row, tagged with that
   period's code.

2. RAG tonnage columns are taken AS REPORTED, from the plain (non-Projected,
   non-defaulted) columns of each period's own section:
     Total Household packaging-X          <- "Household Packaging Tonnage"
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

3b. The H1/H2 "Packaging Data - Submitted & Projected" sections are NOT a
    full per-producer dump - they only exist as an audit trail for
    producers who had at least one blank/unrated material in either
    period. A producer whose H1 and H2 submissions were both fully RAG-
    rated for every material never appears in either section at all (of
    ~4,300 producers in Calculation Result, only ~1,950 show up in H1/H2 -
    confirmed by set-difference against the Results File used to build
    this script). For that "calc-only" majority we have no per-period, no
    per-RAG-category data whatsoever - only the annual Calculation Result
    total. Per the same "SMCW" principle as (3): the household/public-bin
    TOTAL tonnage (Total Household packaging-X / Public binned-X /
    Household drinks containers-Glass) is taken from Calculation Result
    and attributed 100% to the H2 row, 0 to H1. The RAM/RAM-M/R/G/A
    breakdown columns are left BLANK (not derived) for these producers -
    Calculation Result's own Red/Amber/Green figures already have any
    unrated tonnage defaulted into Red (same "output of blank processing"
    problem as decision 2), so reusing them here would silently violate
    the "report the input, not the defaulted output" rule. We simply don't
    have the input for these producers.

4. Org_ID = Subsidiary ID when present, else Producer ID (each subsidiary is
   its own org-data entity). Org_name is looked up from the "Calculation
   Result" section's "Producer / Subsidiary Name" column for that exact
   (Producer ID, Subsidiary ID) pair.

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
   the "1 Country Apportionment %s" table at the top of the file (verified:
   e.g. two different producers' Aluminium England/Wales fee figures are
   both in exactly the 17.24%/27.59% ratio set globally for Aluminium,
   regardless of where either producer is actually registered) - i.e. it's
   a cost-recovery allocation, not a signal of which nation the producer
   itself is enrolled in. There is no registered-nation field anywhere in
   this export. See decision 10.

10. Organisation registration/submission-metadata fields that have no
    signal anywhere in the Results File - CH_number, Nation_of_enrolment,
    Enrolment_date_time, Nation_of_Compliance_Scheme_regulator, the
    Organisation_data_* first/latest submission metadata columns,
    Single_File_Submission_* flags, and the fps/lps/fos/los filename
    columns - are hard-coded to a single fixed default value per column
    (see the DEFAULT_* constants), not derived or fabricated per-row. Per
    the user: downstream tooling just needs these columns non-blank: the
    actual value doesn't matter, so a single unvarying constant is used
    rather than inventing plausible-looking per-row data.

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
    ("Fibre composite", "Fibre Composite"),
    ("Glass", "Glass"),
    ("Paper or card", "Paper / Card"),
    ("Plastic", "Plastic"),
    ("Steel", "Steel"),
    ("Wood", "Wood"),
    ("Other materials", "Other"),
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
DEFAULT_SINGLE_FILE_SUBMISSION = "N"
DEFAULT_FILENAME = "N/A"


def read_utf16_rows(path):
    with open(path, encoding="utf-16le") as f:
        return f.readlines()


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


def forward_fill_group(group_row):
    filled = []
    last = ""
    for cell in group_row:
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
    group = forward_fill_group(parse_csv_line(lines[header_idx - 1]))
    header = parse_csv_line(lines[header_idx])

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
        self.src_material = f"{src_material} Breakdown"

    def idx(self, col_name):
        return col_index(self.group, self.header, self.src_material, col_name)

    def household_total(self):
        return self.idx("Household Packaging Tonnage")

    def public_bin_total(self):
        return self.idx("Public Bin Packaging Tonnage")

    def household_rag(self, suffix):
        return self.idx(f"Household {suffix}")

    def public_bin_rag(self, suffix):
        return self.idx(f"Public Bin {suffix}")

    def drinks_total(self):
        return self.idx("Household Drinks Containers Tonnage")

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


def populate_totals_only_from_calc(row_out, calc_row, calc_group, calc_header, target_name, src_material):
    """
    For producers with no H1/H2 audit row at all (see design decision 3b):
    populate just the material's total tonnage from the annual Calculation
    Result row, same treatment as Self-managed consumer waste. No RAM/RAG
    breakdown is populated - see module docstring for why.
    """
    group_name = f"{src_material} Breakdown"
    hh_idx = col_index(calc_group, calc_header, group_name, "Household Packaging Tonnage")
    pb_idx = col_index(calc_group, calc_header, group_name, "Public Bin Tonnage")

    row_out[f"Total Household packaging-{target_name}"] = fmt(num(calc_row, hh_idx))
    row_out[f"Public binned-{target_name}"] = fmt(num(calc_row, pb_idx))

    if target_name == "Glass":
        drinks_idx = col_index(calc_group, calc_header, group_name, "Household Drinks Containers Tonnage - Glass")
        row_out["Household drinks containers-Glass (Kg)"] = fmt(num(calc_row, drinks_idx))


def main():
    base = Path(__file__).resolve().parent
    results_file = Path(sys.argv[1]) if len(sys.argv) > 1 else base / "78-R180smoketest_Results File_20260720.csv"
    output_file = Path(sys.argv[2]) if len(sys.argv) > 2 else base / "v_extract_recent_pom_org_data_from_results.csv"
    target_header_ref = base.parent / "v_extract_recent_pom_org_data.csv"

    with open(target_header_ref, newline="", encoding="utf-8", errors="replace") as f:
        target_header = next(csv.reader(f))

    lines = read_utf16_rows(results_file)

    h2_rows, h2_group, h2_header = build_section(lines, "H2 Packaging Data - Submitted & Projected")
    h1_rows, h1_group, h1_header = build_section(lines, "H1 Packaging Data - Submitted & Projected")
    calc_rows, calc_group, calc_header = build_section(lines, "Calculation Result")

    calc_lookup = build_lookup(calc_rows, calc_group, calc_header)
    name_idx = col_index(calc_group, calc_header, "", "Producer / Subsidiary Name")
    smcw_idx_by_material = {
        src: col_index(calc_group, calc_header, f"{src} Breakdown", "Self Managed Consumer Waste Tonnage")
        for src, _ in MATERIALS
    }

    def blank_row():
        return {h: "" for h in target_header}

    def base_row(pid, sid, period_code, half):
        row = blank_row()
        row["Org_ID"] = sid if sid else pid
        calc_row = calc_lookup.get((pid, sid))
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

        if half == "H2":
            for src, _ in MATERIALS:
                idx = smcw_idx_by_material[src]
                if calc_row is not None and idx is not None:
                    disp = dict(MATERIALS)[src]
                    row[f"Self-managed consumer waste-{disp}"] = fmt(num(calc_row, idx))
        return row

    out_rows = []

    period_col_idx_h2 = col_index(h2_group, h2_header, "", "Submission period code")
    pid_idx_h2 = col_index(h2_group, h2_header, "", "Producer ID")
    sid_idx_h2 = col_index(h2_group, h2_header, "", "Subsidiary ID")
    for r in h2_rows:
        pid = r[pid_idx_h2].strip()
        sid = r[sid_idx_h2].strip()
        period_code = r[period_col_idx_h2].strip()
        row_out = base_row(pid, sid, period_code, "H2")
        for src_material, disp in MATERIALS:
            mcols = MaterialCols(h2_group, h2_header, src_material)
            populate_material_columns(row_out, r, mcols, disp)
        out_rows.append(row_out)

    period_col_idx_h1 = col_index(h1_group, h1_header, "", "Submission period code")
    pid_idx_h1 = col_index(h1_group, h1_header, "", "Producer ID")
    sid_idx_h1 = col_index(h1_group, h1_header, "", "Subsidiary ID")
    h1_keys = set()
    for r in h1_rows:
        pid = r[pid_idx_h1].strip()
        sid = r[sid_idx_h1].strip()
        period_code = r[period_col_idx_h1].strip()
        h1_keys.add((pid, sid))
        row_out = base_row(pid, sid, period_code, "H1")
        for src_material, disp in MATERIALS:
            mcols = MaterialCols(h1_group, h1_header, src_material)
            populate_material_columns(row_out, r, mcols, disp)
        out_rows.append(row_out)

    h2_keys = set()
    for r in h2_rows:
        h2_keys.add((r[pid_idx_h2].strip(), r[sid_idx_h2].strip()))

    # Both sections cover a single H1/H2 pair per run - confirmed uniform
    # across every row in this Results File.
    period_code_h2 = h2_rows[0][period_col_idx_h2].strip()
    period_code_h1 = h1_rows[0][period_col_idx_h1].strip()

    # Design decision 3b: producers present in Calculation Result but never
    # audited in either H1 or H2 section - i.e. every material in both
    # periods was fully RAG-rated, so no "blank" needed inferring. Emit the
    # same two rows as everyone else, but sourced from the annual total
    # (100% H2 / 0% H1), with RAM breakdown left blank - see docstring.
    audited_keys = h1_keys | h2_keys
    calc_only_keys = sorted(k for k in calc_lookup if k not in audited_keys)
    for pid, sid in calc_only_keys:
        calc_row = calc_lookup[(pid, sid)]

        h2_row_out = base_row(pid, sid, period_code_h2, "H2")
        for src_material, disp in MATERIALS:
            populate_totals_only_from_calc(h2_row_out, calc_row, calc_group, calc_header, disp, src_material)
        out_rows.append(h2_row_out)

        h1_row_out = base_row(pid, sid, period_code_h1, "H1")
        out_rows.append(h1_row_out)

    with open(output_file, "w", newline="", encoding="utf-8") as f:
        writer = csv.DictWriter(f, fieldnames=target_header)
        writer.writeheader()
        writer.writerows(out_rows)

    print(f"Wrote {len(out_rows)} rows to {output_file}")


if __name__ == "__main__":
    sys.exit(main())
