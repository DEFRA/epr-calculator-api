using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class OrganisationIDToInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "organisation_id",
                table: "pom_data",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(400)",
                oldMaxLength: 400,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "organisation_id",
                table: "organisation_data",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(400)",
                oldMaxLength: 400,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "organisation_id",
                table: "calculator_run_pom_data_detail",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(400)",
                oldMaxLength: 400,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "organisation_id",
                table: "calculator_run_organization_data_detail",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(400)",
                oldMaxLength: 400,
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 1,
                column: "created_at",
                value: new DateTime(2024, 10, 24, 13, 55, 26, 589, DateTimeKind.Local).AddTicks(5014));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 2,
                column: "created_at",
                value: new DateTime(2024, 10, 24, 13, 55, 26, 589, DateTimeKind.Local).AddTicks(5017));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 3,
                column: "created_at",
                value: new DateTime(2024, 10, 24, 13, 55, 26, 589, DateTimeKind.Local).AddTicks(5019));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 4,
                column: "created_at",
                value: new DateTime(2024, 10, 24, 13, 55, 26, 589, DateTimeKind.Local).AddTicks(5022));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 5,
                column: "created_at",
                value: new DateTime(2024, 10, 24, 13, 55, 26, 589, DateTimeKind.Local).AddTicks(5024));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "organisation_id",
                table: "pom_data",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "organisation_id",
                table: "organisation_data",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "organisation_id",
                table: "calculator_run_pom_data_detail",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "organisation_id",
                table: "calculator_run_organization_data_detail",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 1,
                column: "created_at",
                value: new DateTime(2024, 10, 21, 14, 0, 55, 593, DateTimeKind.Local).AddTicks(6723));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 2,
                column: "created_at",
                value: new DateTime(2024, 10, 21, 14, 0, 55, 593, DateTimeKind.Local).AddTicks(6733));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 3,
                column: "created_at",
                value: new DateTime(2024, 10, 21, 14, 0, 55, 593, DateTimeKind.Local).AddTicks(6740));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 4,
                column: "created_at",
                value: new DateTime(2024, 10, 21, 14, 0, 55, 593, DateTimeKind.Local).AddTicks(6746));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 5,
                column: "created_at",
                value: new DateTime(2024, 10, 21, 14, 0, 55, 593, DateTimeKind.Local).AddTicks(6754));
        }
    }
}
