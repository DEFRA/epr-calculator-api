using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTransformTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "total_cost",
                table: "lapcap_data_detail",
                type: "decimal(18,3)",
                precision: 18,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.CreateTable(
                name: "transform_partial",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    calculator_run_id = table.Column<int>(type: "int", nullable: false),
                    producer_id = table.Column<int>(type: "int", nullable: false),
                    subsidiary_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    producer_name = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    trading_name = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    level = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    submission_year = table.Column<int>(type: "int", nullable: false),
                    days_in_submission_year = table.Column<int>(type: "int", nullable: false),
                    joining_date = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    days_obligated = table.Column<int>(type: "int", nullable: true),
                    obligated_factor = table.Column<decimal>(type: "decimal(16,12)", precision: 16, scale: 12, nullable: false),
                    material_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    household_tonnage = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    household_tonnage_red = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    household_tonnage_amber = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    household_tonnage_green = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    household_tonnage_red_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    household_tonnage_amber_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    household_tonnage_green_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    public_bin_tonnage = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    public_bin_tonnage_red = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    public_bin_tonnage_amber = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    public_bin_tonnage_green = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    public_bin_tonnage_red_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    public_bin_tonnage_amber_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    public_bin_tonnage_green_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    hdc_tonnage = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    hdc_tonnage_red = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    hdc_tonnage_amber = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    hdc_tonnage_green = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    hdc_tonnage_red_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    hdc_tonnage_amber_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    hdc_tonnage_green_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    smcw_tonnage = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transform_partial", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "transform_projected_h1",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    calculator_run_id = table.Column<int>(type: "int", nullable: false),
                    producer_id = table.Column<int>(type: "int", nullable: false),
                    subsidiary_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    material_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    submission_period = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    level = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    household_tonnage = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    household_tonnage_red = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    household_tonnage_amber = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    household_tonnage_green = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    household_tonnage_red_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    household_tonnage_amber_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    household_tonnage_green_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    public_bin_tonnage = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    public_bin_tonnage_red = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    public_bin_tonnage_amber = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    public_bin_tonnage_green = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    public_bin_tonnage_red_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    public_bin_tonnage_amber_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    public_bin_tonnage_green_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    hdc_tonnage = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    hdc_tonnage_red = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    hdc_tonnage_amber = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    hdc_tonnage_green = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    hdc_tonnage_red_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    hdc_tonnage_amber_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    hdc_tonnage_green_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    household_tonnage_without_ram = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    public_bin_tonnage_without_ram = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    hdc_tonnage_without_ram = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    projected_household_tonnage = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    projected_household_tonnage_red = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    projected_household_tonnage_amber = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    projected_household_tonnage_green = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    projected_household_tonnage_red_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    projected_household_tonnage_amber_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    projected_household_tonnage_green_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    projected_public_bin_tonnage = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    projected_public_bin_tonnage_red = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    projected_public_bin_tonnage_amber = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    projected_public_bin_tonnage_green = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    projected_public_bin_tonnage_red_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    projected_public_bin_tonnage_amber_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    projected_public_bin_tonnage_green_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    projected_hdc_tonnage = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    projected_hdc_tonnage_red = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    projected_hdc_tonnage_amber = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    projected_hdc_tonnage_green = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    projected_hdc_tonnage_red_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    projected_hdc_tonnage_amber_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    projected_hdc_tonnage_green_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    h2_ram_proportions_red = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    h2_ram_proportions_amber = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    h2_ram_proportions_green = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    h2_ram_proportions_red_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    h2_ram_proportions_amber_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    h2_ram_proportions_green_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transform_projected_h1", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "transform_projected_h2",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    calculator_run_id = table.Column<int>(type: "int", nullable: false),
                    producer_id = table.Column<int>(type: "int", nullable: false),
                    subsidiary_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    material_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    submission_period = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    level = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    household_tonnage = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    household_tonnage_red = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    household_tonnage_amber = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    household_tonnage_green = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    household_tonnage_red_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    household_tonnage_amber_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    household_tonnage_green_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    public_bin_tonnage = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    public_bin_tonnage_red = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    public_bin_tonnage_amber = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    public_bin_tonnage_green = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    public_bin_tonnage_red_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    public_bin_tonnage_amber_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    public_bin_tonnage_green_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    hdc_tonnage = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    hdc_tonnage_red = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    hdc_tonnage_amber = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    hdc_tonnage_green = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    hdc_tonnage_red_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    hdc_tonnage_amber_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    hdc_tonnage_green_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    household_tonnage_without_ram = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    public_bin_tonnage_without_ram = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    hdc_tonnage_without_ram = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    projected_household_tonnage = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    projected_household_tonnage_red = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    projected_household_tonnage_amber = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    projected_household_tonnage_green = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    projected_household_tonnage_red_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    projected_household_tonnage_amber_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    projected_household_tonnage_green_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    projected_public_bin_tonnage = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    projected_public_bin_tonnage_red = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    projected_public_bin_tonnage_amber = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    projected_public_bin_tonnage_green = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    projected_public_bin_tonnage_red_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    projected_public_bin_tonnage_amber_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    projected_public_bin_tonnage_green_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    projected_hdc_tonnage = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    projected_hdc_tonnage_red = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    projected_hdc_tonnage_amber = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    projected_hdc_tonnage_green = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    projected_hdc_tonnage_red_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    projected_hdc_tonnage_amber_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    projected_hdc_tonnage_green_medical = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transform_projected_h2", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "transform_scaled",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    calculator_run_id = table.Column<int>(type: "int", nullable: false),
                    producer_id = table.Column<int>(type: "int", nullable: false),
                    subsidiary_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    producer_name = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    trading_name = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    submission_period = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    level = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    is_subtotal = table.Column<bool>(type: "bit", nullable: false),
                    days_in_submission_period = table.Column<int>(type: "int", nullable: false),
                    days_in_whole_period = table.Column<int>(type: "int", nullable: false),
                    scaled_factor = table.Column<decimal>(type: "decimal(16,12)", precision: 16, scale: 12, nullable: false),
                    material_id = table.Column<int>(type: "int", nullable: false),
                    packaging_type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    tonnage = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    scaled_tonnage = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transform_scaled", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_transform_partial_calculator_run_id",
                table: "transform_partial",
                column: "calculator_run_id");

            migrationBuilder.CreateIndex(
                name: "IX_transform_projected_h1_calculator_run_id",
                table: "transform_projected_h1",
                column: "calculator_run_id");

            migrationBuilder.CreateIndex(
                name: "IX_transform_projected_h2_calculator_run_id",
                table: "transform_projected_h2",
                column: "calculator_run_id");

            migrationBuilder.CreateIndex(
                name: "IX_transform_scaled_calculator_run_id",
                table: "transform_scaled",
                column: "calculator_run_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "transform_partial");

            migrationBuilder.DropTable(
                name: "transform_projected_h1");

            migrationBuilder.DropTable(
                name: "transform_projected_h2");

            migrationBuilder.DropTable(
                name: "transform_scaled");

            migrationBuilder.AlterColumn<decimal>(
                name: "total_cost",
                table: "lapcap_data_detail",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)",
                oldPrecision: 18,
                oldScale: 3);
        }
    }
}
