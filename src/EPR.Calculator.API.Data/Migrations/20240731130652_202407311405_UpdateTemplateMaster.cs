using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    [ExcludeFromCodeCoverage]
    /// <inheritdoc />
    public partial class _202407311405_UpdateTemplateMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "default_parameter_template_master",
                columns: new[] { "parameter_unique_ref", "parameter_type", "parameter_category", "valid_Range_from", "valid_Range_to" },
                values: new Object[,] {
                                {
                                    "TONT-AD", "Amount Decrease", "Tonnage change threshold", 0.00M, 999999999.99M
                                }
                }
            );

            migrationBuilder.UpdateData(
                table: "default_parameter_setting_detail",
                keyColumn: "parameter_unique_ref",
                keyValue: "TONT-DI",
                column: "parameter_unique_ref",
                value: "TONT-AD"
            );

            migrationBuilder.DeleteData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "TONT-DI"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Down is not needed for this as this is a Data seed table and not a Transaction table
        }
    }
}
