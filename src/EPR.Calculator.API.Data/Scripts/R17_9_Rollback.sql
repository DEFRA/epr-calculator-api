BEGIN TRANSACTION;
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[organisation_data]') AND [c].[name] = N'has_h1');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [organisation_data] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [organisation_data] DROP COLUMN [has_h1];
GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[organisation_data]') AND [c].[name] = N'has_h2');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [organisation_data] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [organisation_data] DROP COLUMN [has_h2];
GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[calculator_run_organization_data_detail]') AND [c].[name] = N'has_h1');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [calculator_run_organization_data_detail] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [calculator_run_organization_data_detail] DROP COLUMN [has_h1];
GO

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[calculator_run_organization_data_detail]') AND [c].[name] = N'has_h2');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [calculator_run_organization_data_detail] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [calculator_run_organization_data_detail] DROP COLUMN [has_h2];
GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20260319164244_AddHasH1AndHasH2OrganisationData';
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[producer_reported_material]') AND [c].[name] = N'packaging_tonnage_amber');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [producer_reported_material] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [producer_reported_material] DROP COLUMN [packaging_tonnage_amber];
GO

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[producer_reported_material]') AND [c].[name] = N'packaging_tonnage_amber_medical');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [producer_reported_material] DROP CONSTRAINT [' + @var5 + '];');
ALTER TABLE [producer_reported_material] DROP COLUMN [packaging_tonnage_amber_medical];
GO

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[producer_reported_material]') AND [c].[name] = N'packaging_tonnage_green');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [producer_reported_material] DROP CONSTRAINT [' + @var6 + '];');
ALTER TABLE [producer_reported_material] DROP COLUMN [packaging_tonnage_green];
GO

DECLARE @var7 sysname;
SELECT @var7 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[producer_reported_material]') AND [c].[name] = N'packaging_tonnage_green_medical');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [producer_reported_material] DROP CONSTRAINT [' + @var7 + '];');
ALTER TABLE [producer_reported_material] DROP COLUMN [packaging_tonnage_green_medical];
GO

DECLARE @var8 sysname;
SELECT @var8 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[producer_reported_material]') AND [c].[name] = N'packaging_tonnage_red');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [producer_reported_material] DROP CONSTRAINT [' + @var8 + '];');
ALTER TABLE [producer_reported_material] DROP COLUMN [packaging_tonnage_red];
GO

DECLARE @var9 sysname;
SELECT @var9 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[producer_reported_material]') AND [c].[name] = N'packaging_tonnage_red_medical');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [producer_reported_material] DROP CONSTRAINT [' + @var9 + '];');
ALTER TABLE [producer_reported_material] DROP COLUMN [packaging_tonnage_red_medical];
GO

DECLARE @var10 sysname;
SELECT @var10 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[pom_data]') AND [c].[name] = N'packaging_material_subtype');
IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [pom_data] DROP CONSTRAINT [' + @var10 + '];');
ALTER TABLE [pom_data] DROP COLUMN [packaging_material_subtype];
GO

DECLARE @var11 sysname;
SELECT @var11 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[pom_data]') AND [c].[name] = N'ram_rag_rating');
IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [pom_data] DROP CONSTRAINT [' + @var11 + '];');
ALTER TABLE [pom_data] DROP COLUMN [ram_rag_rating];
GO

DECLARE @var12 sysname;
SELECT @var12 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[calculator_run_pom_data_detail]') AND [c].[name] = N'packaging_material_subtype');
IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [calculator_run_pom_data_detail] DROP CONSTRAINT [' + @var12 + '];');
ALTER TABLE [calculator_run_pom_data_detail] DROP COLUMN [packaging_material_subtype];
GO

DECLARE @var13 sysname;
SELECT @var13 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[calculator_run_pom_data_detail]') AND [c].[name] = N'ram_rag_rating');
IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [calculator_run_pom_data_detail] DROP CONSTRAINT [' + @var13 + '];');
ALTER TABLE [calculator_run_pom_data_detail] DROP COLUMN [ram_rag_rating];
GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20260316112551_AddModulationFields';
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DELETE FROM [default_parameter_template_master]
WHERE [parameter_unique_ref] = N'LRET-AL-G';
SELECT @@ROWCOUNT;

GO

DELETE FROM [default_parameter_template_master]
WHERE [parameter_unique_ref] = N'LRET-AL-R';
SELECT @@ROWCOUNT;

