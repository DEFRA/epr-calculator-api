BEGIN TRANSACTION;
DROP PROCEDURE IF EXISTS dbo.TruncateDataApiLoadPoms;

DROP PROCEDURE IF EXISTS dbo.TruncateDataApiLoadOrganisations;

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20261105000000_AddTruncateProceduresForDataApiLoadTables';

COMMIT;
GO

BEGIN TRANSACTION;
DELETE FROM [data_api_load_organisations];

DELETE FROM [data_api_load_poms];

ALTER TABLE [data_api_load_poms]          DROP COLUMN [regulator_status];

ALTER TABLE [data_api_load_organisations] ALTER COLUMN [regulator_status] nvarchar(50) NOT NULL;

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20261104000000_NullableRegulatorStatusOnDataApiLoadTables';

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [data_api_load_organisations] DROP COLUMN [organisation_size];

ALTER TABLE [data_api_load_poms]          DROP COLUMN [organisation_size];

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20261103000000_AddOrganisationSizeToDataApiLoadTables';

COMMIT;
GO

BEGIN TRANSACTION;
DROP TABLE [data_api_load_organisations];

DROP TABLE [data_api_load_poms];

DROP TABLE [calculator_run_organisation];

DROP INDEX [IX_index_calculator_run] ON [calculator_run];

DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[producer_detail]') AND [c].[name] = N'joiner_date');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [producer_detail] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [producer_detail] DROP COLUMN [joiner_date];

DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[producer_detail]') AND [c].[name] = N'leaver_date');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [producer_detail] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [producer_detail] DROP COLUMN [leaver_date];

DECLARE @var2 nvarchar(max);
SELECT @var2 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[producer_detail]') AND [c].[name] = N'num_days_obligated');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [producer_detail] DROP CONSTRAINT ' + @var2 + ';');
ALTER TABLE [producer_detail] DROP COLUMN [num_days_obligated];

DECLARE @var3 nvarchar(max);
SELECT @var3 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[producer_detail]') AND [c].[name] = N'status_code');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [producer_detail] DROP CONSTRAINT ' + @var3 + ';');
ALTER TABLE [producer_detail] DROP COLUMN [status_code];

DECLARE @var4 nvarchar(max);
SELECT @var4 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[calculator_run]') AND [c].[name] = N'org_pom_data_loaded_at');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [calculator_run] DROP CONSTRAINT ' + @var4 + ';');
ALTER TABLE [calculator_run] DROP COLUMN [org_pom_data_loaded_at];

EXEC sp_rename N'calculator_run_pom_data_master_bk', N'calculator_run_pom_data_master';

EXEC sp_rename N'calculator_run_organization_data_master_bk', N'calculator_run_organization_data_master';

EXEC sp_rename N'calculator_run_pom_data_detail_bk', N'calculator_run_pom_data_detail';

EXEC sp_rename N'calculator_run_organization_data_detail_bk', N'calculator_run_organization_data_detail';

CREATE TABLE [organisation_data] (
    [num_days_obligated] int NULL,
    [error_code] nvarchar(max) NULL,
    [has_h1] bit NOT NULL,
    [has_h2] bit NOT NULL,
    [joiner_date] nvarchar(50) NULL,
    [leaver_date] nvarchar(50) NULL,
    [load_ts] datetime2 NOT NULL,
    [obligation_status] nvarchar(10) NOT NULL,
    [organisation_id] int NOT NULL,
    [organisation_name] nvarchar(400) NOT NULL,
    [status_code] nvarchar(max) NULL,
    [submitter_id] uniqueidentifier NULL,
    [subsidiary_id] nvarchar(400) NULL,
    [trading_name] nvarchar(400) NULL
);

CREATE TABLE [pom_data] (
    [load_ts] datetime2 NOT NULL,
    [organisation_id] int NULL,
    [packaging_activity] nvarchar(400) NULL,
    [packaging_class] nvarchar(400) NULL,
    [packaging_material] nvarchar(max) NULL,
    [packaging_material_subtype] nvarchar(max) NULL,
    [packaging_material_weight] float NULL,
    [packaging_type] nvarchar(400) NULL,
    [ram_rag_rating] nvarchar(max) NULL,
    [submission_period] nvarchar(400) NULL,
    [submission_period_desc] nvarchar(max) NULL,
    [submitter_id] uniqueidentifier NULL,
    [subsidiary_id] nvarchar(400) NULL
);

