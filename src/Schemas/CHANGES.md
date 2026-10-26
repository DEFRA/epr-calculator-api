# Billing JSON Schema Changes

This document describes the structural improvements made when moving from the original billing JSON format to the new schema used from the 2026 billing year onwards.

## Summary

The new schema eliminates redundancy, simplifies per-producer data shapes, and removes currency symbols from monetary values, making the output easier to parse programmatically.

---

## Root-level additions

The new schema promotes three fields to the document root that were previously nested inside `calcResultDetail`:

| Field | Type | Example |
|---|---|---|
| `runId` | integer | `42` |
| `financialYear` | string | `"2025-26"` |
| `badDebtProvisionPercentage` | string | `"6.00"` |

These fields allow a consumer to identify and filter documents without having to descend into nested objects.

---

## `calculationResults` replaced by `producers` and `materials`

### Old structure

```json
{
  "calculationResults": {
    "producerCalculationResults": [
      {
        "producerID": "1",
        "producerDisposalFeesWithBadDebtProvision1": {
          "materialBreakdown": [
            {
              "materialName": "Aluminium",
              "pricePerTonne": "£0.5000",
              "producerDisposalFeeWithBadDebtProvision": "£643.98",
              ...
            }
          ]
        },
        "feeForLADisposalCosts1": {
          "totalProducerFeeForLADisposalCostsWithoutBadDebtProvision": "£607.52",
          "badDebtProvisionForLADisposalCosts": "£36.45"
        },
        "feeForCommsCostsWithBadDebtProvision_2a": { ... },
        "feeForCommsCostsWithBadDebtProvision_2b": { ... },
        "feeForCommsCostsWithBadDebtProvision_2c": { ... },
        "feeForSAOperatingCostsWithBadDebtProvision_3": { ... },
        "feeForLADataPrepCostsWithBadDebtProvision_4": { ... },
        "feeForSASetUpCostsWithBadDebtProvision_5": { ... }
      }
    ]
  }
}
```

### New structure

```json
{
  "materials": [
    {
      "materialName": "Aluminium",
      "disposalPricePerTonne": {
        "redAndRedMedical":     "0.5000",
        "amberAndAmberMedical": "0.4500",
        "greenAndGreenMedical": "0.3000"
      },
      "commsPricePerTonne": "0.4200"
    }
  ],
  "producers": [
    {
      "producerID": "1",
      "disposalCosts":        { "base": "607.52", "badDebtProvision": "36.45", "total": "643.97", "england": "...", ... },
      "commsCostsByMaterial": { "base": "...", "badDebtProvision": "...", "total": "...", "england": "...", ... },
      "commsCostsUKWide":     { ... },
      "commsCostsByCountry":  { ... },
      "saOperatingCosts":     { ... },
      "laDataPrepCosts":      { ... },
      "saSetUpCosts":         { ... },
      "totalBill":            { ... },
      "disposalFeesByMaterial": [ ... ]
    }
  ]
}
```

---

## Key improvements

### 1. Price-per-tonne moved to run level

**Before:** each material entry in every producer's `materialBreakdown` repeated `pricePerTonne`. For a run with 500 producers and 8 materials, this was 4,000 repetitions of the same value.

**After:** `materials[]` at the root contains one entry per material with the disposal and comms prices for the whole run. Consumers join on `materialName`.

### 2. Consistent fee shape across all sections

**Before:** each of the seven fee sections (1, 2a, 2b, 2c, 3, 4, 5) had its own bespoke property names:

```json
"feeForLADisposalCosts1": {
  "totalProducerFeeForLADisposalCostsWithoutBadDebtProvision": "£607.52",
  "badDebtProvisionForLADisposalCosts": "£36.45"
}
```

**After:** all sections use the same compact `feeWithCountries` shape:

```json
"disposalCosts": {
  "base":             "607.52",
  "badDebtProvision": "36.45",
  "total":            "643.97",
  "england":          "321.98",
  "wales":            "160.99",
  "scotland":         "96.00",
  "northernIreland":  "65.00"
}
```

This means consumers can handle every fee section with the same parsing logic. Section names are also shortened and consistent (`disposalCosts`, `commsCostsByMaterial`, `commsCostsUKWide`, `commsCostsByCountry`, `saOperatingCosts`, `laDataPrepCosts`, `saSetUpCosts`).

### 3. Currency values without `£` symbols

