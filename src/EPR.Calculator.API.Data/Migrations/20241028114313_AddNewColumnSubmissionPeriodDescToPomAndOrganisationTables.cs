using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNewColumnSubmissionPeriodDescToPomAndOrganisationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "submission_period_desc",
                table: "pom_data",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "submission_period_desc",
                table: "organisation_data",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "submission_period_desc",
                table: "calculator_run_pom_data_detail",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "submission_period_desc",
                table: "calculator_run_organization_data_detail",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 1,
                column: "created_at",
                value: new DateTime(2024, 10, 28, 11, 43, 13, 492, DateTimeKind.Local).AddTicks(8134));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 2,
                column: "created_at",
                value: new DateTime(2024, 10, 28, 11, 43, 13, 492, DateTimeKind.Local).AddTicks(8137));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 3,
                column: "created_at",
                value: new DateTime(2024, 10, 28, 11, 43, 13, 492, DateTimeKind.Local).AddTicks(8139));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 4,
                column: "created_at",
                value: new DateTime(2024, 10, 28, 11, 43, 13, 492, DateTimeKind.Local).AddTicks(8141));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 5,
                column: "created_at",
                value: new DateTime(2024, 10, 28, 11, 43, 13, 492, DateTimeKind.Local).AddTicks(8143));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "submission_period_desc",
                table: "pom_data");

            migrationBuilder.DropColumn(
                name: "submission_period_desc",
                table: "organisation_data");

            migrationBuilder.DropColumn(
                name: "submission_period_desc",
                table: "calculator_run_pom_data_detail");

            migrationBuilder.DropColumn(
                name: "submission_period_desc",
                table: "calculator_run_organization_data_detail");

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 1,
                column: "created_at",
                value: new DateTime(2024, 10, 28, 9, 23, 4, 741, DateTimeKind.Local).AddTicks(9985));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 2,
                column: "created_at",
                value: new DateTime(2024, 10, 28, 9, 23, 4, 741, DateTimeKind.Local).AddTicks(9991));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 3,
                column: "created_at",
                value: new DateTime(2024, 10, 28, 9, 23, 4, 741, DateTimeKind.Local).AddTicks(9995));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 4,
                column: "created_at",
                value: new DateTime(2024, 10, 28, 9, 23, 4, 742, DateTimeKind.Local));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 5,
                column: "created_at",
                value: new DateTime(2024, 10, 28, 9, 23, 4, 742, DateTimeKind.Local).AddTicks(4));
        }
    }
}
