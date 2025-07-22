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
            var createSPSql = "IF OBJECT_ID(N'[dbo].[InsertInvoiceDetailsAtProducerLevel]', 'P') IS NOT NULL  \r\nDROP PROCEDURE [dbo].[InsertInvoiceDetailsAtProducerLevel];\r\nGO\r\nDECLARE @sql NVARCHAR(MAX) \r\nSET @sql = N'CREATE PROCEDURE [dbo].[InsertInvoiceDetailsAtProducerLevel]\r\n(\r\n    @instructionConfirmedBy NVARCHAR(4000),\r\n    @instructionConfirmedDate DATETIME2(7),\r\n    @calculatorRunID INT\r\n)\r\nAS\r\nBEGIN\r\n    SET NOCOUNT OFF\r\n    -- Temp table to hold calculated values\r\n    CREATE TABLE #CalculatedValues (\r\n        calculator_run_id INT,\r\n        producer_id INT,\r\n        total_producer_bill_with_bad_debt DECIMAL(18,2),\r\n        current_year_invoice_total_to_date DECIMAL(18,2),\r\n        amount_liability_difference_calc_vs_prev DECIMAL(18,2),\r\n        suggested_billing_instruction NVARCHAR(4000),\r\n        billing_instruction_accept_reject NVARCHAR(4000),\r\n        invoice_amount DECIMAL(18,2),\r\n        instruction_confirmed_by NVARCHAR(4000),\r\n        instruction_confirmed_date DATETIME2(7),\r\n        billing_instruction_id NVARCHAR(4000)\r\n    );\r\n\r\n    -- Insert into temp table\r\n    INSERT INTO #CalculatedValues\r\n    SELECT \r\n        prsbi.calculator_run_id,\r\n        prsbi.producer_id,\r\n        prsbi.total_producer_bill_with_bad_debt, \r\n        prsbi.current_year_invoice_total_to_date,\r\n        prsbi.amount_liability_difference_calc_vs_prev,\r\n        prsbi.suggested_billing_instruction,\r\n        prsbi.billing_instruction_accept_reject,\r\n        dbo.GetInvoiceAmount(\r\n            prsbi.billing_instruction_accept_reject,\r\n            prsbi.suggested_billing_instruction,\r\n            prsbi.total_producer_bill_with_bad_debt,\r\n            prsbi.amount_liability_difference_calc_vs_prev\r\n        ) AS invoice_amount,\r\n        @instructionConfirmedBy AS instruction_confirmed_by,\r\n        @instructionConfirmedDate AS instruction_confirmed_date,\r\n        CONCAT(prsbi.calculator_run_id, ''-'', prsbi.producer_id) AS billing_instruction_id\r\n    FROM dbo.producer_resultfile_suggested_billing_instruction AS prsbi\r\n    WHERE prsbi.calculator_run_id = @calculatorRunID AND LOWER(prsbi.billing_instruction_accept_reject) = ''accepted'';\r\n\r\n    -- First SELECT using the temp table\r\n\tINSERT INTO [dbo].[producer_designated_run_invoice_instruction] (\r\n\t\tproducer_id,\r\n\t\tcalculator_run_id,\r\n\t\tcurrent_year_invoiced_total_after_this_run,\r\n\t\tinvoice_amount,\r\n\t\toutstanding_balance,\r\n\t\tbilling_instruction_id,\r\n\t\tinstruction_confirmed_date,\r\n\t\tinstruction_confirmed_by\r\n\t\t)\r\n\t\t\tSELECT \r\n\t\t\t\tcv.producer_id,\r\n\t\t\t\tcv.calculator_run_id,\r\n\t\t\t\tdbo.GetCurrentYearInvoicedTotalAfterThisRun(\r\n\t\t\t\t\tcv.billing_instruction_accept_reject,\r\n\t\t\t\t\tcv.suggested_billing_instruction,\r\n\t\t\t\t\tcv.current_year_invoice_total_to_date,\r\n\t\t\t\t\tcv.invoice_amount\r\n\t\t\t\t) AS current_year_invoiced_total_after_this_run,\r\n\t\t\t\tcv.invoice_amount,\r\n\t\t\t\tdbo.GetOutstandingBalance(\r\n\t\t\t\t\tcv.billing_instruction_accept_reject,\r\n\t\t\t\t\tcv.suggested_billing_instruction,\r\n\t\t\t\t\tcv.total_producer_bill_with_bad_debt,\r\n\t\t\t\t\tcv.amount_liability_difference_calc_vs_prev\r\n\t\t\t\t) AS outstanding_balance,\r\n\t\t\t\tcv.billing_instruction_id,\r\n\t\t\t\tcv.instruction_confirmed_date,\r\n\t\t\t\tcv.instruction_confirmed_by\t\t\t\r\n\t\t\tFROM #CalculatedValues AS cv;\r\n\r\n    DROP TABLE #CalculatedValues;\r\nEND'\r\nEXEC(@sql)";
            migrationBuilder.Sql(createSPSql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var dropSPSql = "IF OBJECT_ID(N'[dbo].[InsertInvoiceDetailsAtProducerLevel]', 'P') IS NOT NULL  \r\nDROP PROCEDURE [dbo].[InsertInvoiceDetailsAtProducerLevel];\r\nGO";
            migrationBuilder.Sql(dropSPSql);
        }
    }
}
