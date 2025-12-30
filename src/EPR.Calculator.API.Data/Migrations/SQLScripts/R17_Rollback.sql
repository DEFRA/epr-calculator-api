BEGIN TRANSACTION;
GO

DECLARE @var0 sysname;
SELECT @var0 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[error_report]') AND [c].[name] = N'error_code');
IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [error_report] DROP CONSTRAINT [' + @var0 + '];');
ALTER TABLE [error_report] DROP COLUMN [error_code];
GO

ALTER TABLE [error_report] ADD [error_type_id] int NOT NULL DEFAULT 0;
GO

CREATE TABLE [error_type] (
    [id] int NOT NULL IDENTITY,
    [name] nvarchar(250) NOT NULL,
    CONSTRAINT [PK_error_type] PRIMARY KEY ([id])
);
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'id', N'name') AND [object_id] = OBJECT_ID(N'[error_type]'))
    SET IDENTITY_INSERT [error_type] ON;
INSERT INTO [error_type] ([id], [name])
VALUES (1, N'Missing Registration Data'),
(2, N'Conflicting Obligations (Leaver Codes)'),
(3, N'Conflicting Obligations (Blank)'),
(4, N'No longer trading'),
(5, N'Not Obligated'),
(6, N'Compliance Scheme Leaver'),
(7, N'Compliance Scheme to Direct Producer'),
(8, N'Invalid Leaver Code'),
(9, N'Date input issue'),
(10, N'Invalid Organisation ID'),
(11, N'Missing POM data');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'id', N'name') AND [object_id] = OBJECT_ID(N'[error_type]'))
    SET IDENTITY_INSERT [error_type] OFF;
GO

CREATE INDEX [IX_error_report_error_type_id] ON [error_report] ([error_type_id]);
GO

CREATE UNIQUE INDEX [IX_error_type_name] ON [error_type] ([name]);
GO

ALTER TABLE [error_report] ADD CONSTRAINT [FK_error_report_error_type_error_type_id] FOREIGN KEY ([error_type_id]) REFERENCES [error_type] ([id]) ON DELETE CASCADE;
GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20251209164843_AmendErrorReport';
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

REVOKE EXECUTE ON [dbo].[InsertInvoiceDetailsAtProducerLevel] FROM PUBLIC;
GO

REVOKE EXECUTE ON [dbo].[CreateRunPom] FROM PUBLIC;
GO

REVOKE EXECUTE ON [dbo].[CreateRunOrganization] FROM PUBLIC;
GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20251209101514_GrantPermissions';
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DELETE FROM [error_type]
WHERE [id] = 11;
SELECT @@ROWCOUNT;

GO

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[organisation_data]') AND [c].[name] = N'error_code');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [organisation_data] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [organisation_data] DROP COLUMN [error_code];
GO

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[organisation_data]') AND [c].[name] = N'num_days_obligated');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [organisation_data] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [organisation_data] DROP COLUMN [num_days_obligated];
GO

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[organisation_data]') AND [c].[name] = N'status_code');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [organisation_data] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [organisation_data] DROP COLUMN [status_code];
GO

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[calculator_run_organization_data_detail]') AND [c].[name] = N'error_code');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [calculator_run_organization_data_detail] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [calculator_run_organization_data_detail] DROP COLUMN [error_code];
GO

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[calculator_run_organization_data_detail]') AND [c].[name] = N'num_days_obligated');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [calculator_run_organization_data_detail] DROP CONSTRAINT [' + @var5 + '];');
ALTER TABLE [calculator_run_organization_data_detail] DROP COLUMN [num_days_obligated];
GO

EXEC sp_rename N'[calculator_run_organization_data_detail].[status_code]', N'submission_period_desc', N'COLUMN';
GO

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[organisation_data]') AND [c].[name] = N'organisation_id');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [organisation_data] DROP CONSTRAINT [' + @var6 + '];');
ALTER TABLE [organisation_data] ALTER COLUMN [organisation_id] int NULL;
GO

DECLARE @var7 sysname;
SELECT @var7 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[organisation_data]') AND [c].[name] = N'obligation_status');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [organisation_data] DROP CONSTRAINT [' + @var7 + '];');
ALTER TABLE [organisation_data] ALTER COLUMN [obligation_status] nvarchar(max) NOT NULL;
GO

ALTER TABLE [organisation_data] ADD [submission_period_desc] nvarchar(max) NOT NULL DEFAULT N'';
GO

DECLARE @var8 sysname;
SELECT @var8 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[calculator_run_organization_data_detail]') AND [c].[name] = N'organisation_id');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [calculator_run_organization_data_detail] DROP CONSTRAINT [' + @var8 + '];');
ALTER TABLE [calculator_run_organization_data_detail] ALTER COLUMN [organisation_id] int NULL;
GO

