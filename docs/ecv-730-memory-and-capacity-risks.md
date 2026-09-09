# ECV-730 — compute requirements for the calculator service

**Branch:** `ECV-730-NF`
**Date:** 2026-09-09
**Purpose:** state the compute the calculator service needs after ECV-730 and
why, so its App Service plan can be sized deliberately rather than by default.

## Summary

ECV-730 makes three intentional changes to where the calculator's work runs:

1. The PayCal organisation/POM selection logic (`sp_GetPaycalOrgData`,
   `sp_GetPaycalPomData`, `fn_ProducerObligationDetermination`) moves out of
   Synapse into the service. The filtering Synapse used to do now happens in
   process.
2. Work that previously ran in a Function App consolidates into the calculator
   web app.
3. `DataApi` becomes a standalone, streamed data service intended to serve other
   clients over time, not only this calculator.

The capacity consequence follows directly from (1): during a run the service now
streams and filters the full accepted-file dataset (~2M POM rows) in memory,
where Synapse previously returned ~50k pre-filtered rows. A run's working set is
therefore materially higher than before the change — this is expected, not a
regression.

**Measured at 2025 volume, after the fixes on this branch, a full run plus its
three file downloads has a peak working set of ~1.2 GB and completes cleanly
under a 2 GB heap limit (five-run soak, no OOM, end-of-run managed heap 0.15 GB);
under a 1 GB limit it is unchanged (peak still ~1.2 GB) — the peak is the
calculator run itself, `StoreProducerFees` retaining ~1.25 GB, not the downloads,
which run at ~0.74–1.05 GB working set. The footprint is manageable. The
problem is that in both production and pre-prod the calculator web app shares one
8 GB P1v3 instance with five other applications, where any one app's realistic
burst budget is ~2–4 GB and a neighbour can spike at any time. The service needs
a dedicated App Service plan, or a move to the larger P2v3 plan, so a run can
reserve what it needs without contending.**

Code work for the new volumes on this branch, in the order it landed and moved
the number:

1. The fee/SMCW writes are batched so they no longer exhaust the EF change
   tracker — this is what threw `OutOfMemoryException` at 2–3 GB.
2. The billing-JSON download serializes straight to the response instead of
   building the whole JSON document — it was the OOM-at-2 GB point (~3 GB peak).
3. All three downloads read the producer fee rows off the reader as a deferred
   sequence instead of materialising the whole `ProducerFees` graph (~1 GB).
4. The DataApi picks the winning POM file from a first metadata-only pass so it
   never buffers the full raw stream.

