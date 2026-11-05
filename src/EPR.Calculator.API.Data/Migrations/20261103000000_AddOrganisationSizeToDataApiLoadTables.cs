using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganisationSizeToDataApiLoadTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Existing rows get an empty size, which the size filter excludes, until the next refresh replaces them.
            migrationBuilder.Sql("ALTER TABLE [data_api_load_organisations] ADD [organisation_size] nvarchar(10) NOT NULL CONSTRAINT [DF_data_api_load_organisations_organisation_size] DEFAULT '';");
            migrationBuilder.Sql("ALTER TABLE [data_api_load_organisations] DROP CONSTRAINT [DF_data_api_load_organisations_organisation_size];");
            migrationBuilder.Sql("ALTER TABLE [data_api_load_poms]          ADD [organisation_size] nvarchar(10) NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE [data_api_load_organisations] DROP COLUMN [organisation_size];");
            migrationBuilder.Sql("ALTER TABLE [data_api_load_poms]          DROP COLUMN [organisation_size];");
        }
    }
}
