using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrganisationSproc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop the existing procedure if it exists
            var dropOrgProcSql = @"
                IF OBJECT_ID(N'[dbo].[CreateRunOrganization]', N'P') IS NOT NULL
                    DROP PROCEDURE [dbo].[CreateRunOrganization]";
            migrationBuilder.Sql(dropOrgProcSql);

            // Recreate the procedure using EXEC(@Sql) pattern
            var createOrgProcSql = @"
                DECLARE @Sql NVARCHAR(MAX)
                SET @Sql = N'
                CREATE PROCEDURE [dbo].[CreateRunOrganization]
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

                END'
                EXEC(@Sql)";
            migrationBuilder.Sql(createOrgProcSql);
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the existing procedure if it exists
            var dropOrgProcSql = @"
                IF OBJECT_ID(N'[dbo].[CreateRunOrganization]', N'P') IS NOT NULL
                    DROP PROCEDURE [dbo].[CreateRunOrganization]";
            migrationBuilder.Sql(dropOrgProcSql);

            // Recreate the original procedure using EXEC(@Sql)
            var revertOrgProcSql = @"
                DECLARE @Sql NVARCHAR(MAX)
                SET @Sql = N'
                CREATE PROCEDURE [dbo].[CreateRunOrganization]
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

                END'
                EXEC(@Sql)";
            migrationBuilder.Sql(revertOrgProcSql);
        }

    }
}
