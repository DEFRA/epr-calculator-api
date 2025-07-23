BEGIN TRANSACTION;
GO

DECLARE @var0 sysname;

DECLARE @SystemUser NVARCHAR(100) = N'System User';
DECLARE @TestUser NVARCHAR(100) = N'Test User';

SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[calculator_run]') AND [c].[name] = N'HasBillingFileGenerated');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [calculator_run] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [calculator_run] DROP COLUMN [HasBillingFileGenerated];
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'id', N'created_by', N'status') AND [object_id] = OBJECT_ID(N'[calculator_run_classification]'))
    SET IDENTITY_INSERT [calculator_run_classification] ON;
INSERT INTO [calculator_run_classification] ([id], [created_by], [status])
VALUES 
(12, @SystemUser, N'INTERIM RE-CALCULATION RUN COMPLETED'),
(13, @SystemUser, N'FINAL RE-CALCULATION RUN COMPLETED'),
(14, @SystemUser, N'FINAL RUN COMPLETED');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'id', N'created_by', N'status') AND [object_id] = OBJECT_ID(N'[calculator_run_classification]'))
    SET IDENTITY_INSERT [calculator_run_classification] OFF;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250721090348_AddNewPostInitialStatuses', N'8.0.7');
GO

COMMIT;
GO

