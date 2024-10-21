using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOtherMaterialsDefaultParameterMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "COMC-OT",
                column: "parameter_category",
                value: "Other materials");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "COMC-OT",
                column: "parameter_category",
                value: "Other");
        }
    }
}
