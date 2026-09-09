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
three file downloads completes under a 2 GB heap limit on macOS (five-run soak,
no OOM, end-of-run managed heap 0.15 GB, peak working set 1.94 GB). The exports
peak at ~1–1.6 GB and leave no residue; the build/write pipeline retains ~1.3 GB.
The one large transient is the DataApi buffering all ~2.1M raw POM rows (~2.5 GB)
before de-duplicating them — real, and reducible by a two-pass file selection
(separate ticket). Sizing aside, the problem is that in both production and
pre-prod the calculator web app shares one 8 GB P1v3 instance with five other
applications, where any one app's realistic burst budget is ~2–4 GB and a
neighbour can spike at any time. The service needs a dedicated App Service plan,
or a move to the larger P2v3 plan, so a run can reserve what it needs without
contending.**

Code work for the new volumes on this branch: the fee/SMCW writes are batched so
they no longer exhaust the EF change tracker, and all three file exports now
stream the producer fee rows off the reader instead of materialising the whole
fee graph — this took the run from OOM-at-2 GB to a clean 2 GB soak with the
exports no longer the constraint. Remaining structural costs: the fee builder
(~23.6 GB churn) and the DataApi raw-POM buffer; see
[Follow-up code work](#follow-up-code-work).

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

Figures below are **after** all three exports were made to stream the producer
fee rows off the reader rather than materialise the whole `ProducerFees` graph
(see [Producer-fee read](#producer-fee-read--streamed-on-this-branch)), and after
the perf test's fake POM stream was switched from `CsvHelper.GetRecords<dynamic>`
to a field-by-field read. The export path is no longer the memory constraint at
any tested ceiling.

| | value |
|---|---|
| DataApi `GetProducerData` (2 GB heap limit) | completes — ~8 s, ~4.8 GB allocation churn, heap settles ~2.5 GB; returns 7,000 organisations / 6,638 producers / 2 errors |
| Full run, unconstrained GC | ~63 s; ~51 GB allocation churn |
| Full run, 2 GB heap limit | **five-run soak completes with no OOM** on macOS. End-of-run managed heap **0.15 GB**. Export heap peaks: results CSV ~1.34 GB, billing CSV ~1.59 GB, billing JSON ~1.0 GB. Whole-test peak working set **1.94 GB** (97 % of the 2 GB ceiling). |
| Full run, 1 GB heap limit | the **calc run** fails in `BufferPomStream` — it holds ~2.5 GB of raw POM rows (see below), which no CSV-parser change removes. The exports were not reached; on their own they fit ~1–1.6 GB. |

Averaged over the five 2 GB soak runs: calc 61.6 s, results-CSV export 9.1 s,
billing run 0.5 s, billing-CSV export 8.8 s, billing-JSON export 8.7 s.

The 1.94 GB working-set peak is a macOS soft-limit figure. The calc run's
managed heap still reaches ~2.5 GB during `BufferPomStream` and ~2.8 GB during
`Transpose`, so a **hard** limit (Linux cgroup) below ~3 GB would still OOM in
the calc's DataApi load — see [the DataApi buffer](#the-dataapi-pom-buffer-is-the-real-floor).

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

**DataApi — `ProducerDataService.GetProducerData` / `CommonDataApiLoader.LoadDataCore`** (~4.8 GB churn, 8 s, heap settles ~2.5 GB)

| sub-stage | allocated | time | heap after |
|---|---|---|---|
| `BufferOrganisationStream` | 27 MB | 0.1 s | ~140 MB |
| `SelectLatestOrganisationFiles` | 16 MB | 0.04 s | ~150 MB |
| `BufferPomStream` (buffers the raw ~2.1M-row POM stream) | **4.81 GB** | 6.2 s | ~2.52 GB |
| `SelectLatestPomFiles` | 54 MB | 0.5 s | ~2.55 GB |
| `PomEligibilityFilter` / `ApplyPeriodFlags` | ~6 MB each | <0.1 s | ~2.55 GB |
| `ProducerPomAligner.Align` | 30 MB | 0.05 s | ~2.6 GB |
| `ProducerDataTransposer.Transpose` | 300 MB | 1.3 s | ~2.78 GB |

Buffering the raw POM stream is the whole DataApi cost; every filter after it is
6–50 MB because it re-references the buffered rows rather than copying them. The
~2.5 GB retained `List<PayCalPom>` (all 2.1M rows, pre-dedup) is genuine — it is
the same in production (SQL reader instead of CSV) and unchanged by the test's
switch to field-by-field parsing. See
[the DataApi POM buffer](#the-dataapi-pom-buffer-is-the-real-floor).

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
object) rather than `GetRecords<dynamic>`. The last change cut whole-test peak
working set from 2.25 GB to 1.94 GB — it removes the parser's churn, not the
buffer it feeds.

### The DataApi POM buffer is the real floor

`ProducerDataService` buffers the **entire raw POM stream** into a
`List<PayCalPom>` before `SelectLatestPomFiles` dedups it — at 2025 volume that
is ~2.1M rows (every resubmission file version) and ~2.5 GB retained during
`BufferPomStream`, rising to ~2.8 GB through `Transpose`. This is **not** a
harness artifact: switching the test parser from `dynamic` to field-by-field
left the retained figure unchanged, because the cost is the 2.1M-object list
itself. Production reads the same rows off a SQL reader into the same list.

Reducing it is a **DataApi change, not a test change**: a two-pass file
selection — pass 1 buffers only the per-file `(org, submitter, period, filename,
createdDate)` tuples (tiny) to pick the winning file per group; pass 2 re-streams
and keeps only rows whose filename won. That would take the buffer from ~2.5 GB
to a few hundred MB. Separate ticket.

Reading:

- **Every export streams and fits comfortably** — results CSV ~1.34 GB, billing
  CSV ~1.59 GB, billing JSON ~1.0 GB, none near a 2 GB ceiling, end-of-run
  managed heap 0.15 GB.
- **The calc run's raw-POM buffer sets the floor** — ~2.5 GB retained during
  `BufferPomStream`, ~2.8 GB through `Transpose`. The 2 GB soak passes only on
  macOS's soft limit (working set 1.94 GB); a hard limit below ~3 GB OOMs here.
  The two-pass file selection above is what removes it.
- **The fee builder is the other structural cost** (~23.6 GB churn, see below),
  and its per-producer cost rises faster than linearly — a future year with more
  obligated producers needs its own measurement.

## Compute requirement

The service-side footprint of a run at 2025 volume, after the export streaming
work, is: exports peak at ~1–1.6 GB and leave no residue; the build/write
pipeline retains ~1.3 GB; and the DataApi buffers ~2.5–2.8 GB of raw POM rows
during the load (real, and reducible by a two-pass file selection — see
[Follow-up code work](#dataapi-raw-pom-buffer--separate-ticket)). The problem is
not the absolute number — it is that whatever a run needs has to be *reserved*,
repeatedly, on an instance the calculator does not have to itself.

1. **A calculator run should not share an instance with other apps.** On the
   P1v3 plan, after platform overhead six apps share ~6.5 GB per instance and any
   one app's realistic burst budget is ~2–4 GB — and a neighbour can spike into
   it at any time. Move the calculator web app to its own App Service plan, or
   onto the P2v3 plan (16 GB / 4 vCPU) as the Function App consolidation frees
   room there. On a dedicated plan the run has the whole instance and the
   platform will not kill a neighbour (or be killed by one).
2. **Size for a run, not for idle.** The steady-state API footprint is small; the
   requirement is driven by the periodic calc run streaming ~2M rows and building
   the result set. Once the DataApi figure is confirmed against a real SQL read,
   target headroom of at least 2× that peak on an instance the calculator does
   not contend for. The fee-builder work would bring it down further.
3. **Confirm run concurrency is one per instance.** Runs are queue-driven and
   assumed serial, but this is not enforced; two concurrent runs on one instance
   would double the transient memory.

Explicitly **not** proposed: setting `DOTNET_GCHeapHardLimit` on the app as a
production mitigation. That only makes the process fail sooner under a limit it
should not be sharing in the first place. It is used here only as a test tool.

## Follow-up code work

### DataApi raw-POM buffer — separate ticket

`ProducerDataService` reads the whole raw POM stream into a `List<PayCalPom>`
(~2.1M rows / ~2.5 GB at 2025 volume, ~90 % of them superseded resubmission
versions) before `AcceptedFileSelector.SelectLatestPomFiles` dedups it. That
buffer is the calc run's memory floor.

A two-pass read removes it: pass 1 streams the source and keeps only the per-file
`(organisationId, submitterId, submissionPeriod, fileName, createdDate)` tuples,
from which `SelectLatestPomFiles`'s existing logic picks the winning file name
per group; pass 2 re-streams and yields only rows whose file name won. Pass 1's
buffer is a few hundred KB; the pipeline downstream of `StreamPoms` is unchanged.
Costs a second read of the source (a second SQL query in production, a second
file scan in the perf test).

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

- **The exports are no longer the constraint.** All three stream; a five-run 2 GB
  soak completes with no OOM, end-of-run managed heap 0.15 GB, peak working set
  1.94 GB.
- **The calc run's raw-POM buffer is the floor** (~2.5 GB retained,
  `BufferPomStream`). A hard limit below ~3 GB OOMs there. The
  [two-pass file selection](#dataapi-raw-pom-buffer--separate-ticket) removes it;
  until then the plan must be sized for it plus the fee builder's retained
  ~1.3 GB.
- **Re-measure after a growth year.** The fee builder's per-producer cost rises
  faster than linearly, so a future year with materially more obligated producers
  needs its own measurement.
- **Production hosting confirmed** — `PRDRWDWEBAS1403` is P1v3, 2 instances, 6
  apps, same as pre-prod. Still outstanding: the names of the five co-tenant apps
  and the plan's live memory metrics, which need reader access on
  `PRDRWDWEBRG1401`.