GO

DELETE FROM [default_parameter_template_master]
WHERE [parameter_unique_ref] = N'LRET-FC-G';
SELECT @@ROWCOUNT;

GO

DELETE FROM [default_parameter_template_master]
WHERE [parameter_unique_ref] = N'LRET-FC-R';
SELECT @@ROWCOUNT;

GO

DELETE FROM [default_parameter_template_master]
WHERE [parameter_unique_ref] = N'LRET-GL-G';
SELECT @@ROWCOUNT;

GO

DELETE FROM [default_parameter_template_master]
WHERE [parameter_unique_ref] = N'LRET-GL-R';
SELECT @@ROWCOUNT;

GO

DELETE FROM [default_parameter_template_master]
WHERE [parameter_unique_ref] = N'LRET-OT-G';
SELECT @@ROWCOUNT;

GO

DELETE FROM [default_parameter_template_master]
WHERE [parameter_unique_ref] = N'LRET-OT-R';
SELECT @@ROWCOUNT;

GO

DELETE FROM [default_parameter_template_master]
WHERE [parameter_unique_ref] = N'LRET-PC-G';
SELECT @@ROWCOUNT;

GO

DELETE FROM [default_parameter_template_master]
WHERE [parameter_unique_ref] = N'LRET-PC-R';
SELECT @@ROWCOUNT;

GO

DELETE FROM [default_parameter_template_master]
WHERE [parameter_unique_ref] = N'LRET-PL-G';
SELECT @@ROWCOUNT;

GO

DELETE FROM [default_parameter_template_master]
WHERE [parameter_unique_ref] = N'LRET-PL-R';
SELECT @@ROWCOUNT;

GO

DELETE FROM [default_parameter_template_master]
WHERE [parameter_unique_ref] = N'LRET-ST-G';
SELECT @@ROWCOUNT;

GO

DELETE FROM [default_parameter_template_master]
WHERE [parameter_unique_ref] = N'LRET-ST-R';
SELECT @@ROWCOUNT;

GO

DELETE FROM [default_parameter_template_master]
WHERE [parameter_unique_ref] = N'LRET-WD-G';
SELECT @@ROWCOUNT;

GO

DELETE FROM [default_parameter_template_master]
WHERE [parameter_unique_ref] = N'LRET-WD-R';
SELECT @@ROWCOUNT;

GO

DELETE FROM [default_parameter_template_master]
WHERE [parameter_unique_ref] = N'REDM-RF';
SELECT @@ROWCOUNT;

GO

UPDATE [default_parameter_template_master] SET [parameter_category] = N'Late reporting tonnage', [parameter_type] = N'Aluminium', [valid_Range_to] = 999999999.99
WHERE [parameter_unique_ref] = N'LRET-AL';
SELECT @@ROWCOUNT;

GO

UPDATE [default_parameter_template_master] SET [parameter_category] = N'Late reporting tonnage', [parameter_type] = N'Aluminium', [valid_Range_to] = 999999999.99
WHERE [parameter_unique_ref] = N'LRET-FC';
SELECT @@ROWCOUNT;

GO

UPDATE [default_parameter_template_master] SET [parameter_category] = N'Late reporting tonnage', [parameter_type] = N'Aluminium', [valid_Range_to] = 999999999.99
WHERE [parameter_unique_ref] = N'LRET-GL';
SELECT @@ROWCOUNT;

GO

UPDATE [default_parameter_template_master] SET [parameter_category] = N'Late reporting tonnage', [parameter_type] = N'Aluminium', [valid_Range_to] = 999999999.99
WHERE [parameter_unique_ref] = N'LRET-OT';
SELECT @@ROWCOUNT;

GO

UPDATE [default_parameter_template_master] SET [parameter_category] = N'Late reporting tonnage', [parameter_type] = N'Aluminium', [valid_Range_to] = 999999999.99
WHERE [parameter_unique_ref] = N'LRET-PC';
SELECT @@ROWCOUNT;

GO

UPDATE [default_parameter_template_master] SET [parameter_category] = N'Late reporting tonnage', [parameter_type] = N'Aluminium', [valid_Range_to] = 999999999.99
WHERE [parameter_unique_ref] = N'LRET-PL';
SELECT @@ROWCOUNT;

GO

