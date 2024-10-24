using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class CalcRunPomAndOrganisationRemoveKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_calculator_run_pom_data_detail",
                table: "calculator_run_pom_data_detail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_calculator_run_organization_data_detail",
                table: "calculator_run_organization_data_detail");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "calculator_run_pom_data_detail",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "calculator_run_organization_data_detail",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_calculator_run_pom_data_detail",
                table: "calculator_run_pom_data_detail",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_calculator_run_organization_data_detail",
                table: "calculator_run_organization_data_detail",
                column: "Id");

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 1,
                column: "created_at",
                value: new DateTime(2024, 10, 21, 12, 7, 1, 550, DateTimeKind.Local).AddTicks(7348));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 2,
                column: "created_at",
                value: new DateTime(2024, 10, 21, 12, 7, 1, 550, DateTimeKind.Local).AddTicks(7355));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 3,
                column: "created_at",
                value: new DateTime(2024, 10, 21, 12, 7, 1, 550, DateTimeKind.Local).AddTicks(7360));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 4,
                column: "created_at",
                value: new DateTime(2024, 10, 21, 12, 7, 1, 550, DateTimeKind.Local).AddTicks(7365));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 5,
                column: "created_at",
                value: new DateTime(2024, 10, 21, 12, 7, 1, 550, DateTimeKind.Local).AddTicks(7370));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_calculator_run_pom_data_detail",
                table: "calculator_run_pom_data_detail");

            migrationBuilder.DropPrimaryKey(
                name: "PK_calculator_run_organization_data_detail",
                table: "calculator_run_organization_data_detail");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "calculator_run_pom_data_detail");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "calculator_run_organization_data_detail");

            migrationBuilder.AddPrimaryKey(
                name: "PK_calculator_run_pom_data_detail",
                table: "calculator_run_pom_data_detail",
                columns: new[] { "organisation_id", "subsidiary_id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_calculator_run_organization_data_detail",
                table: "calculator_run_organization_data_detail",
                columns: new[] { "organisation_id", "subsidiary_id" });

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 1,
                column: "created_at",
                value: new DateTime(2024, 10, 18, 12, 4, 37, 821, DateTimeKind.Local).AddTicks(2280));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 2,
                column: "created_at",
                value: new DateTime(2024, 10, 18, 12, 4, 37, 821, DateTimeKind.Local).AddTicks(2289));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 3,
                column: "created_at",
                value: new DateTime(2024, 10, 18, 12, 4, 37, 821, DateTimeKind.Local).AddTicks(2296));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 4,
                column: "created_at",
                value: new DateTime(2024, 10, 18, 12, 4, 37, 821, DateTimeKind.Local).AddTicks(2302));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 5,
                column: "created_at",
                value: new DateTime(2024, 10, 18, 12, 4, 37, 821, DateTimeKind.Local).AddTicks(2308));
        }
    }
}
