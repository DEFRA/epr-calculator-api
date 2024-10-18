using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class OrganisationAndPomChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_pom_data",
                table: "pom_data");

            migrationBuilder.DropPrimaryKey(
                name: "PK_organization_data",
                table: "organization_data");

            migrationBuilder.RenameTable(
                name: "organization_data",
                newName: "organisation_data");

            migrationBuilder.AlterColumn<string>(
                name: "subsidiary_id",
                table: "pom_data",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(400)",
                oldMaxLength: 400);

            migrationBuilder.AlterColumn<string>(
                name: "organisation_id",
                table: "pom_data",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(400)",
                oldMaxLength: 400);

            migrationBuilder.AlterColumn<string>(
                name: "subsidiary_id",
                table: "organisation_data",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(400)",
                oldMaxLength: 400);

            migrationBuilder.AlterColumn<string>(
                name: "organisation_id",
                table: "organisation_data",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(400)",
                oldMaxLength: 400);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "organisation_data",
                newName: "organization_data");

            migrationBuilder.AlterColumn<string>(
                name: "subsidiary_id",
                table: "pom_data",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(400)",
                oldMaxLength: 400,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "organisation_id",
                table: "pom_data",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(400)",
                oldMaxLength: 400,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "subsidiary_id",
                table: "organization_data",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(400)",
                oldMaxLength: 400,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "organisation_id",
                table: "organization_data",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(400)",
                oldMaxLength: 400,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_pom_data",
                table: "pom_data",
                columns: new[] { "organisation_id", "subsidiary_id" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_organization_data",
                table: "organization_data",
                columns: new[] { "organisation_id", "subsidiary_id" });

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 1,
                column: "created_at",
                value: new DateTime(2024, 10, 7, 9, 25, 51, 263, DateTimeKind.Local).AddTicks(8112));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 2,
                column: "created_at",
                value: new DateTime(2024, 10, 7, 9, 25, 51, 263, DateTimeKind.Local).AddTicks(8115));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 3,
                column: "created_at",
                value: new DateTime(2024, 10, 7, 9, 25, 51, 263, DateTimeKind.Local).AddTicks(8118));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 4,
                column: "created_at",
                value: new DateTime(2024, 10, 7, 9, 25, 51, 263, DateTimeKind.Local).AddTicks(8120));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 5,
                column: "created_at",
                value: new DateTime(2024, 10, 7, 9, 25, 51, 263, DateTimeKind.Local).AddTicks(8127));
        }
    }
}
