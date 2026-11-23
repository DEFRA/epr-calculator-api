Usage:

`bash
cd tools
nix develop                                      # drops into a shell with R + gmp
Rscript build_org_extract_from_results_file.R <results.csv> <out.csv>
Rscript verify_results_csv.R <results.csv> [--verbose]
`

# or without entering a shell:
`bash
nix run .#build-org-extract -- <results.csv> <out.csv>   # must be run from tools/
nix run .#verify-results-csv -- <results.csv> [--verbose]
`

When modifying these scripts (especially the producer-table column schema in
`build_producer_table_schema`), run both the Python and R verify scripts
against a results fixture with a full producer table and modulation section,
e.g.:

```bash
python3 verify_results_csv.py ../src/EPR.Calculator.API.IntegrationTests/ExpectedData/2026-results.csv
nix run .#verify-results-csv -- ../src/EPR.Calculator.API.IntegrationTests/ExpectedData/2026-results.csv
```

Both should report "No discrepancies found." A schema/offset mismatch won't
always raise an exception -- it can silently misalign columns instead, so a
clean run on both is the check, not just "it didn't crash."

Notable things found along the way (worth knowing about, not just implementation trivia):

- A real correctness bug in my own translation, not the Python original: parsing the whole file into one padded matrix (for speed) silently let a short physical line's forward-filled section label bleed into columns that never existed on that line. Fixed by tracking each row's true length and marking padding beyond it as unmatchable.
- gmp::as.bigz("0998644") reads a leading "0" as a C-style octal prefix, silently corrupting any parsed value with a leading zero (very common for anything under 1.0, e.g. "0.998644"). Worked around by routing through as.numeric() first.
- Performance: build_org_extract runs in ~76s (vs Python's ~8s); verify_results_csv runs in ~5min on the 3400-producer file (vs Python's ~6s). This is the real cost of exact rational arithmetic through gmp's S4 dispatch in an interpreted loop — I cut the worst of it (caching powers of ten instead of recomputing them per check), but it's inherently much slower than Python's Decimal. Both are one-off verification tools, so this seemed like an acceptable tradeoff given exactness was the explicit priority, but flagging it in case that changes the calculus for you.

## align_billing.py

Changes an original results or billing CSV to today's layout, so it can be compared with an export of the same run. It never changes a value other than a zero's format or a negative amount's sign, never adds, drops or reorders a row, and never takes anything from the export. It does change headings, columns, quoting and number formats, as listed below. It was written to check runs imported with `POST /v1/runs/import` against the files they came from.

```bash
python3 -I align_billing.py <original.csv>
python3 -I align_billing.py <runs.zip>
code --diff "compare/<original>.original-aligned.csv" "path/to/downloaded/csv"
```

It writes `compare/<original>.original-aligned.csv`, in a `compare` folder next to the original, and prints the command to compare it with the downloaded export. A file already in today's layout, such as run 102's, is copied there unchanged, under its own name, so it's clear nothing was changed. Given the zip the import takes, it does that for each results and billing CSV in it, in run order, and skips the optional CSVs.

Changed to today's layout:

- Headings and labels reworded to today's wording (`REWORDED`, and `REWORDED_HEADINGS` for wording changed in one table only), and the small tables' `Total` headings as `Total UK`.
- Columns today's layout adds (`ADDED_COLUMNS`). The Summary columns (ECV-768) are copied from the columns they repeat, Partial Calculation? is `No`, and the rest, such as Registration Status Code and late reporting's red, amber and green split, are left empty.
- Columns today's layout doesn't have are dropped (`REMOVED_COLUMNS`): Subsidiary ID from Cancelled Producers, and Net Tonnage from the partial obligation table.
- Late reporting's total moved after the red, amber and green split, where today's layout has it (`MOVED_TO_END`).

Changed to today's formatting:

- Section titles quoted, except the Error Table's, and every line but a producer's ending with a comma. Titles and a producer table's heading rows end with one after their last value; the fee table's heading rows are as wide as the table instead. Empty cells outside the producer rows unquoted, and empty producer values quoted in the columns today's file always quotes them (`QUOTED_WHEN_EMPTY`).
- The fee table's Totals row: in a billing file, with its label just before the first group of columns; in a results file, with its label in the Leaver's Date column and its empty cells before the registration status code quoted.
- Zeros printed like the other values in their column. In a column of zeros, as `ZERO_FORMATS_BY_HEADING` or `ZERO_FORMATS` say: `£0` as `£0.00`, `0.00` as `0.000`, `0%` as `0.00000000%`.
- Negative amounts: `£-12.34` as `-£12.34`.
- A BOM at the start.

Not changed:

- Every other value, as printed. Numbers aren't reformatted, so `3581.4320` against `3581.432` shows in the diff.
- Lines and sections today's layout adds, such as the Cut-off Date line, the Partial Obligation Calculation section or the Rejected Report, aren't added, so they show in the diff as only in the export.

The previous version, which built the aligned file from the export's layout instead, is in a stash: `git stash list` shows it as "align_billing.py: version that builds the aligned file from the downloaded export's layout".

Billing CSVs from production hold producer data: keep them, and the files this writes, outside the repo.

## align_billing_json.py

Changes a copy of an imported run's billing JSON export to production's formatting, so its fingerprints can be compared with those taken from production's billing JSON. It works the opposite way to `align_billing.py`: production's JSON can't be copied off the machine it's on, so it changes ours to look like production's, rather than production's to look like ours. It changes how values are written and the order of producers, never a value.

```bash
python3 -I align_billing_json.py <folder with our {run id}billing.json files> <folder with production's billing CSVs>
```

It writes `aligned/{run id}billing.json` in the first folder. Production's billing CSVs are only read for run 102's order. The rules are for the five production runs imported with `POST /v1/runs/import`, and are listed at the top of the script:

- a zero tonnage change written as `0` where the previous invoiced tonnage is zero, as production wrote it, rather than the stored `0.000`
- for the year's first run (10): zero previous invoiced tonnages as `0`, production's placeholder cancelled producer instead of an empty list, and late reporting in the usual material order
- for the 2025 layout (10, 51, 68): a zero share of all producers' tonnage as `0%`, no `cutOffDate`, and the SA set-up cost totals under their old misspelt names with their values swapped
- for run 84: `modulationResults` written as null
- for run 102: producers in the order of production's billing CSV fee table

It leaves every value as it is, including the 1p on two producers' "without bad debt" disposal fees in runs 10 and 51.

## billing_json_fingerprints

Scripts that print short hashes of a billing JSON file, and never names or figures, so production's JSON can be compared with ours without copying it off the machine it's on. Run each on production's files there and on ours, then compare the outputs here:

- `allfp.py`: a hash of each section, each results section and each producer field, and counts of names with commas, spaces at either end, or nothing
- `deep.py`: a hash of every value in the summary, the cancelled producers, the other parameters and four producer fee sections, by path
- `avd.py`: value shapes (each digit shown as 9) and counts for the tonnage fields, a hash per 10 cancelled producers, and hashes of the names with commas or spaces at either end, by producer ID
- `blocks.py`: a hash of the "without bad debt" disposal fees per block of producers, to find a producer whose fees differ
- `compare.py`: compares two outputs of `allfp.py` or `deep.py` and prints only what differs; `avd.py`'s outputs compare with `diff`

The outputs taken from production are hashes of producer data: keep them outside the repo.
