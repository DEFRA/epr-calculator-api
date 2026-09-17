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

Notable things found along the way (worth knowing about, not just implementation trivia):

- A real correctness bug in my own translation, not the Python original: parsing the whole file into one padded matrix (for speed) silently let a short physical line's forward-filled section label bleed into columns that never existed on that line. Fixed by tracking each row's true length and marking padding beyond it as unmatchable.
- gmp::as.bigz("0998644") reads a leading "0" as a C-style octal prefix, silently corrupting any parsed value with a leading zero (very common for anything under 1.0, e.g. "0.998644"). Worked around by routing through as.numeric() first.
- Performance: build_org_extract runs in ~76s (vs Python's ~8s); verify_results_csv runs in ~5min on the 3400-producer file (vs Python's ~6s). This is the real cost of exact rational arithmetic through gmp's S4 dispatch in an interpreted loop — I cut the worst of it (caching powers of ten instead of recomputing them per check), but it's inherently much slower than Python's Decimal. Both are one-off verification tools, so this seemed like an acceptable tradeoff given exactness was the explicit priority, but flagging it in case that changes the calculus for you.