UPDATE [default_parameter_template_master] SET [parameter_category] = N'Late reporting tonnage', [parameter_type] = N'Aluminium', [valid_Range_to] = 999999999.99
WHERE [parameter_unique_ref] = N'LRET-ST';
SELECT @@ROWCOUNT;

GO

UPDATE [default_parameter_template_master] SET [parameter_category] = N'Late reporting tonnage', [parameter_type] = N'Aluminium', [valid_Range_to] = 999999999.99
WHERE [parameter_unique_ref] = N'LRET-WD';
SELECT @@ROWCOUNT;

GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20260311141533_AddModulationDefaultParameters';
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20260303000000_FixRecreatePomOrgDataTables';
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

ALTER TABLE [calculator_run] DROP CONSTRAINT [FK_calculator_run_calculator_run_relative_years_relative_year];
GO

ALTER TABLE [calculator_run_organization_data_master] DROP CONSTRAINT [FK_calculator_run_organization_data_master_calculator_run_relative_years_relative_year];
GO

ALTER TABLE [calculator_run_pom_data_master] DROP CONSTRAINT [FK_calculator_run_pom_data_master_calculator_run_relative_years_relative_year];
GO

ALTER TABLE [default_parameter_setting_master] DROP CONSTRAINT [FK_default_parameter_setting_master_calculator_run_relative_years_relative_year];
GO

ALTER TABLE [lapcap_data_master] DROP CONSTRAINT [FK_lapcap_data_master_calculator_run_relative_years_relative_year];
GO

ALTER TABLE [calculator_run_relative_years] DROP CONSTRAINT [PK_calculator_run_relative_years];
GO

DROP INDEX [IX_lapcap_data_master_relative_year] ON [lapcap_data_master];
GO

DROP INDEX [IX_default_parameter_setting_master_relative_year] ON [default_parameter_setting_master];
GO

DROP INDEX [IX_calculator_run_pom_data_master_relative_year] ON [calculator_run_pom_data_master];
GO

DROP INDEX [IX_calculator_run_organization_data_master_relative_year] ON [calculator_run_organization_data_master];
GO

DROP INDEX [IX_calculator_run_relative_year] ON [calculator_run];
GO

DROP INDEX [IX_index_calculator_run] ON [calculator_run];
GO

EXEC sp_rename N'[calculator_run_relative_years]', N'calculator_run_financial_years';
GO

ALTER TABLE [lapcap_data_master] ADD [projection_year] nvarchar(450) NOT NULL DEFAULT N'';
GO

ALTER TABLE [default_parameter_setting_master] ADD [parameter_year] nvarchar(450) NOT NULL DEFAULT N'';
GO

ALTER TABLE [calculator_run_pom_data_master] ADD [calendar_year] nvarchar(max) NOT NULL DEFAULT N'';
GO

ALTER TABLE [calculator_run_organization_data_master] ADD [calendar_year] nvarchar(max) NOT NULL DEFAULT N'';
GO

ALTER TABLE [calculator_run_financial_years] ADD [financial_Year] nvarchar(450) NOT NULL DEFAULT N'';
GO

ALTER TABLE [calculator_run] ADD [financial_year] nvarchar(450) NOT NULL DEFAULT N'';
GO

EXEC(N'UPDATE lapcap_data_master                        SET projection_year = CAST(relative_year AS VARCHAR(4)) + ''-'' + RIGHT(''0'' + CAST((relative_year + 1) % 100 AS VARCHAR(2)), 2)')
GO

EXEC(N'UPDATE default_parameter_setting_master          SET parameter_year  = CAST(relative_year AS VARCHAR(4)) + ''-'' + RIGHT(''0'' + CAST((relative_year + 1) % 100 AS VARCHAR(2)), 2)')
GO

EXEC(N'UPDATE calculator_run_pom_data_master            SET calendar_year   = relative_year - 1')
GO

EXEC(N'UPDATE calculator_run_organization_data_master   SET calendar_year   = relative_year - 1')
GO

EXEC(N'UPDATE calculator_run_financial_years            SET financial_Year  = CAST(relative_year AS VARCHAR(4)) + ''-'' + RIGHT(''0'' + CAST((relative_year + 1) % 100 AS VARCHAR(2)), 2)')
GO

