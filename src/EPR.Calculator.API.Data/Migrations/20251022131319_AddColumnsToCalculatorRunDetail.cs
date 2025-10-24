using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnsToCalculatorRunDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_valid",
                table: "calculator_run_pom_data_detail",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "submitter_org_id",
                table: "calculator_run_pom_data_detail",
                type: "nvarchar(4000)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_valid",
                table: "calculator_run_organization_data_detail",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "joiner_date",
                table: "calculator_run_organization_data_detail",
                type: "nvarchar(4000)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "leaver_code",
                table: "calculator_run_organization_data_detail",
                type: "nvarchar(4000)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "leaver_date",
                table: "calculator_run_organization_data_detail",
                type: "nvarchar(4000)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "submitter_org_id",
                table: "calculator_run_organization_data_detail",
                type: "nvarchar(4000)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_valid",
                table: "calculator_run_pom_data_detail");

            migrationBuilder.DropColumn(
                name: "SubmitterOrgId",
                table: "calculator_run_pom_data_detail");

            migrationBuilder.DropColumn(
                name: "is_valid",
                table: "calculator_run_organization_data_detail");

            migrationBuilder.DropColumn(
                name: "joiner_date",
                table: "calculator_run_organization_data_detail");

            migrationBuilder.DropColumn(
                name: "leaver_code",
                table: "calculator_run_organization_data_detail");

            migrationBuilder.DropColumn(
                name: "leaver_date",
                table: "calculator_run_organization_data_detail");

            migrationBuilder.DropColumn(
                name: "submitter_org_id",
                table: "calculator_run_organization_data_detail");
        }
    }
}
