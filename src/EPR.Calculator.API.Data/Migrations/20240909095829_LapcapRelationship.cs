using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class LapcapRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_lapcap_data_detail_lapcap_data_template_master_unique_ref",
                table: "lapcap_data_detail");

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 1,
                column: "created_at",
                value: new DateTime(2024, 9, 9, 10, 58, 28, 730, DateTimeKind.Local).AddTicks(9507));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 2,
                column: "created_at",
                value: new DateTime(2024, 9, 9, 10, 58, 28, 730, DateTimeKind.Local).AddTicks(9513));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 3,
                column: "created_at",
                value: new DateTime(2024, 9, 9, 10, 58, 28, 730, DateTimeKind.Local).AddTicks(9519));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 4,
                column: "created_at",
                value: new DateTime(2024, 9, 9, 10, 58, 28, 730, DateTimeKind.Local).AddTicks(9523));

            migrationBuilder.UpdateData(
                table: "calculator_run_classification",
                keyColumn: "id",
                keyValue: 5,
                column: "created_at",
                value: new DateTime(2024, 9, 9, 10, 58, 28, 730, DateTimeKind.Local).AddTicks(9528));

            migrationBuilder.CreateIndex(
                name: "IX_lapcap_data_detail_lapcap_data_template_master_unique_ref",
                table: "lapcap_data_detail",
                column: "lapcap_data_template_master_unique_ref");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_lapcap_data_detail_lapcap_data_template_master_unique_ref",
                table: "lapcap_data_detail");

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

            migrationBuilder.CreateIndex(
                name: "IX_lapcap_data_detail_lapcap_data_template_master_unique_ref",
                table: "lapcap_data_detail",
                column: "lapcap_data_template_master_unique_ref",
                unique: true);
        }
    }
}
