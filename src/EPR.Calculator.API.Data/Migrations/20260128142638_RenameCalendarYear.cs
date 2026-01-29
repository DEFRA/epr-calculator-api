using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameCalendarYear : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "calendar_year",
                table: "calculator_run_pom_data_master",
                newName: "relative_year");

            migrationBuilder.RenameColumn(
                name: "calendar_year",
                table: "calculator_run_organization_data_master",
                newName: "relative_year");

            var createRunOrgSqlString = @"IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreateRunOrganization]') AND type = N'P')
                DROP PROCEDURE [dbo].[CreateRunOrganization];
                declare @Sql varchar(max);
                SET @Sql = N'CREATE PROCEDURE [dbo].[CreateRunOrganization]                
                (                    @RunId int,                    @relativeyear varchar(400),                    @createdBy varchar(400)                )                
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
                        (relative_year, created_at, created_by, effective_from, effective_to)                    
                    values                    
                        (@relativeyear, @DateNow, @createdBy, @DateNow, NULL)                    
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
                        error_code,
                        joiner_date,
                        leaver_date)                    
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
                    error_code,
                    joiner_date,
                    leaver_date
                    from                             
                        dbo.organisation_data                    
                    Update dbo.calculator_run Set calculator_run_organization_data_master_id = @orgDataMasterid where id = @RunId                
                    END'
                EXEC(@Sql)";

            migrationBuilder.Sql(createRunOrgSqlString);

            // Grant EXECUTE on stored procedures to PUBLIC
            migrationBuilder.Sql(@"GRANT EXEC ON [dbo].[CreateRunOrganization] TO PUBLIC;");

            var createRunPomSqlString = @"IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreateRunPom]') AND type = N'P')
			DROP PROCEDURE [dbo].[CreateRunPom];
                declare @Sql varchar(max);
				SET @Sql = N'CREATE PROCEDURE [dbo].[CreateRunPom]
		(
			-- Add the parameters for the stored procedure here
			@RunId int,
			@relativeyear varchar(400),
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
			(relative_year, created_at, created_by, effective_from, effective_to)
			values
			(@relativeyear, @DateNow, @createdBy, @DateNow, NULL)

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
		EXEC(@Sql)";
            migrationBuilder.Sql(createRunPomSqlString);

            // Grant EXECUTE on stored procedures to PUBLIC
            migrationBuilder.Sql(@"GRANT EXEC ON [dbo].[CreateRunPom] TO PUBLIC;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "relative_year",
                table: "calculator_run_pom_data_master",
                newName: "calendar_year");

            migrationBuilder.RenameColumn(
                name: "relative_year",
                table: "calculator_run_organization_data_master",
                newName: "calendar_year");

            var createRunOrgSqlString = @"IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreateRunOrganization]') AND type = N'P')
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
                        error_code,
                        joiner_date,
                        leaver_date)                    
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
                    error_code,
                    joiner_date,
                    leaver_date
                    from                             
                        dbo.organisation_data                    
                    Update dbo.calculator_run Set calculator_run_organization_data_master_id = @orgDataMasterid where id = @RunId                
                    END'
                EXEC(@Sql)";

            migrationBuilder.Sql(createRunOrgSqlString);

            // Grant EXECUTE on stored procedures to PUBLIC
            migrationBuilder.Sql(@"GRANT EXEC ON [dbo].[CreateRunOrganization] TO PUBLIC;");

            var createRunPomSqlString = @"IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreateRunPom]') AND type = N'P')
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
		EXEC(@Sql)";
            migrationBuilder.Sql(createRunPomSqlString);

            // Grant EXECUTE on stored procedures to PUBLIC
            migrationBuilder.Sql(@"GRANT EXEC ON [dbo].[CreateRunPom] TO PUBLIC;");
        }
    }
}
