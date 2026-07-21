using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProducerFeeTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "producer_reported_material_projected",
                newName: "producer_material_packaging");

            migrationBuilder.RenameIndex(
                name: "IX_producer_reported_material_projected_material_id",
                newName: "IX_producer_material_packaging_material_id",
                table: "producer_material_packaging");

            migrationBuilder.RenameIndex(
                name: "IX_producer_reported_material_projected_producer_detail_id",
                newName: "IX_producer_material_packaging_producer_detail_id",
                table: "producer_material_packaging");

            migrationBuilder.CreateTable(
                name: "calc_result_producer_fees",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    calculator_run_id = table.Column<int>(type: "int", nullable: false),
                    total = table.Column<string>(type: "json", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calc_result_producer_fees", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "calc_result_producer_fee_detail",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    producer_fees_id = table.Column<int>(type: "int", nullable: false),
                    detail = table.Column<string>(type: "json", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calc_result_producer_fee_detail", x => x.id);
                    table.ForeignKey(
                        name: "FK_calc_result_producer_fee_detail_calc_result_producer_fees_producer_fees_id",
                        column: x => x.producer_fees_id,
                        principalTable: "calc_result_producer_fees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_calc_result_producer_fee_detail_producer_fees_id",
                table: "calc_result_producer_fee_detail",
                column: "producer_fees_id");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "calc_result_producer_fee_detail");

            migrationBuilder.DropTable(
                name: "calc_result_producer_fees");

            migrationBuilder.RenameIndex(
                name: "IX_producer_material_packaging_material_id",
                newName: "IX_producer_reported_material_projected_material_id",
                table: "producer_material_packaging");

            migrationBuilder.RenameIndex(
                name: "IX_producer_material_packaging_producer_detail_id",
                newName: "IX_producer_reported_material_projected_producer_detail_id",
                table: "producer_material_packaging");

            migrationBuilder.RenameTable(
                name: "producer_material_packaging",
                newName: "producer_reported_material_projected");
        }
    }
}
