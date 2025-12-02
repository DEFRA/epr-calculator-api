using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSubmitterIdObligationStatusInPOMandOrg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "submitter_id",
                table: "pom_data",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "obligation_status",
                table: "organisation_data",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "submitter_id",
                table: "organisation_data",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "submitter_id",
                table: "calculator_run_pom_data_detail",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "obligation_status",
                table: "calculator_run_organization_data_detail",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "submitter_id",
                table: "calculator_run_organization_data_detail",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "submitter_id",
                table: "pom_data");

            migrationBuilder.DropColumn(
                name: "obligation_status",
                table: "organisation_data");

            migrationBuilder.DropColumn(
                name: "submitter_id",
                table: "organisation_data");

            migrationBuilder.DropColumn(
                name: "submitter_id",
                table: "calculator_run_pom_data_detail");

            migrationBuilder.DropColumn(
                name: "obligation_status",
                table: "calculator_run_organization_data_detail");

            migrationBuilder.DropColumn(
                name: "submitter_id",
                table: "calculator_run_organization_data_detail");
        }
    }
}
