# Session write-up: Step 3 (obligation determination + POM/org data selection into C#)

**Branch:** `ECV-730-CF`
**Commits this session:** `57b4cf7`..`b3d2b9d` (5 commits authored this session, on top of Steps 1–2 which were already complete), then rebased onto a teammate's `43ddecf` pulled from origin (see stage 5) — final tips `43ddecf`..`f81195a`.
**Scope:** 31 files changed, +5907/−3718 lines (this session's own commits, pre-rebase)

## Context

This session picked up Step 3 of the original DataApi-extraction plan (Steps 1–2, already done, had moved Synapse data streaming and org/POM alignment out of `BackgroundService` and into the `DataApi` sub-module). Step 3's goal, as originally framed: inline the business logic from `fn_ProducerObligationDetermination.sql` into C#, and thin `sp_GetPaycalOrgData`/`sp_GetPaycalPomData` down to "get accepted data only" — continuing to shrink what SQL is responsible for versus what the (eventually separately-deployable) DataApi module owns.

The work happened in four stages, each verified end-to-end before moving to the next, a fifth stage resolving test-infrastructure issues discovered along the way, and a sixth incorporating a teammate's follow-on commit pulled from origin.

---

## 1. Obligation determination inlined into C# (`57b4cf7`)

**What moved:** `fn_ProducerObligationDetermination.sql` (265-line SQL table-valued function) — a decision tree classifying each producer registration as Obligated (`O`) / Not Obligated (`N`) / Error (`E`), based on leaver codes, joiner/leaver dates, regulator status, and corporate-group/subsidiary inheritance rules — deleted entirely and reimplemented in C#.

**New component:** `IProducerObligationDeterminer` / `ProducerObligationDeterminer` (`EPR.Calculator.API.DataApi/CommonDataApi/ObligationDetermination/`). Operates on the whole in-memory list of a run's raw `PayCalOrganisation` rows (not row-by-row), since the decision requires cross-row aggregation — pivot counts of Obligated/NotObligated/Blank/Invalid per `(ProducerId, SubmissionPeriodYear)`, where `ProducerId = SubsidiaryId ?? OrganisationId`. Ported stage-by-stage from the SQL CTE pipeline, cross-checked against the equivalent PySpark implementation in the sibling `epr-data` repo (`producer_obligation_determination.py`), which is the canonical source used by the PayCal/Obligation Calculator applications.

**Supporting changes:**
- `PayCalOrganisation` gained a `RegulatorStatus` field (needed to detect `Regulator_Status == 'Cancelled'`, which forces Not Obligated) and had `ObligationStatus`/`NumDaysObligated`/`ErrorCode` become C#-computed outputs instead of SQL inputs.
- `sp_GetPaycalOrgData.sql` thinned: the decision-tree CTEs (`raw_obligation`, `status_inheritance`, `pivot_counts`, `decision_tree`, `rule_11_12`, `rule_13_14`, `rule_16`) removed; kept only file-selection/dedup (`latest_accepted_registration_files`, `latest_accepted_registrations`) plus the pre-existing H1/H2 flag computation (moved out separately in stage 3 below).
- `CommonDataApiLoader.StreamOrganisations` restructured from one-pass streaming-with-inline-mapping into two passes: stream all raw rows, run `IProducerObligationDeterminer.Determine` over the full list, then map to `CalculatorRunOrganisation`.
- Registered in DI (`ServiceConfiguration.cs`).

**Test coverage:** ~100 business-rule scenarios from `epr-data`'s `myc-obligation-determination-test-cases.csv` ported into `ProducerObligationDeterminerTests.cs`, driven by a purpose-built CSV parser (`ObligationTestCaseLoader.cs`) mirroring the Python loader's title/subtitle parsing and Obligated/Partial/Error expected-value semantics. 81 scenarios execute (some hand-authored ones don't apply to the C# port's input shape) plus 2 hand-written cases for `RegulatorStatus = "Cancelled"` (not covered by the CSV). All pass.

**Known caveat, flagged at the time:** the SQL side of this change (`sp_GetPaycalOrgData.sql`) is a best-effort port, untestable in this environment (no live Synapse connection) — the integration tests exercise only the C# port via a fake stream handler that bypasses the real stored proc.

---

## 2. POM eligibility (H1+H2 gate + registration-exists gate) moved to C# (`7585db0`)

**What moved:** `sp_GetPaycalPomData.sql`'s `LatestAcceptedPomsWith2Period` (a producer must have submitted both halves of the year) and `Latest_Org_Data_Selection` (a producer must have a matching accepted registration) CTEs — deleted from SQL, reimplemented in C#.

**New component:** `IPomEligibilityFilter` / `PomEligibilityFilter` (`EPR.Calculator.API.DataApi/CommonDataApi/PomEligibility/`). Groups the raw POM stream by `(OrganisationId, SubmitterId, Year)` (subsidiary-agnostic, matching the original SQL), classifies each `SubmissionPeriod` string into H1/H2 (`2024-P1`/`P2`/`P3` or `<year>-H1` → H1; `2024-P4` or `<year>-H2` → H2), and keeps only POMs whose group has both — combined with a registration-exists check against the organisation IDs from the (already-loaded) org stream.

**Wiring:** `CommonDataApiLoader.LoadDataCore` restructured again — both raw streams (orgs, poms) now complete before `IPomEligibilityFilter.Filter` runs (it needs the org stream's ID set), and the filtered POMs feed the final mapping to `AlignmentPom`.

**Significant discovery during fixture repair:** the integration test fixtures were built without any awareness of the H1+H2 gate (since it previously lived only in SQL, which the fake stream handler bypasses entirely) — almost every producer in `2025-pom-data.csv`/`2026-pom-data.csv` was a deliberate single-period test case. Ported faithfully, the gate excluded nearly all of them. After confirming with the user this was a genuine, never-actually-exercised business rule (not a regression), added the missing complementary H1/H2 periods to every affected producer in both fixture years, then regenerated expected output and verified it byte-identical to the pre-change baseline (confirming the fix changes nothing except correctly enforcing a rule that was silently untested before).

---

## 3. Packaging-type filter and org H1/H2 flags moved to C# (`96f51a2`)

Follow-on request: move `sp_GetPaycalPomData.sql`'s `packaging_type`/`packaging_material` WHERE clause into C#, which in turn unblocked moving `sp_GetPaycalOrgData.sql`'s remaining `organisation_period_flags` (per-subsidiary HasH1/HasH2) computation into C# too — since once the POM stream is no longer pre-filtered by packaging type in SQL, C# has the full data needed to compute these flags itself, eliminating a second, independent (and previously slightly differently-scoped) POM query that used to live inside the org proc.

**Changes:**
- `sp_GetPaycalPomData.sql`: dropped the `packaging_type IN ('HH','CW','PB') OR (packaging_type='HDC' AND packaging_material='GL')` filter — now returns all accepted, in-scope POM rows regardless of type.
- `ProducerPomAligner.Align` (`DataApi/CommonDataApi/Alignment/`): gained the equivalent business-rule filter, applied when building the POM lookup used for alignment.
- `sp_GetPaycalOrgData.sql`: dropped the `latest_accepted_pom`/`organisation_period_flags` CTEs and the `LEFT JOIN` entirely — now a much simpler query with no POM access at all.
- New `IOrganisationPeriodFlagsCalculator` / `OrganisationPeriodFlagsCalculator` (`DataApi/CommonDataApi/PomEligibility/`): computes each org/subsidiary's HasH1/HasH2 in C# from the (now type-unfiltered) POM stream, grouped per `(OrganisationId, SubsidiaryId, SubmitterId)` — a finer granularity than `IPomEligibilityFilter`'s org-level gate, matching the SQL's original per-subsidiary scope. Shares period-classification logic with `IPomEligibilityFilter` via a new `SubmissionPeriodClassification` static helper (extracted to avoid duplicating the H1/H2 string-matching rules).
- Wired into `CommonDataApiLoader` alongside the eligibility filter, after both streams complete.

**Latent bug fixed along the way:** `SynapseContext.cs` was still mapping `obligation_status`/`num_days_obligated`/`error_code` to SQL columns that `sp_GetPaycalOrgData` had already stopped selecting (from stage 1's work) — would have thrown "invalid column name" against a real Synapse connection, never caught because tests bypass the real SQL. Fixed by `Ignore()`-ing those properties (and the new `HasH1`/`HasH2`) in the EF model instead of mapping them to columns.

**New test coverage:** `OrganisationPeriodFlagsCalculatorTests.cs` and `PomEligibilityFilterTests.cs` (neither component had dedicated unit tests before this stage), plus new `ProducerPomAlignerTests.cs` cases for the packaging-type filter (reportable types HH/CW/PB, HDC+GL, and exclusion of unreportable types/HDC+non-Glass).

---

## 4. Bug investigation and resolution (`d322197`, `355cec2`, `d548b98`)

While verifying stage 3's fixture changes, a discrepancy surfaced: a producer's "Household Tonnage" in the exported results CSV appeared to reflect only one submission period's weight instead of the sum of both. This was documented as a suspected product bug (`docs/producer-fees-multi-period-tonnage-bug.md`), initially believed to affect `main` too, then narrowed to `ECV-730-CF` only after empirical testing against `main`'s own code showed it summed correctly there.

**Resolution:** direct instrumentation of every step in the fee-calculation/export pipeline (`ProducerFeesUtil.GetTonnage`, `ProducerRowBuilder.GetProducerRow`'s `FeesByMaterial` assignment, `Section1MaterialsExporter.AppendRow`, and the exact CSV-write call) showed the **correct summed value at every single point**, on `ECV-730-CF` — there was no product bug. The real fault was in the **test harness**: `CalculatorRunIntegrationTests.RunTest`'s `RECORD_EXPECTED=1` regeneration mode wrote to a path relative to the test process's working directory (`bin/Debug/net10.0/`, not the source tree), so every attempted fixture regeneration this session (and evidently before it) silently landed in a build-output copy that the next `dotnet build` overwrote from the untouched, stale source `ExpectedData/*` files. Compounded by a long-lived, reused `Testcontainers` SQL Server container accumulating state across many hours of ad-hoc debugging, which produced at least one misleading "looks fine" result mid-investigation.

**Fix:** pointed the (temporary, since-reverted) `RECORD_EXPECTED` scaffolding at the actual source `ExpectedData/` path; tore down the polluted container; regenerated all six `ExpectedData/*` files (2025/2026 × results/billing CSV/JSON) against a fresh container; manually reviewed every changed row before trusting it (confirmed already-correct producers' own tonnage was unchanged with only run-wide percentage-derived fields shifting, no negative/NaN values, and the new `410000` "Missing Registration Data"/"Missing POM Data" rows are the correct, intended output of stage 3's H1/H2 fix for that fixture's "different submitter" scenario). Verified green twice against a fresh container. The bug doc was rewritten to explain the true root cause for future readers.

---

## 6. Pulled in from origin and rebased on top: SQL fully inlined into C#, plus OpenTelemetry (`43ddecf`)

Partway through stage 5's clean-up, a teammate (Nicholas Featch) pushed `ECV-730 inline sp_GetPaycalOrgData & sp_GetPaycalPomData and add OpenTelemetry logging to DataApi` to `origin/ECV-730-CF`, based on the same commit (`96f51a2`) this session's stage-4/5 commits were also based on — a genuine divergence. Fetched and, at the user's direction, rebased this session's four commits on top of it (`git rebase origin/ECV-730-CF`), rather than the other way round. It applied cleanly with no conflicts despite touching nearly every file this session had also changed, and the full test suite (`DataApi.UnitTests`, `BackgroundService.UnitTests`, `API.UnitTests`, `IntegrationTests`) was re-verified green afterwards, including against a freshly-created `Testcontainers` instance.

**Intention of that commit:** take the SQL-thinning this session had been doing one step further — rather than the DataApi module continuing to depend on `sp_GetPaycalOrgData`/`sp_GetPaycalPomData` as separately-deployed Synapse stored procedures (with the `.sql` files in this repo serving only as a reference copy of what needed to exist on the Synapse side), the *text* of those thinned queries is now embedded directly in the C# handlers that call them (`StreamOrganisationsRequestHandler.cs`, `StreamPomsRequestHandler.cs`) via `FromSqlInterpolated(...)`, and both `.sql` files are deleted from the repo entirely. The query logic itself is unchanged — it's the same thinned SQL this session had already arrived at (file-selection/dedup CTEs only, no obligation-decision or H1/H2 logic) — just relocated from a named stored procedure invocation (`EXEC [dbo].[sp_GetPaycalOrgData] ...`) to inline raw-SQL text in the handler. This means DataApi no longer requires any pre-existing SQL object on the Synapse side at all for these two queries — the query is entirely owned and versioned in code.

**Supporting changes in the same commit:**
- `StreamOrganisationsRequestHandler`/`StreamPomsRequestHandler` switched from an injected `SynapseContext` to `IDbContextFactory<SynapseContext>` (one context per streaming call) and threaded a `CancellationToken` through `Handle(...)`.
- New `DataApiTelemetry` (`DataApi/CommonDataApi/Infrastructure/`): a small `ActivitySource`-based OpenTelemetry helper (source name `"epr.paycal"`, deliberately matching `BackgroundService`'s existing telemetry source so traces from both projects land in the same pipeline, since `DataApi` can't reference `BackgroundService`'s own `Telemetry` type). Wrapped around the two streaming handlers and around this session's business-logic components (`ProducerObligationDeterminer`, `OrganisationPeriodFlagsCalculator`, `PomEligibilityFilter`) — no logic changes to any of them, just an activity span added around each.
- `ProducerPomAligner`, `CommonDataApiLoader`, `ServiceConfiguration.cs`, `FakeCommonDataApiStreams.cs`, `SynapseContext.cs`, `appsettings.json` all picked up matching small adjustments (mostly cancellation-token plumbing and DI registration for the new telemetry/factory types).

---

## 7. Cutoff-date/resubmission file selection moved to C# (uncommitted)

Follow-on request, after stage 6's SQL inlining: move the cutoff-date logic still embedded in both handlers' SQL — "a resubmission created after the cut-off date doesn't count; fall back to the latest still-eligible file" — into C#, exposing whether a candidate file is an initial submission or a resubmission. Scoped explicitly with the user via `AskUserQuestion`: preserve the existing "fall back to an earlier eligible file" behaviour exactly (rather than a simpler but behaviour-changing alternative), which requires the SQL to stop ranking/deduping altogether and hand C# the *full* candidate-file list per org/submitter/period.

**Entities** (`DataApi/CommonDataApi/Entities/{PayCalOrganisation,PayCalPom}.cs`): each gained `FileName`, `IsResubmission`, `CreatedDateTime` — file-selection inputs only, explicitly commented as not carried past that stage (nothing downstream reads them). Mapped to `file_name`/`is_resubmission`/`created_date_time` columns in `SynapseContext.cs`.

**SQL rewritten in both handlers** to stop selecting a winner at all — they now return every accepted-status candidate file unfiltered (no `ROW_NUMBER()`/dedup, no cut-off filtering, `Handle(...)` lost its `cutOffDate` parameter entirely):
- `StreamPomsRequestHandler.cs`: the `latest_accepted_pom` CTE (which used `ROW_NUMBER() ... PARTITION BY org/submitter/period ORDER BY CreatedDateTime DESC` plus the cut-off `WHERE`) became `candidate_pom_files`, with `SELECT DISTINCT` added — necessary because the old `ROW_NUMBER()` was incidentally collapsing `rpd.Pom`'s per-line-item duplication down to one row before the outer re-expansion join; removing it without adding `DISTINCT` would have caused row multiplication in the outer join to `rpd.POM`.
- `StreamOrganisationsRequestHandler.cs`: same shape change (`larf_base`/`latest_accepted_registration_files` → single `candidate_registration_files` CTE, `DISTINCT`, no ranking, no cut-off `WHERE`). Initially left as two CTEs (an intermediate `candidate_registrations` re-joining to `CompanyDetails`), then collapsed into the single-CTE-plus-main-`SELECT` shape to match `StreamPomsRequestHandler` — the middle CTE wasn't doing anything the outer `SELECT`'s join/`WHERE` couldn't do directly.

**New component:** `IAcceptedFileSelector` / `AcceptedFileSelector` (`DataApi/AcceptedFileSelection/`) — one generic algorithm shared by both entity types via delegates (group-key selector, file-name/`IsResubmission`/`CreatedDateTime` selectors). Per group: filter to eligible candidates (not a resubmission, or no cut-off, or created on/before the cut-off), pick the latest by `CreatedDateTime` as the winner, keep every row belonging to the winning file, and drop groups with no eligible candidate at all.

**Wiring:** `CommonDataApiLoader` applies the selector to the raw organisation stream before `IProducerObligationDeterminer.Determine` (which needs the winner already decided, since its own aggregation is per producer/period) and to the raw POM stream before returning. Registered in DI (`ServiceConfiguration.cs`). `FakeCommonDataApiStreams.cs` and `CommonDataApiLoaderTests.cs` updated to match the simplified `Handle(...)` signature and the new dependency (mocked as pass-through, since neither fake did cut-off filtering to begin with).

**Test coverage:** new `AcceptedFileSelectorTests.cs` (`DataApi.UnitTests/AcceptedFileSelection/`) — 12 scenarios ported directly from `epr-data`'s `test_paycal_orgdata_sql.py`/`test_paycal_pomdata_sql.py` reference cases (Initial/Resub/Resub2 chains, before/after cut-off), restricted to the subset actually relevant to this component — regulator-status filtering (Pending exclusion, POM's `Regulator_Status = 'Accepted'` filter) remains SQL's job and never reaches the selector — plus 4 hand-written edge cases (no eligible candidate in a group, `cutOffDate: null` disabling the cut-off entirely, grouping isolation across org/submitter/year, and a winning file's line items all surviving together). 16 tests total, all passing.

**Known caveat:** same as stage 6 — the SQL text changes are unexecuted/unverified against a real Synapse warehouse in this environment (integration tests exercise the fakes, not this SQL). Needs review by someone with warehouse access before deployment.

**Status:** implemented and fully green, committed as `db020f4`.

---

## 8. Single-request DataApi boundary: alignment + error detection consolidated (`27a2e09`)

Follow-on request: with Steps 1–7 having moved every piece of org/POM business logic into the `DataApi` project, review how `BackgroundService` actually *calls* that logic ahead of DataApi's eventual extraction to its own service — with the explicit goal of a single request returning fully-transposed data, rather than the two streaming calls plus several more in-process stages that existed at the time.

**What the review found:** `CommonDataApiLoader` made two concurrent streaming calls (`IStreamOrganisationsRequestHandler`/`IStreamPomsRequestHandler`), then ran five more processing stages client-side — `IAcceptedFileSelector`, `IProducerObligationDeterminer`, `IPomEligibilityFilter`, `IOrganisationPeriodFlagsCalculator`, and a private mapper (`PayCalOrganisation`/`PayCalPom` → `AlignmentOrganisation`/`AlignmentPom`) — using types that physically lived in `DataApi` but executed wherever `BackgroundService` ran, purely as an artifact of the project split rather than any real ownership boundary. A further two stages (`IProducerPomAligner.DedupeOrganisations`/`.Align`, in `ProducerDataTransposer`) plus `ErrorReportService`'s four error/warning detection rules ran the same way.

**Consolidated into one DataApi entry point:** `IProducerDataService.GetProducerData(relativeYear, cutOffDate, materialCodes, invoicedOrganisationIds)` (`DataApi/CommonDataApi/ProducerDataService.cs`) now owns the entire pipeline — streaming, file selection, obligation determination, POM eligibility, period flags, mapping, error/warning detection, and alignment — returning one `ProducerCalculationData { Organisations, Producers, Errors }`.

**New DataApi component:** `IProducerErrorDetector`/`ProducerErrorDetector` (`DataApi/Alignment/ProducerErrorDetector.cs`) — ported from `ErrorReportService`'s four static rules (`HandleMissingRegistrationData`, `HandleMissingPomData`, `HandleObligatedErrors`, `HandleObligatedWarnings`) plus the holding-company roll-up, now operating on `AlignmentOrganisation`/`AlignmentPom` and producing `ProducerCalculationError` (`OrganisationId`/`SubsidiaryId`/`ErrorCode`/`LeaverCode`/`IsWarning`). Each org/subsidiary in the result is represented against an error, POM data (via `Producers`), or both for a warning (which is kept in calculation, so it still gets aligned).

**BackgroundService thinned:** `CommonDataApiLoader` now just checks the `Enabled` flag, gathers the small inputs only it can supply (`materialCodes` via `IMaterialService`, at this stage `invoicedOrganisationIds` via `IInvoicedProducerService`), and makes the one call. `ProducerDataTransposer` no longer runs alignment itself. `ErrorReportService` shrank to a pure `PersistErrors` writer.

**Test coverage:** business-rule tests moved to `DataApi.UnitTests/Alignment/ProducerErrorDetectorTests.cs` (17 tests, ported 1:1 from the old `ErrorReportServiceTests.cs`); new `DataApi.UnitTests/CommonDataApi/ProducerDataServiceTests.cs` (6 tests) covering the orchestration itself — notably the previously-untested sequencing where a hard error's org/subsidiary must be excluded from alignment even though a matching POM exists, while a warning's must not.

**Status:** implemented, fully green against the whole suite plus both integration tests (byte-identical to fixture data, confirming the consolidation changed nothing observable), committed as `27a2e09`.

---

## 9. Removed billing-history dependency from DataApi's contract (`ea9f0ca`)

Follow-on to stage 8, raised by the user: didn't want a potentially-large `invoicedOrganisationIds` set fed into DataApi at all — less because of literal payload size (bounded by the large-producer population, likely low thousands) and more because DataApi's contract shouldn't need billing-history knowledge it has no other reason to own; that data lives entirely in Calculator API's own DB (`ProducerResultFileSuggestedBillingInstruction` and related tables via `IInvoicedProducerService`).

**The problem:** `HandleObligatedErrors`/`HandleObligatedWarnings`'s old gate — <span style="white-space:nowrap">`poms.Any(matches) || invoicedOrganisationIds.Contains(o.OrganisationId)`</span> — only ever needed the invoiced half for organisations with **no** POM match (a POM match alone always satisfied the gate). So the fix was to stop gating on it in DataApi at all.

**Change:** `ProducerErrorDetector` now always includes every qualifying `"E"`-status/warning-eligible organisation unconditionally, and each `ProducerCalculationError` gained a `HasPomMatch` flag recording whether a current-year POM match was found (`true` unconditionally for the POM-driven `MissingRegistrationData`/`MissingPOMData` categories, computed per-row for `HandleObligatedErrors`/`HandleObligatedWarnings`). `IProducerDataService.GetProducerData` no longer takes `invoicedOrganisationIds` at all.

**The decision moved to `ErrorReportService.PersistErrors`**, which now also takes `relativeYear`, looks up invoiced organisations itself (`IInvoicedProducerService`, re-added as a dependency here), and keeps a row when `e.HasPomMatch || invoicedOrganisationIds.Contains(e.OrganisationId)`. The holding-company roll-up had to move with it rather than stay in `ProducerErrorDetector`: it's computed by grouping on which errors are subsidiary-scoped, and doing that *before* the invoiced-filter (as stage 8 did, inside DataApi) would leave an orphaned roll-up row for a producer whose only underlying error got filtered out — a real behaviour bug, not just a relocation. Caught by reasoning through the ordering before implementing, then locked in with a dedicated regression test, `PersistErrors_DoesNotOrphanRollup_WhenOnlySubsidiaryErrorWasFilteredOut`.

**Test coverage:** `ProducerErrorDetectorTests.cs` updated (unconditional-inclusion + `HasPomMatch` assertions replace the old invoiced-gating tests); `ErrorReportServiceTests.cs` rewritten with 9 tests covering the filter, the roll-up, and the ordering interaction between them.

**Status:** implemented, fully green against the whole suite plus both integration tests (again byte-identical), committed as `ea9f0ca`.

---

## 10. Reshaped DataApi's producer response into a single `ProducerRecord` list (`69eb8cc`)

Follow-on request, raised after reviewing stage 8's `ProducerCalculationData { Organisations, Producers, Errors }` shape: the user felt returning three parallel lists wasn't RESTful and asked for a review of what actually blocked collapsing it into `List<ProducerRecord>` — one row per organisation, carrying its own error/warning/packaging data, instead of a shape callers have to reassemble themselves.

**What the review found:** the split existed because `ProducerPomAligner.Align` silently dropped any obligated organisation with no POM data of its own (a holding company whose subsidiaries report on its behalf) — so `ProducerFeesBuilder`/`CalcResultScaledupProducersBuilder` had to bypass `Producers` and re-query the unfiltered `Organisations` population directly to find that parent's identity. A second, softer finding: dropping non-obligated/no-error organisations from the response entirely was safe, since `BillingFileService`/`InvoicedProducerService` already fall back across previous runs for a cancelled/lapsed producer's "last known name" — nothing needed same-run visibility into organisations that were never obligated and never errored.

**New type:** `ProducerRecord` (`DataApi/Alignment/ProducerRecord.cs`, replacing `AlignedProducer`) — org identity plus `Errors` (hard, exclusionary) and `Warnings` (soft, non-exclusionary — can coexist with `ReportedMaterials`) as separate lists, since a single org/subsidiary can pick up more than one hard error (e.g. an "E"-status org whose POM data also fails the missing-registration check) and a warning never excludes packaging data the way a hard error does. `ProducerCalculationError` trimmed of its now-redundant `OrganisationId`/`SubsidiaryId` (the parent `ProducerRecord` already carries them); a new small carrier, `OrganisationCalculationError` (org/sub key + `ProducerCalculationError`), fills the two remaining spots that still need a flat, keyed list — the detector's output, and re-flattening a record's errors/warnings for persistence.

**`ProducerPomAligner.Align`** no longer drops an obligated organisation with zero matched POM data — it now yields a record with empty `ReportedMaterials`, closing the gap the review found.

**`ProducerDataService.GetProducerDataCore`** gained the merge logic that reassembles the three-way split into one list: aligned obligated records (with detection's errors/warnings attached, matched by org/subsidiary since detection doesn't distinguish submitters), "E"-status organisations the aligner never looks at, and "orphan" `MissingRegistrationData` errors — POM-driven, so they can reference an org/subsidiary with no exact registration match — borrowing identity from any other row sharing the `OrganisationId` where one exists, falling back to an empty identity in the rare case none does (mirroring the pre-existing behaviour where such an organisation never got a `CalculatorRunOrganisation` snapshot either).

**`ProducerCalculationData` deleted.** `IProducerDataService.GetProducerData` now returns `Task<IReadOnlyList<ProducerRecord>>` directly. `ProducerDataTransposer` writes every record to `CalculatorRunOrganisation` (replacing the old, separately-carried `Organisations` list — the deliberate persistence-level narrowing the review justified), but keeps writing `ProducerDetail`/`ProducerReportedMaterial` only for records with non-empty `ReportedMaterials`, preserving the existing gate that keeps empty-POM obligated parents out of `ProducerDetail`.

**Test coverage:** all three affected DataApi/BackgroundService unit test projects updated for the new shape; new cases added for the empty-POM obligated org now yielding a record and the orphan-error identity fallback; new `ProducerDataTransposerTests.cs` (no dedicated test file existed for the transposer before this) covering the three-way split at persistence time.

**Status:** implemented, fully green against the whole suite plus both integration tests, committed as `69eb8cc`.

---

## 11. Replaced `ObligationStatus` filtering with an explicit `IsError` flag (`25a19ee`)

Follow-on discussion: three downstream consumers (`ProducerFeesBuilder`, `CalcResultScaledupProducersBuilder`, `InvoicedProducerService`) filtered `CalculatorRunOrganisation` on `ObligationStatus == "O"` to find "the real, calculation-valid row" as opposed to one that exists only to carry error data. Talked through with the user whether that filter was still correct now that stage 10 could put a hard error onto an otherwise-obligated record (or, more rarely, let an orphan record borrow an "O" `ObligationStatus` despite having no usable identity) — concluding the two concepts aren't the same axis, and `ObligationStatus == "O"` gets both edge cases wrong.

**New property:** `ProducerRecord.IsError` (`Errors.Count > 0`, computed, not settable) — the two-way split the user asked for: valid-for-calculation (with or without a warning) versus error/excluded. Persisted as a new `is_error` column on `CalculatorRunOrganisation` (migration `AddIsErrorToCalculatorRunOrganisation`); `ProducerDataTransposer` sets it from `record.IsError`.

**Consumers switched from `ObligationStatus == "O"` to `!IsError`:** `ProducerFeesBuilder`/`CalcResultScaledupProducersBuilder`'s parent-organisation lookups, and `InvoicedProducerService`'s cross-run tie-break when picking the preferred org-name snapshot. (`CalcResultPartialObligationBuilder`'s `ProducerDetail.ObligationStatus` check was left alone at this stage — it's on a different table that never contains error rows in the first place, so it was already redundant either way.)

**Test coverage:** no existing test needed changes (every fixture's default `IsError = false` already matched the intended "valid" case); added targeted assertions locking in the semantics — `IsError` true for hard-errored and orphan-error records, false for the happy path, and, the key one, false for a record carrying only a warning (proving a warning alone doesn't flip the split); extended `ProducerDataTransposerTests.cs` to confirm `IsError` persists from a record's `Errors`, not its `Warnings`.

**Status:** implemented, fully green against the whole suite plus both integration tests, committed as `25a19ee`.

---

## 12. Removed the now-dead `ObligationStatus`/`HasH1`/`HasH2`/`SubmitterId` fields (`06b0b18`)

Follow-on request: with stage 11 replacing every real reader of `ProducerRecord.ObligationStatus`, and `HasH1`/`HasH2`/`SubmitterId` having no reader anywhere outside DataApi's own internal dedup/matching logic (confirmed by grepping the whole app for each field before removing it), the user asked to drop all four from `ProducerRecord` outright — and, once confirmed the corresponding DB columns had no external reader either, to drop the columns too rather than leave them silently unpopulated going forward.

**Removed from `ProducerRecord`** (`DataApi/Alignment/ProducerRecord.cs`): `ObligationStatus`, `HasH1`, `HasH2`, `SubmitterId`. The internal `AlignmentOrganisation`/`AlignmentPom`/`PayCalOrganisation`/`PayCalPom` types are untouched — they still need these fields for dedup, alignment, and POM matching; only the DataApi *response* type lost them.

**Removed from persisted entities:** `SubmitterId`/`ObligationStatus`/`HasH1`/`HasH2` from `CalculatorRunOrganisation`; `SubmitterId`/`ObligationStatus` from `ProducerDetail` (it never had `HasH1`/`HasH2`) — along with their EF `TypeConfiguration` mappings.

**`CalcResultPartialObligationBuilder`**'s `pd.ObligationStatus == Obligated` filter on `ProducerDetail` replaced with just `pd.DaysObligated != null` — safe because a `ProducerDetail` row is now provably always obligated: it's only ever created for a record with reported materials, and only the aligner (which only ever processes "O"-status organisations) ever produces those.

**Migration** `RemoveUnusedObligationAndSubmitterColumns` drops exactly those six columns — verified against a clean model-snapshot diff before committing.

**Dead code removed:** `ObligationStates` (the BackgroundService-side status-constant helper) had zero remaining consumers after this stage and was deleted.

**Test coverage:** mechanical updates only — every fixture/assertion referencing a removed field had its now-invalid property initializer dropped; no new behavioural coverage needed since this was pure removal of unused surface.

**Status:** implemented, fully green against the whole suite plus both integration tests, committed as `06b0b18`.

---

## Files changed this session

**New C# components** (`EPR.Calculator.API.DataApi/`):
- `CommonDataApi/ObligationDetermination/ProducerObligationDeterminer.cs`
- `CommonDataApi/PomEligibility/PomEligibilityFilter.cs`
- `CommonDataApi/PomEligibility/OrganisationPeriodFlagsCalculator.cs`
- `CommonDataApi/PomEligibility/SubmissionPeriodClassification.cs`
- `AcceptedFileSelection/AcceptedFileSelector.cs` (stage 7)
- `Alignment/ProducerCalculationError.cs`, `Alignment/ProducerErrorCodes.cs`, `Alignment/ProducerErrorDetector.cs` (stage 8, `ProducerErrorDetector`/`ProducerCalculationError` reworked in stage 9 — unconditional inclusion + `HasPomMatch`; `ProducerCalculationError` trimmed of `OrganisationId`/`SubsidiaryId` in stage 10)
- `CommonDataApi/ProducerDataService.cs` (stage 8 — the single DataApi entry point; stage 9 dropped `invoicedOrganisationIds` from `GetProducerData`; stage 10 gained the `ProducerRecord` merge logic and dropped `ProducerCalculationData` entirely; stage 12 stopped setting `ObligationStatus`/`HasH1`/`HasH2`/`SubmitterId`)
- `Alignment/ProducerRecord.cs` (stage 10, replacing `Alignment/AlignedProducer.cs` — the single unified response record; gained `IsError` in stage 11; lost `ObligationStatus`/`HasH1`/`HasH2`/`SubmitterId` in stage 12)
- `Alignment/OrganisationCalculationError.cs` (stage 10 — the org/sub-keyed error carrier used by the detector's output and by re-flattening a record's errors/warnings for persistence)
- ~~`CommonDataApi/ProducerCalculationData.cs`~~ (deleted in stage 10 — superseded by the flat `IReadOnlyList<ProducerRecord>` response)
- ~~`Alignment/AlignedProducer.cs`~~ (deleted in stage 10 — superseded by `ProducerRecord`)

**Modified product code:**
- `DataApi/CommonDataApi/Entities/PayCalOrganisation.cs` (added `RegulatorStatus`; stage 7 added `FileName`/`IsResubmission`/`CreatedDateTime`)
- `DataApi/CommonDataApi/Entities/PayCalPom.cs` (stage 7 added `FileName`/`IsResubmission`/`CreatedDateTime`)
- `DataApi/CommonDataApi/Infrastructure/SynapseContext.cs` (unmapped C#-computed fields; stage 7 mapped the three new file-selection columns on both entities)
- `DataApi/CommonDataApi/Alignment/ProducerPomAligner.cs` (packaging-type filter; stage 10 stopped dropping an obligated organisation with zero matched POM data - now yields a record with empty `ReportedMaterials`; stage 12 stopped setting `ObligationStatus`/`HasH1`/`HasH2`/`SubmitterId`)
- `DataApi/StoredProcs/sp_GetPaycalOrgData.sql` (thinned twice by this session: obligation logic, then H1/H2; then deleted entirely in stage 6, inlined into `StreamOrganisationsRequestHandler.cs`; stage 7 dropped its ranking/cut-off logic entirely)
- `DataApi/StoredProcs/sp_GetPaycalPomData.sql` (thinned twice by this session: eligibility gates, then packaging type; then deleted entirely in stage 6, inlined into `StreamPomsRequestHandler.cs`; stage 7 dropped its ranking/cut-off logic entirely)
- `DataApi/StoredProcs/fn_ProducerObligationDetermination.sql` (deleted in stage 1)
- `BackgroundService/Services/DataLoading/CommonDataApiLoader.cs` (restructured pipeline three times across the three moves; stage 7 wired in `IAcceptedFileSelector`; stage 8 thinned drastically down to the `Enabled` check + gathering `materialCodes`/`invoicedOrganisationIds` + one DataApi call; stage 9 dropped the `invoicedOrganisationIds` gathering entirely)
- `BackgroundService/Services/DataLoading/CommonDataApiLoaderMapper.cs` (deleted in stage 8 — the `PayCal*` → `Alignment*` mapping, including Guid/RAG-rating validation, moved into `ProducerDataService`)
- `BackgroundService/Services/ErrorReportService.cs` (shrunk to a pure `PersistErrors` DB writer in stage 8; regained the invoiced-filter and holding-roll-up logic in stage 9, now taking `relativeYear`; stage 10 changed its parameter from a flat `ProducerCalculationError` list to the keyed `OrganisationCalculationError` list)
- `BackgroundService/Services/ProducerDataTransposer.cs` (stage 8 — no longer runs `IProducerPomAligner` itself, just persists DataApi's result; stage 9 threads `relativeYear` through to `PersistErrors`; stage 10 rewritten to split one `IReadOnlyList<ProducerRecord>` across `CalculatorRunOrganisation`/`ProducerDetail`/`PersistErrors`; stage 11 sets the new `IsError` column; stage 12 stopped mapping the removed fields)
- `BackgroundService/Features/CalculatorRuns/CalculatorRunDataInitializer.cs` (stage 8 — updated to the new return shape; stage 10 updated again for `IReadOnlyList<ProducerRecord>`)
- `EPR.Calculator.API/App/ServiceConfiguration.cs` (DI registrations; stage 8 added `IProducerErrorDetector`/`IProducerDataService`)
- `BackgroundService/Builder/Summary/ProducerFeesBuilder.cs`, `BackgroundService/Builder/ScaledupProducers/CalcResultScaledupProducersBuilder.cs` (stage 11 — parent-organisation lookup switched from `ObligationStatus == "O"` to `!IsError`)
- `BackgroundService/Services/InvoicedProducerService.cs` (stage 11 — cross-run preferred-snapshot tie-break switched from `ObligationStatus` to `IsError`)
- `BackgroundService/Builder/PartialObligations/CalcResultPartialObligationBuilder.cs` (stage 12 — dropped the now-tautological `ProducerDetail.ObligationStatus == "O"` check)
- ~~`BackgroundService/Services/ObligationStates.cs`~~ (deleted in stage 12 — zero remaining consumers)
- `EPR.Calculator.API.Data/DataModels/CalculatorRunOrganisation.cs`, `.../TypeConfigurations/CalculatorRunOrganisationConfiguration.cs` (stage 10 doc comment rewritten for the narrower persistence contract; stage 11 added `IsError`/`is_error`; stage 12 dropped `SubmitterId`/`ObligationStatus`/`HasH1`/`HasH2`)
- `EPR.Calculator.API.Data/DataModels/ProducerDetail.cs`, `.../TypeConfigurations/ProducerDetailConfiguration.cs` (stage 12 — dropped `SubmitterId`/`ObligationStatus`)
- New migrations `AddIsErrorToCalculatorRunOrganisation` (stage 11) and `RemoveUnusedObligationAndSubmitterColumns` (stage 12), plus the regenerated `ApplicationDBContextModelSnapshot.cs`

**New test coverage:**
- `DataApi.UnitTests/CommonDataApi/ObligationDetermination/{ObligationTestCaseLoader,ProducerObligationDeterminerTests}.cs`
- `DataApi.UnitTests/CommonDataApi/PomEligibility/{PomEligibilityFilterTests,OrganisationPeriodFlagsCalculatorTests}.cs`
- `DataApi.UnitTests/TestData/myc-obligation-determination-test-cases.csv` (ported from `epr-data`)
- `DataApi.UnitTests/CommonDataApi/Alignment/ProducerPomAlignerTests.cs` (extended in earlier stages; stage 10 — five tests changed from "excludes the organisation" to "produces a record with no reported materials", plus a new explicit no-POM-data case; stage 12 dropped the removed-field assertions)
- Extended `BackgroundService.UnitTests/Services/DataLoading/CommonDataApiLoaderTests.cs`
- `DataApi.UnitTests/AcceptedFileSelection/AcceptedFileSelectorTests.cs` (stage 7)
- `DataApi.UnitTests/Alignment/ProducerErrorDetectorTests.cs` (stage 8 — ported from the old `ErrorReportServiceTests.cs`; stage 9 reworked for unconditional inclusion/`HasPomMatch`)
- `DataApi.UnitTests/CommonDataApi/ProducerDataServiceTests.cs` (stage 8 — new orchestration coverage, including the hard-error-vs-warning alignment-exclusion sequencing that had no prior test; stage 10 rewritten for the unified `ProducerRecord` list, plus a new orphan-identity-fallback case; stage 11 added `IsError` assertions, including the "warning alone doesn't flip it" case; stage 12 dropped the removed-field assertions)
- `BackgroundService.UnitTests/Services/DataLoading/CommonDataApiLoaderTests.cs` (stage 8 rewritten for the thinned loader; stage 9 dropped the invoiced-related setup; stage 10 updated to `IReadOnlyList<ProducerRecord>`; stage 12 dropped the removed-field initializers)
- `BackgroundService.UnitTests/Services/ErrorReportServiceTests.cs` (stage 8 rewritten as persistence-only tests; stage 9 rewritten again — filter/roll-up/ordering coverage, including the orphan-roll-up regression case; stage 10 switched its error builder to `OrganisationCalculationError`)
- `BackgroundService.UnitTests/Services/ProducerDataTransposerTests.cs` (new in stage 10 — no dedicated transposer test file existed before; covers the three-way `CalculatorRunOrganisation`/`ProducerDetail`/error-flattening split; stage 11 added the `IsError`-from-`Errors`-not-`Warnings` case; stage 12 dropped the removed-field initializers)
- `BackgroundService.UnitTests/Builder/CalcResultScaledupProducersBuilderTest.cs`, `BackgroundService.UnitTests/Builder/CalcResultPartialObligationBuilderTest.cs`, `BackgroundService.UnitTests/TestHelpers/TestData/TestDataHelper.cs` (stage 12 — dropped `ObligationStatus`/`SubmitterId` fixture initializers now that defaults suffice)

**Integration test fixtures:**
- `IntegrationTests/TestData/{2025,2026}-pom-data.csv` (added complementary H1/H2 periods for the eligibility gate)
- `IntegrationTests/FakeCommonDataApiStreams.cs`, `BaseIntegrationTest.cs` (pass-through fake for `IProducerObligationDeterminer`/`IPomEligibilityFilter` where fixtures encode pre-resolved data)
- `IntegrationTests/ExpectedData/*` (all six files, regenerated per stage 4's fix)

**Documentation:**
- `docs/producer-fees-multi-period-tonnage-bug.md` (written, then corrected to reflect the true root cause)
- This file.

## Verification status

- **`DataApi.UnitTests`**: 175 tests, all passing (includes ~100 ported obligation-determination scenarios, eligibility/flags/alignment tests, stage 7's 16 `AcceptedFileSelector` tests, stages 8–9's `ProducerErrorDetectorTests`/`ProducerDataServiceTests`, and stage 10's additional `ProducerDataServiceTests`/`ProducerPomAlignerTests` cases for the unified-record merge and the orphan-identity fallback).
- **`BackgroundService.UnitTests`**: 456 tests, all passing (down from 479 pre-stage-8 — stage 8 deleted `CommonDataApiLoaderMapperTests.cs` outright and traded fine-grained coverage for the leaner DataApi-side tests; stage 10 added the new `ProducerDataTransposerTests.cs`, the file's first dedicated test coverage).
- **`EPR.Calculator.API.UnitTests`**: 346 tests, all passing (untouched by stages 8–12).
- **`IntegrationTests`**: 2 tests (`IntegrationTest_2025`/`2026`), passing — confirmed stable across repeated runs against a freshly-created `Testcontainers` SQL Server instance (not the long-lived, potentially-stale one used for most of this session's debugging). Stage 7's output was byte-identical to the pre-change baseline, confirming the existing fixtures (single candidate file per group, `IsResubmission` defaulting `false`) flow through the new selector unchanged. Stages 8 through 12 were each independently verified byte-identical too, confirming the DataApi consolidation, the invoiced-filter/roll-up relocation, the unified-record reshape, the `IsError` split, and the final field/column removal each changed nothing observable end-to-end.

## Known caveats for whoever picks this branch up next

- The Synapse queries (formerly `sp_GetPaycalOrgData.sql`/`sp_GetPaycalPomData.sql`, now inlined as raw SQL text in `StreamOrganisationsRequestHandler.cs`/`StreamPomsRequestHandler.cs` per stage 6) are a best-effort port, never executed against a real Synapse warehouse in this environment. They should be reviewed by someone with that access before deployment.
- The integration test suite's `Testcontainers` SQL Server instance is configured with `WithReuse(true)`. That's convenient for fast local iteration but means state accumulates across every test run in a session — as this session found out, that can produce misleading results during heavy ad-hoc debugging. Worth remembering to `docker rm -f` it (find via `docker ps --filter "label=org.testcontainers=true"`) before trusting a result that seems surprising.
- Stage 8's consolidation dropped one piece of telemetry rather than porting it: `CommonDataApiLoader` used to wrap each stream's *first item* with `ITelemetry<T>.Metric(..., StreamDelayThreshold)`, logging a warning if either stream took over 5 minutes to start yielding rows. That's a BackgroundService-specific concept (`ITelemetry<T>`'s named `Metrics` enum + threshold-warning semantics) with no equivalent in DataApi's own telemetry (`DataApiTelemetry`, a plain `ActivitySource` wrapper). `ProducerDataService.GetProducerData` is still wrapped in a single activity span end-to-end, so total duration is still visible, but a slow-to-start stream specifically won't trigger the old dedicated warning any more.
- Stage 12's `RemoveUnusedObligationAndSubmitterColumns` migration is data-losing once applied to a real database — `Down()` re-adds the six columns but with no data, since dropping a column throws away its contents. Both this migration and stage 11's `AddIsErrorToCalculatorRunOrganisation` were generated and validated (`dotnet ef migrations add` against a dummy connection string, then a clean model-snapshot-diff review) but never applied to a real SQL Server instance in this environment — same caveat as the Synapse queries above, review before deploying.
- Stage 10's persistence-level narrowing (only obligated/errored organisations get a `CalculatorRunOrganisation` row, not the full unfiltered Synapse population) is a deliberate behaviour change from every prior stage, justified in the session's conversation by tracing `BillingFileService`'s and `InvoicedProducerService`'s cross-run name-lookup fallbacks — but it hasn't been observed against a long history of real production data, only against the integration test fixtures' two-run scenario. Worth keeping an eye on the first few real runs after deployment for a cancelled/lapsed producer whose name can't be found at all (only possible if it was cancelled on its very first-ever appearance, with zero prior run history to fall back to).
