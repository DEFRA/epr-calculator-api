# epr-calculator-api

## Overview

RESTful API for performing EPR cost calculations driven by a frontend. The results can be reviewed via a CSV file before being shared with FSS.

## Prerequisites

First, follow the [epr-local-environment](https://github.com/DEFRA/epr-local-environment) setup, using the `paycal` profile.

Once you are happy with your local setup, stop the `epr-calculator-api` Docker API service so that this project can be run locally instead.

## How to run locally

Create a local application settings file:

```
cp src/EPR.Calculator.API/appsettings.template.jsonc src/EPR.Calculator.API/appsettings.local.json
```

Copy the required environment values from `epr-local-environment`, such as the database connection and authentication settings, into your local secrets `.json` file.

In VS Code, you can create the secrets file by right-clicking `src/EPR.Calculator.API/EPR.Calculator.API.csproj`.

Run the project with:

```
dotnet run --project src/EPR.Calculator.API
```

## How to migrate the database

The database should already be set up via `epr-local-environment`.

If you have modified the Entity Framework domain models and need to make a schema change, first create a new migration:

```
dotnet ef migrations add MyNewSchemaChange --project src/EPR.Calculator.API.Data --startup-project src/EPR.Calculator.API
```

Review the migration files that are created. If everything looks correct, apply the migration to your local database:

```
dotnet ef database update MyNewSchemaChange --project src/EPR.Calculator.API.Data --startup-project src/EPR.Calculator.API
```

### Updating `migrations.sql`

The pipelines require `migrations.sql` to be kept up to date. After creating a migration, regenerate the SQL migration script:

```
dotnet ef migrations script -o src/EPR.Calculator.API.Data/Scripts/migrations.sql -i --project src/EPR.Calculator.API.Data --startup-project src/EPR.Calculator.API
```

### Creating rollback scripts

This is done before each release.

Before running the command below, replace ChangesFrom, ChangesTo, and RXX_X with the appropriate migration names and release number:

```
dotnet ef migrations script ChangesFrom ChangesTo -o src/EPR.Calculator.API.Data/Migrations/SQLScripts/RXX_X_Rollback.sql --project src/EPR.Calculator.API.Data --startup-project src/EPR.Calculator.API
```

## How to run tests

To run both the integration and unit tests:

```
dotnet test src
```

To run the performance test, we recommend closing your IDE as this increases runs by 10-12 seconds, then start with:
```
dotnet test src/EPR.Calculator.API.IntegrationTests --filter "TestCategory=PerformanceTests"
```

## Licence

Copyright (c) 2023 Defra

This source code is licensed under the Open Government Licence v3.0. To view this licence, visit <https://www.nationalarchives.gov.uk/doc/open-government-licence/version/3> or write to the Information Policy Team, The National Archives, Kew, Richmond, Surrey, TW9 4DU.
