using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCreateRunOrganizationSProc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var createRunSqlString = "IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND OBJECT_ID = OBJECT_ID('[dbo].[CreteRunOrganization]]'))\r\n" +
                                    "DROP PROCEDURE [dbo].[CreateRunOrganization];\r\n" +
                                    "declare @Sql varchar(max)\r\nSET @Sql = N'CREATE PROCEDURE [dbo].[CreateRunOrganization]\r\n" +
                                    "(\r\n@RunId int,\r\n@calendarYear varchar(400),\r\n@createdBy varchar(400)\r\n)\r\nAS\r\nBEGIN\r\nSET NOCOUNT ON\r\n\r\ndeclare @DateNow datetime, @orgDataMasterid int\r\nSET @DateNow = GETDATE()\r\n\r\ndeclare @oldCalcRunOrgMasterId int\r\nSET @oldCalcRunOrgMasterId = (select top 1 id from dbo.calculator_run_organization_data_master order by id desc)\r\n\r\nUpdate calculator_run_organization_data_master SET effective_to = @DateNow WHERE id = @oldCalcRunOrgMasterId\r\n\r\nINSERT into dbo.calculator_run_organization_data_master\r\n(calendar_year, created_at, created_by, effective_from, effective_to)\r\nvalues\r\n(@calendarYear, @DateNow, @createdBy, @DateNow, NULL)\r\n\r\nSET @orgDataMasterid  = CAST(scope_identity() AS int);\r\n\r\nINSERT \r\ninto \r\n    dbo.calculator_run_organization_data_detail\r\n    (calculator_run_organization_data_master_id, \r\n        load_ts,\r\n        organisation_id,\r\n        organisation_name,\r\n        trading_name,\r\n        submission_period_desc,\r\n        subsidiary_id,\r\n\t\t\t\t\t\t\tobligation_status, \r\n\t\t\t\t\t\t\tsubmitter_id)\r\nSELECT  @orgDataMasterid, \r\n        load_ts,\r\n        organisation_id,\r\n        organisation_name,\r\n        trading_name,\r\n        submission_period_desc\r\n        CASE\t\t\t\r\n\t\t\t\t\t\t\tWHEN LTRIM(RTRIM(subsidiary_id)) = ''\r\n\t\t\t\t\t\t\tTHEN NULL\r\n\t\t\t\t\t\t\tELSE subsidiary_id\r\n\t\t\t\t\t\t\tEND\t\t\t\r\n\t\t\t\t\t\t\tas subsidiary_id,\r\n\t\t\t\t\t\t\tobligation_status, \r\n\t\t\t\t\t\t\tsubmitter_id\r\n        from \r\n        dbo.organisation_data\r\n\r\nUpdate dbo.calculator_run Set calculator_run_organization_data_master_id = @orgDataMasterid where id = @RunId\r\n\r\nEND\r\n'\r\nEXEC(@Sql)";
            migrationBuilder.Sql(createRunSqlString);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var createRunSqlString = "IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND OBJECT_ID = OBJECT_ID('[dbo].[CreteRunOrganization]]'))\r\n" +
                "DROP PROCEDURE [dbo].[CreateRunOrganization];\r\n" +
                "\r\ndeclare @Sql varchar(max)\r\nSET @Sql = N'\r\nCREATE PROCEDURE [dbo].[CreateRunOrganization]\r\n                (\r\n                    @RunId int,\r\n                    @calendarYear varchar(400),\r\n                    @createdBy varchar(400)\r\n                )\r\n                AS\r\n                BEGIN\r\n                    SET NOCOUNT ON\r\n\r\n                    declare @DateNow datetime, @orgDataMasterid int\r\n                    SET @DateNow = GETDATE()\r\n\r\n                    declare @oldCalcRunOrgMasterId int\r\n                    SET @oldCalcRunOrgMasterId = (select top 1 id from dbo.calculator_run_organization_data_master order by id desc)\r\n\r\n                    Update calculator_run_organization_data_master SET effective_to = @DateNow WHERE id = @oldCalcRunOrgMasterId\r\n\r\n                    INSERT into dbo.calculator_run_organization_data_master\r\n                    (calendar_year, created_at, created_by, effective_from, effective_to)\r\n                    values\r\n                    (@calendarYear, @DateNow, @createdBy, @DateNow, NULL)\r\n\r\n                    SET @orgDataMasterid  = CAST(scope_identity() AS int);\r\n\r\n                    INSERT \r\n                    into \r\n                        dbo.calculator_run_organization_data_detail\r\n                        (calculator_run_organization_data_master_id, \r\n                            load_ts,\r\n                            organisation_id,\r\n                            organisation_name,\r\n                            trading_name,\r\n                            submission_period_desc,\r\n                            subsidiary_id)\r\n                    SELECT  @orgDataMasterid, \r\n                            load_ts,\r\n                            organisation_id,\r\n                            organisation_name,\r\n                            trading_name,\r\n                            submission_period_desc,\r\n                            CASE\t\t\t\r\n\t\t\t\t\t\t\tWHEN LTRIM(RTRIM(subsidiary_id)) = ''\r\n\t\t\t\t\t\t\tTHEN NULL\r\n\t\t\t\t\t\t\tELSE subsidiary_id\r\n\t\t\t\t\t\t\tEND\t\t\t\r\n\t\t\t\t\t\t\tas subsidiary_id \r\n                            from \r\n                            dbo.organisation_data\r\n\r\n                    Update dbo.calculator_run Set calculator_run_organization_data_master_id = @orgDataMasterid where id = @RunId\r\n\r\n                END'\r\n\t\t\t\tEXEC(@Sql)";


            migrationBuilder.Sql(createRunSqlString);
        }
    }
}
