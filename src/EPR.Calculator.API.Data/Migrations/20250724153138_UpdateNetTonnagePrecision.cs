using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNetTonnagePrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "invoiced_net_tonnage",
                table: "producer_invoiced_material_net_tonnage",
                type: "decimal(18,3)",
                maxLength: 4000,
                precision: 18,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldMaxLength: 4000,
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "invoiced_net_tonnage",
                table: "producer_invoiced_material_net_tonnage",
                type: "decimal(18,2)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)",
                oldMaxLength: 4000,
                oldPrecision: 18,
                oldScale: 3,
                oldNullable: true);
        }
    }
}
