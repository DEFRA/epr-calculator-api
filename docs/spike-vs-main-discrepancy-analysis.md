# Spike vs main: results-file discrepancy analysis

**Branch:** `ECV-730-CF` (spike — Synapse procedures moved into the codebase)
**Compared runs:**
- DEV: `spike8` (Run Id 52) vs `main2` (Run Id 53) — Parts 1–2.
- TST: `spikeTst1` (Run Id 56) vs `mainTst1` (Run Id 55) — Part 3.

Both financial year 2026-27, cut-off date NA, same LAPCAP / parameter files.
**Status:** DEV root cause confirmed against the warehouse. TST "Cancelled Producers"
issue diagnosed from code; needs confirmation against the calculator API database.

## Context

In this spike the PayCal organisation/POM stored procedures (`sp_GetPaycalOrgData`,
`sp_GetPaycalPomData`, `fn_ProducerObligationDetermination`) have been moved out of Synapse
and reimplemented in C# inside the `DataApi` module. The long-term aim is for `DataApi` to
become a separately-deployable service that streams data over HTTP. This is work in progress.

Running the same inputs through the spike and through `main` produces results files that
differ. This document reconciles every difference and maps it to a cause.

## Part 1 — what actually differs

Every difference between the two files traces back to **one root cause**: three producers —
**165026**, **167432**, **169063** — are in the obligated pool on `main` but are excluded
in the spike (as cancelled / not-obligated / missing-registration). Everything else is
arithmetic downstream of that.

### Producers affected

| Producer | On `main` | In the spike |
|---|---|---|
| **165026** (PINK PINK NAILS LTD, subs 165023, 165027) | Clean, fully obligated. Blank registration status code. Previous Invoiced Tonnage 141.0 (invoiced in run 28). Contributes tonnage to the calculation. | Excluded. Appears in **Cancelled Producers** *and* in the **Error Table** ×3 as **"Missing Registration Data"**. |
| **167432** (SHIMMZU LTD) | Absent from the results file entirely (no calc row, no error). | **Error Table**: "Not Obligated". |
| **169063** (MEDIPRAX LIMITED, subs 169064, 169065) | Absent entirely. | **Error Table** ×3: "Not Obligated". |
| 168946 (OSSETT CATERING LIMITED) | "Not Obligated" error + Cancelled Producers. | Identical — the spike changed nothing for it. |

### Section-by-section

| Section | Difference |
|---|---|
| Header / input files | Run name / id / timestamps only. |
| LAPCAP Data, Late Reporting, Parameters – Other, Apportionment % | Identical. |
| Parameters – Comms Costs | £ rates per country **identical**. Only the tonnage columns differ (e.g. Aluminium Household Tonnage 3178.302 → 3052.302, −126.000). |
| LA Disposal Cost Data | Same — rates identical, tonnages lower in the spike. |
| Modulation Calculation | Same — rates identical, tonnages lower. Green Modulation Factor 0.859992 → 0.859993 (rounding off the smaller pool). |
| Cancelled Producers | `main`: `168946` only. Spike: `168946` + **`165026`**. This section lists producers invoiced in a *previous* run this FY (run 28, "inlinefunction12") that are **not** in the current run — so it is a *symptom* of 165026 being excluded, not a separate cause. |
| H1 / H2 Packaging Data | **Byte-identical for every common producer.** Only difference: `main` has 3 extra rows for `165026`. |
| Calculation Result | 183 of 184 common producer rows differ — but **every one is identical through column 527** (all reported / submitted / projected tonnage) and **differs only from column 528 onward** ("Percentage of Producer Tonnage vs All Producers" and everything derived from it: comms costs, SA operating costs, disposal fees, totals, liability difference). Differences are ~0.05–0.1%. Plus `main`'s 3 extra `165026` rows. |
| Error Table | `main` is a strict **subset** of the spike. Spike adds `165026` (×3, "Missing Registration Data"), `167432` ("Not Obligated"), `169063` (×3, "Not Obligated"). |

### Confirmed *not* a factor

- **Reported tonnage / POM data.** Every common producer's submitted and projected tonnage
  columns are byte-identical across the two runs.
- **"Previous invoices went missing" (the migration `20260902173358_ReplaceOrgPomStagingWithCalculatorRunOrganisation`).**
  For every common producer, "Previous Invoiced Tonnage", "Current Year Invoiced Total To
  Date" and "Suggested Billing Instruction" are identical between the runs, and both files
  show run 28's invoice for 165026 / 168946. The migration only drops the org/POM **staging**
  tables (`organisation_data`, `pom_data`, `calculator_run_organization_data_*`,
  `calculator_run_pom_data_*`), not billing / invoice history. The invoice-suppression path
  is intact.