DECLARE @var9 sysname;
SELECT @var9 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[calculator_run_organization_data_detail]') AND [c].[name] = N'obligation_status');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [calculator_run_organization_data_detail] DROP CONSTRAINT [' + @var9 + '];');
ALTER TABLE [calculator_run_organization_data_detail] ALTER COLUMN [obligation_status] nvarchar(max) NOT NULL;
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
						submission_period_desc,                            
						subsidiary_id,
						obligation_status,
						submitter_id)                    
					SELECT  @orgDataMasterid,                             
					load_ts,                            
					organisation_id,                            
					organisation_name,                            
					trading_name,                            
					submission_period_desc,                            
					CASE WHEN LTRIM(RTRIM(subsidiary_id)) = '''' THEN NULL ELSE subsidiary_id END as subsidiary_id,
					obligation_status,
					submitter_id
					from                             
						dbo.organisation_data                    
					Update dbo.calculator_run Set calculator_run_organization_data_master_id = @orgDataMasterid where id = @RunId                
					END'
				EXEC(@Sql)
GO

GRANT EXEC ON [dbo].[InsertInvoiceDetailsAtProducerLevel] TO PUBLIC;
GO

GRANT EXEC ON [dbo].[CreateRunPom] TO PUBLIC;
GO

GRANT EXEC ON [dbo].[CreateRunOrganization] TO PUBLIC;
GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20251204164331_AddColumnsToOrgDetailAndModifySproc';
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
						submission_period_desc,                            
						subsidiary_id)                    
					SELECT  @orgDataMasterid,                             
					load_ts,                            
					organisation_id,                            
					organisation_name,                            
					trading_name,                            
					submission_period_desc,                            
					CASE WHEN LTRIM(RTRIM(subsidiary_id)) = '''' THEN NULL ELSE subsidiary_id END as subsidiary_id                             
					from                             
						dbo.organisation_data                    
					Update dbo.calculator_run Set calculator_run_organization_data_master_id = @orgDataMasterid where id = @RunId                
					END'
				EXEC(@Sql)
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
						subsidiary_id)
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
						as subsidiary_id
						from 
						dbo.pom_data

				 Update dbo.calculator_run Set calculator_run_pom_data_master_id = @pomDataMasterid where id = @RunId

			END'
			EXEC(@Sql)
GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20251117130042_ModifyCreateRunOrgAndCreateRunPOMSprocs';
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

DECLARE @var10 sysname;
SELECT @var10 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[pom_data]') AND [c].[name] = N'submitter_id');
IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [pom_data] DROP CONSTRAINT [' + @var10 + '];');
ALTER TABLE [pom_data] DROP COLUMN [submitter_id];
GO

DECLARE @var11 sysname;
SELECT @var11 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[organisation_data]') AND [c].[name] = N'obligation_status');
IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [organisation_data] DROP CONSTRAINT [' + @var11 + '];');
ALTER TABLE [organisation_data] DROP COLUMN [obligation_status];
GO

DECLARE @var12 sysname;
SELECT @var12 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[organisation_data]') AND [c].[name] = N'submitter_id');
IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [organisation_data] DROP CONSTRAINT [' + @var12 + '];');
ALTER TABLE [organisation_data] DROP COLUMN [submitter_id];
GO

DECLARE @var13 sysname;
SELECT @var13 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[calculator_run_pom_data_detail]') AND [c].[name] = N'submitter_id');
IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [calculator_run_pom_data_detail] DROP CONSTRAINT [' + @var13 + '];');
ALTER TABLE [calculator_run_pom_data_detail] DROP COLUMN [submitter_id];
GO

DECLARE @var14 sysname;
SELECT @var14 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[calculator_run_organization_data_detail]') AND [c].[name] = N'obligation_status');
IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [calculator_run_organization_data_detail] DROP CONSTRAINT [' + @var14 + '];');
ALTER TABLE [calculator_run_organization_data_detail] DROP COLUMN [obligation_status];
GO

DECLARE @var15 sysname;
SELECT @var15 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[calculator_run_organization_data_detail]') AND [c].[name] = N'submitter_id');
IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [calculator_run_organization_data_detail] DROP CONSTRAINT [' + @var15 + '];');
ALTER TABLE [calculator_run_organization_data_detail] DROP COLUMN [submitter_id];
GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20251117125358_AddSubmitterIdObligationStatusInPOMandOrg';
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

GRANT EXEC ON [dbo].[InsertInvoiceDetailsAtProducerLevel] TO PUBLIC;
GO

GRANT EXEC ON [dbo].[CreateRunPom] TO PUBLIC;
GO

GRANT EXEC ON [dbo].[CreateRunOrganization] TO PUBLIC;
GO

COMMIT;
GO

