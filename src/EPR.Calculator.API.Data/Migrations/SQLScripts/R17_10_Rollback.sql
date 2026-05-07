BEGIN TRANSACTION;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'relative_year', N'description') AND [object_id] = OBJECT_ID(N'[calculator_run_relative_years]'))
    SET IDENTITY_INSERT [calculator_run_relative_years] ON;
INSERT INTO [calculator_run_relative_years] ([relative_year], [description])
VALUES (2023, NULL);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'relative_year', N'description') AND [object_id] = OBJECT_ID(N'[calculator_run_relative_years]'))
    SET IDENTITY_INSERT [calculator_run_relative_years] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'relative_year', N'description') AND [object_id] = OBJECT_ID(N'[calculator_run_relative_years]'))
    SET IDENTITY_INSERT [calculator_run_relative_years] ON;
INSERT INTO [calculator_run_relative_years] ([relative_year], [description])
VALUES (2024, NULL);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'relative_year', N'description') AND [object_id] = OBJECT_ID(N'[calculator_run_relative_years]'))
    SET IDENTITY_INSERT [calculator_run_relative_years] OFF;
GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20260416083412_RemoveUnsupportedRelativeYears';
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP TABLE [producer_reported_material_projected];
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[producer_reported_material]') AND [c].[name] = N'submission_period');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [producer_reported_material] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [producer_reported_material] DROP COLUMN [submission_period];
GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20260401135019_AddSubmissionPeriodProducerReportedMaterialAndProjectedTable';
GO

COMMIT;
GO

