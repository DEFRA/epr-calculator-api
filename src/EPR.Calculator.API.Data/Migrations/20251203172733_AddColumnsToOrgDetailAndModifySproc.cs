using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnsToOrgDetailAndModifySproc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "submission_period_desc",
                table: "organisation_data");

            migrationBuilder.RenameColumn(
                name: "submission_period_desc",
                table: "calculator_run_organization_data_detail",
                newName: "status_code");

            migrationBuilder.AlterColumn<string>(
                name: "obligation_status",
                table: "organisation_data",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "error_code",
                table: "organisation_data",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "num_days_obligated",
                table: "organisation_data",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "status_code",
                table: "organisation_data",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "obligation_status",
                table: "calculator_run_organization_data_detail",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "error_code",
                table: "calculator_run_organization_data_detail",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "num_days_obligated",
                table: "calculator_run_organization_data_detail",
                type: "int",
                nullable: true);

            migrationBuilder.InsertData(
                table: "error_type",
                columns: new[] { "id", "name" },
                values: new object[] { 11, "Missing POM data" });

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "error_type",
                keyColumn: "id",
                keyValue: 11);

            migrationBuilder.DropColumn(
                name: "error_code",
                table: "organisation_data");

            migrationBuilder.DropColumn(
                name: "num_days_obligated",
                table: "organisation_data");

            migrationBuilder.DropColumn(
                name: "status_code",
                table: "organisation_data");

            migrationBuilder.DropColumn(
                name: "error_code",
                table: "calculator_run_organization_data_detail");

            migrationBuilder.DropColumn(
                name: "num_days_obligated",
                table: "calculator_run_organization_data_detail");

            migrationBuilder.RenameColumn(
                name: "status_code",
                table: "calculator_run_organization_data_detail",
                newName: "submission_period_desc");

            migrationBuilder.AlterColumn<string>(
                name: "obligation_status",
                table: "organisation_data",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AddColumn<string>(
                name: "submission_period_desc",
                table: "organisation_data",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "obligation_status",
                table: "calculator_run_organization_data_detail",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

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
				EXEC(@Sql)";

            migrationBuilder.Sql(createRunOrgSqlString);
        }
    }
}
