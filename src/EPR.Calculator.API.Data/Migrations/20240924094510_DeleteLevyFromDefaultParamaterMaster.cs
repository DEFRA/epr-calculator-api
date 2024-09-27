using System;
using System.Data.SqlTypes;
using System.Text;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class DeleteLevyFromDefaultParamaterMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sb = new StringBuilder();
            sb.AppendLine("delete d from dbo.default_parameter_setting_detail d");
            sb.AppendLine("inner join dbo.default_parameter_template_master m");
            sb.AppendLine("on d.parameter_unique_ref = m.parameter_unique_ref");
            sb.AppendLine("where m.parameter_type = 'Levy'");
            var sqlString = sb.ToString();
            migrationBuilder.Sql(sqlString);

            migrationBuilder.Sql("delete from dbo.default_parameter_template_master where parameter_type = 'Levy'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "default_parameter_template_master",
                columns: new[] { "parameter_unique_ref", "parameter_type", "parameter_category", "valid_Range_from", "valid_Range_to" },
                values: new object[,]
                {
                    { "LEVY-ENG", "Levy", "England", 0m, 999999999.99m },
                    { "LEVY-NIR", "Levy", "Northern Ireland", 0m, 999999999.99m },
                    { "LEVY-SCT", "Levy", "Scotland", 0m, 999999999.99m },
                    { "LEVY-WLS", "Levy", "Wales", 0m, 999999999.99m }
                });
        }
    }
}