EXEC(N'UPDATE calculator_run                            SET financial_year  = CAST(relative_year AS VARCHAR(4)) + ''-'' + RIGHT(''0'' + CAST((relative_year + 1) % 100 AS VARCHAR(2)), 2)')
GO

DECLARE @var14 sysname;
SELECT @var14 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[lapcap_data_master]') AND [c].[name] = N'relative_year');
IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [lapcap_data_master] DROP CONSTRAINT [' + @var14 + '];');
ALTER TABLE [lapcap_data_master] DROP COLUMN [relative_year];
GO

DECLARE @var15 sysname;
SELECT @var15 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[default_parameter_setting_master]') AND [c].[name] = N'relative_year');
IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [default_parameter_setting_master] DROP CONSTRAINT [' + @var15 + '];');
ALTER TABLE [default_parameter_setting_master] DROP COLUMN [relative_year];
GO

DECLARE @var16 sysname;
SELECT @var16 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[calculator_run_pom_data_master]') AND [c].[name] = N'relative_year');
IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [calculator_run_pom_data_master] DROP CONSTRAINT [' + @var16 + '];');
ALTER TABLE [calculator_run_pom_data_master] DROP COLUMN [relative_year];
GO

DECLARE @var17 sysname;
SELECT @var17 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[calculator_run_organization_data_master]') AND [c].[name] = N'relative_year');
IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [calculator_run_organization_data_master] DROP CONSTRAINT [' + @var17 + '];');
ALTER TABLE [calculator_run_organization_data_master] DROP COLUMN [relative_year];
GO

DECLARE @var18 sysname;
SELECT @var18 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[calculator_run_financial_years]') AND [c].[name] = N'relative_year');
IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [calculator_run_financial_years] DROP CONSTRAINT [' + @var18 + '];');
ALTER TABLE [calculator_run_financial_years] DROP COLUMN [relative_year];
GO

DECLARE @var19 sysname;
SELECT @var19 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[calculator_run]') AND [c].[name] = N'relative_year');
IF @var19 IS NOT NULL EXEC(N'ALTER TABLE [calculator_run] DROP CONSTRAINT [' + @var19 + '];');
ALTER TABLE [calculator_run] DROP COLUMN [relative_year];
GO

ALTER TABLE [calculator_run_financial_years] ADD CONSTRAINT [PK_calculator_run_financial_years] PRIMARY KEY ([financial_Year]);
GO

CREATE INDEX [IX_lapcap_data_master_projection_year] ON [lapcap_data_master] ([projection_year]);
GO

CREATE INDEX [IX_default_parameter_setting_master_parameter_year] ON [default_parameter_setting_master] ([parameter_year]);
GO

CREATE INDEX [IX_calculator_run_financial_year] ON [calculator_run] ([financial_year]);
GO

CREATE NONCLUSTERED INDEX [IX_index_calculator_run] ON [calculator_run] ([calculator_run_classification_id], [financial_year], [is_billing_file_generating], [id]) INCLUDE ([name], [created_by], [created_at], [updated_by], [updated_at], [calculator_run_organization_data_master_id], [calculator_run_pom_data_master_id], [default_parameter_setting_master_id], [lapcap_data_master_id]);
GO

ALTER TABLE [calculator_run] ADD CONSTRAINT [FK_calculator_run_calculator_run_financial_years_financial_year] FOREIGN KEY ([financial_year]) REFERENCES [calculator_run_financial_years] ([financial_Year]) ON DELETE CASCADE;
GO

ALTER TABLE [default_parameter_setting_master] ADD CONSTRAINT [FK_default_parameter_setting_master_calculator_run_financial_years_parameter_year] FOREIGN KEY ([parameter_year]) REFERENCES [calculator_run_financial_years] ([financial_Year]) ON DELETE CASCADE;
GO

