# Bug: "Calculation Result" fee report only reflects one submission period's tonnage for normal (non-scaled, non-partial) producers

## Status: Resolved (2026-09-03) — turned out not to be a code bug at all

**There was no bug in `ProducerFeesUtil`/`ProducerRowBuilder`/`Section1MaterialsExporter` on either `main` or `ECV-730-CF`.** Every step of the fee-calculation and CSV-export pipeline was instrumented directly (`ProducerRowBuilder.GetProducerRow`'s `materialFeeSummary` assignment, `Section1MaterialsExporter.AppendRow`, and the exact CSV-write call in `AppendProducerDisposalFeesByMaterial`) and all three showed the **correct summed tonnage** (e.g. 866.848 for two 433.424 periods) at every point, on `ECV-730-CF`.

The apparent bug — `ExpectedData/2025-results.csv` (and the other five `ExpectedData/*` files) showing an under-summed value as "expected", with the integration test passing anyway — was caused by a **test-harness bug**, not a product bug:

`CalculatorRunIntegrationTests.RunTest`'s `RECORD_EXPECTED=1` regeneration mode wrote to the *relative* path `ExpectedData/{relativeYear}-results.csv`. Under `dotnet test`, the working directory is the build output folder (`bin/Debug/net10.0/`), not the source tree — so every "regeneration" done this way (across this session, and apparently before it too) was writing to `bin/Debug/net10.0/ExpectedData/*`, a copy that the next `dotnet build` silently overwrites from the (stale, unchanged) source `ExpectedData/*` files via the project's `CopyToOutputDirectory` setting. The source-tree `ExpectedData/*.csv`/`*.json` files themselves were never actually updated by any of these "regenerations" — they stayed frozen at whatever they were minted from, well before the `ECV-730-CF` work being done in this session.

A second, compounding factor: this session's `EPR.Calculator.API.IntegrationTests` runs share one long-lived `Testcontainers` SQL Server container (`WithReuse(true)`), reused across many hours of ad-hoc debugging (absurd placeholder weights, repeated seeding, etc.). At least one investigation this session ran against that polluted container and got a false "matches" result — the real fix required tearing the container down (`docker rm -f`) to get a trustworthy read.

### Resolution

1. Fixed `CalculatorRunIntegrationTests.RunTest`'s `RECORD_EXPECTED` mode to write to the actual source `ExpectedData/` path, not the CWD-relative one (kept as ad-hoc local scaffolding during investigation, reverted afterwards — it isn't meant to be a permanent test-code feature).
2. Removed the stale, long-lived reused SQL container and regenerated all six `ExpectedData/*` files (`2025`/`2026` × `results.csv`/`billing.csv`/`billing.json`) against a genuinely fresh container.
3. Manually reviewed every changed row before trusting the regeneration: confirmed already-correct producers (e.g. 110000, which is scaled-up and was never affected) kept their own tonnage figures unchanged, with only run-wide percentage-derived fields shifting (expected, since other producers' correctly-summed tonnage now contributes more to the total pool); confirmed no negative values or NaN anywhere in the diff; confirmed the newly-appearing `410000` "Missing Registration Data"/"Missing POM Data" rows are the intended output of that fixture's "different submitter" scenario (from the `IOrganisationPeriodFlagsCalculator` work done earlier in this branch).
4. Re-ran the full suite (`DataApi.UnitTests`, `BackgroundService.UnitTests`, `API.UnitTests`, `IntegrationTests`) twice against a fresh container to confirm stable green.

### Why the earlier investigation reached the wrong conclusion

The original write-up (below, kept for context) correctly found that `main`'s code sums multi-period tonnage fine, and initially inferred the opposite for `ECV-730-CF` from a `ProducerReportedMaterial`/`ProducerMaterialPackaging`/`GetTonnage` trace that *did* show the correct summed value at every point it checked — the same conclusion reached here. The mistake was in interpreting *why* the final exported `ExpectedData` file still showed the old value: it was assumed to reflect current, correct code behaviour being asserted as "expected" (a real regression), rather than a frozen, stale fixture that the test harness had a bug preventing from ever being refreshed.

---

## Original investigation (superseded, kept for context)

A producer with more than one `ProducerReportedMaterial` row for the same material/packaging type (i.e. they reported in more than one submission period, e.g. both an H1 and H2 return) appeared to only have **one** of those periods' tonnage reflected in the "Calculation Result" section of the exported results CSV. Extensive tracing (`ProducerReportedMaterial` → `ProducerMaterialPackaging` → `ProducerFeesBuilder`'s join → `ProducerFeesUtil.GetTonnage` → `ProducerRowBuilder.GetProducerRow`'s `FeesByMaterial` assignment → `Section1MaterialsExporter.AppendRow` → the exact CSV-write call) confirmed the correct summed value present at **every single step**, on `ECV-730-CF`. The unresolved mystery was how the value could be correct at the write call yet still show the old value in the file on disk — which is exactly what the test-harness bug above explains: the "old value in the file on disk" was never actually written by any of these correct runs; it was a stale file left over from before this branch's work, never overwritten due to the CWD/build-output mixup.

## Reproduction (for reference — no longer indicates a bug)

1. Seed two `ProducerReportedMaterial`/`ProducerMaterialPackaging` rows for the same producer/material/packaging type but different `SubmissionPeriod`, where the producer is fully obligated and not scaled-up.
2. Run a calculator run and export the results CSV.
3. Observe: the tonnage/fee columns correctly reflect the sum of both periods (confirmed via direct instrumentation and via a from-scratch `ExpectedData` regeneration against a fresh container).
