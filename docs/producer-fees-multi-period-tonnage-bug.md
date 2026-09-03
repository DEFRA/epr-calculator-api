# Bug: "Calculation Result" fee report only reflects one submission period's tonnage for normal (non-scaled, non-partial) producers

## Status

Confirmed present on `main` (verified against commit `de831cb0`, pre-dating the `ECV-730-CF` branch). Not introduced or affected by any of the `ECV-730-CF` work (DataApi extraction, obligation determination, POM eligibility). Not fixed as part of that branch — needs its own ticket.

## Summary

A producer with more than one `ProducerReportedMaterial` row for the same material/packaging type (i.e. they reported in more than one submission period, e.g. both an H1 and H2 return) only has **one** of those periods' tonnage reflected in the "Calculation Result" section of the exported results CSV (`Section1MaterialsExporter` / the "Household Tonnage" / "Total Tonnage" / "Net Tonnage" columns and the fees derived from them). The other period's reported weight is silently dropped from this specific report section — it is not summed in.

This only affects producers that are:
- **Obligated for the full year** (no `DaysObligated`, so `CalcResultPartialObligationBuilder` skips them), and
- **Not scaled-up** (none of their submission periods have `SubmissionPeriodLookup.ScaleupFactor > 1`, so `CalcResultScaledupProducersBuilder` skips them).

Producers that *are* scaled-up or partial-obligation are unaffected, because both of those builders explicitly iterate and rewrite every entry in `ProducerDetail.ProducerReportedMaterials` — that incidentally exercises the correct "sum every row" path. A plain full-year, non-scaled producer with multiple periods never goes through either rewrite, and hits whatever is dropping the extra period(s) instead.

## How this was found

Working on `ECV-730-CF` (porting `sp_GetPaycalPomData`'s "must have submitted both H1 and H2" business rule from SQL into C#, see `IPomEligibilityFilter`), the existing integration test fixtures (`2025-pom-data.csv` / `2026-pom-data.csv`) needed extra rows added so several test producers would satisfy the new gate (previously this rule lived only in SQL and was never exercised by the integration tests, which bypass the real stored procs). Producer `210000/210001` ("Non Partial P1 L1 Ltd") had its H2 (`2024-P4`) row added with a large placeholder weight to make the effect obvious — and the exported CSV's "Household Tonnage" figure for that producer did not change at all when the placeholder weight was changed from realistic to an absurd value (`9999999`), which is what exposed the bug.

## Confirmed facts (traced end-to-end)

1. **`ProducerReportedMaterial`** (populated by the data-load phase, per submission period): has both rows correctly — verified by direct query, e.g. for producer 210001:
   ```
   2024-P1, HH, 433.424
   2024-P4, HH, 9999.999
   ```
2. **`ProducerMaterialPackaging`** (written by `ResultBuilder`/`CalcResultWriter.StoreProducerMaterialPackaging`, from the in-memory `producers` list just before the fees stage): also has both rows correctly, with the same values. This confirms nothing upstream of the fees calculation (data load, obligation determination, POM eligibility, scaling, partial-obligation) drops or merges the extra period.
3. **`ProducerFeesBuilder`'s join** (`ProducerDetail` ⋈ `ProducerMaterialPackaging` by `pd.Id == prm.ProducerDetailId`, filtered to the run): reproduced this exact query directly against the DB and confirmed it returns both rows for producer 210001.
4. **`ProducerFeesUtil.GetTonnage`** (the function that sums `ProducerMaterialPackaging` rows for a given producer/material/packaging type): instrumented directly and confirmed `projectedMaterialsLookup[(producer.ProducerId, producer.SubsidiaryId)]` contains **both** rows at this call site, and `prms.Sum(p => p.PackagingTonnage)` correctly computes **10433.423** (433.424 + 9999.999).
5. **Yet the exported CSV's "Household Tonnage" column for this producer shows only `433.424`** — the P1-only value, not the correctly-computed sum from step 4.

So the sum is computed correctly in memory, then something between that computation and the CSV write reduces it back to a single period's value.

## Where the trace stopped

The CSV column traces back to `Section1MaterialsExporter.AppendProducerDisposalFeesByMaterial`, which reads from `producer.FeeDetail.DisposalFeesByMaterial` — a `[NotMapped]` property on the `FeeDetail` **EF entity** (`EPR.Calculator.API.Data/DataModels/FeeDetail.cs`), cached from its `MaterialFees` navigation collection. `FeeDetail` is persisted via `calcResultWriter.StoreProducerFees` and (presumably) re-read from the database before/during CSV export, rather than the CSV exporter using the in-memory object built by `ProducerRowBuilder` directly.

The investigation did not go further than this — it's not yet established whether:
- the DB round-trip (write via `StoreProducerFees` / `MaterialFees` navigation, then re-read) is where the extra period is lost, or
- there's an earlier aggregation step between `ProducerRowBuilder.GetProducerRow` (which correctly computes the summed tonnage into `materialFeeSummary`/`FeesByMaterial`) and `FeeDetail` being persisted, or
- something else entirely.

**Suggested next step for whoever picks this up**: instrument (or step through) `ProducerRowBuilder.GetProducerRow`'s `result.FeeDetail.FeesByMaterial = materialFeeSummary` assignment (line ~298) to confirm the correct summed value is present there, then follow it through `calcResultWriter.StoreProducerFees` and back out through whatever query the CSV exporter (or `producerFeesBuilder`'s row list) uses to obtain `ProducerFeeExportRow`/`FeeDetail` instances for export. That's the next unexamined segment of the pipe.