ALTER TABLE [lapcap_data_master] ADD CONSTRAINT [FK_lapcap_data_master_calculator_run_financial_years_projection_year] FOREIGN KEY ([projection_year]) REFERENCES [calculator_run_financial_years] ([financial_Year]) ON DELETE CASCADE;
GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20260224105304_UseRelativeYear';
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreateRunOrganization]') AND type = N'P')
                DROP PROCEDURE [dbo].[CreateRunOrganization];
                declare @Sql varchar(max);
                SET @Sql = N'CREATE PROCEDURE [dbo].[CreateRunOrganization]                
                (                    @RunId int,                    @calendarYear varchar(400),                    @createdBy varchar(400)                )                
                AS                
                BEGIN                    
                SET NOCOUNT ON                    
                    declare @DateNow datetime, @orgDataMasterid int                    
                        SET @DateNow = GETDATE()                    
                    declare @oldCalcRunOrgMasterId int                    
                        SET @oldCalcRunOrgMasterId = (select top 1 id from dbo.calculator_run_organization_data_master order by id desc)                    
                    Update calculator_run_organization_data_master SET effective_to = @DateNow 
                        WHERE id = @oldCalcRunOrgMasterId                    
                    INSERT into dbo.calculator_run_organization_data_master                    
                        (calendar_year, created_at, created_by, effective_from, effective_to)                    
                    values                    
                        (@calendarYear, @DateNow, @createdBy, @DateNow, NULL)                    
                    SET @orgDataMasterid  = CAST(scope_identity() AS int);                    
                    INSERT  into dbo.calculator_run_organization_data_detail                        
                        (calculator_run_organization_data_master_id,
                        load_ts,organisation_id,
                        organisation_name,
                        trading_name,                            
                        subsidiary_id,
                        obligation_status,
                        submitter_id,
                        status_code,
                        num_days_obligated,
                        error_code)                    
                    SELECT  @orgDataMasterid,                             
                    load_ts,                            
                    organisation_id,                            
                    organisation_name,                            
                    trading_name,                            
                    CASE WHEN LTRIM(RTRIM(subsidiary_id)) = '''' THEN NULL ELSE subsidiary_id END as subsidiary_id,
                    obligation_status,
                    submitter_id,
                    status_code,
                    num_days_obligated,
                    error_code
                    from                             
                        dbo.organisation_data                    
                    Update dbo.calculator_run Set calculator_run_organization_data_master_id = @orgDataMasterid where id = @RunId                
                    END'
                EXEC(@Sql)
GO

GRANT EXEC ON [dbo].[CreateRunOrganization] TO PUBLIC;
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreateRunPom]') AND type = N'P')
                DROP PROCEDURE [dbo].[CreateRunPom];
                    declare @Sql varchar(max);
                    SET @Sql = N'CREATE PROCEDURE [dbo].[CreateRunPom]
            (
                -- Add the parameters for the stored procedure here
                @RunId int,
                @calendarYear varchar(400),
                @createdBy varchar(400)
            )
            AS
            BEGIN
                -- SET NOCOUNT ON added to prevent extra result sets from
                -- interfering with SELECT statements.
                SET NOCOUNT ON

                declare @DateNow datetime, @pomDataMasterid int
                SET @DateNow = GETDATE()

                declare @oldCalcRunPomMasterId int
                SET @oldCalcRunPomMasterId = (select top 1 id from dbo.calculator_run_pom_data_master order by id desc)
                Update calculator_run_pom_data_master SET effective_to = @DateNow WHERE id = @oldCalcRunPomMasterId

                INSERT into dbo.calculator_run_pom_data_master
                (calendar_year, created_at, created_by, effective_from, effective_to)
                values
                (@calendarYear, @DateNow, @createdBy, @DateNow, NULL)

                SET @pomDataMasterid  = CAST(scope_identity() AS int);

                INSERT into 
                    dbo.calculator_run_pom_data_detail
                    (calculator_run_pom_data_master_id, 
                        load_ts,
                        organisation_id,
                        packaging_activity,
                        packaging_type,
                        packaging_class,
                        packaging_material,
                        packaging_material_weight,
                        submission_period,
                        submission_period_desc,
                        subsidiary_id,
                        submitter_id)
                SELECT  @pomDataMasterid,
                        load_ts,
                        organisation_id,
                        packaging_activity,
                        packaging_type,
                        packaging_class,
                        packaging_material,
                        packaging_material_weight,
                        submission_period,
                        submission_period_desc,
                        CASE            
                        WHEN LTRIM(RTRIM(subsidiary_id)) = ''''
                        THEN NULL
                        ELSE subsidiary_id
                        END         
                        as subsidiary_id,
                        submitter_id
                        from 
                        dbo.pom_data

                Update dbo.calculator_run Set calculator_run_pom_data_master_id = @pomDataMasterid where id = @RunId

            END'
            EXEC(@Sql)
GO

GRANT EXEC ON [dbo].[CreateRunPom] TO PUBLIC;
GO

