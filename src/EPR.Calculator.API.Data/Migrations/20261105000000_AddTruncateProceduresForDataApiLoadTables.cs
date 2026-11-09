using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTruncateProceduresForDataApiLoadTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Lets the API truncate these tables without having ALTER on them. Skipped unless the login can create
            // EXECUTE AS OWNER procedures and grant on them, as the API falls back to DELETE without them.
            // EXEC because CREATE PROCEDURE can't sit inside an IF block.
            // The grants go to the API's identity in each environment. Each database only has its own, and granting
            // to a missing user fails.
            migrationBuilder.Sql(
                """
                IF HAS_PERMS_BY_NAME(NULL, N'DATABASE', N'CONTROL') = 1
                BEGIN
                    EXEC(N'CREATE PROCEDURE dbo.TruncateDataApiLoadOrganisations WITH EXECUTE AS OWNER AS TRUNCATE TABLE dbo.data_api_load_organisations;');
                    EXEC(N'CREATE PROCEDURE dbo.TruncateDataApiLoadPoms WITH EXECUTE AS OWNER AS TRUNCATE TABLE dbo.data_api_load_poms;');

                    DECLARE @grants nvarchar(max) = (
                        SELECT STRING_AGG(CONVERT(nvarchar(max),
                            N'GRANT EXECUTE ON dbo.TruncateDataApiLoadOrganisations TO ' + name + N'; '
                            + N'GRANT EXECUTE ON dbo.TruncateDataApiLoadPoms TO ' + name + N';'), N' ')
                        FROM sys.database_principals
                        WHERE name IN (
                            N'DEVRWDWEBWA1422', N'DEVRWDWEBWA2422', N'DEVRWDWEBWA3422', N'DEVRWDWEBWA4422',
                            N'DEVRWDWEBWA7422', N'DEVRWDWEBWA8422', N'DEVRWDWEBWA9422', N'DEVRWDWEBWAE422',
                            N'DEVRWDWEBWAF422', N'TSTRWDWEBWA1422', N'PRERWDWEBWA1422', N'PRERWDWEBWA2422',
                            N'PRDRWDWEBWA1422'));

                    EXEC(@grants);
                END
                ELSE
                    PRINT N'Skipped creating the DataApi load table truncate procedures: the login does not have CONTROL on the database.';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.TruncateDataApiLoadPoms;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.TruncateDataApiLoadOrganisations;");
        }
    }
}
