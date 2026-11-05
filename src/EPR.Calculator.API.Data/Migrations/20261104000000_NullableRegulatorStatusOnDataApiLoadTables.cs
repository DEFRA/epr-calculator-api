using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class NullableRegulatorStatusOnDataApiLoadTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE [data_api_load_organisations] ALTER COLUMN [regulator_status] nvarchar(50) NULL;");
            migrationBuilder.Sql("ALTER TABLE [data_api_load_poms]          ADD [regulator_status] nvarchar(50) NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rows can have a null regulator_status, and we can't guarantee the winners under the older rules, so
            // remove them all so no run is attempted with this data. The next load replaces them.
            migrationBuilder.Sql("DELETE FROM [data_api_load_organisations];");
            migrationBuilder.Sql("DELETE FROM [data_api_load_poms];");
            migrationBuilder.Sql("ALTER TABLE [data_api_load_poms]          DROP COLUMN [regulator_status];");
            migrationBuilder.Sql("ALTER TABLE [data_api_load_organisations] ALTER COLUMN [regulator_status] nvarchar(50) NOT NULL;");
        }
    }
}
