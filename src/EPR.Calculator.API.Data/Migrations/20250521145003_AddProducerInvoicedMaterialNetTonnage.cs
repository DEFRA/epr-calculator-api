using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProducerInvoicedMaterialNetTonnage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "producer_invoiced_material_net_tonnage",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    calculator_run_id = table.Column<int>(type: "int", nullable: false),
                    material_id = table.Column<int>(type: "int", nullable: false),
                    producer_id = table.Column<int>(type: "int", nullable: false),
                    invoiced_net_tonnage = table.Column<decimal>(type: "decimal(18,2)", maxLength: 4000, nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_producer_invoiced_material_net_tonnage", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "producer_invoiced_material_net_tonnage");
        }
    }
}
