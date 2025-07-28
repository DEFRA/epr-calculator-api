using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFunctionGetInvoiceAmount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var createFunctionSql = "IF OBJECT_ID(N'[dbo].[GetInvoiceAmount]', 'FN') IS NOT NULL\r\n    DROP FUNCTION [dbo].[GetInvoiceAmount]\r\nGO\r\ndeclare @sql nvarchar(max)\r\nSET @sql = N'\r\nCREATE FUNCTION [dbo].[GetInvoiceAmount] ( \r\n    @billingInstructionAcceptReject VARCHAR(250),\r\n    @suggestedBillingInstruction    VARCHAR(250),\r\n    @totalProducerBillWithBadDebtProvision DECIMAL(18,2),\r\n    @LiabilityDifference           DECIMAL(18,2)\r\n)\r\nRETURNS DECIMAL(18,2)\r\nAS\r\nBEGIN\r\n    IF @billingInstructionAcceptReject <> ''Accepted''\r\n        RETURN NULL;\r\n\r\n    RETURN \r\n        CASE \r\n            WHEN @suggestedBillingInstruction IN (''INITIAL'', ''REBILL'') THEN @totalProducerBillWithBadDebtProvision\r\n            WHEN @suggestedBillingInstruction = ''DELTA'' THEN @LiabilityDifference\r\n            ELSE NULL\r\n        END;\r\nEND'\r\n\r\nEXEC(@sql)";
            migrationBuilder.Sql(createFunctionSql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var dropFunctionSql = "IF OBJECT_ID(N'[dbo].[GetInvoiceAmount]', 'FN') IS NOT NULL\r\n    DROP FUNCTION [dbo].[GetInvoiceAmount]\r\nGO";
            migrationBuilder.Sql(dropFunctionSql);
        }
    }
}
