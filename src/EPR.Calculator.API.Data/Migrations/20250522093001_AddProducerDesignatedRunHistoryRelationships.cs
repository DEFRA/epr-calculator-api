using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProducerDesignatedRunHistoryRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_producer_resultfile_suggested_billing_instruction_calculator_run_id",
                table: "producer_resultfile_suggested_billing_instruction",
                column: "calculator_run_id");

            migrationBuilder.CreateIndex(
                name: "IX_producer_invoiced_material_net_tonnage_calculator_run_id",
                table: "producer_invoiced_material_net_tonnage",
                column: "calculator_run_id");

            migrationBuilder.CreateIndex(
                name: "IX_producer_invoiced_material_net_tonnage_material_id",
                table: "producer_invoiced_material_net_tonnage",
                column: "material_id");

            migrationBuilder.CreateIndex(
                name: "IX_producer_designated_run_invoice_instruction_calculator_run_id",
                table: "producer_designated_run_invoice_instruction",
                column: "calculator_run_id");

            migrationBuilder.AddForeignKey(
                name: "FK_producer_designated_run_invoice_instruction_calculator_run_calculator_run_id",
                table: "producer_designated_run_invoice_instruction",
                column: "calculator_run_id",
                principalTable: "calculator_run",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_producer_invoiced_material_net_tonnage_calculator_run_calculator_run_id",
                table: "producer_invoiced_material_net_tonnage",
                column: "calculator_run_id",
                principalTable: "calculator_run",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_producer_invoiced_material_net_tonnage_material_material_id",
                table: "producer_invoiced_material_net_tonnage",
                column: "material_id",
                principalTable: "material",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_producer_resultfile_suggested_billing_instruction_calculator_run_calculator_run_id",
                table: "producer_resultfile_suggested_billing_instruction",
                column: "calculator_run_id",
                principalTable: "calculator_run",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_producer_designated_run_invoice_instruction_calculator_run_calculator_run_id",
                table: "producer_designated_run_invoice_instruction");

            migrationBuilder.DropForeignKey(
                name: "FK_producer_invoiced_material_net_tonnage_calculator_run_calculator_run_id",
                table: "producer_invoiced_material_net_tonnage");

            migrationBuilder.DropForeignKey(
                name: "FK_producer_invoiced_material_net_tonnage_material_material_id",
                table: "producer_invoiced_material_net_tonnage");

            migrationBuilder.DropForeignKey(
                name: "FK_producer_resultfile_suggested_billing_instruction_calculator_run_calculator_run_id",
                table: "producer_resultfile_suggested_billing_instruction");

            migrationBuilder.DropIndex(
                name: "IX_producer_resultfile_suggested_billing_instruction_calculator_run_id",
                table: "producer_resultfile_suggested_billing_instruction");

            migrationBuilder.DropIndex(
                name: "IX_producer_invoiced_material_net_tonnage_calculator_run_id",
                table: "producer_invoiced_material_net_tonnage");

            migrationBuilder.DropIndex(
                name: "IX_producer_invoiced_material_net_tonnage_material_id",
                table: "producer_invoiced_material_net_tonnage");

            migrationBuilder.DropIndex(
                name: "IX_producer_designated_run_invoice_instruction_calculator_run_id",
                table: "producer_designated_run_invoice_instruction");
        }
    }
}
