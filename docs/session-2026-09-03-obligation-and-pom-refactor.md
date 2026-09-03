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

## Files changed this session

**New C# components** (`EPR.Calculator.API.DataApi/CommonDataApi/`):
- `ObligationDetermination/ProducerObligationDeterminer.cs`
- `PomEligibility/PomEligibilityFilter.cs`
- `PomEligibility/OrganisationPeriodFlagsCalculator.cs`
- `PomEligibility/SubmissionPeriodClassification.cs`

**Modified product code:**
- `DataApi/CommonDataApi/Entities/PayCalOrganisation.cs` (added `RegulatorStatus`)
- `DataApi/CommonDataApi/Infrastructure/SynapseContext.cs` (unmapped C#-computed fields)
- `DataApi/CommonDataApi/Alignment/ProducerPomAligner.cs` (packaging-type filter)
- `DataApi/StoredProcs/sp_GetPaycalOrgData.sql` (thinned twice by this session: obligation logic, then H1/H2; then deleted entirely in stage 6, inlined into `StreamOrganisationsRequestHandler.cs`)
- `DataApi/StoredProcs/sp_GetPaycalPomData.sql` (thinned twice by this session: eligibility gates, then packaging type; then deleted entirely in stage 6, inlined into `StreamPomsRequestHandler.cs`)
- `DataApi/StoredProcs/fn_ProducerObligationDetermination.sql` (deleted in stage 1)
- `BackgroundService/Services/DataLoading/CommonDataApiLoader.cs` (restructured pipeline three times across the three moves)
- `EPR.Calculator.API/App/ServiceConfiguration.cs` (DI registrations)

**New test coverage:**
- `DataApi.UnitTests/CommonDataApi/ObligationDetermination/{ObligationTestCaseLoader,ProducerObligationDeterminerTests}.cs`
- `DataApi.UnitTests/CommonDataApi/PomEligibility/{PomEligibilityFilterTests,OrganisationPeriodFlagsCalculatorTests}.cs`
- `DataApi.UnitTests/TestData/myc-obligation-determination-test-cases.csv` (ported from `epr-data`)
- Extended `DataApi.UnitTests/CommonDataApi/Alignment/ProducerPomAlignerTests.cs`
- Extended `BackgroundService.UnitTests/Services/DataLoading/CommonDataApiLoaderTests.cs`

**Integration test fixtures:**
- `IntegrationTests/TestData/{2025,2026}-pom-data.csv` (added complementary H1/H2 periods for the eligibility gate)
- `IntegrationTests/FakeCommonDataApiStreams.cs`, `BaseIntegrationTest.cs` (pass-through fake for `IProducerObligationDeterminer`/`IPomEligibilityFilter` where fixtures encode pre-resolved data)
- `IntegrationTests/ExpectedData/*` (all six files, regenerated per stage 4's fix)

**Documentation:**
- `docs/producer-fees-multi-period-tonnage-bug.md` (written, then corrected to reflect the true root cause)
- This file.

## Verification status

- **`DataApi.UnitTests`**: 131 tests, all passing (includes ~100 ported obligation-determination scenarios, new eligibility/flags/alignment tests).
- **`BackgroundService.UnitTests`**: 479 tests, all passing.
- **`EPR.Calculator.API.UnitTests`**: 346 tests, all passing.
- **`IntegrationTests`**: 2 tests (`IntegrationTest_2025`/`2026`), passing — confirmed stable across repeated runs against a freshly-created `Testcontainers` SQL Server instance (not the long-lived, potentially-stale one used for most of this session's debugging).

## Known caveats for whoever picks this branch up next

- The Synapse queries (formerly `sp_GetPaycalOrgData.sql`/`sp_GetPaycalPomData.sql`, now inlined as raw SQL text in `StreamOrganisationsRequestHandler.cs`/`StreamPomsRequestHandler.cs` per stage 6) are a best-effort port, never executed against a real Synapse warehouse in this environment. They should be reviewed by someone with that access before deployment.
- The integration test suite's `Testcontainers` SQL Server instance is configured with `WithReuse(true)`. That's convenient for fast local iteration but means state accumulates across every test run in a session — as this session found out, that can produce misleading results during heavy ad-hoc debugging. Worth remembering to `docker rm -f` it (find via `docker ps --filter "label=org.testcontainers=true"`) before trusting a result that seems surprising.
