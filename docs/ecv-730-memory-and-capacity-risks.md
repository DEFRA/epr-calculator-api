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

**Measured at 2025 volume, a full run plus its three file downloads has a peak
working set of ~3 GB and completes cleanly under a 4 GB heap limit (five-run
soak, no OOM). The footprint itself is manageable. The problem is that in both
production and pre-prod the calculator web app shares one 8 GB P1v3 instance with
five other applications, where any one app's realistic burst budget is ~2–4 GB
and a neighbour can spike at any time. The service needs a dedicated App Service
plan, or a move to the larger P2v3 plan, so a run can reserve ~3 GB (with
headroom) without contending.**

Code work for the new volumes is partly done on this branch — the fee and SMCW
writes are now batched so they no longer exhaust the EF change tracker. Two
larger allocation costs remain (the fee builder and the fee read) and would each
bring the run peak down further; see
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

| | value |
|---|---|
| DataApi `GetProducerData` (4 GB heap limit) | completes — ~8 s, ~4.4 GB allocation churn (test-inflated, see below), heap settles ~2.8 GB; returns 7,000 organisations / 6,638 producers / 2 errors |
| Full run, unconstrained GC | ~63 s; ~51 GB allocation churn |
| Full run, 4 GB heap limit | **five-run soak completes with no OOM**, peak working set 2.92 GB (73 % of the 4 GB ceiling), managed heap 2.64 GB live at end, ~50.9 GB churn per run. Bit-for-bit reproducible; run-to-run timing variance under 3 %. |
| Full run, 3 GB heap limit | **five-run soak completes with no OOM** on macOS, peak working set 3.14 GB (105 % of the 3 GB ceiling), managed heap 2.64 GB live at end. The billing-JSON export peaks at ~3.18 GB every run — over the ceiling but not fatal because `DOTNET_GCHeapHardLimit` is soft on macOS. On a Linux container the limit is harder, so 3 GB is not a safe target without the fee-read work. |

Averaged over the five soak runs: calc 60.6 s, results-CSV export 11.4 s,
billing run 0.4 s, billing-CSV export 10.8 s, billing-JSON export 11.2 s.

Before the generator was calibrated, the over-scaled (10,587-producer) dataset
OOM'd a back-to-back soak in a billing-JSON export around run 4. At the corrected
volume that margin problem is gone — the heaviest export (billing JSON) now
peaks at ~3.18 GB, roughly 800 MB clear of the 4 GB ceiling.

### Allocation by stage (4 GB heap limit, per-stage telemetry, calibrated volume)

Per-stage `allocated` is process-wide managed churn while the stage ran, not
retained bytes; `heap after` is the approximate live managed heap when it
finished. Figures are run 1 of the five-run soak; the other four were within
~1 % of every line.

**DataApi — `ProducerDataService.GetProducerData` / `CommonDataApiLoader.LoadDataCore`** (~4.4 GB, 8 s, heap settles ~2.8 GB)

| sub-stage | allocated | time | heap after |
|---|---|---|---|
| `BufferOrganisationStream` | 29 MB | 0.1 s | ~139 MB |
| `SelectLatestOrganisationFiles` | 16 MB | 0.04 s | ~153 MB |
| `BufferPomStream` (buffers the raw ~2.1M-row POM stream) | **4.28 GB** | 7.2 s | ~2.67 GB |
| `SelectLatestPomFiles` | 54 MB | 0.7 s | ~2.72 GB |
| `PomEligibilityFilter` / `ApplyPeriodFlags` | ~6 MB each | <0.1 s | ~2.74 GB |
| `ProducerPomAligner.Align` | 30 MB | 0.05 s | ~2.77 GB |
| `ProducerDataTransposer.Transpose` | 300 MB | 1.6 s | ~2.91 GB |

Buffering the POM stream is the whole DataApi cost; every filter after it is
6–50 MB because it re-references the buffered rows rather than copying them. The
4.3 GB churn / ~2.7 GB retained is inflated in-test by `CsvHelper`
`GetRecords<dynamic>` — production reads that stream off a SQL reader through EF
and is leaner — but the buffered `List<PayCalPom>` itself is genuine retained
memory for the duration of the load.

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

**Read — `CalcResultReader.*`**, once per file export, three exports per calc run

| stage | allocated | time |
|---|---|---|
| `ReadProducerFees` | 2.67 GB | 8.3 s |
| `ReadH1ProjectedData` / `ReadH2ProjectedData` | ~270 / ~250 MB | ~0.5 s each |
| `ReadSmcw` | ~258 MB | 1.2 s |
| everything else combined | < 15 MB | |
| serialize (`ProducerFeesExporter` ~520 MB, rest < 200 MB) | ~0.7–0.9 GB | ~0.6 s |
| **`FileExportService.Export` total** | **4.2–4.9 GB** | 11–12 s |

`AsSplitQuery()` on `ReadProducerFees` (this branch) took the read from ~21 s to
~13 s at the over-scaled volume, ~8 s at the calibrated one; the ~2.7 GB churn /
~1 GB retained per call is the streamed-read ticket.

