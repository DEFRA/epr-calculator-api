using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDisplayOrderToMaterial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "display_order",
                table: "material",
                type: "int",
                nullable: false,
                defaultValue: 0);

            string[] codesInDisplayOrder = ["AL", "FC", "GL", "PC", "PL", "ST", "WD", "OT"];
            for (var i = 0; i < codesInDisplayOrder.Length; i++)
            {
                migrationBuilder.UpdateData(
                    table: "material",
                    keyColumn: "code",
                    keyValue: codesInDisplayOrder[i],
                    column: "display_order",
                    value: i + 1);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "display_order",
                table: "material");
        }
    }
}
