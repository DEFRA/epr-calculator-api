using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    [ExcludeFromCodeCoverage]
    /// <inheritdoc />
    public partial class _202407311451_UpdateTemplateMasterValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "MATT-PD",
                column: "valid_Range_from",
                value: -1000M
            );

            migrationBuilder.UpdateData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "MATT-PD",
                column: "valid_Range_to",
                value: 0M
            );

            migrationBuilder.UpdateData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "TONT-PD",
                column: "valid_Range_from",
                value: -1000M
            );

            migrationBuilder.UpdateData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "TONT-PD",
                column: "valid_Range_to",
                value: 0M
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Down is not needed for this as this is a Data seed table and not a Transaction table
        }
    }
}
