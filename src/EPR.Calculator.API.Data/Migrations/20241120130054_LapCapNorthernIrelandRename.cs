using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class LapCapNorthernIrelandRename : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "NI-AL",
                column: "country",
                value: "Northern Ireland");

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "NI-FC",
                column: "country",
                value: "Northern Ireland");

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "NI-GL",
                column: "country",
                value: "Northern Ireland");

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "NI-OT",
                column: "country",
                value: "Northern Ireland");

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "NI-PC",
                column: "country",
                value: "Northern Ireland");

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "NI-PL",
                column: "country",
                value: "Northern Ireland");

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "NI-ST",
                column: "country",
                value: "Northern Ireland");

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "NI-WD",
                column: "country",
                value: "Northern Ireland");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "NI-AL",
                column: "country",
                value: "NI");

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "NI-FC",
                column: "country",
                value: "NI");

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "NI-GL",
                column: "country",
                value: "NI");

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "NI-OT",
                column: "country",
                value: "NI");

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "NI-PC",
                column: "country",
                value: "NI");

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "NI-PL",
                column: "country",
                value: "NI");

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "NI-ST",
                column: "country",
                value: "NI");

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "NI-WD",
                column: "country",
                value: "NI");
        }
    }
}
