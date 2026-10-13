BEGIN TRANSACTION;
DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20260928111801_UpdateHistoricRunData';

COMMIT;
GO

BEGIN TRANSACTION;
UPDATE [calculator_run_classification] SET [created_by] = N'Test User'
WHERE [id] = 2;
SELECT @@ROWCOUNT;


UPDATE [calculator_run_classification] SET [created_by] = N'Test User'
WHERE [id] = 3;
SELECT @@ROWCOUNT;


UPDATE [calculator_run_classification] SET [created_by] = N'Test User'
WHERE [id] = 4;
SELECT @@ROWCOUNT;


UPDATE [calculator_run_classification] SET [created_by] = N'Test User'
WHERE [id] = 5;
SELECT @@ROWCOUNT;


UPDATE [calculator_run_classification] SET [created_by] = N'Test user'
WHERE [id] = 8;
SELECT @@ROWCOUNT;


UPDATE [calculator_run_classification] SET [created_by] = N'Test user', [status] = N'INTERIM RE-CALCULATION RUN'
WHERE [id] = 9;
SELECT @@ROWCOUNT;


UPDATE [calculator_run_classification] SET [status] = N'INTERIM RE-CALCULATION RUN COMPLETED'
WHERE [id] = 12;
SELECT @@ROWCOUNT;


IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'id', N'created_by', N'status') AND [object_id] = OBJECT_ID(N'[calculator_run_classification]'))
    SET IDENTITY_INSERT [calculator_run_classification] ON;
INSERT INTO [calculator_run_classification] ([id], [created_by], [status])
VALUES (1, N'Test User', N'IN THE QUEUE'),
(10, N'Test user', N'FINAL RUN'),
(11, N'Test user', N'FINAL RE-CALCULATION RUN'),
(13, N'System User', N'FINAL RE-CALCULATION RUN COMPLETED'),
(14, N'System User', N'FINAL RUN COMPLETED');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'id', N'created_by', N'status') AND [object_id] = OBJECT_ID(N'[calculator_run_classification]'))
    SET IDENTITY_INSERT [calculator_run_classification] OFF;

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20260917104423_RemoveFinalRunClassifications';

COMMIT;
GO

