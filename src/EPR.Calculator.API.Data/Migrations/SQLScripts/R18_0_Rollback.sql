BEGIN TRANSACTION;
GO

DROP INDEX [UX_lapcap_data_master_active_relative_year] ON [lapcap_data_master];
GO

DROP INDEX [UX_default_parameter_setting_master_active_relative_year] ON [default_parameter_setting_master];
GO

CREATE INDEX [IX_lapcap_data_master_relative_year] ON [lapcap_data_master] ([relative_year]);
GO

CREATE INDEX [IX_default_parameter_setting_master_relative_year] ON [default_parameter_setting_master] ([relative_year]);
GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20260616144420_AddActiveDefaultParameterAndLapcapConstraint';
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[transform_projected_h1]') AND [c].[name] = N'h2_ram_proportions_red_medical');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [transform_projected_h1] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [transform_projected_h1] ALTER COLUMN [h2_ram_proportions_red_medical] decimal(18,3) NOT NULL;
GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[transform_projected_h1]') AND [c].[name] = N'h2_ram_proportions_red');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [transform_projected_h1] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [transform_projected_h1] ALTER COLUMN [h2_ram_proportions_red] decimal(18,3) NOT NULL;
GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[transform_projected_h1]') AND [c].[name] = N'h2_ram_proportions_green_medical');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [transform_projected_h1] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [transform_projected_h1] ALTER COLUMN [h2_ram_proportions_green_medical] decimal(18,3) NOT NULL;
GO

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[transform_projected_h1]') AND [c].[name] = N'h2_ram_proportions_green');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [transform_projected_h1] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [transform_projected_h1] ALTER COLUMN [h2_ram_proportions_green] decimal(18,3) NOT NULL;
GO

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[transform_projected_h1]') AND [c].[name] = N'h2_ram_proportions_amber_medical');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [transform_projected_h1] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [transform_projected_h1] ALTER COLUMN [h2_ram_proportions_amber_medical] decimal(18,3) NOT NULL;
GO

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[transform_projected_h1]') AND [c].[name] = N'h2_ram_proportions_amber');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [transform_projected_h1] DROP CONSTRAINT [' + @var5 + '];');
ALTER TABLE [transform_projected_h1] ALTER COLUMN [h2_ram_proportions_amber] decimal(18,3) NOT NULL;
GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20260604163323_AmendTransformProjectedH1';
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP INDEX [IX_index_calculator_run] ON [calculator_run];
GO

ALTER TABLE [calculator_run] ADD [is_billing_file_generating] bit NULL;
GO

UPDATE
    run
SET
    run.is_billing_file_generating = 0
FROM
    dbo.calculator_run AS run
WHERE
    run.is_billing_file_generating IS NULL AND
    run.billing_run_status = 'Completed'
GO

UPDATE
    run
SET
    run.is_billing_file_generating = 1
FROM
    dbo.calculator_run AS run
WHERE
    run.is_billing_file_generating IS NULL AND
    run.billing_run_status not in ('Unknown', 'None')
GO

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[calculator_run]') AND [c].[name] = N'billing_run_started_at');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [calculator_run] DROP CONSTRAINT [' + @var6 + '];');
ALTER TABLE [calculator_run] DROP COLUMN [billing_run_started_at];
GO

DECLARE @var7 sysname;
SELECT @var7 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[calculator_run]') AND [c].[name] = N'billing_run_status');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [calculator_run] DROP CONSTRAINT [' + @var7 + '];');
ALTER TABLE [calculator_run] DROP COLUMN [billing_run_status];
GO

CREATE NONCLUSTERED INDEX [IX_index_calculator_run] ON [calculator_run] ([calculator_run_classification_id], [relative_year], [is_billing_file_generating], [id]) INCLUDE ([name], [created_by], [created_at], [updated_by], [updated_at], [calculator_run_organization_data_master_id], [calculator_run_pom_data_master_id], [default_parameter_setting_master_id], [lapcap_data_master_id]);
GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20260603161538_BillingRunStatus';
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DROP TABLE [transform_partial];
GO

DROP TABLE [transform_projected_h1];
GO

DROP TABLE [transform_projected_h2];
GO

DROP TABLE [transform_scaled];
GO

DECLARE @var8 sysname;
SELECT @var8 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[lapcap_data_detail]') AND [c].[name] = N'total_cost');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [lapcap_data_detail] DROP CONSTRAINT [' + @var8 + '];');
ALTER TABLE [lapcap_data_detail] ALTER COLUMN [total_cost] decimal(18,2) NOT NULL;
GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20260602101250_AddTransformTables';
GO

COMMIT;
GO

