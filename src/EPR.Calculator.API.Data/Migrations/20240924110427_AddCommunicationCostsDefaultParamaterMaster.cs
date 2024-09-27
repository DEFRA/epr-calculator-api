using System;
using System.Text;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCommunicationCostsDefaultParamaterMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
               table: "default_parameter_template_master",
               columns: new[] { "parameter_unique_ref", "parameter_category", "parameter_type", "valid_Range_from", "valid_Range_to" },
               values: new object[,]
               {
                { "COMC-UK", "United Kingdom", "Communication costs by country", 0m, 999999999.99m },
                { "COMC-ENG", "England", "Communication costs by country", 0m, 999999999.99m },
                { "COMC-WLS", "Wales", "Communication costs by country", 0m, 999999999.99m },
                { "COMC-SCT", "Scotland", "Communication costs by country", 0m, 999999999.99m },
                { "COMC-NIR", "Northern Ireland", "Communication costs by country", 0m, 999999999.99m },
             });

            var sb = new StringBuilder();
            sb.AppendLine("update dbo.default_parameter_template_master");
            sb.AppendLine("set parameter_type = 'Communication costs by material'");
            sb.AppendLine("where parameter_type = 'Communication costs'");
            var sqlString = sb.ToString();
            migrationBuilder.Sql(sqlString);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Down is not needed for this as this is a Data seed table and not a Transaction table
        }
    }
}