**Whole calc run** `CalculatorRunProcessor.Process`: ~50.9 GB churn, ~1 min 3 s,
retained heap ~1.26 GB. Finalize (4,201 billing instructions + 33,608 invoiced
net tonnages) is ~120 MB; the billing run proper is 1–7 MB.

The producer-fee data dominates on all of build, write and read.

### Heap consistency and test retention

At the calibrated volume the calc pipeline is bit-for-bit reproducible run to
run: ~50.9 GB churn per run, fee builder ~23.6 GB every time, every
`ReadProducerFees` call (three per run across the five-run soak) ~2.67 GB /
~8 s, retained heap ~1.25 GB at the end of each calc. Nothing in the pipeline
leaks or grows — peak working set over the whole five-run soak was **2.92 GB,
no higher than a single run**, and the managed heap was 2.64 GB live at the end.

The GC's timing under a hard limit still matters. Each file export needs ~1 GB
retained (the fee graph) plus a transient serializer buffer, and that memory is
only reclaimed when the next large allocation forces a blocking gen-2 collection.
Run back to back with no gap, an export starts on the previous export's
uncollected residue. Production does not do that — each export is a separate HTTP
request, minutes to hours apart, with a full GC cycle's worth of idle between
them — so the perf test models it by forcing a blocking gen-2 collection between
the calc and each export (`CollectAndMeasure` in `CalculatorRunPerformanceTests`).

With that in place, at the calibrated volume the per-export figures are stable
run to run:

| export | heap on entry | heap at peak |
|---|---|---|
| results CSV | ~0.14 GB | ~1.7–1.9 GB |
| billing CSV | ~0.15 GB | ~2.2 GB |
| billing JSON | ~1.6 GB | ~3.18 GB |

Billing JSON still enters highest — ~1.6 GB even after a forced collection,
because the JSON serializer's pooled `ArrayPool` buffers are not returned to the
GC — and it is the heaviest of the three. At ~3.18 GB it has ~800 MB of
clearance under a 4 GB ceiling, but it sits *above* a 3 GB one: the 3 GB soak
completes only because `DOTNET_GCHeapHardLimit` is soft on macOS. At the earlier
over-scaled volume this same export peaked at ~4.3 GB and was the one that OOM'd
a soak; calibrating the dataset removed that. The streamed fee read (below) would
take its peak down another ~1 GB — enough to make a hard 3 GB limit safe.

Test hygiene also addressed on this branch so the soak measures the service, not
the harness: every controller call now goes through `CallController<T>`, which
builds and disposes a fresh DI scope per call (replacing the long-lived
`CreateController` that pinned a scope and its `ApplicationDBContext` for the
whole test), and the reused `ApplicationDBContext` gets a `ChangeTracker.Clear()`
each iteration so the billing-seed entities (~6.7k rows/run) don't accumulate.
`fakeOrganisationsStream.Organisations` is held for the whole test by design as
the source data; the POM stream is a re-invoked factory so its ~2.1M rows are
streamed from disk per run and never retained as a list.

The `CommonDataApiLoader` ~4.4 GB churn is **inflated by the test**: the perf
test's fake stream parses ~2.1M rows with `CsvHelper.GetRecords<dynamic>` (a
dynamic object and per-field strings per row). In production that stream is EF
Core reading a SQL result set, which is much leaner — the real DataApi figure is
well below 4 GB. The individual DataApi filter stages (`AcceptedFileSelector`,
`PomEligibilityFilter`, `ProducerPomAligner`) are 6–50 MB each because they
re-reference the already-materialised rows rather than copying them.

Reading:

- **DataApi is not the constraint.** It streams, filters and aligns ~2M rows
  inside a 4 GB heap in about 8 seconds, retaining a buffered `List<PayCalPom>`
  of ~2.7 GB while it runs and handing on a small result. Buffering the raw
  stream is an accepted design point for a service that will serve multiple
  clients.
- **At the calibrated 2025 volume a full run + all three downloads fits inside a
  4 GB heap with ~1 GB to spare** — peak working set 2.92 GB, five-run soak with
  no OOM. The heaviest moment is the billing-JSON export at ~3.18 GB.
- **The margin is not large.** Under a 3 GB limit the soak still completes on
  macOS but peak working set is 3.14 GB and the billing-JSON export runs ~3.18 GB
  — over the ceiling, tolerated only because the macOS limit is soft. A hard 3 GB
  limit, more producers in a future year, or the streamed fee read not landing
  all eat the margin. The fee builder and fee read are the two structural costs
  (see below).

## Compute requirement

The measured footprint of a run at 2025 volume is ~3 GB (2.92 GB peak working
set, billing-JSON export the high-water mark at ~3.18 GB). The problem is not the
absolute number — it is that this has to be *reserved*, repeatedly, on an
instance the calculator does not have to itself.

