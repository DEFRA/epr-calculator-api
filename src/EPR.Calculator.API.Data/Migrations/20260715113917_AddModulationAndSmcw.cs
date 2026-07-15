using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddModulationAndSmcw : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "calc_result_modulation",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    calculator_run_id = table.Column<int>(type: "int", nullable: false),
                    green_factor = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    red_factor = table.Column<decimal>(type: "decimal(6,3)", precision: 6, scale: 3, nullable: false),
                    material_modulations = table.Column<string>(type: "json", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calc_result_modulation", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "calc_result_smcw",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    calculator_run_id = table.Column<int>(type: "int", nullable: false),
                    material_totals = table.Column<string>(type: "json", nullable: false),
                    producer_totals = table.Column<string>(type: "json", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calc_result_smcw", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "calc_result_modulation");

            migrationBuilder.DropTable(
                name: "calc_result_smcw");
        }
    }
}
