BEGIN TRANSACTION;
GO


DELETE FROM [calculator_run_classification]
WHERE [id] = 12;
SELECT @@ROWCOUNT;

GO

DELETE FROM [calculator_run_classification]
WHERE [id] = 13;
SELECT @@ROWCOUNT;

GO

DELETE FROM [calculator_run_classification]
WHERE [id] = 14;
SELECT @@ROWCOUNT;

GO

ALTER TABLE [calculator_run] ADD [HasBillingFileGenerated] bit NOT NULL DEFAULT CAST(0 AS bit);
GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20250721090348_AddNewPostInitialStatuses';
GO

COMMIT;
GO

