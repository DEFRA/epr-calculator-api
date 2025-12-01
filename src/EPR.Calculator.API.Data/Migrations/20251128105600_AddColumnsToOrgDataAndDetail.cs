using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnsToOrgDataAndDetail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "obligation_status",
                table: "organisation_data",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "error_code_desc",
                table: "organisation_data",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "partial_obligation_percentage",
                table: "organisation_data",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "status_code",
                table: "organisation_data",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "obligation_status",
                table: "calculator_run_organization_data_detail",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "error_code_desc",
                table: "calculator_run_organization_data_detail",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "partial_obligation_percentage",
                table: "calculator_run_organization_data_detail",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "status_code",
                table: "calculator_run_organization_data_detail",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "error_type",
                columns: new[] { "id", "name" },
                values: new object[] { 11, "Missing POM data" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "error_type",
                keyColumn: "id",
                keyValue: 11);

            migrationBuilder.DropColumn(
                name: "error_code_desc",
                table: "organisation_data");

            migrationBuilder.DropColumn(
                name: "partial_obligation_percentage",
                table: "organisation_data");

            migrationBuilder.DropColumn(
                name: "status_code",
                table: "organisation_data");

            migrationBuilder.DropColumn(
                name: "error_code_desc",
                table: "calculator_run_organization_data_detail");

            migrationBuilder.DropColumn(
                name: "partial_obligation_percentage",
                table: "calculator_run_organization_data_detail");

            migrationBuilder.DropColumn(
                name: "status_code",
                table: "calculator_run_organization_data_detail");

            migrationBuilder.AlterColumn<string>(
                name: "obligation_status",
                table: "organisation_data",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "obligation_status",
                table: "calculator_run_organization_data_detail",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);
        }
    }
}
