using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSubmissionPeriodProducerReportedMaterialAndProjectedTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "submission_period",
                table: "producer_reported_material",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "producer_reported_material_projected",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    material_id = table.Column<int>(type: "int", nullable: false),
                    producer_detail_id = table.Column<int>(type: "int", nullable: false),
                    packaging_type = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    packaging_tonnage = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    packaging_tonnage_red = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    packaging_tonnage_amber = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    packaging_tonnage_green = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    packaging_tonnage_red_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    packaging_tonnage_amber_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    packaging_tonnage_green_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    submission_period = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_producer_reported_material_projected", x => x.id);
                    table.ForeignKey(
                        name: "FK_producer_reported_material_projected_material_material_id",
                        column: x => x.material_id,
                        principalTable: "material",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_producer_reported_material_projected_producer_detail_producer_detail_id",
                        column: x => x.producer_detail_id,
                        principalTable: "producer_detail",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_producer_reported_material_projected_material_id",
                table: "producer_reported_material_projected",
                column: "material_id");

            migrationBuilder.CreateIndex(
                name: "IX_producer_reported_material_projected_producer_detail_id",
                table: "producer_reported_material_projected",
                column: "producer_detail_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "producer_reported_material_projected");

            migrationBuilder.DropColumn(
                name: "submission_period",
                table: "producer_reported_material");
        }
    }
}
