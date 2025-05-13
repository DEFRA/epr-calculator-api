using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlterCreateRunOrganisationSproc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var alterOrgSql = @"
                ALTER PROCEDURE [dbo].[CreateRunOrganization]
                (
                    @RunId int,
                    @calendarYear varchar(400),
                    @createdBy varchar(400)
                )
                AS
                BEGIN
                    SET NOCOUNT ON

                    declare @DateNow datetime, @orgDataMasterid int
                    SET @DateNow = GETDATE()

                    declare @oldCalcRunOrgMasterId int
                    SET @oldCalcRunOrgMasterId = (select top 1 id from dbo.calculator_run_organization_data_master order by id desc)

                    Update calculator_run_organization_data_master SET effective_to = @DateNow WHERE id = @oldCalcRunOrgMasterId

                    INSERT into dbo.calculator_run_organization_data_master
                    (calendar_year, created_at, created_by, effective_from, effective_to)
                    values
                    (@calendarYear, @DateNow, @createdBy, @DateNow, NULL)

                    SET @orgDataMasterid  = CAST(scope_identity() AS int);

                    INSERT 
                    into 
                        dbo.calculator_run_organization_data_detail
                        (calculator_run_organization_data_master_id, 
                            load_ts,
                            organisation_id,
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
                            subsidiary_id  
                            from 
                            dbo.organisation_data

                    Update dbo.calculator_run Set calculator_run_organization_data_master_id = @orgDataMasterid where id = @RunId

                END";
            migrationBuilder.Sql(alterOrgSql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var revertOrgSql = @"
                ALTER PROCEDURE [dbo].[CreateRunOrganization]
                (
                    @RunId int,
                    @calendarYear varchar(400),
                    @createdBy varchar(400)
                )
                AS
                BEGIN
                    SET NOCOUNT ON

                    declare @DateNow datetime, @orgDataMasterid int
                    SET @DateNow = GETDATE()

                    declare @oldCalcRunOrgMasterId int
                    SET @oldCalcRunOrgMasterId = (select top 1 id from dbo.calculator_run_organization_data_master order by id desc)

                    Update calculator_run_organization_data_master SET effective_to = @DateNow WHERE id = @oldCalcRunOrgMasterId

                    INSERT into dbo.calculator_run_organization_data_master
                    (calendar_year, created_at, created_by, effective_from, effective_to)
                    values
                    (@calendarYear, @DateNow, @createdBy, @DateNow, NULL)

                    SET @orgDataMasterid  = CAST(scope_identity() AS int);

                    INSERT 
                    into 
                        dbo.calculator_run_organization_data_detail
                        (calculator_run_organization_data_master_id, 
                            load_ts,
                            organisation_id,
                            organisation_name,			
                            submission_period_desc,
                            subsidiary_id)
                    SELECT  @orgDataMasterid, 
                            load_ts,
                            organisation_id,
                            organisation_name,				
                            submission_period_desc,
                            subsidiary_id  
                            from 
                            dbo.organisation_data

                    Update dbo.calculator_run Set calculator_run_organization_data_master_id = @orgDataMasterid where id = @RunId

                END";
            migrationBuilder.Sql(revertOrgSql);
        }
    }
}
