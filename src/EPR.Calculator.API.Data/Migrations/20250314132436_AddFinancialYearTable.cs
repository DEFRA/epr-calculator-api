﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancialYearTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "projection_year",
                table: "lapcap_data_master",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "parameter_year",
                table: "default_parameter_setting_master",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.AlterColumn<string>(
                name: "financial_year",
                table: "calculator_run",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250);

            migrationBuilder.CreateTable(
                name: "calculator_run_financial_years",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    financial_Year = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calculator_run_financial_years", x => x.id);
                    table.UniqueConstraint("AK_calculator_run_financial_years_financial_Year", x => x.financial_Year);
                });

            migrationBuilder.InsertData(
                table: "calculator_run_financial_years",
                columns: new[] { "id", "description", "financial_Year" },
                values: new object[] { 1, null, "2024-25" });

            migrationBuilder.CreateIndex(
                name: "IX_lapcap_data_master_projection_year",
                table: "lapcap_data_master",
                column: "projection_year");

            migrationBuilder.CreateIndex(
                name: "IX_default_parameter_setting_master_parameter_year",
                table: "default_parameter_setting_master",
                column: "parameter_year");

            migrationBuilder.CreateIndex(
                name: "IX_calculator_run_financial_year",
                table: "calculator_run",
                column: "financial_year");

            migrationBuilder.AddForeignKey(
                name: "FK_calculator_run_calculator_run_financial_years_financial_year",
                table: "calculator_run",
                column: "financial_year",
                principalTable: "calculator_run_financial_years",
                principalColumn: "financial_Year",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_default_parameter_setting_master_calculator_run_financial_years_parameter_year",
                table: "default_parameter_setting_master",
                column: "parameter_year",
                principalTable: "calculator_run_financial_years",
                principalColumn: "financial_Year",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_lapcap_data_master_calculator_run_financial_years_projection_year",
                table: "lapcap_data_master",
                column: "projection_year",
                principalTable: "calculator_run_financial_years",
                principalColumn: "financial_Year",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_calculator_run_calculator_run_financial_years_financial_year",
                table: "calculator_run");

            migrationBuilder.DropForeignKey(
                name: "FK_default_parameter_setting_master_calculator_run_financial_years_parameter_year",
                table: "default_parameter_setting_master");

            migrationBuilder.DropForeignKey(
                name: "FK_lapcap_data_master_calculator_run_financial_years_projection_year",
                table: "lapcap_data_master");

            migrationBuilder.DropTable(
                name: "calculator_run_financial_years");

            migrationBuilder.DropIndex(
                name: "IX_lapcap_data_master_projection_year",
                table: "lapcap_data_master");

            migrationBuilder.DropIndex(
                name: "IX_default_parameter_setting_master_parameter_year",
                table: "default_parameter_setting_master");

            migrationBuilder.DropIndex(
                name: "IX_calculator_run_financial_year",
                table: "calculator_run");

            migrationBuilder.AlterColumn<string>(
                name: "projection_year",
                table: "lapcap_data_master",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "parameter_year",
                table: "default_parameter_setting_master",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "financial_year",
                table: "calculator_run",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
