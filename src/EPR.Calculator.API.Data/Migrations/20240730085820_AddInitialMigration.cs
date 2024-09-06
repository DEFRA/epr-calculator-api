using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EPR.Calculator.API.Data.Migrations
{
    [ExcludeFromCodeCoverage]
    /// <inheritdoc />
    public partial class AddInitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "default_parameter_setting_master",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    parameter_year = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    effective_from = table.Column<DateTime>(type: "datetime2", nullable: false),
                    effective_to = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_by = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_default_parameter_setting_master", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "default_parameter_template_master",
                columns: table => new
                {
                    parameter_unique_ref = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    parameter_type = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    parameter_category = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    valid_Range_from = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    valid_Range_to = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_default_parameter_template_master", x => x.parameter_unique_ref);
                });

            migrationBuilder.CreateTable(
                name: "default_parameter_setting_detail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    default_parameter_setting_master_id = table.Column<int>(type: "int", nullable: false),
                    parameter_unique_ref = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    parameter_value = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_default_parameter_setting_detail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_default_parameter_setting_detail_default_parameter_setting_master_default_parameter_setting_master_id",
                        column: x => x.default_parameter_setting_master_id,
                        principalTable: "default_parameter_setting_master",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_default_parameter_setting_detail_default_parameter_template_master_parameter_unique_ref",
                        column: x => x.parameter_unique_ref,
                        principalTable: "default_parameter_template_master",
                        principalColumn: "parameter_unique_ref",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "default_parameter_template_master",
                columns: new[] { "parameter_unique_ref", "parameter_category", "parameter_type", "valid_Range_from", "valid_Range_to" },
                values: new object[,]
                {
                    { "BADEBT-P", "Communication costs", "Aluminium", 0m, 999999999.99m },
                    { "COMC-AL", "Communication costs", "Aluminium", 0m, 999999999.99m },
                    { "COMC-FC", "Communication costs", "Fibre composite", 0m, 999999999.99m },
                    { "COMC-GL", "Communication costs", "Glass", 0m, 999999999.99m },
                    { "COMC-OT", "Communication costs", "Other", 0m, 999999999.99m },
                    { "COMC-PC", "Communication costs", "Paper or card", 0m, 999999999.99m },
                    { "COMC-PL", "Communication costs", "Plastic", 0m, 999999999.99m },
                    { "COMC-ST", "Communication costs", "Steel", 0m, 999999999.99m },
                    { "COMC-WD", "Communication costs", "Wood", 0m, 999999999.99m },
                    { "LAPC-ENG", "Local authority data preparation costs", "England", 0m, 999999999.99m },
                    { "LAPC-NIR", "Local authority data preparation costs", "Northern Ireland", 0m, 999999999.99m },
                    { "LAPC-SCT", "Local authority data preparation costs", "Scotland", 0m, 999999999.99m },
                    { "LAPC-WLS", "Local authority data preparation costs", "Wales", 0m, 999999999.99m },
                    { "LEVY-ENG", "Levy", "England", 0m, 999999999.99m },
                    { "LEVY-NIR", "Levy", "Northern Ireland", 0m, 999999999.99m },
                    { "LEVY-SCT", "Levy", "Scotland", 0m, 999999999.99m },
                    { "LEVY-WLS", "Levy", "Wales", 0m, 999999999.99m },
                    { "LRET-AL", "Late reporting tonnage", "Aluminium", 0m, 999999999.99m },
                    { "LRET-FC", "Late reporting tonnage", "Aluminium", 0m, 999999999.99m },
                    { "LRET-GL", "Late reporting tonnage", "Aluminium", 0m, 999999999.99m },
                    { "LRET-OT", "Late reporting tonnage", "Aluminium", 0m, 999999999.99m },
                    { "LRET-PC", "Late reporting tonnage", "Aluminium", 0m, 999999999.99m },
                    { "LRET-PL", "Late reporting tonnage", "Aluminium", 0m, 999999999.99m },
                    { "LRET-ST", "Late reporting tonnage", "Aluminium", 0m, 999999999.99m },
                    { "LRET-WD", "Late reporting tonnage", "Aluminium", 0m, 999999999.99m },
                    { "MATT-AD", "Materiality threshold", "Amount Decrease", 0m, 999999999.99m },
                    { "MATT-AI", "Materiality threshold", "Amount Increase", 0m, 999999999.99m },
                    { "MATT-PD", "Materiality threshold", "Percent Decrease", 0m, -1000.00m },
                    { "MATT-PI", "Materiality threshold", "Percent Increase", 0m, 1000.00m },
                    { "SAOC-ENG", "Scheme administrator operating costs", "England", 0m, 999999999.99m },
                    { "SAOC-NIR", "Scheme administrator operating costs", "Northern Ireland", 0m, 999999999.99m },
                    { "SAOC-SCT", "Scheme administrator operating costs", "Scotland", 0m, 999999999.99m },
                    { "SAOC-WLS", "Scheme administrator operating costs", "Wales", 0m, 999999999.99m },
                    { "SCSC-ENG", "Scheme setup costs", "England", 0m, 999999999.99m },
                    { "SCSC-NIR", "Scheme setup costs", "Northern Ireland", 0m, 999999999.99m },
                    { "SCSC-SCT", "Scheme setup costs", "Scotland", 0m, 999999999.99m },
                    { "SCSC-WLS", "Scheme setup costs", "Wales", 0m, 999999999.99m },
                    { "TONT-AI", "Tonnage change threshold", "Amount Increase", 0m, 999999999.99m },
                    { "TONT-DI", "Tonnage change threshold", "Amount Decrease", 0m, 999999999.99m },
                    { "TONT-PD", "Tonnage change threshold", "Percent Decrease", 0m, -1000.00m },
                    { "TONT-PI", "Tonnage change threshold", "Percent Increase", 0m, 1000.00m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_default_parameter_setting_detail_default_parameter_setting_master_id",
                table: "default_parameter_setting_detail",
                column: "default_parameter_setting_master_id");

            migrationBuilder.CreateIndex(
                name: "IX_default_parameter_setting_detail_parameter_unique_ref",
                table: "default_parameter_setting_detail",
                column: "parameter_unique_ref");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "default_parameter_setting_detail");

            migrationBuilder.DropTable(
                name: "default_parameter_setting_master");

            migrationBuilder.DropTable(
                name: "default_parameter_template_master");
        }
    }
}
