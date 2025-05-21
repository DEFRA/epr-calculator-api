using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProducerDesignatedRunInvoiceInstruction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "producer_designated_run_invoice_instruction",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    producer_id = table.Column<int>(type: "int", nullable: false),
                    calculator_run_id = table.Column<int>(type: "int", nullable: false),
                    current_year_invoiced_total_after_this_run = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    invoice_amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    outstanding_balance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    billing_instruction_id = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    instruction_confirmed_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    instruction_confirmed_by = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_producer_designated_run_invoice_instruction", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "producer_designated_run_invoice_instruction");
        }
    }
}
