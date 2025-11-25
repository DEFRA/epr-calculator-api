using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNonClusteredIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_producer_designated_run_invoice_instruction_calculator_run_id",
                table: "producer_designated_run_invoice_instruction");

            migrationBuilder.DropIndex(
                name: "IX_calculator_run_calculator_run_classification_id",
                table: "calculator_run");

            migrationBuilder.CreateIndex(
                name: "IX_index_producer_invoiced_material_net_tonnage",
                table: "producer_invoiced_material_net_tonnage",
                columns: new[] { "producer_id", "calculator_run_id", "id" })
                .Annotation("SqlServer:Clustered", false)
                .Annotation("SqlServer:Include", new[] { "material_id", "invoiced_net_tonnage" });

            migrationBuilder.CreateIndex(
                name: "IX_index_producer_designated_run_invoice",
                table: "producer_designated_run_invoice_instruction",
                columns: new[] { "calculator_run_id", "producer_id", "id" })
                .Annotation("SqlServer:Clustered", false)
                .Annotation("SqlServer:Include", new[] { "current_year_invoiced_total_after_this_run", "invoice_amount", "outstanding_balance", "billing_instruction_id", "instruction_confirmed_date", "instruction_confirmed_by" });

            migrationBuilder.CreateIndex(
                name: "IX_index_calculator_run",
                table: "calculator_run",
                columns: new[] { "calculator_run_classification_id", "financial_year", "is_billing_file_generating", "id" })
                .Annotation("SqlServer:Clustered", false)
                .Annotation("SqlServer:Include", new[] { "name", "created_by", "created_at", "updated_by", "updated_at", "calculator_run_organization_data_master_id", "calculator_run_pom_data_master_id", "default_parameter_setting_master_id", "lapcap_data_master_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_index_producer_invoiced_material_net_tonnage",
                table: "producer_invoiced_material_net_tonnage");

            migrationBuilder.DropIndex(
                name: "IX_index_producer_designated_run_invoice",
                table: "producer_designated_run_invoice_instruction");

            migrationBuilder.DropIndex(
                name: "IX_index_calculator_run",
                table: "calculator_run");

            migrationBuilder.CreateIndex(
                name: "IX_producer_designated_run_invoice_instruction_calculator_run_id",
                table: "producer_designated_run_invoice_instruction",
                column: "calculator_run_id");

            migrationBuilder.CreateIndex(
                name: "IX_calculator_run_calculator_run_classification_id",
                table: "calculator_run",
                column: "calculator_run_classification_id");
        }
    }
}