**Before:** all monetary values carried a `£` prefix (e.g. `"£643.98"`), requiring callers to strip the symbol before parsing.

**After:** all monetary values are plain decimal strings (e.g. `"643.98"`), matching standard JSON numeric conventions and eliminating the parsing step.

The pattern for 2 decimal place values is now `^-?[0-9]+\.[0-9]{2}$`; for 4 decimal place prices it is `^[0-9]+\.[0-9]{4}$`.

### 4. RAG modulation expressed at the right level

**Before:** per-producer material items repeated RAG-split prices in each producer's `materialBreakdown`.

**After:** the run-level `materials[].disposalPricePerTonne` carries the price once. For modulated runs (2026 onwards) it is the RAG breakdown (`redAndRedMedical`, `amberAndAmberMedical`, `greenAndGreenMedical`); for unmodulated runs (2025) it is a single price per tonne (e.g. `"0.0046"`). Per-producer tonnage items still carry RAG-split tonnage and fee breakdowns where the modulated path is active.

### 5. Shorter property names

| Old name | New name |
|---|---|
| `feeForLADisposalCosts1` | `disposalCosts` |
| `feeForCommsCostsWithBadDebtProvision_2a` | `commsCostsByMaterial` |
| `feeForCommsCostsWithBadDebtProvision_2b` | `commsCostsUKWide` |
| `feeForCommsCostsWithBadDebtProvision_2c` | `commsCostsByCountry` |
| `feeForSAOperatingCostsWithBadDebtProvision_3` | `saOperatingCosts` |
| `feeForLADataPrepCostsWithBadDebtProvision_4` | `laDataPrepCosts` |
| `feeForSASetUpCostsWithBadDebtProvision_5` | `saSetUpCosts` |
| `producerDisposalFeeWithBadDebtProvision` | `fee` |
| `producerDisposalFeeWithoutBadDebtProvision` | `feeWithoutBadDebt` |
| `calculationOfSuggestedBillingInstructionsAndInvoiceAmounts` | `invoice` |

---

## Schema files

| File | Purpose |
|---|---|
| `2025-billing.schema.json` | Validates billing JSON for 2025 runs (non-modulated) |
| `2026-billing.schema.json` | Validates billing JSON for 2026 runs (with RAG modulation) |

---

## Post-initial review changes (2026-06-13)

### Removed unused £-prefixed currency defs

`currency2dp`, `currency2pOrHyphen`, `currency2dpOrNull`, and `currency4dp` were defined but never referenced. They appear to be leftovers from an earlier design where monetary strings carried a `£` prefix. All active monetary values use the no-prefix `currency` / `pricePerTonne` / `currencyOrNull` defs, so these four were deleted.

### `currencyOrHyphen` → `currencyOrNull`

The original `currencyOrHyphen` def allowed either a decimal string or the literal `"-"`, matching a spreadsheet/CSV convention for "not applicable". This was used for `invoice.suggestedAmount` and `invoice.invoicedToDate`.

**Why changed:** `-` is a display value, not a data value. Using it in JSON conflates presentation with data, requiring every consumer to special-case the string rather than handling a standard JSON null. The rest of the schema already uses plain decimal strings with no display formatting, so `-` was inconsistent.

**Decision:** replaced with `currencyOrNull` (`type: ["string", "null"]`). The `JsonExporter` (or any rendering layer) is responsible for mapping `null → "-"` at the point of display.

Note, `costOrNull` could be dropped entirely with an optional (not required) `cost`, which is more idiomatic JSON. The decision to keep `costOrNull` is incase consumers rely on a fixed shape without a key-existence check (e.g. always destructuring `{ suggestedAmount, invoicedToDate }`)

### `currency` → `cost`, `currencyOrNull` → `costOrNull`, `ragCurrency` → `ragCost`

Renamed the three monetary def names from `currency*` to `cost*`. The `currency` prefix described the value format (GBP decimal string); `cost` describes what the value represents. This aligns with the rest of the schema's naming convention (`disposalCosts`, `commsCosts`, etc.) and makes the defs easier to read in context. No C# changes required - def names are internal to the schema and not referenced by the exporter.

## Why string rather than jsnumber for costs?

two related precision concerns:

JSON number precision loss - JSON numbers are parsed as IEEE 754 doubles by most consumers. A value like 38244049.46 can't be represented exactly as a double, so round-tripping through a JSON parser can silently introduce drift (e.g. 38244049.459999999...). As a string, "38244049.46" is exact.

