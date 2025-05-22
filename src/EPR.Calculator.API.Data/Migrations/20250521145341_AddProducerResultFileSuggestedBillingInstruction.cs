using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProducerResultFileSuggestedBillingInstruction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "producer_resultfile_suggested_billing_instruction",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    calculator_run_id = table.Column<int>(type: "int", nullable: false),
                    producer_id = table.Column<int>(type: "int", nullable: false),
                    total_producer_bill_with_bad_debt = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    current_year_invoice_total_to_date = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    tonnage_change_since_last_invoice = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    amount_liability_difference_calc_vs_prev = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    material_pound_threshold_breached = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    tonnage_pound_threshold_breached = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    percentage_liability_difference_calc_vs_prev = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    material_percentage_threshold_breached = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    tonnage_percentage_threshold_breached = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    suggested_billing_instruction = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    suggested_invoice_amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    billing_instruction_accept_reject = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    reason_for_rejection = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    last_modified_accept_reject_by = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    last_modified_accept_reject = table.Column<DateTime>(type: "datetime2", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_producer_resultfile_suggested_billing_instruction", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "producer_resultfile_suggested_billing_instruction");
        }
    }
}
