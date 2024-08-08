using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTemplateMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("delete dbo.default_parameter_setting_detail");
            migrationBuilder.Sql("delete dbo.default_parameter_setting_master");
            migrationBuilder.Sql("delete dbo.default_parameter_template_master");
            migrationBuilder.InsertData(
               table: "default_parameter_template_master",
               columns: new[] { "parameter_unique_ref", "parameter_category", "parameter_type", "valid_Range_from", "valid_Range_to" },
               values: new object[,]
               {
                { "COMC-AL", "Aluminium", "Communication costs", 0m, 999999999.99m },
                { "COMC-FC", "Fibre composite", "Communication costs", 0m, 999999999.99m },
                { "COMC-GL", "Glass", "Communication costs", 0m, 999999999.99m },
                { "COMC-PC", "Paper or card", "Communication costs", 0m, 999999999.99m },
                { "COMC-PL", "Plastic", "Communication costs", 0m, 999999999.99m },
                { "COMC-ST",  "Steel", "Communication costs", 0m, 999999999.99m },
                { "COMC-WD", "Wood", "Communication costs", 0m, 999999999.99m },
                { "COMC-OT", "Other", "Communication costs", 0m, 999999999.99m },
                { "SAOC-ENG", "Scheme administrator operating costs", "England", 0m, 999999999.99m },
                { "SAOC-WLS", "Scheme administrator operating costs", "Wales", 0m, 999999999.99m },
                { "SAOC-SCT", "Scheme administrator operating costs", "Scotland", 0m, 999999999.99m },
                { "SAOC-NIR", "Scheme administrator operating costs", "Northern Ireland", 0m, 999999999.99m },
                { "LAPC-ENG", "Local authority data preparation costs", "England", 0m, 999999999.99m },
                { "LAPC-WLS", "Local authority data preparation costs", "Wales", 0m, 999999999.99m },
                { "LAPC-SCT", "Local authority data preparation costs", "Scotland", 0m, 999999999.99m },
                { "LAPC-NIR", "Local authority data preparation costs", "Northern Ireland", 0m, 999999999.99m },
                { "SCSC-ENG", "Scheme setup costs", "England", 0m, 999999999.99m },
                { "SCSC-WLS", "Scheme setup costs", "Wales", 0m, 999999999.99m },
                { "SCSC-SCT", "Scheme setup costs", "Scotland", 0m, 999999999.99m },
                { "SCSC-NIR", "Scheme setup costs", "Northern Ireland", 0m, 999999999.99m },
                { "LRET-AL", "Late reporting tonnage", "Aluminium", 0m, 999999999.99m },
                { "LRET-FC", "Late reporting tonnage", "Aluminium", 0m, 999999999.99m },
                { "LRET-GL", "Late reporting tonnage", "Aluminium", 0m, 999999999.99m },
                { "LRET-PC", "Late reporting tonnage", "Aluminium", 0m, 999999999.99m },
                { "LRET-PL", "Late reporting tonnage", "Aluminium", 0m, 999999999.99m },
                { "LRET-ST", "Late reporting tonnage", "Aluminium", 0m, 999999999.99m },
                { "LRET-WD", "Late reporting tonnage", "Aluminium", 0m, 999999999.99m },
                { "LRET-OT", "Late reporting tonnage", "Aluminium", 0m, 999999999.99m },
                { "BADEBT-P", "Communication costs", "Aluminium", 0m, 999999999.99m },
                { "MATT-AI", "Materiality threshold", "Amount Increase", 0m, 999999999.99m },
                { "MATT-AD", "Materiality threshold", "Amount Decrease", 0m, 999999999.99m },
                { "MATT-PI", "Materiality threshold", "Percent Increase", 0m, 1000.00m },
                { "MATT-PD", "Materiality threshold", "Percent Decrease", 0m, -1000.00m },
                { "TONT-AI", "Tonnage change threshold", "Amount Increase", 0m, 999999999.99m },
                { "TONT-AD", "Tonnage change threshold", "Amount Increase", 0m, 999999999.99m },
                { "TONT-PI", "Tonnage change threshold", "Percent Increase", 0m, 1000.00m },
                { "TONT-PD", "Tonnage change threshold", "Percent Decrease", 0m, -1000.00m },
                { "LEVY-ENG", "Levy", "England", 0m, 999999999.99m },
                { "LEVY-WLS", "Levy", "Wales", 0m, 999999999.99m },
                { "LEVY-SCT", "Levy", "Scotland", 0m, 999999999.99m },
                { "LEVY-NIR", "Levy", "Northern Ireland", 0m, 999999999.99m },
             });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