ALTER TABLE [calculator_run] ADD [calculator_run_organization_data_master_id] int NULL;

ALTER TABLE [calculator_run] ADD [calculator_run_pom_data_master_id] int NULL;


                EXEC(N'
                    UPDATE r
                    SET calculator_run_organization_data_master_id = b.calculator_run_organization_data_master_id,
                        calculator_run_pom_data_master_id = b.calculator_run_pom_data_master_id
                    FROM calculator_run r
                    INNER JOIN calculator_run_bk b
                        ON b.id = r.id;
                ');

DROP TABLE [calculator_run_bk];

CREATE INDEX [IX_calculator_run_calculator_run_organization_data_master_id] ON [calculator_run] ([calculator_run_organization_data_master_id]);

CREATE INDEX [IX_calculator_run_calculator_run_pom_data_master_id] ON [calculator_run] ([calculator_run_pom_data_master_id]);

CREATE NONCLUSTERED INDEX [IX_index_calculator_run] ON [calculator_run] ([classification], [relative_year], [billing_run_status], [id]) INCLUDE ([name], [created_by], [created_at], [updated_by], [updated_at], [calculator_run_organization_data_master_id], [calculator_run_pom_data_master_id], [default_parameter_setting_master_id], [lapcap_data_master_id]);

ALTER TABLE [calculator_run] ADD CONSTRAINT [FK_calculator_run_calculator_run_organization_data_master_calculator_run_organization_data_master_id] FOREIGN KEY ([calculator_run_organization_data_master_id]) REFERENCES [calculator_run_organization_data_master] ([id]);

ALTER TABLE [calculator_run] ADD CONSTRAINT [FK_calculator_run_calculator_run_pom_data_master_calculator_run_pom_data_master_id] FOREIGN KEY ([calculator_run_pom_data_master_id]) REFERENCES [calculator_run_pom_data_master] ([id]);

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20261102000000_DataApiSchemaChanges';

COMMIT;
GO

BEGIN TRANSACTION;
EXEC(N'
UPDATE
    dbo.calculator_run
SET
    billing_run_status = ''Running''
WHERE
    billing_run_status = ''Started''
')

ALTER TABLE [calculator_run_billing_file_metadata] ADD [billing_file_authorised_by] nvarchar(400) NULL;

ALTER TABLE [calculator_run_billing_file_metadata] ADD [billing_file_authorised_date] datetime2 NULL;

EXEC(N'
UPDATE
    metadata
SET
    metadata.billing_file_authorised_by = run.billing_file_shared_by,
    metadata.billing_file_authorised_date = run.billing_file_shared_at
FROM
    dbo.calculator_run_billing_file_metadata AS metadata
INNER JOIN
    dbo.calculator_run AS run
    ON run.id = metadata.calculator_run_id
WHERE
    run.is_billing_file_shared = 1
    AND metadata.id = (
        SELECT TOP (1)
            latest.id
        FROM
            dbo.calculator_run_billing_file_metadata AS latest
        WHERE
            latest.calculator_run_id = run.id
        ORDER BY
            latest.billing_file_created_date DESC,
            latest.id DESC
    )
')

EXEC(N'
UPDATE
    dbo.calculator_run
SET
    classification =
        CASE
            WHEN classification = ''Initial'' AND is_billing_file_shared = 1       THEN ''7''  -- INITIAL RUN COMPLETED
            WHEN classification = ''Initial''                                      THEN ''8''  -- INITIAL RUN
            WHEN classification = ''Recalculation'' AND is_billing_file_shared = 1 THEN ''12'' -- RECALCULATION RUN COMPLETED
            WHEN classification = ''Recalculation''                                THEN ''9''  -- RECALCULATION RUN
            WHEN classification = ''Test''                                         THEN ''4''  -- TEST RUN
            WHEN classification = ''Deleted''                                      THEN ''6''  -- DELETED
            WHEN calculation_run_status IN (''None'', ''Started'')                 THEN ''2''  -- RUNNING
            WHEN calculation_run_status = ''Completed''                            THEN ''3''  -- UNCLASSIFIED
            ELSE ''5''                                                                         -- ERROR
        END
')

DECLARE @var5 nvarchar(max);
SELECT @var5 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[calculator_run]') AND [c].[name] = N'billing_file_shared_at');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [calculator_run] DROP CONSTRAINT ' + @var5 + ';');
ALTER TABLE [calculator_run] DROP COLUMN [billing_file_shared_at];

DECLARE @var6 nvarchar(max);
SELECT @var6 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[calculator_run]') AND [c].[name] = N'billing_file_shared_by');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [calculator_run] DROP CONSTRAINT ' + @var6 + ';');
ALTER TABLE [calculator_run] DROP COLUMN [billing_file_shared_by];

DECLARE @var7 nvarchar(max);
SELECT @var7 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[calculator_run]') AND [c].[name] = N'calculation_run_status');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [calculator_run] DROP CONSTRAINT ' + @var7 + ';');
ALTER TABLE [calculator_run] DROP COLUMN [calculation_run_status];

DECLARE @var8 nvarchar(max);
SELECT @var8 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[calculator_run]') AND [c].[name] = N'is_billing_file_shared');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [calculator_run] DROP CONSTRAINT ' + @var8 + ';');
ALTER TABLE [calculator_run] DROP COLUMN [is_billing_file_shared];

