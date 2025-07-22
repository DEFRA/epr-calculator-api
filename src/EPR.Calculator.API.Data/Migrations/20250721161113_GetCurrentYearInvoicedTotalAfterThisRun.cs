using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class GetCurrentYearInvoicedTotalAfterThisRun : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var createFunctionSql = "IF OBJECT_ID(N'[dbo].[GetCurrentYearInvoicedTotalAfterThisRun]', 'FN') IS NOT NULL  \r\nDROP FUNCTION [dbo].GetCurrentYearInvoicedTotalAfterThisRun\r\nGO\r\nDECLARE @sql NVARCHAR(MAX) \r\nSET @sql = N'CREATE FUNCTION [dbo].[GetCurrentYearInvoicedTotalAfterThisRun] ( \r\n    @billingInstructionAcceptReject      VARCHAR(250),\r\n    @suggestedBillingInstruction         VARCHAR(250),\r\n    @currentYearInvoicedTotalToDate      DECIMAL(18,2),\r\n    @invoiceAmount                       DECIMAL(18,2)\r\n)\r\nRETURNS DECIMAL(18,2)\r\nAS\r\nBEGIN\r\n    -- Rule 1: Cancelled instruction always returns NULL\r\n    IF @suggestedBillingInstruction = ''CANCEL''\r\n        RETURN NULL;\r\n\r\n    -- Rule 2: Rejected INITIAL returns NULL\r\n    IF @billingInstructionAcceptReject = ''Rejected'' AND @suggestedBillingInstruction = ''INITIAL''\r\n        RETURN NULL;\r\n\r\n    -- Rule 3: Rejected (but not INITIAL) returns current total as-is\r\n    IF @billingInstructionAcceptReject = ''Rejected''\r\n        RETURN ISNULL(@currentYearInvoicedTotalToDate, 0);\r\n\r\n    -- Rule 4: Accepted or any other case adds invoice amount\r\n    RETURN ISNULL(@currentYearInvoicedTotalToDate, 0) + ISNULL(@invoiceAmount, 0);\r\nEND'\r\nEXEC(@sql)";
            migrationBuilder.Sql(createFunctionSql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var dropFunctionSql = "IF OBJECT_ID(N'[dbo].[GetCurrentYearInvoicedTotalAfterThisRun]', 'FN') IS NOT NULL  \r\nDROP FUNCTION [dbo].GetCurrentYearInvoicedTotalAfterThisRun\r\nGO";
            migrationBuilder.Sql(dropFunctionSql);
        }
    }
}
