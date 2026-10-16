using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganisationSizeToDataApiLoadTables : Migration
    {
        private static readonly string[] LoadTables = ["data_api_load_organisations", "data_api_load_poms"];

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rows already staged came from sprocs that only returned large organisations.
            foreach (var table in LoadTables)
            {
                migrationBuilder.Sql($@"
                    IF COL_LENGTH(N'[{table}]', N'organisation_size') IS NULL
                    ALTER TABLE [{table}]
                        ADD [organisation_size] nvarchar(10) NULL
                        CONSTRAINT [DF_{table}_organisation_size] DEFAULT N'L' WITH VALUES;");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var table in LoadTables)
            {
                migrationBuilder.Sql($@"
                    IF COL_LENGTH(N'[{table}]', N'organisation_size') IS NOT NULL
                    BEGIN
                        ALTER TABLE [{table}] DROP CONSTRAINT [DF_{table}_organisation_size];
                        ALTER TABLE [{table}] DROP COLUMN [organisation_size];
                    END");
            }
        }
    }
}