EXEC sp_rename N'[calculator_run].[classification]', N'calculator_run_classification_id', 'COLUMN';

DROP INDEX [IX_index_calculator_run] ON [calculator_run];
DECLARE @var9 nvarchar(max);
SELECT @var9 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[calculator_run]') AND [c].[name] = N'calculator_run_classification_id');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [calculator_run] DROP CONSTRAINT ' + @var9 + ';');
ALTER TABLE [calculator_run] ALTER COLUMN [calculator_run_classification_id] int NOT NULL;
CREATE NONCLUSTERED INDEX [IX_index_calculator_run] ON [calculator_run] ([calculator_run_classification_id], [relative_year], [billing_run_status], [id]) INCLUDE ([name], [created_by], [created_at], [updated_by], [updated_at], [calculator_run_organization_data_master_id], [calculator_run_pom_data_master_id], [default_parameter_setting_master_id], [lapcap_data_master_id]);

CREATE TABLE [calculator_run_classification] (
    [id] int NOT NULL IDENTITY,
    [created_by] nvarchar(400) NOT NULL,
    [status] nvarchar(250) NOT NULL,
    CONSTRAINT [PK_calculator_run_classification] PRIMARY KEY ([id])
);

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'id', N'created_by', N'status') AND [object_id] = OBJECT_ID(N'[calculator_run_classification]'))
    SET IDENTITY_INSERT [calculator_run_classification] ON;
INSERT INTO [calculator_run_classification] ([id], [created_by], [status])
VALUES (2, N'System User', N'RUNNING'),
(3, N'System User', N'UNCLASSIFIED'),
(4, N'System User', N'TEST RUN'),
(5, N'System User', N'ERROR'),
(6, N'System User', N'DELETED'),
(7, N'System User', N'INITIAL RUN COMPLETED'),
(8, N'System User', N'INITIAL RUN'),
(9, N'System User', N'RECALCULATION RUN'),
(12, N'System User', N'RECALCULATION RUN COMPLETED');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'id', N'created_by', N'status') AND [object_id] = OBJECT_ID(N'[calculator_run_classification]'))
    SET IDENTITY_INSERT [calculator_run_classification] OFF;

ALTER TABLE [calculator_run] ADD CONSTRAINT [FK_calculator_run_calculator_run_classification_calculator_run_classification_id] FOREIGN KEY ([calculator_run_classification_id]) REFERENCES [calculator_run_classification] ([id]) ON DELETE CASCADE;

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20261027000000_SplitRunClassification';

COMMIT;
GO

