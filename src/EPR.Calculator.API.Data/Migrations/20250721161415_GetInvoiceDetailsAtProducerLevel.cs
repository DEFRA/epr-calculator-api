using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class GetInvoiceDetailsAtProducerLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var createSPSql = "IF OBJECT_ID(N'[dbo].[GetInvoiceDetailsAtProducerLevel]', 'P') IS NOT NULL  \r\nDROP PROCEDURE [dbo].GetInvoiceDetailsAtProducerLevel\r\nGO\r\nDECLARE @sql NVARCHAR(MAX) \r\nSET @sql = N'CREATE PROCEDURE [dbo].[GetInvoiceDetailsAtProducerLevel]\r\n(\r\n    @instructionConfirmedBy NVARCHAR(4000),\r\n    @instructionConfirmedDate DATE,\r\n    @calculatorRunID INT\r\n)\r\nAS\r\nBEGIN\r\n    SET NOCOUNT ON;\r\n\r\n    ;WITH CalculatedValues AS (\r\n        SELECT \r\n            prsbi.calculator_run_id,\r\n            prsbi.producer_id,\r\n            prsbi.total_producer_bill_with_bad_debt, \r\n            prsbi.current_year_invoice_total_to_date,\r\n            prsbi.amount_liability_difference_calc_vs_prev,\r\n            prsbi.suggested_billing_instruction,\r\n            prsbi.suggested_invoice_amount,\r\n            prsbi.billing_instruction_accept_reject,\r\n            dbo.GetInvoiceAmount(\r\n                prsbi.billing_instruction_accept_reject,\r\n                prsbi.suggested_billing_instruction,\r\n                prsbi.total_producer_bill_with_bad_debt,\r\n                prsbi.amount_liability_difference_calc_vs_prev\r\n            ) AS invoice_amount,\r\n\r\n            @instructionConfirmedBy AS instruction_confirmed_by,\r\n            @instructionConfirmedDate AS instruction_confirmed_date,\r\n            CONCAT(prsbi.calculator_run_id, ''-'', prsbi.producer_id) AS billing_instruction_id\r\n\r\n        FROM dbo.producer_resultfile_suggested_billing_instruction AS prsbi\r\n        WHERE prsbi.calculator_run_id = @calculatorRunID\r\n    )\r\n\r\n    SELECT \r\n        cv.calculator_run_id,\r\n        cv.producer_id,\r\n        cv.total_producer_bill_with_bad_debt, \r\n        cv.current_year_invoice_total_to_date,\r\n        cv.amount_liability_difference_calc_vs_prev,\r\n        cv.suggested_billing_instruction,\r\n        cv.suggested_invoice_amount,\r\n        cv.billing_instruction_accept_reject,\r\n        cv.invoice_amount,\r\n        dbo.GetCurrentYearInvoicedTotalAfterThisRun(\r\n            cv.billing_instruction_accept_reject,\r\n            cv.suggested_billing_instruction,\r\n            cv.current_year_invoice_total_to_date,\r\n            cv.invoice_amount\r\n        ) AS current_year_invoiced_total_after_this_run,\r\n        dbo.GetOutstandingBalance(\r\n            cv.billing_instruction_accept_reject,\r\n            cv.suggested_billing_instruction,\r\n            cv.total_producer_bill_with_bad_debt,\r\n            cv.amount_liability_difference_calc_vs_prev\r\n        ) AS outstanding_balance,\r\n        cv.instruction_confirmed_by,\r\n        cv.instruction_confirmed_date,\r\n        cv.billing_instruction_id\r\n\r\n    FROM CalculatedValues AS cv;\r\nEND\r\nEXEC(@sql)'\r\n\r\n\r\n\r\n\r\n\t";
            migrationBuilder.Sql(createSPSql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var dropSPSql = "IF OBJECT_ID(N'[dbo].[GetInvoiceDetailsAtProducerLevel]', 'P') IS NOT NULL  \r\nDROP PROCEDURE [dbo].GetInvoiceDetailsAtProducerLevel\r\nGO";
            migrationBuilder.Sql(dropSPSql);
        }
    }
}
