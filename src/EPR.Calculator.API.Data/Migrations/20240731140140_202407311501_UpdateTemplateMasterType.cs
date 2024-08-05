using Microsoft.EntityFrameworkCore.Migrations;
using System.Diagnostics.CodeAnalysis;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    [ExcludeFromCodeCoverage]
    /// <inheritdoc />
    public partial class _202407311501_UpdateTemplateMasterType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "LRET-FC",
                column: "parameter_type",
                value: "Fibre Composite"
            );

            migrationBuilder.UpdateData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "LRET-GL",
                column: "parameter_type",
                value: "Glass"
            );

            migrationBuilder.UpdateData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "LRET-OT",
                column: "parameter_type",
                value: "Other"
            );

            migrationBuilder.UpdateData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "LRET-PC",
                column: "parameter_type",
                value: "Paper Or Card"
            );

            migrationBuilder.UpdateData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "LRET-PL",
                column: "parameter_type",
                value: "Plastic"
            );

            migrationBuilder.UpdateData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "LRET-ST",
                column: "parameter_type",
                value: "Steel"
            );

            migrationBuilder.UpdateData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "LRET-WD",
                column: "parameter_type",
                value: "Wood"
            );

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Down is not needed for this as this is a Data seed table and not a Transaction table
        }
    }
}
