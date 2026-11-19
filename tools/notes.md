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

Changes an original results or billing CSV to today's layout, with a light touch, so it can be compared with an export of the same run. It was written to check runs imported with `POST /v1/runs/import` against the files they came from.

```bash
python3 -I align_billing.py <original.csv>
python3 -I align_billing.py <runs.zip>
code --diff "compare/<original>.original-aligned.csv" "path/to/downloaded/csv"
```

It writes `compare/<original>.original-aligned.csv`, in a `compare` folder next to the original, and prints the command to compare it with the downloaded export. Given the zip the import takes, it does that for each results and billing CSV in it, in run order, and skips the optional CSVs.

Changed to today's layout:

- Headings and labels reworded to today's wording (`REWORDED`, and `REWORDED_HEADINGS` for wording changed in one table only), and the small tables' `Total` headings as `Total UK`.
- Columns today's layout adds (`ADDED_COLUMNS`). The Summary columns (ECV-768) are copied from the columns they repeat, Partial Calculation? is `No`, and the rest, such as Registration Status Code and late reporting's red, amber and green split, are left empty.
- Columns today's layout doesn't have are dropped (`REMOVED_COLUMNS`): Subsidiary ID from Cancelled Producers, and Net Tonnage from the partial obligation table.
- Late reporting's total moved after the red, amber and green split, where today's layout has it (`MOVED_TO_END`).

Changed to today's formatting:

- Section titles quoted, and every line but a producer's ending with a comma. Titles and a producer table's heading rows end with one after their last value; the fee table's heading rows are as wide as the table instead. Empty cells outside the producer rows unquoted, and empty producer values quoted in the columns today's file always quotes them (`QUOTED_WHEN_EMPTY`).
- The fee table's Totals row with its label just before the first group of columns.
- Zeros printed like the other values in their column. In a column of zeros, as `ZERO_FORMATS_BY_HEADING` or `ZERO_FORMATS` say: `£0` as `£0.00`, `0.00` as `0.000`, `0%` as `0.00000000%`.
- Negative amounts: `£-12.34` as `-£12.34`.
- A BOM at the start.

Not changed:

- Every other value, as printed. Numbers aren't reformatted, so `3581.4320` against `3581.432` shows in the diff.
- Lines and sections today's layout adds, such as the Cut-off Date line, the Partial Obligation Calculation section or the Rejected Report, aren't added, so they show in the diff as only in the export.

The previous version, which built the aligned file from the export's layout instead, is in a stash: `git stash list` shows it as "align_billing.py: version that builds the aligned file from the downloaded export's layout".

Billing CSVs from production hold producer data: keep them, and the files this writes, outside the repo.