IF OBJECT_ID(N'[dbo].[InsertInvoiceDetailsAtProducerLevel]', 'P') IS NOT NULL  
DROP PROCEDURE [dbo].[InsertInvoiceDetailsAtProducerLevel];
GO

DECLARE @sql NVARCHAR(MAX) 
SET @sql = N'CREATE PROCEDURE [dbo].[InsertInvoiceDetailsAtProducerLevel]
    (
        @instructionConfirmedBy NVARCHAR(4000),
        @instructionConfirmedDate DATETIME2(7),
        @calculatorRunID INT
    )
    AS
    BEGIN
        SET NOCOUNT OFF
        -- Temp table to hold calculated values
        CREATE TABLE #CalculatedValues (
            calculator_run_id INT,
            producer_id INT,
            total_producer_bill_with_bad_debt DECIMAL(18,2),
            current_year_invoice_total_to_date DECIMAL(18,2),
            amount_liability_difference_calc_vs_prev DECIMAL(18,2),
            suggested_billing_instruction NVARCHAR(4000),
            billing_instruction_accept_reject NVARCHAR(4000),
            invoice_amount DECIMAL(18,2),
            instruction_confirmed_by NVARCHAR(4000),
            instruction_confirmed_date DATETIME2(7),
            billing_instruction_id NVARCHAR(4000)
        );

        -- Insert into temp table
        INSERT INTO #CalculatedValues
        SELECT 
            prsbi.calculator_run_id,
            prsbi.producer_id,
            prsbi.total_producer_bill_with_bad_debt, 
            prsbi.current_year_invoice_total_to_date,
            prsbi.amount_liability_difference_calc_vs_prev,
            prsbi.suggested_billing_instruction,
            prsbi.billing_instruction_accept_reject,
            dbo.GetInvoiceAmount(
                prsbi.billing_instruction_accept_reject,
                prsbi.suggested_billing_instruction,
                prsbi.total_producer_bill_with_bad_debt,
                prsbi.amount_liability_difference_calc_vs_prev
            ) AS invoice_amount,
            @instructionConfirmedBy AS instruction_confirmed_by,
            @instructionConfirmedDate AS instruction_confirmed_date,
            CONCAT(prsbi.calculator_run_id, ''_'', prsbi.producer_id) AS billing_instruction_id
        FROM dbo.producer_resultfile_suggested_billing_instruction AS prsbi
        WHERE prsbi.calculator_run_id = @calculatorRunID

        -- First SELECT using the temp table
    	INSERT INTO [dbo].[producer_designated_run_invoice_instruction] (
    		producer_id,
    		calculator_run_id,
    		current_year_invoiced_total_after_this_run,
    		invoice_amount,
    		outstanding_balance,
    		billing_instruction_id,
    		instruction_confirmed_date,
    		instruction_confirmed_by
    		)
    			SELECT 
    				cv.producer_id,
    				cv.calculator_run_id,
    				dbo.GetCurrentYearInvoicedTotalAfterThisRun(
    					cv.billing_instruction_accept_reject,
    					cv.suggested_billing_instruction,
    					cv.current_year_invoice_total_to_date,
    					cv.invoice_amount
    				) AS current_year_invoiced_total_after_this_run,
    				cv.invoice_amount,
    				dbo.GetOutstandingBalance(
    					cv.billing_instruction_accept_reject,
    					cv.suggested_billing_instruction,
    					cv.total_producer_bill_with_bad_debt,
    					cv.amount_liability_difference_calc_vs_prev
    				) AS outstanding_balance,
    				cv.billing_instruction_id,
    				cv.instruction_confirmed_date,
    				cv.instruction_confirmed_by			
    			FROM #CalculatedValues AS cv;

        DROP TABLE #CalculatedValues;
    END'
EXEC(@sql)
GO

GRANT EXEC ON [dbo].[InsertInvoiceDetailsAtProducerLevel] TO PUBLIC;
GO


                IF OBJECT_ID(N'[dbo].[GetCurrentYearInvoicedTotalAfterThisRun]', 'FN') IS NOT NULL
                    DROP FUNCTION [dbo].GetCurrentYearInvoicedTotalAfterThisRun
