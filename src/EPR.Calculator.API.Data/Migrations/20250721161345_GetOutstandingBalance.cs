using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class GetOutstandingBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var createFunctionSql = "IF OBJECT_ID(N'[dbo].[GetOutstandingBalance]', 'FN') IS NOT NULL  \r\nDROP FUNCTION [dbo].GetOutstandingBalance\r\nGO\r\nDECLARE @sql NVARCHAR(MAX) \r\nSET @sql = N'CREATE FUNCTION [dbo].[GetOutstandingBalance] (\r\n    @billingInstructionAcceptReject        VARCHAR(250),\r\n    @suggestedBillingInstruction           VARCHAR(250),\r\n    @totalProducerBillWithBadDebtProvision DECIMAL(18,2),\r\n    @LiabilityDifference                   DECIMAL(18,2)\r\n)\r\nRETURNS DECIMAL(18,2)\r\nAS\r\nBEGIN\r\n    RETURN \r\n        CASE \r\n            WHEN @billingInstructionAcceptReject <> ''Accepted'' AND @suggestedBillingInstruction = ''INITIAL''\r\n                THEN @totalProducerBillWithBadDebtProvision\r\n\r\n            WHEN @billingInstructionAcceptReject <> ''Accepted''\r\n                THEN @LiabilityDifference\r\n\r\n            ELSE NULL\r\n        END;\r\nEND\r\nEXEC(@sql)'";
            migrationBuilder.Sql(createFunctionSql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var dropFunctionSql = "IF OBJECT_ID(N'[dbo].[GetOutstandingBalance]', 'FN') IS NOT NULL  \r\nDROP FUNCTION [dbo].GetOutstandingBalance\r\nGO";
            migrationBuilder.Sql(dropFunctionSql);
        }
    }
}