Rounding semantics - financial amounts need to be rounded to exactly 2 decimal places before serialisation, not after. If you emit a raw decimal as a JSON number, the consumer sees 1234.5600000001 or similar depending on their parser. By converting to "F2" format in C# first, you're asserting "this is the canonical rounded value" - the string is the number, not a float approximation of it.

The tradeoff is that consumers can't do arithmetic directly on the values without parsing them back, but for billing data the expectation is display/audit/summation, not in-place computation, so that's acceptable.

## Producers restructured into groups with `members` (2026-07-06)

### Old structure

Every row in `producers[]` had the same shape, distinguished only by a `level` field and an empty-string sentinel:

```json
"producers": [
  { "producerID": "100001", "subsidiaryID": "",       "level": 1, "producerName": "",              "totalBill": "39294451.29", "invoice": { "instruction": "INITIAL", ... }, "disposalFeesByMaterial": [...], ... },
  { "producerID": "100001", "subsidiaryID": "100001",  "level": 2, "producerName": "Good L1 Ltd",   "totalBill": "35722228.44", "invoice": { "instruction": "-", "suggestedAmount": null, ... }, "disposalFeesByMaterial": [...], ... },
  { "producerID": "100001", "subsidiaryID": "100002",  "level": 2, "producerName": "Good L2 Ltd",   "totalBill": "3572222.84",  "invoice": { "instruction": "-", "suggestedAmount": null, ... }, "disposalFeesByMaterial": [...], ... },
  { "producerID": "200003", "subsidiaryID": "",       "level": 1, "producerName": "Partial H1 L1 Ltd", "totalBill": "832826.65", "invoice": { "instruction": "INITIAL", ... }, "disposalFeesByMaterial": [...], ... }
]
```

**Problems this caused:**

- A composite producer (an organisation with subsidiaries) was represented as a synthetic `level: 1` row with `producerName: ""` and `subsidiaryID: ""`, standing in as a "group total" row, followed by one `level: 2` row per constituent (including the parent organisation itself). A single-organisation producer (no subsidiaries) had no `level: 2` rows at all, and its real data sat directly on the `level: 1` row.
- Consumers had to scan the whole array and group by `producerID` to reconstruct which rows belonged together, since nothing in the schema enforced the grouping - it was implicit in row ordering and field values.
- Whether a producer was composite or single-organisation could only be inferred by checking for the presence of matching `level: 2` rows elsewhere in the array, not from the shape of a single row.
- `invoice` is only meaningful at the group level (billing instructions and suggested amounts are decided once per `producerID`), but every `level: 2` row was still required to carry a full `invoice` object, populated with placeholder `"-"`/`null` values.

### New structure

`producers[]` now has one entry per `producerID` (the group). Each entry carries the group's aggregate financials (summed across all members) directly, and a `members[]` array with one entry per constituent organisation:

```json
"producers": [
  {
    "producerID": "100001",
    "invoice": { "instruction": "INITIAL", "suggestedAmount": "39294451.29", "invoicedToDate": null },
    "totalBill": "39294451.29",
    "disposalFeesByMaterial": [ ... ],
    "disposalCosts": { ... }, "commsCostsByMaterial": { ... }, ...,
    "members": [
      { "subsidiaryID": null,      "producerName": "Good L1 Ltd", "totalBill": "35722228.44", "disposalFeesByMaterial": [ ... ], ... },
      { "subsidiaryID": "100002", "producerName": "Good L2 Ltd",  "totalBill": "3572222.84",  "disposalFeesByMaterial": [ ... ], ... }
    ]
  },
  {
    "producerID": "200003",
    "invoice": { "instruction": "INITIAL", "suggestedAmount": "832826.65", "invoicedToDate": null },
    "totalBill": "832826.65",
    "disposalFeesByMaterial": [ ... ],
    "disposalCosts": { ... }, "commsCostsByMaterial": { ... }, ...,
    "members": [
      { "subsidiaryID": null, "producerName": "Partial H1 L1 Ltd", "totalBill": "832826.65", "disposalFeesByMaterial": [ ... ], ... }
    ]
  }
]
```

