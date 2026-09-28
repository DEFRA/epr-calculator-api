using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class PomAndOrganisationProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // CREATE PROCEDURE can't sit inside the IF block that idempotent scripts wrap each statement in, hence EXEC.
            // Pass the SQL to EXEC directly rather than through a declared variable: EF Core 9+ scripts emit a whole
            // migration as one batch, so two Sql() calls that each DECLARE @Sql fail with "The variable name '@Sql'
            // has already been declared".
            migrationBuilder.Sql(
                """
                EXEC(N'CREATE PROCEDURE [dbo].[CreateRunPom]
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

                    SET @pomDataMasterid = CAST(scope_identity() AS int);

                    INSERT into dbo.calculator_run_pom_data_detail
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
                    SELECT @pomDataMasterid,
                        load_ts,
                        organisation_id,
                        packaging_activity,
                        packaging_type,
                        packaging_class,
                        packaging_material,
                        packaging_material_weight,
                        submission_period,
                        submission_period_desc,
                        subsidiary_id
                    from dbo.pom_data

                    Update dbo.calculator_run Set calculator_run_pom_data_master_id = @pomDataMasterid where id = @RunId
                END')
                """);

            migrationBuilder.Sql(
                """
                EXEC(N'CREATE PROCEDURE [dbo].[CreateRunOrganization]
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

                    declare @DateNow datetime, @orgDataMasterid int
                    SET @DateNow = GETDATE()

                    declare @oldCalcRunOrgMasterId int
                    SET @oldCalcRunOrgMasterId = (select top 1 id from dbo.calculator_run_organization_data_master order by id desc)
                    Update calculator_run_organization_data_master SET effective_to = @DateNow WHERE id = @oldCalcRunOrgMasterId

                    INSERT into dbo.calculator_run_organization_data_master
                        (calendar_year, created_at, created_by, effective_from, effective_to)
                    values
                        (@calendarYear, @DateNow, @createdBy, @DateNow, NULL)

                    SET @orgDataMasterid = CAST(scope_identity() AS int);

                    INSERT into dbo.calculator_run_organization_data_detail
                        (calculator_run_organization_data_master_id,
                        load_ts,
                        organisation_id,
                        organisation_name,
                        submission_period_desc,
                        subsidiary_id)
                    SELECT @orgDataMasterid,
                        load_ts,
                        organisation_id,
                        organisation_name,
                        submission_period_desc,
                        subsidiary_id
                    from dbo.organisation_data

                    Update dbo.calculator_run Set calculator_run_organization_data_master_id = @orgDataMasterid where id = @RunId
                END')
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var dropOrgSql = "DROP PROCEDURE[dbo].[CreateRunOrganization]";
            migrationBuilder.Sql(dropOrgSql);
            var dropPomSql = "DROP PROCEDURE[dbo].[CreateRunPom]";
            migrationBuilder.Sql(dropPomSql);
        }
    }
}
