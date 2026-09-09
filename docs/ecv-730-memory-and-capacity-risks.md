# ECV-730 — compute requirements for the calculator service

**Branch:** `ECV-730-NF`
**Date:** 2026-09-08
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
three file downloads has a peak working set of ~1.3 GB and completes cleanly
under a 2 GB heap limit (five-run soak, no OOM, end-of-run managed heap 0.15 GB);
under a 1 GB limit it completes on macOS's soft limit, with managed-heap peaks of
~1.45 GB (billing-CSV export) and ~1.25 GB (`StoreProducerFees`). The footprint
is manageable. The problem is that in both production and pre-prod the calculator
web app shares one 8 GB P1v3 instance with five other applications, where any one
app's realistic burst budget is ~2–4 GB and a neighbour can spike at any time.
The service needs a dedicated App Service plan, or a move to the larger P2v3
plan, so a run can reserve what it needs without contending.**

Code work for the new volumes on this branch: the fee/SMCW writes are batched so
they no longer exhaust the EF change tracker; all three file exports stream the
producer fee rows off the reader instead of materialising the whole fee graph;
and the DataApi picks the winning POM file from a first metadata-only pass so it
never buffers the full raw stream. Together these took the run from OOM-at-2 GB
to a clean 1 GB soak. The remaining structural cost is the fee builder
(~23.6 GB churn); see [Follow-up code work](#follow-up-code-work).

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

Figures below are **after** the memory work on this branch: all three exports
stream the producer fee rows off the reader
(see [Producer-fee read](#producer-fee-read--streamed-on-this-branch)); the perf
test's fake POM stream is read field-by-field, not as `dynamic`; and the DataApi
picks the winning POM file in a first pass over just the file metadata, so it
never buffers the full raw stream
(see [the DataApi POM buffer](#the-dataapi-pom-buffer--reduced-on-this-branch)).

| | value |
|---|---|
| DataApi `GetProducerData` (1 GB heap limit) | completes — ~10 s, ~9.7 GB allocation churn (two source reads), heap settles ~0.6 GB; returns 7,000 organisations / 6,638 producers / 2 errors |
| Full run, unconstrained GC | ~63 s; ~56 GB allocation churn |
| Full run, 2 GB heap limit | **five-run soak, no OOM.** End-of-run managed heap **0.15 GB**. Whole-test peak working set **1.34 GB** (67 % of the 2 GB ceiling). |
| Full run, 1 GB heap limit | **five-run soak, no OOM** on macOS. Peak working set 1.19 GB (119 % of the 1 GB ceiling — over by working-set accounting, tolerated on the soft limit). Managed heap still peaks ~1.45 GB in the billing-CSV export and ~1.25 GB in `StoreProducerFees`, so a **hard** 1 GB limit would OOM there — but the calc's DataApi load, which used to fail here outright, now peaks ~0.6 GB. |

Averaged over the five soak runs (either ceiling): calc ~63 s, results-CSV export
~9.1 s, billing run ~0.5 s, billing-CSV export ~9.0 s, billing-JSON export ~8.6 s.
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

**Read / export — `CalcResultReader.*` + exporters**, three exports per calc run

All three exports now stream the producer fee rows (`StreamProducerFeeDetails` —
a deferred `IEnumerable` consumed once) instead of calling `ReadProducerFees`.

| stage | allocated | heap at peak | time |
|---|---|---|---|
| `ReadH1ProjectedData` / `ReadH2ProjectedData` / `ReadSmcw` | ~250 MB each | | ~0.5–1.2 s |
| **results CSV export** | ~2.9 GB | ~1.34 GB | ~9 s |
| **billing CSV export** | ~3.2 GB | ~1.59 GB | ~9 s |
| **billing JSON export** | ~0.8 GB | ~1.0 GB | ~2.3 s |

The CSV exports still churn ~3 GB and hold ~1.3–1.6 GB — that is now the CSV text
itself (`StringBuilder` → `string` → BOM-prefixed `byte[]` for a ~45 MB file,
plus the exporter's per-row string building), not the fee graph. Billing JSON is
lighter still because it serializes straight to the response with no assembled
document. `AsSplitQuery()` on `ReadProducerFees` (now unused by the export path,
kept for callers) took that read from ~21 s to ~8 s at the calibrated volume.

**Whole calc run** `CalculatorRunProcessor.Process`: ~50.9 GB churn, ~1 min 3 s,
retained heap ~1.26 GB. Finalize (4,201 billing instructions + 33,608 invoiced
net tonnages) is ~120 MB; the billing run proper is 1–7 MB.

The producer-fee data dominates on all of build, write and read.

### Heap consistency and test retention

At the calibrated volume the calc pipeline is bit-for-bit reproducible run to
run: ~51 GB churn per run, fee builder ~23.6 GB every time, retained heap
~1.3 GB at the end of each calc. Nothing in the pipeline leaks or grows, and the
**end-of-run managed heap is 0.15 GB** — the streamed exports leave no residue.

Each export enters at ~0.15 GB (a forced blocking gen-2 collection between
sections — `CollectAndMeasure` in `CalculatorRunPerformanceTests` — models
production's separate, time-separated requests) and peaks at:

| export | heap at peak |
|---|---|
| results CSV | ~1.34 GB |
| billing CSV | ~1.59 GB |
| billing JSON | ~1.0 GB |

None of the three is close to the 2 GB ceiling. The residual CSV cost is the
~45 MB CSV text held as a `StringBuilder`/`string`/`byte[]` during assembly, not
the fee data.

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
  1 GB soak completes on macOS (working set 1.19 GB).
- **Every export streams** — results CSV ~1.34 GB, billing CSV ~1.45–1.59 GB,
  billing JSON ~1.0 GB, end-of-run managed heap 0.15 GB. The residual CSV cost is
  the ~45 MB file text held as `StringBuilder`/`string`/`byte[]` during assembly.
- **The remaining managed-heap peaks are the billing-CSV export (~1.45 GB) and
  `StoreProducerFees` (~1.25 GB)** — both fine at 2 GB, both over a hard 1 GB.
- **The fee builder is the other structural cost** (~23.6 GB churn, see below),
  and its per-producer cost rises faster than linearly — a future year with more
  obligated producers needs its own measurement.

## Compute requirement

The service-side footprint of a run at 2025 volume, after the memory work on this
branch, is modest: exports peak at ~1–1.6 GB managed heap and leave no residue;
`StoreProducerFees` retains ~1.25 GB; the DataApi load peaks ~0.6 GB. The
whole-run peak working set is ~1.3 GB under a 2 GB ceiling. The problem is not
the absolute number — it is that whatever a run needs has to be *reserved*,
repeatedly, on an instance the calculator does not have to itself.

1. **A calculator run should not share an instance with other apps.** On the
   P1v3 plan, after platform overhead six apps share ~6.5 GB per instance and any
   one app's realistic burst budget is ~2–4 GB — and a neighbour can spike into
   it at any time. Move the calculator web app to its own App Service plan, or
   onto the P2v3 plan (16 GB / 4 vCPU) as the Function App consolidation frees
   room there. On a dedicated plan the run has the whole instance and the
   platform will not kill a neighbour (or be killed by one).
2. **Size for a run, not for idle.** The steady-state API footprint is small; the
   requirement is driven by the periodic calc run. At 2025 volume the run peaks
   at ~1.3–1.5 GB; target headroom of at least 2× that on an instance the
   calculator does not contend for. The fee-builder work would bring it down
   further.
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

`CalcResultReader.ReadProducerFees` used to load the whole `ProducerFees` object
with all detail rows and their JSON graphs in one go — ~8 s and ~2.67 GB of
allocation, retaining ~1 GB — three times per calc run (results CSV, billing
CSV, billing JSON).

All three export paths now stream instead. `CalcResultReader.StreamProducerFeeDetails`
returns the per-producer `FeeDetail` rows as a **deferred `IEnumerable` read
straight off the reader**, consumed once:

- **Billing JSON**: `CalculationResultsJson` projects the rows lazily and
  `BillingFileJsonWriter` serializes with `JsonSerializer.SerializeAsync` to the
  response body via a `StreamCallbackResult` — the fee graph, the JSON model
  list, and the output buffer are none of them held whole. ~3.18 GB peak (OOM at
  2 GB) → **~1.0 GB peak, ~0.8 GB churn, ~2.3 s**.
- **Results CSV / billing CSV**: `ProducerFeesExporter` (already a single
  forward pass) and the summary part-exporters (which only need `.Total`) take
  the streamed enumerable; `GetResult` reads the fee `Total` only.
  ~1.9 GB heap / ~4.2 GB churn / ~11 s → **~1.3–1.6 GB heap / ~3 GB churn /
  ~9 s**. The residual is the ~45 MB CSV text held during assembly, not the fee
  data.

`GetResult` no longer materialises `ProducerFees.Details` at all. Output is
byte-identical on all three (golden-file integration test). `ReadProducerFees`
is kept for other callers but is unused by the export path.

### Export byte conversion

- **Billing-JSON output — superseded by the streaming change above.** The path
  briefly used `JsonSerializer.SerializeToUtf8Bytes` (skipping an intermediate
  UTF-16 string) but still buffered the whole output; it now streams to the
  response with `SerializeAsync` and buffers nothing.
- **CSV BOM copy — not yet done.** `FileExportService.ToUtf8WithBom` runs
  `Encoding.UTF8.GetBytes(content)` and then copies the result again into a
  second array to prepend the 3-byte BOM — a full second copy of every CSV.
  Encoding the BOM and the content into one pre-sized buffer removes the copy.

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

- **The run peaks at ~1.3 GB working set** at 2025 volume. Exports, the DataApi
  load and the fee write are all streamed or bounded; the highest managed-heap
  points are the billing-CSV export (~1.45 GB) and `StoreProducerFees` (~1.25 GB),
  both comfortable at 2 GB.
- **Re-measure after a growth year.** The fee builder's per-producer cost rises
  faster than linearly, so a future year with materially more obligated producers
  needs its own measurement.
- **Optional next step:** the billing-CSV export still assembles the whole ~45 MB
  file as a `StringBuilder`/`string`/`byte[]`. Writing it to the response stream
  (as the billing JSON now does) would drop its ~1.45 GB peak, but it is no longer
  on the critical path for a 2 GB plan.
- **Production hosting confirmed** — `PRDRWDWEBAS1403` is P1v3, 2 instances, 6
  apps, same as pre-prod. Still outstanding: the names of the five co-tenant apps
  and the plan's live memory metrics, which need reader access on
  `PRDRWDWEBRG1401`.
