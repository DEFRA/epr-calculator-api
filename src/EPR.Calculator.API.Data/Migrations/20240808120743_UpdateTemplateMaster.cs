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
                { "SAOC-ENG", "England", "Scheme administrator operating costs", 0m, 999999999.99m },
                { "SAOC-WLS", "Wales", "Scheme administrator operating costs", 0m, 999999999.99m },
                { "SAOC-SCT", "Scotland", "Scheme administrator operating costs", 0m, 999999999.99m },
                { "SAOC-NIR", "Northern Ireland", "Scheme administrator operating costs", 0m, 999999999.99m },
                { "LAPC-ENG", "England", "Local authority data preparation costs", 0m, 999999999.99m },
                { "LAPC-WLS", "Wales", "Local authority data preparation costs", 0m, 999999999.99m },
                { "LAPC-SCT", "Scotland", "Local authority data preparation costs", 0m, 999999999.99m },
                { "LAPC-NIR", "Northern Ireland", "Local authority data preparation costs", 0m, 999999999.99m },
                { "SCSC-ENG",  "England", "Scheme setup costs", 0m, 999999999.99m },
                { "SCSC-WLS",  "Wales", "Scheme setup costs", 0m, 999999999.99m },
                { "SCSC-SCT",  "Scotland", "Scheme setup costs", 0m, 999999999.99m },
                { "SCSC-NIR", "Northern Ireland", "Scheme setup costs", 0m, 999999999.99m },
                { "LRET-AL",  "Aluminium", "Late reporting tonnage", 0m, 999999999.99m },
                { "LRET-FC",  "Aluminium", "Fibre composite", 0m, 999999999.99m },
                { "LRET-GL",  "Aluminium", "Glass", 0m, 999999999.99m },
                { "LRET-PC",  "Aluminium", "Paper or card", 0m, 999999999.99m },
                { "LRET-PL",  "Aluminium", "Plastic", 0m, 999999999.99m },
                { "LRET-ST",  "Aluminium", "Steel", 0m, 999999999.99m },
                { "LRET-WD",  "Aluminium", "Wood", 0m, 999999999.99m },
                { "LRET-OT",  "Aluminium", "Other", 0m, 999999999.99m },
                { "BADEBT-P", "BadDebt", "Bad debt provision percentage", 0m, 999.99m },
                { "MATT-AI", "Materiality threshold", "Amount Increase", 0m, 999999999.99m },
                { "MATT-AD", "Materiality threshold", "Amount Decrease", -999999999.99m, 0m },
                { "MATT-PI", "Materiality threshold", "Percent Increase", 0m, 999.99m },
                { "MATT-PD", "Materiality threshold", "Percent Decrease", -999.99m, 0m },
                { "TONT-AI",  "Amount Increase", "Tonnage change threshold", 0m, 999999999.99m },
                { "TONT-AD", "Amount Decrease", "Tonnage change threshold", -999999999.99m, 0m },
                { "TONT-PI", "Percent Increase", "Tonnage change threshold", 0m, 999.99m },
                { "TONT-PD", "Percent Decrease", "Tonnage change threshold", -999.99m, 0m },
                { "LEVY-ENG", "England", "Levy", 0m, 999999999.99m },
                { "LEVY-WLS", "Wales", "Levy", 0m, 999999999.99m },
                { "LEVY-SCT", "Scotland", "Levy", 0m, 999999999.99m },
                { "LEVY-NIR", "Northern Ireland", "Levy", 0m, 999999999.99m },
             });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