GO

                DECLARE @sql NVARCHAR(MAX)
                SET @sql = N'CREATE FUNCTION [dbo].[GetCurrentYearInvoicedTotalAfterThisRun] (
                    @billingInstructionAcceptReject      VARCHAR(250),
                    @suggestedBillingInstruction         VARCHAR(250),
                    @currentYearInvoicedTotalToDate      DECIMAL(18,2),
                    @invoiceAmount                       DECIMAL(18,2)
                )
                RETURNS DECIMAL(18,2)
                AS
                BEGIN
                    -- Rule 1: Cancelled and Rejected instruction always returns last invoiced values
                    IF @suggestedBillingInstruction = ''CANCEL'' AND @billingInstructionAcceptReject = ''Rejected''
                         RETURN ISNULL(@currentYearInvoicedTotalToDate, 0);

                    -- Rule 2: Cancelled instruction always returns NULL
                    IF @suggestedBillingInstruction = ''CANCEL'' AND @billingInstructionAcceptReject = ''Accepted''
                        RETURN NULL;

                    -- Rule 3: Rejected INITIAL returns NULL
                    IF @billingInstructionAcceptReject = ''Rejected'' AND @suggestedBillingInstruction = ''INITIAL''
                        RETURN NULL;

                    -- Rule 4: Rejected (but not INITIAL) returns current total as-is
                    IF @billingInstructionAcceptReject = ''Rejected''
                        RETURN ISNULL(@currentYearInvoicedTotalToDate, 0);

                    -- Rule 5: Rebill replaces total with invoice amount
                    IF @suggestedBillingInstruction = ''REBILL''
                        RETURN ISNULL(@invoiceAmount, 0);

                    -- Rule 6: Accepted or any other case adds invoice amount
                    RETURN ISNULL(@currentYearInvoicedTotalToDate, 0) + ISNULL(@invoiceAmount, 0);
                END'
                EXEC(@sql)
GO

IF OBJECT_ID(N'[dbo].[GetInvoiceAmount]', 'FN') IS NOT NULL
    DROP FUNCTION [dbo].[GetInvoiceAmount]
GO

declare @sql nvarchar(max)
SET @sql = N'
CREATE FUNCTION [dbo].[GetInvoiceAmount] ( 
    @billingInstructionAcceptReject VARCHAR(250),
    @suggestedBillingInstruction    VARCHAR(250),
    @totalProducerBillWithBadDebtProvision DECIMAL(18,2),
    @LiabilityDifference           DECIMAL(18,2)
)
RETURNS DECIMAL(18,2)
AS
BEGIN
    IF @billingInstructionAcceptReject <> ''Accepted''
        RETURN NULL;

    RETURN 
        CASE 
            WHEN @suggestedBillingInstruction IN (''INITIAL'', ''REBILL'') THEN @totalProducerBillWithBadDebtProvision
            WHEN @suggestedBillingInstruction = ''DELTA'' THEN @LiabilityDifference
            ELSE NULL
        END;
END'

EXEC(@sql)
GO

IF OBJECT_ID(N'[dbo].[GetOutstandingBalance]', 'FN') IS NOT NULL  
DROP FUNCTION [dbo].GetOutstandingBalance
GO

DECLARE @sql NVARCHAR(MAX) 
SET @sql = N'CREATE FUNCTION [dbo].[GetOutstandingBalance] (
    @billingInstructionAcceptReject        VARCHAR(250),
    @suggestedBillingInstruction           VARCHAR(250),
    @totalProducerBillWithBadDebtProvision DECIMAL(18,2),
    @LiabilityDifference                   DECIMAL(18,2)
)
RETURNS DECIMAL(18,2)
AS
BEGIN
    RETURN 
        CASE 
            WHEN @billingInstructionAcceptReject <> ''Accepted'' AND @suggestedBillingInstruction = ''INITIAL''
                THEN @totalProducerBillWithBadDebtProvision

            WHEN @billingInstructionAcceptReject <> ''Accepted''
                THEN @LiabilityDifference

            ELSE NULL
        END;
END'
EXEC(@sql)
GO

GRANT EXEC ON [dbo].[GetCurrentYearInvoicedTotalAfterThisRun] TO PUBLIC;
GO

GRANT EXEC ON [dbo].[GetInvoiceAmount] TO PUBLIC;
GO

GRANT EXEC ON [dbo].[GetOutstandingBalance] TO PUBLIC;
GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20260220163836_RemoveStoredProcsAndFuncs';
GO

COMMIT;
GO

