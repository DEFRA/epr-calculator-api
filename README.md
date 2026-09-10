# epr-calculator-api

## Overview

RESTful API for performing EPR cost calculations driven by a frontend. Results can be reviewed via a CSV file before being shared with FSS.

## Prerequisites

Follow the [epr-local-environment](https://github.com/DEFRA/epr-local-environment) setup, using the `paycal` profile. Once that's working, stop its `epr-calculator-api` Docker service so this project can be run locally instead.

## How to run locally

```
cp src/EPR.Calculator.API/appsettings.template.jsonc src/EPR.Calculator.API/appsettings.local.json
```

Copy the required environment values (database connection, authentication settings) from `epr-local-environment` into your local secrets file. In VS Code, you can open it by right-clicking `src/EPR.Calculator.API/EPR.Calculator.API.csproj` → *Manage User Secrets*.

```
dotnet run --project src/EPR.Calculator.API
```

## Database migrations

The database is already set up via `epr-local-environment`. After changing an EF Core model, create a migration:

```
dotnet ef migrations add MyNewSchemaChange --project src/EPR.Calculator.API.Data --startup-project src/EPR.Calculator.API
```

Review the generated files, then apply it locally:

```
dotnet ef database update --project src/EPR.Calculator.API.Data --startup-project src/EPR.Calculator.API
```

The pipeline needs `migrations.sql` kept in sync — regenerate it after adding a migration:

```
dotnet ef migrations script -o src/EPR.Calculator.API.Data/Scripts/migrations.sql -i --project src/EPR.Calculator.API.Data --startup-project src/EPR.Calculator.API
```

Before each release, cut a rollback script (replace `ChangesFrom`/`ChangesTo`/`RXX_X` with the real migration names and release number):

```
dotnet ef migrations script ChangesFrom ChangesTo -o src/EPR.Calculator.API.Data/Migrations/SQLScripts/RXX_X_Rollback.sql --project src/EPR.Calculator.API.Data --startup-project src/EPR.Calculator.API
```

## How to run tests

```
dotnet test src
```

runs the unit and integration suites (the integration tests need Docker, for the SQL Server test container).

### Performance test

A separate entry point drives a large synthetic calculator + billing run (~7,000 organisations, ~45,000 POM rows) and reports timing and per-stage memory. It repeats 5 times by default, to show whether numbers are stable or drifting run over run:

```
dotnet run --project src/EPR.Calculator.API.IntegrationTests -- --performance
```

Pass `--runs` for a quicker single pass while iterating (e.g. checking one code change), instead of waiting on the full 5:

```
dotnet run --project src/EPR.Calculator.API.IntegrationTests -- --performance --runs 1
```

To catch a memory regression, cap the managed heap and confirm the run still completes — this repo's peak usage currently fits comfortably inside a 2 GiB limit:

```
DOTNET_GCHeapHardLimit=0x80000000 dotnet run --project src/EPR.Calculator.API.IntegrationTests -- --performance
```

(`0x80000000` is 2 GiB; adjust the hex value to tighten or loosen the ceiling. This only bounds the GC heap, not total process working set, so a passing run can still report a working set slightly above the limit.)

## Licence

Copyright (c) 2023 Defra

This source code is licensed under the Open Government Licence v3.0. To view this licence, visit <https://www.nationalarchives.gov.uk/doc/open-government-licence/version/3> or write to the Information Policy Team, The National Archives, Kew, Richmond, Surrey, TW9 4DU.