1. **A calculator run should not share an instance with other apps.** On the
   P1v3 plan, after platform overhead six apps share ~6.5 GB per instance and any
   one app's realistic burst budget is ~2–4 GB — below what a run needs, and a
   neighbour can spike into it at any time. Move the calculator web app to its
   own App Service plan, or onto the P2v3 plan (16 GB / 4 vCPU) as the Function
   App consolidation frees room there. On a dedicated plan the run has the whole
   instance and the platform will not kill a neighbour (or be killed by one).
2. **Size for a run, not for idle.** The steady-state API footprint is small; the
   requirement is driven by the periodic calc run streaming ~2M rows and building
   the result set. Target headroom of at least 2× the ~3 GB run peak — i.e. an
   instance that can give the calculator ~6 GB without contending with
   neighbours. The fee-builder and fee-read work would bring the run peak down
   and widen that margin.
3. **Confirm run concurrency is one per instance.** Runs are queue-driven and
   assumed serial, but this is not enforced; two concurrent runs on one instance
   would double the transient memory.

Explicitly **not** proposed: setting `DOTNET_GCHeapHardLimit` on the app as a
production mitigation. That only makes the process fail sooner under a limit it
should not be sharing in the first place. It is used here only as a test tool.

## Follow-up code work

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

### Producer-fee read — separate ticket

`CalcResultReader.ReadProducerFees` loads the whole `ProducerFees` object with
all detail rows and their JSON graphs in one go — ~8 s and **~2.67 GB of
allocation** per call at 2025 volume after `AsSplitQuery()`, retaining roughly
1 GB.

The export OOM seen in the perf test at 4 GB was partly a **test artifact** and
partly **over-scaling**. The artifact: the perf test reused hand-built
controllers whose DI scope (and its scoped `ApplicationDBContext` /
`FileExportService`) was pinned for the whole test, so successive exports' fee
data accumulated, and it ran the three exports with no GC between them, unlike
production. Both are fixed on this branch — `CallController<T>` disposes a scope
per call, and `CollectAndMeasure` forces a collection between exports. The
over-scaling: the generator was producing ~1.7× the real producer count, which
pushed the billing-JSON export to ~4.3 GB. At the calibrated volume it peaks at
~3.18 GB and a five-run soak passes with ~800 MB to spare.

Still worth a ticket, because the export path materialises the entire
`ProducerFees` object three times per run (results CSV, billing CSV, billing
JSON) and each is ~8 s / ~1 GB retained. Streaming the detail rows to the
exporter, and reading once and reusing across the three exports, touches
`FileExportService`, `CalcResultReader` and `ProducerFeesExporter`, and would
take the billing-JSON peak down another ~1 GB — enough for a 3 GB limit to be
comfortable. `AsSplitQuery()` is added on this branch as an interim step.

### Export byte conversion — cheap, not yet done

Two small allocations on the export path, both avoidable without touching the
exporters:

- `FileExportService.ToUtf8WithBom` runs `Encoding.UTF8.GetBytes(content)` and
  then copies the result again into a second array to prepend the 3-byte BOM —
  a full second copy of every CSV. Encoding the BOM and the content into one
  pre-sized buffer removes the copy.
- The billing-JSON path calls `billingJsonWriter.WriteToString(...)`, which
  returns a `JsonSerializer.Serialize` **string** — UTF-16, so roughly twice the
  file size (~700 MB for the ~370 MB JSON) — and then `Encoding.UTF8.GetBytes`
  converts that to the `byte[]` actually returned. `JsonSerializer.SerializeToUtf8Bytes`
  (exposed as a `WriteToUtf8Bytes` on `BillingFileJsonWriter`) writes UTF-8
  straight to a `byte[]` and never allocates the intermediate string.

Together that is roughly 1 GB of transient allocation off the billing-JSON
export — the heaviest of the three, currently peaking at ~3.18 GB. The changes
are local to `FileExportService` and `BillingFileJsonWriter`; a cheap way to
widen the margin further ahead of the streamed read.

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
The real memory wins are structural (the batched write above, the fee-builder and
streamed-read tickets), not `= null`.

## Open items

- **Per-run peak established at ~3 GB** for 2025 volume — 4 GB soak clean at 73 %
  of ceiling; 3 GB soak also completes on macOS (soft limit) but peak working set
  is 3.14 GB and the billing-JSON export runs ~3.18 GB, over the ceiling. A hard
  3 GB limit (Linux container) would need the fee-read work first. Not yet run:
  the soak under a 2 GB limit.
- **Re-measure after a growth year.** The fee builder's per-producer cost rises
  faster than linearly, so a future year with materially more obligated producers
  needs its own measurement rather than a linear extrapolation from ~3 GB.
- **Production hosting confirmed** — `PRDRWDWEBAS1403` is P1v3, 2 instances, 6
  apps, same as pre-prod. Still outstanding: the names of the five co-tenant apps
  and the plan's live memory metrics, which need reader access on
  `PRDRWDWEBRG1401`.
