using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddObligationStatusSubmitterIdInDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "obligation_status",
                table: "calculator_run_organization_data_detail",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "submitter_id",
                table: "calculator_run_organization_data_detail",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "obligation_status",
                table: "calculator_run_organization_data_detail");

            migrationBuilder.DropColumn(
                name: "submitter_id",
                table: "calculator_run_organization_data_detail");
        }
    }
}