Note `subsidiaryID` is `null` for the member that is the parent organisation reporting for itself, and a real ID for an actual subsidiary. This matches the domain model elsewhere in the codebase (e.g. `CalcResultSummaryProducerDisposalFees.SubsidiaryId`, `SelfManagedConsumerWasteService`'s `SubsidiaryId`), where a null subsidiary ID is the standard way of saying "this is the organisation itself, not one of its subsidiaries". The old flat structure obscured this: rather than passing the real `null` through, the `level: 2` row for the parent's own data was given a synthetic `subsidiaryID` equal to its own `producerID` (see `100001`/`100001` above), just so every row had a non-empty value to key on. That workaround is no longer needed once membership is structural (an array position) rather than something reconstructed from field values - `subsidiaryID: null` can mean exactly what it means in the domain model.

**Why this shape:**

- `level` and the empty-string sentinels for `producerName`/`subsidiaryID` are gone. A producer is composite if `members.length > 1`, single-organisation if `members.length === 1` - a structural fact rather than something inferred from row contents.
- `members` always has at least one entry, including for a single-organisation producer. There is deliberately no special-cased "flatten to the top level when there's only one member" shape - that would just reintroduce a second row shape for consumers to branch on. A consumer that only cares about the group total never needs to look inside `members` regardless of whether the producer is composite or single-organisation.
- The group keeps its own full aggregate breakdown (`totalBill`, `disposalFeesByMaterial`, `disposalCosts`, etc.), not just `totalBill`. This is the same set of fields as a member carries, now pulled out into a shared `producerFinancials` definition ($defs) referenced by both the group and each member, so a consumer who isn't interested in the per-subsidiary breakdown can read the group's fields directly without summing `members` themselves, regardless of composite vs single-organisation.
- `invoice` moved to the group only, since it was never meaningful per member - it no longer needs a placeholder value on every member.
- `producerID` moved to the group only; members no longer repeat their parent's `producerID` (it's implied by nesting under that group).


### Schema files affected

Only `2026-billing.schema.json` was updated. It had not yet been consumed by any client, so this was made as a direct breaking change rather than an additive/versioned one. `2025-billing.schema.json` is unaffected and keeps the flat `level`/`subsidiaryID` shape - that schema has already shipped.

## Proposed: split monolithic JSON into a run-metadata endpoint plus a paginated producers endpoint (2026-07-06)

Today the billing JSON is generated as a single file and downloaded whole from blob storage via a `/billingRun?runId=123` endpoint. As part of redesigning this as an API, this section proposes splitting delivery into two resources instead of one monolithic document.

### Why split

In a real export sample, `producers` (and its nested `members`) accounts for ~99% of document size; the root-level fields (`runId`, `financialYear`, `badDebtProvisionPercentage`, `modulationResults`, `materials`) are genuinely run-level - one value, or a handful of material rows, shared identically across every producer, not per-producer facts. This mirrors the reasoning in "Price-per-tonne moved to run level" above: for a run with hundreds of producers, repeating `materials` (or the other root fields) once per producer would reintroduce the same duplication that motivated moving price-per-tonne to the root in the first place - just moved from "per material row" to "per producer".

### Proposed shape

- `GET /billing-runs/{runId}` - run metadata only: `financialYear`, `badDebtProvisionPercentage`, `modulationResults`, `materials`. Small and immutable once a run is calculated, so cheap to fetch once and cache.
- `GET /billing-runs/{runId}/producers` - the bulk data, paginated. Each entry is fully self-contained (aggregate totals and `members` are already computed), so no cross-producer merging is needed at read time.
- `GET /billing-runs/{runId}/producers/{producerId}` - single-producer lookup, for callers (e.g. a producer-facing view) that only need one producer's bill.

Consumers join `materials` by `material` name once, then page through `producers` referencing it, instead of receiving it repeated on every row.

### Pagination over streaming

A run is immutable once written, so the usual argument for cursor-based pagination (items shifting between pages as data mutates) doesn't apply - plain offset/page-number pagination against `producerID` order is safe, deterministic, and simpler for consumers building a paginated UI (jump to page, show total count). It also gives retryability, caching, and rate-limiting for free.

A streaming/NDJSON bulk-export variant (e.g. `GET /billing-runs/{runId}/producers/export`) may still be worth adding later as a secondary mode for downstream ETL consumers that want the whole run in one connection, but paginated collection access should be the default.

### Status

Proposal only - not yet implemented or agreed. No endpoints described here exist yet.