## Why it was never caught before

- The bug requires a producer with **multiple reported periods** that is **neither scaled-up nor partial-obligation**. No pre-existing integration test fixture (on `main` or `ECV-730-CF`, prior to this investigation) had such a producer.
- `ProducerDataTransposer.GetProducerReportedMaterials` on `main` (the pre-`ECV-730-CF` equivalent of today's `ProducerPomAligner.GetReportedMaterials`) already grouped POM rows by `(SubmissionPeriod, PackagingType)`, producing one `ProducerReportedMaterial` row per period — this has never been summed into a single row at write time, on `main` or since. There is no unique constraint on the table forcing a merge either.
- `ProducerFeesUtil.cs`, `ProducerRowBuilder.cs`, `ReportedProducerService.cs`, and `CalcResultProjectedProducersBuilder.cs` are byte-identical between `main` and `ECV-730-CF` (`git diff de831cb HEAD -- <these files>` is empty) — confirming this is not a regression introduced by the `ECV-730-CF` work.

## Reproduction

1. Seed two `ProducerReportedMaterial`/`ProducerMaterialPackaging` rows for the same producer/material/packaging type but different `SubmissionPeriod` (e.g. an H1 and an H2 period), where the producer is fully obligated (`DaysObligated == null`) and not on any `SubmissionPeriodLookup` row with `ScaleupFactor > 1`.
2. Run a calculator run and export the results CSV.
3. Observe: the "Household Tonnage" (and derived "Total Tonnage" / "Net Tonnage" / fee) columns for that producer reflect only one period's tonnage, not the sum of both.

## Impact

Understated reported tonnage (and therefore understated disposal/comms fees) in the "Calculation Result" section of the results CSV for any producer who submits more than one period's worth of data in a year without being flagged as scaled-up or partial-obligation. Given `SubmissionPeriodLookup`'s legacy 2024 quarterly codes (P1–P4) and the ongoing H1/H2 half-yearly codes, this is plausible for real producers (e.g. someone submitting a correction/resubmission for a different period than their original return, or genuinely reporting per-quarter). The scale of real-world impact hasn't been assessed — this write-up only establishes the mechanism and confirms it's reproducible.