Together (1)–(4) took the run from OOM-at-2 GB to a clean 1 GB soak. Two later
changes are refinements on top, byte-identical and with no material effect on the
whole-run peak: making the CSV downloads stream end to end (no assembled output
string, no `ResetTotals` string surgery), and interleaving the reader with the
exporter so `GetResult` stops over-reading the large per-producer sections
(transient allocation ~780 MB → ~260 MB, ~0.1 GB off working set at points in the
download sequence). What still dominates a download — ~0.8 GB, transient — is the
H1/H2 projected reads pulling ~25× more rows than the grouped result; reducing it
needs the DB to do the per-producer aggregation, and is 2 GB-safe as it stands.
The remaining structural cost on the calc side is the fee builder (~23.6 GB
churn); see [Follow-up code work](#follow-up-code-work).

## Current hosting

| | Calculator web app | Function app being consolidated |
|---|---|---|
| App Service plan | `PRERWDWEBAS2403` / `PRDRWDWEBAS1403` | `PRERWDWEBAS2405` |
| SKU | **P1v3** — 8 GB / 2 vCPU per instance | P2v3 — 16 GB / 4 vCPU per instance |
| Instances | 2 (scale-out; memory is per-instance, not pooled) | 2 |
| Apps sharing each instance | **6** | 12 |

Production and pre-prod are sized identically: the calculator web app
`PRDRWDWEBWA1422` runs on plan `PRDRWDWEBAS1403` (P1v3, 2 instances, 6 apps, no
slots) in `PRDRWDWEBRG1401`, the same shape as pre-prod's `PRERWDWEBWA2422` on
`PRERWDWEBAS2403`.

On Linux App Service every app on a plan runs on every instance of that plan and
**shares that instance's RAM** — there is no per-app memory isolation. After
~1–1.5 GB of platform overhead, six apps share ~6.5 GB per instance, so any one
app's realistic burst budget is ~2–4 GB and lower when a neighbour is active.
When an instance is under memory pressure the platform kills processes on it —
which can be the calculator mid-run or an unrelated neighbour.

Calculator runs are triggered via the Service Bus queue
`defra.epr.calculator.run` and processed in-process by the BackgroundService
hosted service — inside the calculator web app's container, within that shared
8 GB.

## Measured

`CalculatorRunPerformanceTests`, on macOS, with `DOTNET_GCHeapHardLimit` used to
make the local GC behave like a memory-constrained container:

```sh
# hard heap limit in bytes, hex: 2 GB = 0x80000000, 3 GB = 0xc0000000, 4 GB = 0x100000000
DOTNET_GCHeapHardLimit=0xc0000000 dotnet run --project src/EPR.Calculator.API.IntegrationTests -- --performance
```

The generated dataset is now calibrated to a real 2025-26 PRE2 run
(`pre2test2.csv`, 6,188 producer + subsidiary rows in the results file): the
generator emits **7,000 producer rows** and, with 14 file versions per
submission modelling resubmissions, a **~2.11M-row raw POM stream** — matching
the warehouse's 2025 `raw_stream_rows` of 2,014,238. Earlier figures in this
document were taken at ~1.7× that volume (10,587 producers, 89k organisation
rows) before the generator was calibrated; where a stage number has been
re-measured at the corrected volume it is called out.

Figures below are **after** the memory work on this branch: all three downloads
read the producer fee rows off the reader as a deferred sequence and write their
output to the HTTP response
(see [Producer-fee read](#producer-fee-read--streamed-on-this-branch)); the perf
test's fake POM stream is read field-by-field, not as `dynamic`; and the DataApi
picks the winning POM file in a first pass over just the file metadata, so it
never buffers the full raw stream
(see [the DataApi POM buffer](#the-dataapi-pom-buffer--reduced-on-this-branch)).

| | value |
|---|---|
| DataApi `GetProducerData` (1 GB heap limit) | completes — ~10 s, ~9.7 GB allocation churn (two source reads), heap settles ~0.6 GB; returns 7,000 organisations / 6,638 producers / 2 errors |
| Full run, unconstrained GC | ~63 s; ~56 GB allocation churn |
| Full run, 2 GB heap limit | **five-run soak, no OOM.** End-of-run managed heap **0.15 GB**. Whole-test peak working set **1.23 GB** (62 % of the 2 GB ceiling). |
| Full run, 1 GB heap limit | **five-run soak, no OOM** on macOS. Peak working set **1.21 GB** (121 % of the 1 GB ceiling — over by working-set accounting, tolerated on the soft limit). Near-identical to the 2 GB soak because the peak is the calculator run, not a ceiling-driven collection: `StoreProducerFees` retains ~1.25 GB and `ProducerFeesBuilder` churns 23.6 GB. The three downloads run at ~0.74–1.05 GB working set each and end-of-run managed heap is 0.15 GB. A **hard** 1 GB limit would OOM only in the calc, not in any download or in the DataApi load (which used to fail here outright and now peaks ~0.6 GB). |

Measured against the parent commit (before the CSV downloads were made to stream
end to end) the 1 GB soak is unchanged: peak working set 1.18 GB → 1.21 GB,
end-of-run heap 0.15 GB either way, download working sets within ~0.05 GB. That
change removed the assembled ~45 MB output string and the `ResetTotals` /
`ToUtf8WithBom` copies — worth having for correctness, but the download-side
memory reduction had already landed in the billing-JSON and fee-row streaming
commits before it.

Averaged over the five soak runs (either ceiling): calc ~63 s, results-CSV export
~9.0 s, billing run ~0.5 s, billing-CSV export ~8.7 s, billing-JSON export ~8.6 s.
Per-run allocation rose from ~51 GB to ~56 GB — the DataApi two-pass reads the
source twice.

Before the billing-JSON streaming change the same 2 GB soak OOM'd in the
billing-JSON export on run 1 (at ~3.18 GB against a 4 GB ceiling it fit only on
macOS's soft limit; at 3 GB it was marginal; at 2 GB it failed). Streaming
removed ~2 GB of transient + retained memory from that export and dropped the
end-of-run managed heap from 2.64 GB to 0.15 GB.

### Allocation by stage (4 GB heap limit, per-stage telemetry, calibrated volume)

Per-stage `allocated` is process-wide managed churn while the stage ran, not
retained bytes; `heap after` is the approximate live managed heap when it
finished. Figures are run 1 of the five-run soak; the other four were within
~1 % of every line.

**DataApi — `ProducerDataService.GetProducerData` / `CommonDataApiLoader.LoadDataCore`** (~9.7 GB churn, 10 s, heap settles ~0.6 GB)

| sub-stage | allocated | time | heap after |
|---|---|---|---|
| `BufferOrganisationStream` | 27 MB | 0.1 s | ~140 MB |
| `SelectLatestOrganisationFiles` | 16 MB | 0.04 s | ~150 MB |
| `SelectPomFiles` (pass 1 — pick winning files from metadata) | **4.9 GB churn** | 4.5 s | ~0.24 GB |
| `BufferPomStream` (pass 2 — buffer only the winning files' rows) | **4.75 GB churn** | 4.5 s | ~0.4 GB |
| `PomEligibilityFilter` / `ApplyPeriodFlags` | ~6 MB each | <0.1 s | ~0.4 GB |
| `ProducerPomAligner.Align` | 30 MB | 0.05 s | ~0.4 GB |
| `ProducerDataTransposer.Transpose` | 300 MB | 1.3 s | ~0.6 GB |

The DataApi no longer buffers the full raw POM stream. Pass 1 streams every row
but retains only one `PomFileCandidate` (file metadata) per distinct file — a few
hundred MB — from which `AcceptedFileSelector.SelectWinningPomFileNames` picks the
winning file per org/submitter/period. Pass 2 re-streams and buffers only rows
belonging to a winning file (the de-duplicated set, ~0.4 GB). The two passes cost
a second read of the source (~5 GB extra churn per run in the test; a second SQL
query in production). See
[the DataApi POM buffer](#the-dataapi-pom-buffer--reduced-on-this-branch).

**Build — `ResultBuilder.BuildAsync`** (~46 GB, 52 s, heap ~1.25 GB at end)

| stage | allocated | time |
|---|---|---|
| `ProducerFeesBuilder.ConstructAsync` | **23.6 GB** | 11 s |
| `CalcResultProjectedProducersBuilder.Construct` | 475 MB | 0.5 s |
| `SelfManagedConsumerWasteService.Calculate` | 222 MB | 0.5 s |
| `ReportedProducerService.GetProducers` | 156 MB | 0.4 s |
| everything else combined | < 100 MB | |

**Write — `CalcResultWriter.*`** (persisted inline during build)

| stage | allocated | time |
|---|---|---|
| `StoreProducerFees` | 15.7 GB | 28 s |
| `StoreSmcw` | 3.4 GB | 7.5 s |
| `StoreProjectedH1Data` / `StoreProjectedH2Data` | 1.3 / 1.2 GB | ~1.8 s each |
| `StoreProducerMaterialPackaging` | 113 MB | 0.5 s |
| `StoreCommsCost` / `StoreModulationResult` / `StoreLaDisposalCostData` | 7–45 MB each | 0.03–0.5 s |
| `StorePartialData` and the five small stores | 0–10 MB each | |

The batched-write change on this branch bounds the *tracked-entity* count (which
is what threw `OutOfMemoryException` at 2–3 GB); it does not reduce
`StoreProducerFees`' churn — the fee builder and the streamed read are separate
tickets.

**Read / export — `CalcResultReader.*` + exporters**, three downloads per calc run

Each download first calls `GetResult`, which reads a whole `CalcResult` graph
(projected producers, scaled producers, SMCW, LA disposal, comms cost, …) into
memory — `ReadH1ProjectedData` / `ReadH2ProjectedData` / `ReadSmcw` are ~250 MB
each — then runs ~15 sub-exporters over it. The producer fee rows, the one part
that scales with producer count, are read with `StreamProducerFeeDetails` — a
deferred `IEnumerable` consumed once — instead of the whole `ProducerFees` graph.

| stage | allocated (churn) | working set while running | time |
|---|---|---|---|
| **results CSV download** | ~1.95 GB | ~1.0–1.05 GB | ~9 s |
| **billing CSV download** | ~1.94 GB | ~0.74 GB | ~8.7 s |
| **billing JSON download** | ~0.8 GB | ~0.83 GB | ~2.3 s |

The CSV downloads churn ~2 GB each — the fee-row stream itself is genuinely
streamed and adds ~0 to the retained heap, so the churn is the H1/H2 projected
reads plus per-cell string building in `CsvSanitiser`. `CalcResultsExporter` /
`BillingFileExporter` flush a small reused `StringBuilder` to the response
`TextWriter` after every producer row, so neither the ~45 MB output nor the fee
graph is held whole. A forced full GC between downloads leaves ~0.6 GB of the
projected-read churn live for a run or two, but nothing accumulates — the
end-of-run managed heap is 0.15 GB. `AsSplitQuery()` on `ReadProducerFees` (now
unused by the download path, kept for callers) took that read from ~21 s to ~8 s
at the calibrated volume.

`GetResult` no longer reads the two large per-producer sections (H1/H2 projected,
scaled-up) up front — a deferred loader hands each to its sub-exporter and it is
released before the fee stream. This cut `GetResult`'s transient allocation from
~780 MB to ~260 MB and ~0.1 GB off working set at several points, though not the
whole-run peak (the projected reads still dominate a download, just later); see
[Reader and exporter interleaving](#reader-and-exporter-interleaving--done-on-this-branch-modest-effect).

**Whole calc run** `CalculatorRunProcessor.Process`: ~56 GB churn, ~1 min 2 s,
retained heap ~1.26 GB — this is now the whole test's peak. Finalize (4,201
billing instructions + 33,608 invoiced net tonnages) is ~120 MB; the billing run
proper is 1–7 MB.

The producer-fee data dominates on all of build, write and read.

### Heap consistency and test retention

At the calibrated volume the calc pipeline is bit-for-bit reproducible run to
run: ~56 GB churn per run, fee builder ~23.6 GB every time, retained heap
~1.26 GB at the end of each calc. Nothing in the pipeline leaks or grows across
the five-run soak, and the **end-of-run managed heap is 0.15 GB** — whatever a
download holds is released within a run or two, not accumulated.

Each download runs in its own DI scope (a forced blocking gen-2 collection
between sections — `CollectAndMeasure` in `CalculatorRunPerformanceTests` —
models production's separate, time-separated requests). Their working sets while
running: results CSV ~1.0–1.05 GB, billing CSV ~0.74 GB, billing JSON ~0.83 GB.
None is close to the 2 GB ceiling; the churn figures in the table above are
uncollected allocation, not retained output. A full GC after a download still
leaves ~0.6 GB live for a run or two — the H1/H2 projected read churn (see
[Reader and exporter interleaving](#reader-and-exporter-interleaving--done-on-this-branch-modest-effect))
— but the end-of-run managed heap is 0.15 GB, so nothing accumulates.

Test hygiene also addressed on this branch so the soak measures the service, not
the harness: every controller call now goes through `CallController<T>`, which
builds and disposes a fresh DI scope per call; the reused `ApplicationDBContext`
gets a `ChangeTracker.Clear()` each iteration; and the fake POM stream is read
field-by-field off the `CsvReader` (one `PayCalPom` per row, no per-row dynamic
object) rather than `GetRecords<dynamic>`.

### The DataApi POM buffer — reduced on this branch

`ProducerDataService` used to buffer the **entire raw POM stream** into a
`List<PayCalPom>` before `SelectLatestPomFiles` de-duplicated it — at 2025 volume
~2.1M rows (every resubmission file version), ~2.5 GB retained during
`BufferPomStream` and ~2.8 GB through `Transpose`. That was the calc run's memory
floor: a 1 GB limit failed there outright, and a 2 GB run only survived on macOS's
soft limit. It is not a harness artifact — production reads the same rows off a
SQL reader into the same list.

It now takes **two passes**:

1. `SelectPomFiles` streams every row but keeps only one `PomFileCandidate`
   (`OrganisationId`, `SubmitterId`, `SubmissionPeriod`, `FileName`,
   `IsResubmission`, `CreatedDateTime`) per distinct file — a few hundred MB.
   `AcceptedFileSelector.SelectWinningPomFileNames` then picks the winning file
   name per group (same cut-off / resubmission-fallback rule as before).
2. `BufferPomStream` re-streams the source and buffers only rows whose file won
   its group — the de-duplicated set, ~0.4 GB.

Retained heap through the DataApi load dropped from ~2.8 GB to ~0.6 GB. Output is
byte-identical (golden-file integration test). Cost: a second read of the source
— ~5 GB extra churn per run in the perf test, a second SQL query in production
(the existing one already carries a 10-minute timeout for "poor db performance",
so this is the notable trade-off).

Reading:

- **The DataApi load no longer sets the floor.** It peaks ~0.6 GB retained; a
  1 GB soak completes on macOS (working set 1.21 GB).
- **The downloads run at ~0.67–1.05 GB working set** and leave no residue
  (end-of-run managed heap 0.15 GB). `GetResult` and the exporters now interleave,
  and the fee-row stream is confirmed genuinely streaming (adds ~0). What remains
  is the H1/H2 projected reads — ~0.8 GB, of which ~0.6 GB is read churn a GC
  won't reclaim for ~a minute, because the query pulls ~25× more rows than the
  grouped result. Transient, not a ceiling risk at 2 GB; see
  [Reader and exporter interleaving](#reader-and-exporter-interleaving--done-on-this-branch-modest-effect).
- **The remaining managed-heap peak is the calculator run** — `StoreProducerFees`
  retains ~1.25 GB and `ProducerFeesBuilder` churns 23.6 GB. Fine at 2 GB, over a
  hard 1 GB.
- **The fee builder is the structural cost that remains** (~23.6 GB churn, see
  below), and its per-producer cost rises faster than linearly — a future year
  with more obligated producers needs its own measurement.

## Compute requirement

The service-side footprint of a run at 2025 volume, after the memory work on this
branch, is modest: the three downloads run at ~0.74–1.05 GB working set and leave
no residue; the DataApi load peaks ~0.6 GB; the highest retained point is
`StoreProducerFees` at ~1.25 GB. The whole-run peak working set is ~1.2 GB under
both a 1 GB and a 2 GB ceiling. The problem is not the absolute number — it is
that whatever a run needs has to be *reserved*, repeatedly, on an instance the
calculator does not have to itself.

1. **A calculator run should not share an instance with other apps.** On the
   P1v3 plan, after platform overhead six apps share ~6.5 GB per instance and any
   one app's realistic burst budget is ~2–4 GB — and a neighbour can spike into
   it at any time. Move the calculator web app to its own App Service plan, or
   onto the P2v3 plan (16 GB / 4 vCPU) as the Function App consolidation frees
   room there. On a dedicated plan the run has the whole instance and the
   platform will not kill a neighbour (or be killed by one).
2. **Size for a run, not for idle.** The steady-state API footprint is small; the
   requirement is driven by the periodic calc run. At 2025 volume the run peaks
   at ~1.2–1.3 GB, set by `StoreProducerFees`; target headroom of at least 2×
   that on an instance the calculator does not contend for. The fee-builder work
   would bring it down further.
3. **Confirm run concurrency is one per instance.** Runs are queue-driven and
   assumed serial, but this is not enforced; two concurrent runs on one instance
   would double the transient memory.

Explicitly **not** proposed: setting `DOTNET_GCHeapHardLimit` on the app as a
production mitigation. That only makes the process fail sooner under a limit it
should not be sharing in the first place. It is used here only as a test tool.

## Follow-up code work

### DataApi raw-POM buffer — done on this branch

`ProducerDataService.StreamPoms` used to read the whole raw POM stream into a
`List<PayCalPom>` (~2.1M rows / ~2.5 GB at 2025 volume, ~90 % superseded
resubmission versions) before de-duplicating it. It now does two passes:
`SelectPomFiles` retains only one `PomFileCandidate` per distinct file and
`AcceptedFileSelector.SelectWinningPomFileNames` picks the winners; then
`BufferPomStream` re-streams and buffers only the winning files' rows. Retained
heap through the DataApi load fell from ~2.8 GB to ~0.6 GB, output is
byte-identical, and the cost is a second read of the source (a second SQL query
in production; ~5 GB extra churn per run in the perf test).

### Producer-fee builder — biggest single cost, own ticket

`ProducerFeesBuilder.ConstructAsync` allocates **~23.6 GB** building the fee
objects for ~6,638 producers at 2025 volume (~3.5 MB per producer) — still more
than the rest of the run combined. The builder already has O(1)-lookup
optimisations at the top level, so the cost is likely inside the per-producer
`ProducerRowBuilder.GetProducerRow` and/or the eight `*Producer.SetValues` passes
over `result.Details`. Needs profiling — this is where the run's memory and a
good part of its time go, and the per-producer cost rises faster than linearly
with producer count (it was ~5.7 MB/producer at the over-scaled volume).

### Producer-fee write — done on this branch

`CalcResultWriter.StoreProducerFees` used `dbContext.ProducerDisposalFee.Add(...)`
+ `SaveChangesAsync`. `calc_result_producer_fee_detail` is one row per obligated
producer (~6,638 at 2025 volume) with a JSON `detail` column holding a deeply
nested owned-entity graph (`FeeDetail` → eight cost sections plus `OwnsMany
MaterialFees` → nested tonnage objects). Through the change tracker, `.Add` +
`SaveChanges` created an EF entry per nested node per row — of order a million —
which threw `OutOfMemoryException` under 3 GB.

Now: the parent row goes through the tracker, then the detail rows are inserted
through `SaveChanges` in batches of 500 with `ChangeTracker.Clear()` between each,
bounding the tracked-entity count. `BulkInsertAsync` was tried first but
EFCore.BulkExtensions' `SqlBulkCopy` path on SQL Server can't serialize the
owned-JSON `detail` column (it worked on the SQLite unit test, which uses a
different path — so the unit test is not a guarantee for SQL Server; the
integration perf run is).

`StoreProducerFees` and `StoreSmcw` (see below) both have the shape "one root
row, a large child collection, owned-JSON on the children", so this is a single
`SaveWithBatchedChildren` helper the two delegate to — no reflection, just three
delegates (get children, set children, set parent) passed per call.

### Producer-fee read — streamed on this branch

`CalcResultReader.ReadProducerFees` loaded the whole `ProducerFees` object with
all detail rows and their JSON graphs in one go — ~8 s and ~2.67 GB of
allocation, retaining ~1 GB — three times per calc run. `GetResult` and the
three downloads now use `CalcResultReader.StreamProducerFeeDetails`, which
returns the per-producer `FeeDetail` rows as a **deferred `IEnumerable` read
straight off the reader**, consumed once. `GetResult` no longer materialises
`ProducerFees.Details` at all.

- **Billing JSON** (`02c296c`): `CalculationResultsJson` projects the rows lazily
  and `BillingFileJsonWriter` serializes with `JsonSerializer.SerializeAsync` to
  the response via a `StreamCallbackResult` — no assembled JSON document. This
  was the OOM-at-2 GB point: ~3.18 GB peak → **~0.83 GB working set, ~0.8 GB
  churn, ~2.3 s**.
- **Results CSV / billing CSV** (`03304dd`): `ProducerFeesExporter` and the
  summary part-exporters take the streamed enumerable instead of the fee graph.

`ReadProducerFees` is kept for other callers but is unused by the download path.
Output byte-identical (golden-file integration test).

### CSV download end-to-end streaming — cleanup, no measured memory delta

A follow-up (`c5ed1ec`) made the results-CSV and billing-CSV downloads stream to
the response the way billing JSON already did: `CalcResultsExporter` /
`BillingFileExporter` take a `TextWriter`, build the small fixed report tables
into a local `StringBuilder`, flush, then hand the writer to `ProducerFeesExporter`
which flushes a reused `StringBuilder` after every producer row. The assembled
`StringBuilder` → `string` → BOM-prefixed `byte[]` for the ~45 MB file is gone,
`BillingFileExporter.ResetTotals`' `LastIndexOf("Totals")` string surgery is
replaced by emitting the total row's identity columns directly, and
`FileExportService.ToUtf8WithBom`'s second `byte[]` copy is gone (the
`StreamWriter` emits the BOM).

Measured before/after at a 1 GB ceiling: **no change** — peak working set
1.18 GB → 1.21 GB, end-of-run heap 0.15 GB either way, per-download working sets
within ~0.05 GB. The ~45 MB assembled string was never a material fraction of a
download's footprint (the H1/H2 projected reads are). Worth having for
correctness and for headroom as volumes grow; not a memory win in itself.

### Reader and exporter interleaving — done on this branch, modest effect

`FileExportService.GetResult` used to read the whole `CalcResult` graph — every
section a download needs — and hold all of it for the export's duration. Of that
graph only two sections scale with producer count: the H1/H2 projected producers
(`ReadH1ProjectedData` / `ReadH2ProjectedData`) and the scaled-up producers
(`ReadScaledData`). Everything else — `CalcResultLapcapData`,
`LateReportingTonnage`, `ParameterOtherCost`, `OnePlusFourApportionment`,
`CommsCost`, `LaDisposalCostData`, `ModulationResult`, `SelfManagedConsumerWaste`,
partial obligations (~3 MB), cancelled producers (~0) — is material-keyed or
small.

`GetResult` now skips those two sections; `ProducerReportSections` carries a
deferred loader for each, which `CalcResultsExporter` / `BillingFileExporter`
call immediately before the projected / scaled-up sub-exporter and let fall out
of scope straight after, before the ~9 s fee-row stream. The billing
accepted-producer filter (previously all in `FilterResult`) is baked into the
billing loaders; the billing JSON path, which needs scaled-up whole, pulls that
one section in via `filteredResult with { … }`.

Measured at 2025 volume, 1 GB soak, against the parent commit:

| | before | after |
|---|---|---|
| `GetResult` transient allocation | ~780 MB | ~260 MB |
| billing-CSV download working set | ~0.74 GB | ~0.67 GB |
| billing-JSON download working set | ~0.87 GB | ~0.82 GB |
| calc-run entry working set (post-download) | ~0.77 GB | ~0.65 GB |
| **whole-test peak working set** | **1.18 GB** | **1.21 GB** |
| end-of-run managed heap | 0.15 GB | 0.15 GB |

Output byte-identical (golden-file integration test, both fixtures). The
whole-run peak is unchanged because it is the calculator run
([`StoreProducerFees`](#producer-fee-builder--biggest-single-cost-own-ticket)),
not a download.

Forced-GC probes through a 2026 results-CSV download locate the download's
footprint precisely:

| point (live heap after two compacting gen-2 GCs) | |
|---|---|
| after `GetResult` | ~0.2 GB |
| after `ReadH1ProjectedData` + `ReadH2ProjectedData` | **~1.02 GB** |
| after the projected lists leave scope | ~0.82 GB |
| after the fee-row stream (6,600 rows) | ~0.82 GB **(+0)** |

- **The fee-row stream is genuinely streaming** — `StreamProducerFeeDetails`
  (deferred `IEnumerable` off the reader) adds nothing measurable over 6,600
  rows. No further work needed there.
- **The projected reads add ~0.8 GB.** ~0.2 GB of that is the retained grouped
  objects (~6,000 producers × a 13-material tonnage map, every field used by the
  exporter — about as small as the current model allows). The other **~0.6 GB is
  read churn** — SqlClient / EF shaper buffers — that a compacting GC does not
  reclaim for ~a minute. It is transient (gone by the next operation, end-of-run
  managed heap 0.15 GB) and safe at any ceiling ≥ ~1.5 GB, but under a hard 1 GB
  it is the wall: the heap is at ~1 GB before the fee stream even starts.

The cause is that `ReadH1ProjectedData` / `ReadH2ProjectedData` / `ReadScaledData`
read `TransformProjectedH1` / `H2` / `TransformScaled` — one row per
producer × subsidiary × period × level × **material**, ~150K rows of 58 columns
for H1+H2 combined — then `GroupBy` + `MapTo…MaterialTonnages` collapse the
~13 material rows per producer into one object, ~6,000 out. EF pulls ~25× more
rows off the wire than the result shape. Interleaving does not change that; it
only moved the read out of `GetResult` and into the callback and released the
retained lists sooner. Streaming the reader (`AsAsyncEnumerable`) would not help
either — the projected-producers exporter is two-pass (it builds a
"producers with complete RAM tonnage" set across all H1+H2 rows, then emits the
complement), so its input is re-buffered regardless.

**Not yet done — the real lever:** push the per-producer material aggregation
into the database (a keyless-entity query or view returning ~6,000 pre-pivoted
rows) so SqlClient pulls ~25× less. That is a DB-side change — new view,
migration, rewrite of the three `Read…ProjectedData` / `ReadScaledData` methods,
and a byte-identical check on the projected CSV/JSON output. Since the churn is
transient and 2 GB-safe, it ranks below the fee builder; it is the download
side's remaining structural cost.

### `StoreSmcw` — done on this branch

`CalcResultWriter.StoreSmcw` allocated ~3.4 GB (7.5 s at 2025 volume) via the
same `.Add` + `SaveChanges` pattern on a smaller graph, and now goes through the
same `SaveWithBatchedChildren` helper as the fee write. Its churn is unchanged
(that is the read/build side); the batching is what keeps the tracked-entity
count bounded.

### Minor — done on this branch

`ProducerErrorDetector.HandleMissingRegistrationData` / `HasPomMatch` were
O(orgs × POM-groups) with per-row allocation; rewritten to use hash-set lookups.
Noted because the ported selection/alignment code was written against Synapse's
pre-filtered ~50k rows — worth a pass for other spots with the same shape.

### Considered and not pursued — explicit memory release

Manually nulling / clearing the large in-memory collections (the DataApi row
lists, `ProducerCalculationData`) once consumed was considered and left alone.
The loaded data is already tightly scoped — `CalculatorRunDataInitializer` holds
it as a local that falls out of scope the moment transpose returns — and the GC
reclaims it without help, more so under a memory limit where it collects harder.
The real memory wins are structural (the batched write above, the fee builder),
not `= null`.

## Open items

- **The run peaks at ~1.2 GB working set** at 2025 volume, under both a 1 GB and
  a 2 GB ceiling. The DataApi load and the fee write are bounded; the single
  highest retained point is `StoreProducerFees` at ~1.25 GB, comfortable at 2 GB.
- **A download's H1/H2 projected reads churn ~0.8 GB** (~0.6 GB not
  GC-reclaimable for ~a minute), because `Read…ProjectedData` pulls ~150K rows —
  one per producer × material — to build ~6K grouped objects. Fixing it means the
  DB doing the per-producer material aggregation; transient and 2 GB-safe, so it
  ranks below the fee builder. See
  [Reader and exporter interleaving](#reader-and-exporter-interleaving--done-on-this-branch-modest-effect).
- **Re-measure after a growth year.** The fee builder's per-producer cost rises
  faster than linearly, so a future year with materially more obligated producers
  needs its own measurement.
- **Production hosting confirmed** — `PRDRWDWEBAS1403` is P1v3, 2 instances, 6
  apps, same as pre-prod. Still outstanding: the names of the five co-tenant apps
  and the plan's live memory metrics, which need reader access on
  `PRDRWDWEBRG1401`.
