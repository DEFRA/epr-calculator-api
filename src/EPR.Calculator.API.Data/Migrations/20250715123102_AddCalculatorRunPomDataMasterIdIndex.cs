using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCalculatorRunPomDataMasterIdIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_calculator_run_pom_data_detail_organisation_id",
                table: "calculator_run_pom_data_detail");

            migrationBuilder.CreateIndex(
                name: "IX_calculator_run_pom_data_detail_organisation_id_calculator_run_pom_data_master_id",
                table: "calculator_run_pom_data_detail",
                columns: new[] { "organisation_id", "calculator_run_pom_data_master_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_calculator_run_pom_data_detail_organisation_id_calculator_run_pom_data_master_id",
                table: "calculator_run_pom_data_detail");

            migrationBuilder.CreateIndex(
                name: "IX_calculator_run_pom_data_detail_organisation_id",
                table: "calculator_run_pom_data_detail",
                column: "organisation_id");
        }
    }
}
