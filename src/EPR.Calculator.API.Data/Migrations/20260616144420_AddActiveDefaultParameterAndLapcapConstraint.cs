using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddActiveDefaultParameterAndLapcapConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_lapcap_data_master_relative_year",
                table: "lapcap_data_master");

            migrationBuilder.DropIndex(
                name: "IX_default_parameter_setting_master_relative_year",
                table: "default_parameter_setting_master");

            migrationBuilder.CreateIndex(
                name: "UX_lapcap_data_master_active_relative_year",
                table: "lapcap_data_master",
                column: "relative_year",
                unique: true,
                filter: "effective_to IS NULL");

            migrationBuilder.CreateIndex(
                name: "UX_default_parameter_setting_master_active_relative_year",
                table: "default_parameter_setting_master",
                column: "relative_year",
                unique: true,
                filter: "effective_to IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_lapcap_data_master_active_relative_year",
                table: "lapcap_data_master");

            migrationBuilder.DropIndex(
                name: "UX_default_parameter_setting_master_active_relative_year",
                table: "default_parameter_setting_master");

            migrationBuilder.CreateIndex(
                name: "IX_lapcap_data_master_relative_year",
                table: "lapcap_data_master",
                column: "relative_year");

            migrationBuilder.CreateIndex(
                name: "IX_default_parameter_setting_master_relative_year",
                table: "default_parameter_setting_master",
                column: "relative_year");
        }
    }
}