- **The comparison tool struggling with line numbers.** The "lots of changes in Calculation
  Result" is real and fully explained: the three excluded producers shrink every aggregate
  tonnage, so every producer's percentage-of-pool (column 528) and every derived fee shifts.

## Part 2 — how `main` and the spike differ in implementation

### Pipeline mapping

| Stage | `main` (Synapse) | spike (C#) |
|---|---|---|
| Registration file selection | `fn_ProducerObligationDetermination` CTEs `larf_base` → `latest_accepted_registration_files` (`ROW_NUMBER()` dedup per org / submitter / year) | `StreamOrganisationsRequestHandler` inline SQL returns **all** candidate files → `AcceptedFileSelector.SelectLatestOrganisationFiles` (`MaxBy(CreatedDateTime)` per org / submitter / year) |
| Obligation decision tree | `fn_ProducerObligationDetermination` (`raw_obligation` → `status_inheritance` → `pivot_counts` → `decision_tree` → rules 11/12, 13/14, 16) | `ProducerObligationDeterminer` — faithful port, same rules and order |
| POM file selection | `sp_GetPaycalPomData` `latest_accepted_pom` (dedup per org / submitter / **period string**) | `StreamPomsRequestHandler` inline SQL → `AcceptedFileSelector.SelectLatestPomFiles` (per org / submitter / **period string**) |
| POM eligibility (H1+H2, registration exists) | `sp_GetPaycalPomData` `LatestAcceptedPomsWith2Period` + `Latest_Org_Data_Selection` (join on **org_id + year only**) | `PomEligibilityFilter` — org-level registration check, faithful |
| Error / warning detection | `ErrorReportService` (already C# on `main`) | split: `ProducerErrorDetector` computes every error + sets `HasPomMatch`; `ErrorReportService` applies invoice suppression + holding-company roll-up — same rules |

The two substantial pieces of logic — the obligation decision tree and the error rules —
are essentially identical between `main` and the spike. The live Synapse
`fn_ProducerObligationDetermination` / `sp_GetPaycalOrgData` / `sp_GetPaycalPomData`
definitions were pulled from the warehouse and are byte-for-byte the reference copies the
spike was ported from (only the `CREATE` header differs). `main`'s
`ErrorReportService.HandleMissingRegistrationData` already does the exact
`SubsidiaryId == … && SubmitterId == …` match the spike does. **The divergence is not in the
rules; it is that two filters `sp_GetPaycalPomData` applies *before* returning POM data have
been moved downstream of — or dropped from — the spike's error-detection stage.**

### Confirmed root cause

Verified by replaying `fn_ProducerObligationDetermination` and the `sp_GetPaycalPomData` body
against the warehouse for the three organisation IDs (see `spike-vs-main-synapse-queries.sql`).

Both filters share a consequence: `ProducerErrorDetector.Detect` runs against a POM
population that `sp_GetPaycalPomData` would never have returned, so it raises errors `main`
never sees.

#### A. The reportable-packaging-type filter moved downstream of error detection → 165026

`sp_GetPaycalPomData`'s final `SELECT` has
`WHERE (packaging_type IN ('HH','CW','PB') OR (packaging_type = 'HDC' AND packaging_material = 'GL'))`.
Stage 3 of this branch's refactor **moved that filter out of the POM query and into
`ProducerPomAligner.Align`** (`IsReportablePackaging`, applied when building the alignment
lookup — line 65).

`ProducerDataService.GetProducerDataCore` calls `errorDetector.Detect(organisations, poms)`
**before** `aligner.Align(...)`. So `HandleMissingRegistrationData` sees POM rows of every
packaging type; `Align`'s filter comes too late to matter for it.

For **165026**: the winning registration file (`49700de1`, `Granted`) contains
`CompanyDetails` rows for the parent and subsidiary **165027** only — there is no row for
subsidiary **165023**. Subsidiary 165023 *did* submit POM data, but every line item is
packaging type `NH` / `OW` / `RU` — **none reportable**. On `main`, `sp_GetPaycalPomData`
drops all of 165023's rows, so 165026's POM population is parent + 165027, both of which have
matching registrations → no error. In the spike, 165023's non-reportable POM rows survive
into `HandleMissingRegistrationData`, which finds no registration for `(165026, "165023",
submitter)` → flags the **whole** of 165026 as "Missing Registration Data" → hard error →
165026 excluded from the run → surfaces in Cancelled Producers.

Confirmed: `sp_GetPaycalPomData` returns exactly
`{165026/∅/H1, 165026/∅/H2, 165026/165027/H1, 165026/165027/H2}` for this producer — no
165023.

#### B. The POM-side registration check silently widened to include `Cancelled` → 167432, 169063

On `main` there are two different "latest accepted registration" selections with **different
regulator-status sets**:

- `fn_ProducerObligationDetermination.larf_base`: `IN ('Granted', 'Accepted', 'Cancelled')`
  — obligation determination needs `Cancelled` registrations so it can return a
  "Not Obligated" verdict.
- `sp_GetPaycalPomData.latest_accepted_registration` (feeding `Latest_Org_Data_Selection`,
  the "a registration exists for this POM" gate): `IN ('Granted', 'Accepted')` — **no
  `Cancelled`**.

The spike collapsed both onto a single organisation stream that uses the obligation set
(`'Granted','Accepted','Cancelled'`), and `PomEligibilityFilter`'s registration check is just
`registeredOrganisationIds.Contains(p.OrganisationId)` against that stream. So a producer
whose only registration is `Cancelled` now passes the POM "registration exists" gate.

For **167432** and **169063**: their only 2026 registration file is `Cancelled`
(167432 = `fc392b63`, 169063 = `85fdc5b0`). Obligation determination returns them as `E` /
"Not Obligated" on **both** sides (same decision tree, same input). Then:

- `main`: `sp_GetPaycalPomData` returns **zero** POM rows for them (no `Granted`/`Accepted`
  registration → excluded by `Latest_Org_Data_Selection`).
  `ErrorReportService.HandleObligatedErrors` keeps an `E` error only if the org has a POM row
  **or** was invoiced this FY — neither is true → error dropped → absent from the results.
- spike: their POM rows pass `PomEligibilityFilter` (org id is in the stream, H1+H2 both
  submitted and `Accepted`), so `ProducerErrorDetector.HandleObligatedErrors` computes
  `HasPomMatch = true` → the error is kept regardless of invoice history → shows in the
  Error Table.

`168946` is unaffected because it *was* invoiced in run 28, so its `E` error is kept on both
sides anyway.

### The fix (for whoever picks this up)

`ProducerErrorDetector.Detect` must be given the same POM population `sp_GetPaycalPomData`
would have returned. Options:

1. Apply the reportable-packaging-type filter (`IsReportablePackaging`) and a
   `Granted`/`Accepted`-only registration gate **before** `errorDetector.Detect`, not only in
   `ProducerPomAligner.Align` / not at all — i.e. restore the ordering
   `sp_GetPaycalPomData` had.
2. Keep a separate "POM-side accepted-registration" set (Granted/Accepted only) distinct from
   the obligation stream (which keeps Cancelled), and use it for `PomEligibilityFilter` and
   for `HandleMissingRegistrationData` / `HandleObligatedErrors`.

Also worth deciding deliberately: `HandleMissingRegistrationData` keying on
`(OrganisationId, SubsidiaryId, SubmitterId)` is inherited from `main`, but on `main` it only
ever ran against reportable-type POM rows for `Granted`/`Accepted` orgs. Widening its input
changes what it flags.

### Faithful ports (checked, not a source of divergence)

- `fn_ProducerObligationDetermination` — the live warehouse definition matches the reference;
  `ProducerObligationDeterminer`'s decision tree, status inheritance, pivot counts and rules
  11/12, 13/14, 16 all match it. All six registration rows for the three producers resolve to
  the same obligation status under both the SQL function and the C# port.
- `AcceptedFileSelector` — for these producers it selects the identical winning file as
  `larf_base` → `latest_accepted_registration_files` (no `CreatedDateTime` ties in play).
- `ProducerErrorDetector` / `ErrorReportService` error *rules* — match `main`'s
  `ErrorReportService`; only their inputs differ.

## Part 3 — TST environment (`spikeTst1` vs `mainTst1`)

The TST run is a **closer match than DEV**: the "Calculation Result", "H1/H2 Packaging
Data", LAPCAP, parameters, comms/disposal/modulation and partial-obligation sections are
**byte-identical** between `mainTst1` and `spikeTst1` (298 calculation-result rows, all
identical). Two differences remain.

### 3a. One extra error: `328328` "Not Obligated" — same cause as DEV's 167432 / 169063

`mainTst1` error table: `326845` ("No longer trading"). `spikeTst1`: `326845` **plus
`328328`** ("Not Obligated"). No knock-on effect (328328 was never in the calculation on
either side, and was not previously invoiced, so nothing downstream moves).

Confirmed against the TST warehouse: `328328`'s only 2026 registration file is `Cancelled`,
`fn_ProducerObligationDetermination` returns it as `E` / "Not Obligated", and it has
`Accepted` H1 + H2 POM. This is **exactly divergence B** from Part 2 — `main`'s
`sp_GetPaycalPomData` drops its POM (no `Granted`/`Accepted` registration), so the error is
suppressed; the spike keeps the POM, sets `HasPomMatch = true`, and shows the error. **The
Part 2 fix covers this.**

### 3b. "Cancelled Producers" section is empty in the spike (main has ~88) — migration side effect

`mainTst1` lists ~88 cancelled producers (invoiced in runs 28 `inlinefunction12` and 37
`securitytest4`, now absent from the current run). `spikeTst1`'s section is **completely
empty**. This is **not** related to Parts 1–2, and the Part 2 fix does not touch it.

**Cause:** `CalcResultCancelledProducersBuilder` builds the section from
`InvoicedProducerService.GetInvoicedProducers(...)`, which goes through
`GetInvoicedProducerProjection()` — and that has an **inner join** to
`GetPreferredOrgDetailsProjection()` for each invoiced producer's name / trading name.

This branch repointed `GetPreferredOrgDetailsProjection()` from the old per-run staging table
`calculator_run_organization_data_detail` to the **new `calculator_run_organisation`** table
created by migration `20260902173358_ReplaceOrgPomStagingWithCalculatorRunOrganisation`:

```
- var eligible = dbContext.CalculatorRunOrganisationDataDetails.Where(d => string.IsNullOrEmpty(d.SubsidiaryId));
+ var eligible = dbContext.CalculatorRunOrganisations.Where(o => string.IsNullOrEmpty(o.SubsidiaryId));
```

The migration's `Up()` **creates `calculator_run_organisation` empty** — it drops
`calculator_run_organization_data_detail` (and `_master`, `organisation_data`, etc.) without
copying any data across. So after the migration, `calculator_run_organisation` only has rows
for runs executed **on this branch, after the migration**. Every historical run's org data
is gone.

The ~88 cancelled producers are, by definition, not in the current run, and there is no
earlier spike-branch run either — so none of them has a `calculator_run_organisation` row →
the inner join in `GetInvoicedProducerProjection()` drops every one of them →
`GetInvoicedProducers(...)` returns nothing → the builder's loop produces an empty section.

On `main`, `GetPreferredOrgDetailsProjection()` still reads
`calculator_run_organization_data_detail`, which every historical run populated, so the join
succeeds and the section is full.

The invoice / billing tables themselves (`producer_result_file_suggested_billing_instruction`,
`producer_invoiced_material_net_tonnage`, `producer_designated_run_invoice_instruction`) are
intact — `GetInvoicedProducerIdsForYear()` (no org join) still returns the right ~88 IDs.
Only the name/trading-name enrichment join fails.

Fix: see Part 4.

**To confirm** (calculator API database, not Synapse): `SELECT COUNT(*) FROM
calculator_run_organisation` and `SELECT DISTINCT calculator_run_id FROM
calculator_run_organisation` — expect only the `spikeTst1` run id.

## Part 4 — fixes

### 4a. Error-table fix (DEV divergences A + B, TST `328328`)

`ProducerDataService.GetProducerDataCore` must hand `ProducerErrorDetector.Detect` (and
`ProducerPomAligner.Align`) the same POM population `sp_GetPaycalPomData` produced. Two
changes, both in that method:

1. **Reportable-packaging filter, before error detection.** Apply the `HH`/`CW`/`PB` /
   `HDC`+`GL` filter to the eligible POM list *before* `errorDetector.Detect`, not only
   inside `ProducerPomAligner.Align`. The rule was extracted to
   `Alignment/ReportablePackaging.cs` and is now used in both places.
2. **Cancelled registrations excluded from the POM-eligibility gate.** `PomEligibilityFilter`
   was being passed *every* organisation id in the run's stream — including organisations
   whose only registration is `Cancelled` (the obligation stream has to keep those).
   `sp_GetPaycalPomData.latest_accepted_registration` uses `IN ('Granted','Accepted')` only.
   The id set passed to `pomEligibilityFilter.Filter` now excludes `RegulatorStatus =
   'Cancelled'`; the org stream only ever carries `Granted`/`Accepted`/`Cancelled`, so that is
   equivalent to the stored proc's positive filter.

Known limitation of (2): if an organisation's *latest* registration file is `Cancelled` but
an earlier one was `Granted`/`Accepted`, `sp_GetPaycalPomData` (which filters to
Granted/Accepted *then* takes the latest) would still include it, whereas
`AcceptedFileSelector` picks the latest file overall and this filter then drops it. None of
the observed producers hit this; flagged for the eventual full reconciliation.

### 4b. Migration data-loss audit — `20260902173358_ReplaceOrgPomStagingWithCalculatorRunOrganisation`

The migration `Up()` drops six tables and two `calculator_run` columns and adds
`calculator_run_organisation` + `calculator_run.org_pom_data_loaded_at`, **with no data copy**.
Everything the running service needs from the dropped tables for *historical* runs is
therefore gone. Three concrete gaps:

| Dropped | Replacement | Backfilled? | What breaks for pre-migration runs |
|---|---|---|---|
| `calculator_run_organization_data_detail` (per-run org rows) | `calculator_run_organisation` | **No** | `InvoicedProducerService.GetPreferredOrgDetailsProjection` (→ empty "Cancelled Producers", missing producer names on billing files via the inner join in `GetInvoicedProducerProjection`); `BillingFileService`'s "parent producers" query and its "previous run" name fallback (both now `where org.CalculatorRunId == runId` / `join calculator_run_organisation`); any results-file re-export of an old run. |
| `calculator_run.calculator_run_organization_data_master_id` / `_pom_data_master_id` | `calculator_run.org_pom_data_loaded_at` (nullable) | **No** | `BillingRunContextValidator` now requires `OrgPomDataLoadedAt` to be non-null → **a billing run cannot be created from any pre-migration calculator run** ("Run is missing organisation/POM data"). `CalcResultDetailBuilder` shows a blank "RPD File - ORG/POM" date. |
| new `producer_detail` columns `obligation_status` / `num_days_obligated` / `status_code` / `submitter_id` / `joiner_date` / `leaver_date` | (same columns, now the source of truth) | **No** (additive, so `NULL` for old runs) | `CalcResultPartialObligationBuilder` filters `pd.ObligationStatus == Obligated && pd.DaysObligated != null` — an old run's partial-obligation section re-exports empty. |
| `calculator_run_pom_data_detail` / `_master`, `organisation_data`, `pom_data` | none (POM now lands in `producer_reported_material` per run, plus live Synapse re-stream) | n/a | `producer_reported_material` already carried forward per run and is untouched by the migration, so historical reportable-POM data survives. The raw/unaligned POM snapshot is not kept — acceptable *if* nothing needs to re-derive alignment for an old run. The `organisation_data` / `pom_data` staging tables were transient (populated then consumed per run), so no history is lost there. |

**Fix applied to the migration** (`Up()` reordered so the copies run before the drops):

1. `AddColumn org_pom_data_loaded_at`, then
   `UPDATE calculator_run SET org_pom_data_loaded_at = m.created_at FROM ... calculator_run_organization_data_master m WHERE m.id = calculator_run_organization_data_master_id`.
2. `CreateTable calculator_run_organisation`, then
   `INSERT INTO calculator_run_organisation (...) SELECT ... FROM calculator_run_organization_data_detail d JOIN calculator_run r ON r.calculator_run_organization_data_master_id = d.calculator_run_organization_data_master_id`.
3. `UPDATE producer_detail` — backfill the six new obligation columns from the matching
   `calculator_run_organization_data_detail` row (run + organisation + subsidiary), for rows where
   `obligation_status IS NULL`.
4. Only then the `DropForeignKey` / `DropTable` / `DropColumn` / `DropIndex` block.

`calculator_run_organisation.id` is identity — the copied rows get new ids, so
`BillingFileService`'s `orderby org.Id descending` fallback preserves per-run insert order
but not the exact old `odd.Id` sequence across runs. It is a name-lookup tiebreak only.

**This only fixes environments the migration has not yet run on** (PROD). On DEV and TST the
source tables are already dropped and EF will not re-run the migration — those environments
need the data restored from a backup taken before the migration, or the historical calculator
runs re-executed on this branch, before a like-for-like comparison is meaningful.
