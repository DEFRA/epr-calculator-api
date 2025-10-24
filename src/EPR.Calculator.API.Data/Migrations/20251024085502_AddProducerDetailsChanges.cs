using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProducerDetailsChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "days_in_submission_year",
                table: "producer_detail",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "joiner_date",
                table: "producer_detail",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "leaver_code",
                table: "producer_detail",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "leaver_date",
                table: "producer_detail",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "obligated_days",
                table: "producer_detail",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "obligation_percentage",
                table: "producer_detail",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "days_in_submission_year",
                table: "producer_detail");

            migrationBuilder.DropColumn(
                name: "joiner_date",
                table: "producer_detail");

            migrationBuilder.DropColumn(
                name: "leaver_code",
                table: "producer_detail");

            migrationBuilder.DropColumn(
                name: "leaver_date",
                table: "producer_detail");

            migrationBuilder.DropColumn(
                name: "obligated_days",
                table: "producer_detail");

            migrationBuilder.DropColumn(
                name: "obligation_percentage",
                table: "producer_detail");
        }
    }
}
