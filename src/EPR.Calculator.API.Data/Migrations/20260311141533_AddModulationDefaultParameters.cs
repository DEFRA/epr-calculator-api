using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddModulationDefaultParameters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "LRET-AL",
                columns: new[] { "parameter_category", "parameter_type", "valid_Range_to" },
                values: new object[] { "Aluminium-A", "Late reporting tonnage", 999999999.999m });

            migrationBuilder.UpdateData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "LRET-FC",
                columns: new[] { "parameter_category", "parameter_type", "valid_Range_to" },
                values: new object[] { "Fibre composite-A", "Late reporting tonnage", 999999999.999m });

            migrationBuilder.UpdateData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "LRET-GL",
                columns: new[] { "parameter_category", "parameter_type", "valid_Range_to" },
                values: new object[] { "Glass-A", "Late reporting tonnage", 999999999.999m });

            migrationBuilder.UpdateData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "LRET-OT",
                columns: new[] { "parameter_category", "parameter_type", "valid_Range_to" },
                values: new object[] { "Other materials-A", "Late reporting tonnage", 999999999.999m });

            migrationBuilder.UpdateData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "LRET-PC",
                columns: new[] { "parameter_category", "parameter_type", "valid_Range_to" },
                values: new object[] { "Paper or card-A", "Late reporting tonnage", 999999999.999m });

            migrationBuilder.UpdateData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "LRET-PL",
                columns: new[] { "parameter_category", "parameter_type", "valid_Range_to" },
                values: new object[] { "Plastic-A", "Late reporting tonnage", 999999999.999m });

            migrationBuilder.UpdateData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "LRET-ST",
                columns: new[] { "parameter_category", "parameter_type", "valid_Range_to" },
                values: new object[] { "Steel-A", "Late reporting tonnage", 999999999.999m });

            migrationBuilder.UpdateData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "LRET-WD",
                columns: new[] { "parameter_category", "parameter_type", "valid_Range_to" },
                values: new object[] { "Wood-A", "Late reporting tonnage", 999999999.999m });

            migrationBuilder.InsertData(
                table: "default_parameter_template_master",
                columns: new[] { "parameter_unique_ref", "parameter_category", "parameter_type", "valid_Range_from", "valid_Range_to" },
                values: new object[,]
                {
                    { "LRET-AL-G", "Aluminium-G", "Late reporting tonnage", 0m, 999999999.999m },
                    { "LRET-AL-R", "Aluminium-R", "Late reporting tonnage", 0m, 999999999.999m },
                    { "LRET-FC-G", "Fibre composite-G", "Late reporting tonnage", 0m, 999999999.999m },
                    { "LRET-FC-R", "Fibre composite-R", "Late reporting tonnage", 0m, 999999999.999m },
                    { "LRET-GL-G", "Glass-G", "Late reporting tonnage", 0m, 999999999.999m },
                    { "LRET-GL-R", "Glass-R", "Late reporting tonnage", 0m, 999999999.999m },
                    { "LRET-OT-G", "Other materials-G", "Late reporting tonnage", 0m, 999999999.999m },
                    { "LRET-OT-R", "Other materials-R", "Late reporting tonnage", 0m, 999999999.999m },
                    { "LRET-PC-G", "Paper or card-G", "Late reporting tonnage", 0m, 999999999.999m },
                    { "LRET-PC-R", "Paper or card-R", "Late reporting tonnage", 0m, 999999999.999m },
                    { "LRET-PL-G", "Plastic-G", "Late reporting tonnage", 0m, 999999999.999m },
                    { "LRET-PL-R", "Plastic-R", "Late reporting tonnage", 0m, 999999999.999m },
                    { "LRET-ST-G", "Steel-G", "Late reporting tonnage", 0m, 999999999.999m },
                    { "LRET-ST-R", "Steel-R", "Late reporting tonnage", 0m, 999999999.999m },
                    { "LRET-WD-G", "Wood-G", "Late reporting tonnage", 0m, 999999999.999m },
                    { "LRET-WD-R", "Wood-R", "Late reporting tonnage", 0m, 999999999.999m },
                    { "REDM-RF", "Modulation Factor", "Red modulation factor", 1.000m, 2.000m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "LRET-AL-G");

            migrationBuilder.DeleteData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "LRET-AL-R");

            migrationBuilder.DeleteData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "LRET-FC-G");

            migrationBuilder.DeleteData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "LRET-FC-R");

            migrationBuilder.DeleteData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "LRET-GL-G");

            migrationBuilder.DeleteData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "LRET-GL-R");

            migrationBuilder.DeleteData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "LRET-OT-G");

            migrationBuilder.DeleteData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "LRET-OT-R");

            migrationBuilder.DeleteData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "LRET-PC-G");

            migrationBuilder.DeleteData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "LRET-PC-R");

            migrationBuilder.DeleteData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "LRET-PL-G");

            migrationBuilder.DeleteData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "LRET-PL-R");

            migrationBuilder.DeleteData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "LRET-ST-G");

            migrationBuilder.DeleteData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "LRET-ST-R");

            migrationBuilder.DeleteData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "LRET-WD-G");

            migrationBuilder.DeleteData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "LRET-WD-R");

            migrationBuilder.DeleteData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "REDM-RF");

            migrationBuilder.UpdateData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "LRET-AL",
                columns: new[] { "parameter_category", "parameter_type", "valid_Range_to" },
                values: new object[] { "Late reporting tonnage", "Aluminium", 999999999.99m });

            migrationBuilder.UpdateData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "LRET-FC",
                columns: new[] { "parameter_category", "parameter_type", "valid_Range_to" },
                values: new object[] { "Late reporting tonnage", "Aluminium", 999999999.99m });

            migrationBuilder.UpdateData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "LRET-GL",
                columns: new[] { "parameter_category", "parameter_type", "valid_Range_to" },
                values: new object[] { "Late reporting tonnage", "Aluminium", 999999999.99m });

            migrationBuilder.UpdateData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "LRET-OT",
                columns: new[] { "parameter_category", "parameter_type", "valid_Range_to" },
                values: new object[] { "Late reporting tonnage", "Aluminium", 999999999.99m });

            migrationBuilder.UpdateData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "LRET-PC",
                columns: new[] { "parameter_category", "parameter_type", "valid_Range_to" },
                values: new object[] { "Late reporting tonnage", "Aluminium", 999999999.99m });

            migrationBuilder.UpdateData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "LRET-PL",
                columns: new[] { "parameter_category", "parameter_type", "valid_Range_to" },
                values: new object[] { "Late reporting tonnage", "Aluminium", 999999999.99m });

            migrationBuilder.UpdateData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "LRET-ST",
                columns: new[] { "parameter_category", "parameter_type", "valid_Range_to" },
                values: new object[] { "Late reporting tonnage", "Aluminium", 999999999.99m });

            migrationBuilder.UpdateData(
                table: "default_parameter_template_master",
                keyColumn: "parameter_unique_ref",
                keyValue: "LRET-WD",
                columns: new[] { "parameter_category", "parameter_type", "valid_Range_to" },
                values: new object[] { "Late reporting tonnage", "Aluminium", 999999999.99m });
        }
    }
}
