using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedObligationAndSubmitterColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "obligation_status",
                table: "producer_detail");

            migrationBuilder.DropColumn(
                name: "submitter_id",
                table: "producer_detail");

            migrationBuilder.DropColumn(
                name: "has_h1",
                table: "calculator_run_organisation");

            migrationBuilder.DropColumn(
                name: "has_h2",
                table: "calculator_run_organisation");

            migrationBuilder.DropColumn(
                name: "obligation_status",
                table: "calculator_run_organisation");

            migrationBuilder.DropColumn(
                name: "submitter_id",
                table: "calculator_run_organisation");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "obligation_status",
                table: "producer_detail",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "submitter_id",
                table: "producer_detail",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "has_h1",
                table: "calculator_run_organisation",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "has_h2",
                table: "calculator_run_organisation",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "obligation_status",
                table: "calculator_run_organisation",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "submitter_id",
                table: "calculator_run_organisation",
                type: "uniqueidentifier",
                nullable: true);
        }
    }
}
