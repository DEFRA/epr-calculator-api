using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class JoinerLeaverDatesOrg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "joiner_date",
                table: "organisation_data",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "leaver_date",
                table: "organisation_data",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "joiner_date",
                table: "calculator_run_organization_data_detail",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "leaver_date",
                table: "calculator_run_organization_data_detail",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

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

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "joiner_date",
                table: "organisation_data");

            migrationBuilder.DropColumn(
                name: "leaver_date",
                table: "organisation_data");

            migrationBuilder.DropColumn(
                name: "joiner_date",
                table: "calculator_run_organization_data_detail");

            migrationBuilder.DropColumn(
                name: "leaver_date",
                table: "calculator_run_organization_data_detail");

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
                EXEC(@Sql)";

            migrationBuilder.Sql(createRunOrgSqlString);

            // Grant EXECUTE on stored procedures to PUBLIC
            migrationBuilder.Sql(@"GRANT EXEC ON [dbo].[CreateRunOrganization] TO PUBLIC;");
        }
    }
}
