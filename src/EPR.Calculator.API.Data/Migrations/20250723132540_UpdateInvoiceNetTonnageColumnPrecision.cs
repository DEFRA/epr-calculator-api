using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateInvoiceNetTonnageColumnPrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "invoiced_net_tonnage",
                table: "producer_invoiced_material_net_tonnage",
                type: "decimal(18,3)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "invoiced_net_tonnage",
                table: "producer_invoiced_material_net_tonnage",
                type: "decimal(18,2)",
                nullable: true);
        }
    }
}
