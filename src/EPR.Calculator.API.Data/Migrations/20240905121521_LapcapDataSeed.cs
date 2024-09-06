using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class LapcapDataSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 1,
                column: "created_at",
                value: new DateTime(2024, 9, 5, 13, 15, 21, 595, DateTimeKind.Local).AddTicks(3009));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 2,
                column: "created_at",
                value: new DateTime(2024, 9, 5, 13, 15, 21, 595, DateTimeKind.Local).AddTicks(3012));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 3,
                column: "created_at",
                value: new DateTime(2024, 9, 5, 13, 15, 21, 595, DateTimeKind.Local).AddTicks(3014));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 4,
                column: "created_at",
                value: new DateTime(2024, 9, 5, 13, 15, 21, 595, DateTimeKind.Local).AddTicks(3017));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 5,
                column: "created_at",
                value: new DateTime(2024, 9, 5, 13, 15, 21, 595, DateTimeKind.Local).AddTicks(3019));

            migrationBuilder.InsertData(
                table: "lapcap_data_template_master",
                columns: new[] { "unique_ref", "country", "material", "total_cost_from", "total_cost_to" },
                values: new object[,]
                {
                    { "ENG-AL", "England", "Aluminium", 0m, 999999999.99m },
                    { "ENG-FC", "England", "Fibre composite", 0m, 999999999.99m },
                    { "ENG-GL", "England", "Glass", 0m, 999999999.99m },
                    { "ENG-OT", "England", "Other", 0m, 999999999.99m },
                    { "ENG-PC", "England", "Paper or card", 0m, 999999999.99m },
                    { "ENG-PL", "England", "Plastic", 0m, 999999999.99m },
                    { "ENG-ST", "England", "Steel", 0m, 999999999.99m },
                    { "ENG-WD", "England", "Wood", 0m, 999999999.99m },
                    { "NI-AL", "NI", "Aluminium", 0m, 999999999.99m },
                    { "NI-FC", "NI", "Fibre composite", 0m, 999999999.99m },
                    { "NI-GL", "NI", "Glass", 0m, 999999999.99m },
                    { "NI-OT", "NI", "Other", 0m, 999999999.99m },
                    { "NI-PC", "NI", "Paper or card", 0m, 999999999.99m },
                    { "NI-PL", "NI", "Plastic", 0m, 999999999.99m },
                    { "NI-ST", "NI", "Steel", 0m, 999999999.99m },
                    { "NI-WD", "NI", "Wood", 0m, 999999999.99m },
                    { "SCT-AL", "Scotland", "Aluminium", 0m, 999999999.99m },
                    { "SCT-FC", "Scotland", "Fibre composite", 0m, 999999999.99m },
                    { "SCT-GL", "Scotland", "Glass", 0m, 999999999.99m },
                    { "SCT-OT", "Scotland", "Other", 0m, 999999999.99m },
                    { "SCT-PC", "Scotland", "Paper or card", 0m, 999999999.99m },
                    { "SCT-PL", "Scotland", "Plastic", 0m, 999999999.99m },
                    { "SCT-ST", "Scotland", "Steel", 0m, 999999999.99m },
                    { "SCT-WD", "Scotland", "Wood", 0m, 999999999.99m },
                    { "WLS-AL", "Wales", "Aluminium", 0m, 999999999.99m },
                    { "WLS-FC", "Wales", "Fibre composite", 0m, 999999999.99m },
                    { "WLS-GL", "Wales", "Glass", 0m, 999999999.99m },
                    { "WLS-OT", "Wales", "Other", 0m, 999999999.99m },
                    { "WLS-PC", "Wales", "Paper or card", 0m, 999999999.99m },
                    { "WLS-PL", "Wales", "Plastic", 0m, 999999999.99m },
                    { "WLS-ST", "Wales", "Steel", 0m, 999999999.99m },
                    { "WLS-WD", "Wales", "Wood", 0m, 999999999.99m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "ENG-AL");

            migrationBuilder.DeleteData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "ENG-FC");

            migrationBuilder.DeleteData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "ENG-GL");

            migrationBuilder.DeleteData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "ENG-OT");

            migrationBuilder.DeleteData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "ENG-PC");

            migrationBuilder.DeleteData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "ENG-PL");

            migrationBuilder.DeleteData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "ENG-ST");

            migrationBuilder.DeleteData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "ENG-WD");

            migrationBuilder.DeleteData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "NI-AL");

            migrationBuilder.DeleteData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "NI-FC");

            migrationBuilder.DeleteData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "NI-GL");

            migrationBuilder.DeleteData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "NI-OT");

            migrationBuilder.DeleteData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "NI-PC");

            migrationBuilder.DeleteData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "NI-PL");

            migrationBuilder.DeleteData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "NI-ST");

            migrationBuilder.DeleteData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "NI-WD");

            migrationBuilder.DeleteData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "SCT-AL");

            migrationBuilder.DeleteData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "SCT-FC");

            migrationBuilder.DeleteData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "SCT-GL");

            migrationBuilder.DeleteData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "SCT-OT");

            migrationBuilder.DeleteData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "SCT-PC");

            migrationBuilder.DeleteData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "SCT-PL");

            migrationBuilder.DeleteData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "SCT-ST");

            migrationBuilder.DeleteData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "SCT-WD");

            migrationBuilder.DeleteData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "WLS-AL");

            migrationBuilder.DeleteData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "WLS-FC");

            migrationBuilder.DeleteData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "WLS-GL");

            migrationBuilder.DeleteData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "WLS-OT");

            migrationBuilder.DeleteData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "WLS-PC");

            migrationBuilder.DeleteData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "WLS-PL");

            migrationBuilder.DeleteData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "WLS-ST");

            migrationBuilder.DeleteData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "WLS-WD");

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 1,
                column: "created_at",
                value: new DateTime(2024, 9, 5, 12, 5, 3, 773, DateTimeKind.Local).AddTicks(2558));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 2,
                column: "created_at",
                value: new DateTime(2024, 9, 5, 12, 5, 3, 773, DateTimeKind.Local).AddTicks(2561));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 3,
                column: "created_at",
                value: new DateTime(2024, 9, 5, 12, 5, 3, 773, DateTimeKind.Local).AddTicks(2563));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 4,
                column: "created_at",
                value: new DateTime(2024, 9, 5, 12, 5, 3, 773, DateTimeKind.Local).AddTicks(2565));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 5,
                column: "created_at",
                value: new DateTime(2024, 9, 5, 12, 5, 3, 773, DateTimeKind.Local).AddTicks(2567));
        }
    }
}
