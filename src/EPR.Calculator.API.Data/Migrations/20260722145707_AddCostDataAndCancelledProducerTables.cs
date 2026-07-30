using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCostDataAndCancelledProducerTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "calc_result_cancelled_producer",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    calculator_run_id = table.Column<int>(type: "int", nullable: false),
                    cancelled_producer = table.Column<string>(type: "json", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calc_result_cancelled_producer", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "calc_result_comms_cost",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    calculator_run_id = table.Column<int>(type: "int", nullable: false),
                    comms_cost = table.Column<string>(type: "json", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calc_result_comms_cost", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "calc_result_la_disposal_cost",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    calculator_run_id = table.Column<int>(type: "int", nullable: false),
                    la_disposal_cost = table.Column<string>(type: "json", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calc_result_la_disposal_cost", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "calc_result_lapcap_data",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    calculator_run_id = table.Column<int>(type: "int", nullable: false),
                    lapcap = table.Column<string>(type: "json", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calc_result_lapcap_data", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "calc_result_late_reporting_tonnage",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    calculator_run_id = table.Column<int>(type: "int", nullable: false),
                    late_reporting_tonnage = table.Column<string>(type: "json", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calc_result_late_reporting_tonnage", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "calc_result_one_plus_four_apportionment",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    calculator_run_id = table.Column<int>(type: "int", nullable: false),
                    one_plus_four_apppointment = table.Column<string>(type: "json", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calc_result_one_plus_four_apportionment", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "calc_result_parameter_other_cost",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    calculator_run_id = table.Column<int>(type: "int", nullable: false),
                    parameter_other_cost = table.Column<string>(type: "json", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calc_result_parameter_other_cost", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_calc_result_cancelled_producer_calculator_run_id",
                table: "calc_result_cancelled_producer",
                column: "calculator_run_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "calc_result_cancelled_producer");

            migrationBuilder.DropTable(
                name: "calc_result_comms_cost");

            migrationBuilder.DropTable(
                name: "calc_result_la_disposal_cost");

            migrationBuilder.DropTable(
                name: "calc_result_lapcap_data");

            migrationBuilder.DropTable(
                name: "calc_result_late_reporting_tonnage");

            migrationBuilder.DropTable(
                name: "calc_result_one_plus_four_apportionment");

            migrationBuilder.DropTable(
                name: "calc_result_parameter_other_cost");
        }
    }
}
