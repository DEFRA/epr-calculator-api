using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AllowNegativeLapCap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "ENG-AL",
                column: "total_cost_from",
                value: -999999999.99m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "ENG-FC",
                column: "total_cost_from",
                value: -999999999.99m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "ENG-GL",
                column: "total_cost_from",
                value: -999999999.99m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "ENG-OT",
                column: "total_cost_from",
                value: -999999999.99m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "ENG-PC",
                column: "total_cost_from",
                value: -999999999.99m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "ENG-PL",
                column: "total_cost_from",
                value: -999999999.99m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "ENG-ST",
                column: "total_cost_from",
                value: -999999999.99m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "ENG-WD",
                column: "total_cost_from",
                value: -999999999.99m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "NI-AL",
                column: "total_cost_from",
                value: -999999999.99m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "NI-FC",
                column: "total_cost_from",
                value: -999999999.99m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "NI-GL",
                column: "total_cost_from",
                value: -999999999.99m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "NI-OT",
                column: "total_cost_from",
                value: -999999999.99m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "NI-PC",
                column: "total_cost_from",
                value: -999999999.99m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "NI-PL",
                column: "total_cost_from",
                value: -999999999.99m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "NI-ST",
                column: "total_cost_from",
                value: -999999999.99m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "NI-WD",
                column: "total_cost_from",
                value: -999999999.99m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "SCT-AL",
                column: "total_cost_from",
                value: -999999999.99m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "SCT-FC",
                column: "total_cost_from",
                value: -999999999.99m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "SCT-GL",
                column: "total_cost_from",
                value: -999999999.99m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "SCT-OT",
                column: "total_cost_from",
                value: -999999999.99m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "SCT-PC",
                column: "total_cost_from",
                value: -999999999.99m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "SCT-PL",
                column: "total_cost_from",
                value: -999999999.99m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "SCT-ST",
                column: "total_cost_from",
                value: -999999999.99m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "SCT-WD",
                column: "total_cost_from",
                value: -999999999.99m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "WLS-AL",
                column: "total_cost_from",
                value: -999999999.99m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "WLS-FC",
                column: "total_cost_from",
                value: -999999999.99m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "WLS-GL",
                column: "total_cost_from",
                value: -999999999.99m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "WLS-OT",
                column: "total_cost_from",
                value: -999999999.99m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "WLS-PC",
                column: "total_cost_from",
                value: -999999999.99m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "WLS-PL",
                column: "total_cost_from",
                value: -999999999.99m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "WLS-ST",
                column: "total_cost_from",
                value: -999999999.99m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "WLS-WD",
                column: "total_cost_from",
                value: -999999999.99m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "ENG-AL",
                column: "total_cost_from",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "ENG-FC",
                column: "total_cost_from",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "ENG-GL",
                column: "total_cost_from",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "ENG-OT",
                column: "total_cost_from",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "ENG-PC",
                column: "total_cost_from",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "ENG-PL",
                column: "total_cost_from",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "ENG-ST",
                column: "total_cost_from",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "ENG-WD",
                column: "total_cost_from",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "NI-AL",
                column: "total_cost_from",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "NI-FC",
                column: "total_cost_from",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "NI-GL",
                column: "total_cost_from",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "NI-OT",
                column: "total_cost_from",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "NI-PC",
                column: "total_cost_from",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "NI-PL",
                column: "total_cost_from",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "NI-ST",
                column: "total_cost_from",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "NI-WD",
                column: "total_cost_from",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "SCT-AL",
                column: "total_cost_from",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "SCT-FC",
                column: "total_cost_from",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "SCT-GL",
                column: "total_cost_from",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "SCT-OT",
                column: "total_cost_from",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "SCT-PC",
                column: "total_cost_from",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "SCT-PL",
                column: "total_cost_from",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "SCT-ST",
                column: "total_cost_from",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "SCT-WD",
                column: "total_cost_from",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "WLS-AL",
                column: "total_cost_from",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "WLS-FC",
                column: "total_cost_from",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "WLS-GL",
                column: "total_cost_from",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "WLS-OT",
                column: "total_cost_from",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "WLS-PC",
                column: "total_cost_from",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "WLS-PL",
                column: "total_cost_from",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "WLS-ST",
                column: "total_cost_from",
                value: 0m);

            migrationBuilder.UpdateData(
                table: "lapcap_data_template_master",
                keyColumn: "unique_ref",
                keyValue: "WLS-WD",
                column: "total_cost_from",
                value: 0m);
        }
    }
}
