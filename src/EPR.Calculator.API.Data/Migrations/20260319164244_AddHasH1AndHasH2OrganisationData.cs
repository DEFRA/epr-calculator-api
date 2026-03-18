using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHasH1AndHasH2OrganisationData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "has_h1",
                table: "organisation_data",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "has_h2",
                table: "organisation_data",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "has_h1",
                table: "calculator_run_organization_data_detail",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "has_h2",
                table: "calculator_run_organization_data_detail",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "has_h1",
                table: "organisation_data");

            migrationBuilder.DropColumn(
                name: "has_h2",
                table: "organisation_data");

            migrationBuilder.DropColumn(
                name: "has_h1",
                table: "calculator_run_organization_data_detail");

            migrationBuilder.DropColumn(
                name: "has_h2",
                table: "calculator_run_organization_data_detail");
        }
    }
}
