using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class BackUpSmcwTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Kept as _bk tables so Down can restore them.
            migrationBuilder.RenameTable(
                name: "calc_result_smcw_producer",
                newName: "calc_result_smcw_producer_bk");

            migrationBuilder.RenameTable(
                name: "calc_result_smcw",
                newName: "calc_result_smcw_bk");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "calc_result_smcw_bk",
                newName: "calc_result_smcw");

            migrationBuilder.RenameTable(
                name: "calc_result_smcw_producer_bk",
                newName: "calc_result_smcw_producer");
        }
    }
}
