using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMaterialName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "material",
                keyColumn: "code",
                keyValue: "FC",
                columns: new[] { "name", "description" },
                values: new object[] { "Fibre Composite", "Fibre Composite" });

            migrationBuilder.UpdateData(
                table: "material",
                keyColumn: "code",
                keyValue: "PC",
                columns: new[] { "name", "description" },
                values: new object[] { "Paper or Card", "Paper or Card" });

            migrationBuilder.UpdateData(
                table: "material",
                keyColumn: "code",
                keyValue: "OT",
                columns: new[] { "name", "description" },
                values: new object[] { "Other Materials", "Other Materials" });

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "material",
                keyColumn: "code",
                keyValue: "FC",
                columns: new[] { "name", "description" },
                values: new object[] { "Fibre composite", "Fibre composite" });

            migrationBuilder.UpdateData(
                table: "material",
                keyColumn: "code",
                keyValue: "PC",
                columns: new[] { "name", "description" },
                values: new object[] { "Paper or card", "Paper or card" });

            migrationBuilder.UpdateData(
                table: "material",
                keyColumn: "code",
                keyValue: "OT",
                columns: new[] { "name", "description" },
                values: new object[] { "Other materials", "Other materials" });
        }
    }
}
