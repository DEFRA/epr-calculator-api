using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddModulationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "amber_medical_ram_rag_rating",
                table: "producer_reported_material",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "amber_ram_rag_rating",
                table: "producer_reported_material",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "green_medical_ram_rag_rating",
                table: "producer_reported_material",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "green_ram_rag_rating",
                table: "producer_reported_material",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "red_medical_ram_rag_rating",
                table: "producer_reported_material",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "red_ram_rag_rating",
                table: "producer_reported_material",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "packaging_material_subtype",
                table: "pom_data",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ram_rag_rating",
                table: "pom_data",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "packaging_material_subtype",
                table: "calculator_run_pom_data_detail",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ram_rag_rating",
                table: "calculator_run_pom_data_detail",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "amber_medical_ram_rag_rating",
                table: "producer_reported_material");

            migrationBuilder.DropColumn(
                name: "amber_ram_rag_rating",
                table: "producer_reported_material");

            migrationBuilder.DropColumn(
                name: "green_medical_ram_rag_rating",
                table: "producer_reported_material");

            migrationBuilder.DropColumn(
                name: "green_ram_rag_rating",
                table: "producer_reported_material");

            migrationBuilder.DropColumn(
                name: "red_medical_ram_rag_rating",
                table: "producer_reported_material");

            migrationBuilder.DropColumn(
                name: "red_ram_rag_rating",
                table: "producer_reported_material");

            migrationBuilder.DropColumn(
                name: "packaging_material_subtype",
                table: "pom_data");

            migrationBuilder.DropColumn(
                name: "ram_rag_rating",
                table: "pom_data");

            migrationBuilder.DropColumn(
                name: "packaging_material_subtype",
                table: "calculator_run_pom_data_detail");

            migrationBuilder.DropColumn(
                name: "ram_rag_rating",
                table: "calculator_run_pom_data_detail");
        }
    }
}
