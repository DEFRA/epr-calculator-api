using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EPR.Calculator.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStoredProcsAndFuncs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.CreateRunOrganization");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.CreateRunPom");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS dbo.InsertInvoiceDetailsAtProducerLevel");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS dbo.GetCurrentYearInvoicedTotalAfterThisRun");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS dbo.GetInvoiceAmount");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS dbo.GetOutstandingBalance");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var createRunOrgSqlString = @"IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreateRunOrganization]') AND type = N'P')
                DROP PROCEDURE [dbo].[CreateRunOrganization];
                declare @Sql varchar(max);
                SET @Sql = N'CREATE PROCEDURE [dbo].[CreateRunOrganization]                
                (                    @RunId int,                    @calendarYear varchar(400),                    @createdBy varchar(400)                )                
                AS                
                BEGIN                    
                SET NOCOUNT ON                    
                    declare @DateNow datetime, @orgDataMasterid int                    
                        SET @DateNow = GETDATE()                    
                    declare @oldCalcRunOrgMasterId int                    
                        SET @oldCalcRunOrgMasterId = (select top 1 id from dbo.calculator_run_organization_data_master order by id desc)                    
                    Update calculator_run_organization_data_master SET effective_to = @DateNow 
                        WHERE id = @oldCalcRunOrgMasterId                    
                    INSERT into dbo.calculator_run_organization_data_master                    
                        (calendar_year, created_at, created_by, effective_from, effective_to)                    
                    values                    
                        (@calendarYear, @DateNow, @createdBy, @DateNow, NULL)                    
                    SET @orgDataMasterid  = CAST(scope_identity() AS int);                    
                    INSERT  into dbo.calculator_run_organization_data_detail                        
                        (calculator_run_organization_data_master_id,
                        load_ts,organisation_id,
                        organisation_name,
                        trading_name,                            
                        subsidiary_id,
                        obligation_status,
                        submitter_id,
                        status_code,
                        num_days_obligated,
                        error_code)                    
                    SELECT  @orgDataMasterid,                             
                    load_ts,                            
                    organisation_id,                            
                    organisation_name,                            
                    trading_name,                            
                    CASE WHEN LTRIM(RTRIM(subsidiary_id)) = '''' THEN NULL ELSE subsidiary_id END as subsidiary_id,
                    obligation_status,
                    submitter_id,
                    status_code,
                    num_days_obligated,
                    error_code
                    from                             
                        dbo.organisation_data                    
                    Update dbo.calculator_run Set calculator_run_organization_data_master_id = @orgDataMasterid where id = @RunId                
                    END'
                EXEC(@Sql)";

            migrationBuilder.Sql(createRunOrgSqlString);
            migrationBuilder.Sql(@"GRANT EXEC ON [dbo].[CreateRunOrganization] TO PUBLIC;");

            var createRunPomSqlString = @"IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreateRunPom]') AND type = N'P')
                DROP PROCEDURE [dbo].[CreateRunPom];
                    declare @Sql varchar(max);
                    SET @Sql = N'CREATE PROCEDURE [dbo].[CreateRunPom]
            (
                -- Add the parameters for the stored procedure here
                @RunId int,
                @calendarYear varchar(400),
                @createdBy varchar(400)
            )
            AS
            BEGIN
                -- SET NOCOUNT ON added to prevent extra result sets from
                -- interfering with SELECT statements.
                SET NOCOUNT ON

                declare @DateNow datetime, @pomDataMasterid int
                SET @DateNow = GETDATE()

                declare @oldCalcRunPomMasterId int
                SET @oldCalcRunPomMasterId = (select top 1 id from dbo.calculator_run_pom_data_master order by id desc)
                Update calculator_run_pom_data_master SET effective_to = @DateNow WHERE id = @oldCalcRunPomMasterId

                INSERT into dbo.calculator_run_pom_data_master
                (calendar_year, created_at, created_by, effective_from, effective_to)
                values
                (@calendarYear, @DateNow, @createdBy, @DateNow, NULL)

                SET @pomDataMasterid  = CAST(scope_identity() AS int);

                INSERT into 
                    dbo.calculator_run_pom_data_detail
                    (calculator_run_pom_data_master_id, 
                        load_ts,
                        organisation_id,
                        packaging_activity,
                        packaging_type,
                        packaging_class,
                        packaging_material,
                        packaging_material_weight,
                        submission_period,
                        submission_period_desc,
                        subsidiary_id,
                        submitter_id)
                SELECT  @pomDataMasterid,
                        load_ts,
                        organisation_id,
                        packaging_activity,
                        packaging_type,
                        packaging_class,
                        packaging_material,
                        packaging_material_weight,
                        submission_period,
                        submission_period_desc,
                        CASE            
                        WHEN LTRIM(RTRIM(subsidiary_id)) = ''''
                        THEN NULL
                        ELSE subsidiary_id
                        END         
                        as subsidiary_id,
                        submitter_id
                        from 
                        dbo.pom_data

                Update dbo.calculator_run Set calculator_run_pom_data_master_id = @pomDataMasterid where id = @RunId

            END'
            EXEC(@Sql)";
            migrationBuilder.Sql(createRunPomSqlString);
            migrationBuilder.Sql(@"GRANT EXEC ON [dbo].[CreateRunPom] TO PUBLIC;");

            var createInsertInvoiceSql = "IF OBJECT_ID(N'[dbo].[InsertInvoiceDetailsAtProducerLevel]', 'P') IS NOT NULL  \r\nDROP PROCEDURE [dbo].[InsertInvoiceDetailsAtProducerLevel];\r\nGO\r\nDECLARE @sql NVARCHAR(MAX) \r\nSET @sql = N'CREATE PROCEDURE [dbo].[InsertInvoiceDetailsAtProducerLevel]\r\n    (\r\n        @instructionConfirmedBy NVARCHAR(4000),\r\n        @instructionConfirmedDate DATETIME2(7),\r\n        @calculatorRunID INT\r\n    )\r\n    AS\r\n    BEGIN\r\n        SET NOCOUNT OFF\r\n        -- Temp table to hold calculated values\r\n        CREATE TABLE #CalculatedValues (\r\n            calculator_run_id INT,\r\n            producer_id INT,\r\n            total_producer_bill_with_bad_debt DECIMAL(18,2),\r\n            current_year_invoice_total_to_date DECIMAL(18,2),\r\n            amount_liability_difference_calc_vs_prev DECIMAL(18,2),\r\n            suggested_billing_instruction NVARCHAR(4000),\r\n            billing_instruction_accept_reject NVARCHAR(4000),\r\n            invoice_amount DECIMAL(18,2),\r\n            instruction_confirmed_by NVARCHAR(4000),\r\n            instruction_confirmed_date DATETIME2(7),\r\n            billing_instruction_id NVARCHAR(4000)\r\n        );\r\n\r\n        -- Insert into temp table\r\n        INSERT INTO #CalculatedValues\r\n        SELECT \r\n            prsbi.calculator_run_id,\r\n            prsbi.producer_id,\r\n            prsbi.total_producer_bill_with_bad_debt, \r\n            prsbi.current_year_invoice_total_to_date,\r\n            prsbi.amount_liability_difference_calc_vs_prev,\r\n            prsbi.suggested_billing_instruction,\r\n            prsbi.billing_instruction_accept_reject,\r\n            dbo.GetInvoiceAmount(\r\n                prsbi.billing_instruction_accept_reject,\r\n                prsbi.suggested_billing_instruction,\r\n                prsbi.total_producer_bill_with_bad_debt,\r\n                prsbi.amount_liability_difference_calc_vs_prev\r\n            ) AS invoice_amount,\r\n            @instructionConfirmedBy AS instruction_confirmed_by,\r\n            @instructionConfirmedDate AS instruction_confirmed_date,\r\n            CONCAT(prsbi.calculator_run_id, ''_'', prsbi.producer_id) AS billing_instruction_id\r\n        FROM dbo.producer_resultfile_suggested_billing_instruction AS prsbi\r\n        WHERE prsbi.calculator_run_id = @calculatorRunID\r\n\r\n        -- First SELECT using the temp table\r\n    \tINSERT INTO [dbo].[producer_designated_run_invoice_instruction] (\r\n    \t\tproducer_id,\r\n    \t\tcalculator_run_id,\r\n    \t\tcurrent_year_invoiced_total_after_this_run,\r\n    \t\tinvoice_amount,\r\n    \t\toutstanding_balance,\r\n    \t\tbilling_instruction_id,\r\n    \t\tinstruction_confirmed_date,\r\n    \t\tinstruction_confirmed_by\r\n    \t\t)\r\n    \t\t\tSELECT \r\n    \t\t\t\tcv.producer_id,\r\n    \t\t\t\tcv.calculator_run_id,\r\n    \t\t\t\tdbo.GetCurrentYearInvoicedTotalAfterThisRun(\r\n    \t\t\t\t\tcv.billing_instruction_accept_reject,\r\n    \t\t\t\t\tcv.suggested_billing_instruction,\r\n    \t\t\t\t\tcv.current_year_invoice_total_to_date,\r\n    \t\t\t\t\tcv.invoice_amount\r\n    \t\t\t\t) AS current_year_invoiced_total_after_this_run,\r\n    \t\t\t\tcv.invoice_amount,\r\n    \t\t\t\tdbo.GetOutstandingBalance(\r\n    \t\t\t\t\tcv.billing_instruction_accept_reject,\r\n    \t\t\t\t\tcv.suggested_billing_instruction,\r\n    \t\t\t\t\tcv.total_producer_bill_with_bad_debt,\r\n    \t\t\t\t\tcv.amount_liability_difference_calc_vs_prev\r\n    \t\t\t\t) AS outstanding_balance,\r\n    \t\t\t\tcv.billing_instruction_id,\r\n    \t\t\t\tcv.instruction_confirmed_date,\r\n    \t\t\t\tcv.instruction_confirmed_by\t\t\t\r\n    \t\t\tFROM #CalculatedValues AS cv;\r\n\r\n        DROP TABLE #CalculatedValues;\r\n    END'\r\nEXEC(@sql)";
            migrationBuilder.Sql(createInsertInvoiceSql);
            migrationBuilder.Sql(@"GRANT EXEC ON [dbo].[InsertInvoiceDetailsAtProducerLevel] TO PUBLIC;");

            var createCurrentInvoiceYTDTotalSql = @"
                IF OBJECT_ID(N'[dbo].[GetCurrentYearInvoicedTotalAfterThisRun]', 'FN') IS NOT NULL
                    DROP FUNCTION [dbo].GetCurrentYearInvoicedTotalAfterThisRun
                GO
                DECLARE @sql NVARCHAR(MAX)
                SET @sql = N'CREATE FUNCTION [dbo].[GetCurrentYearInvoicedTotalAfterThisRun] (
                    @billingInstructionAcceptReject      VARCHAR(250),
                    @suggestedBillingInstruction         VARCHAR(250),
                    @currentYearInvoicedTotalToDate      DECIMAL(18,2),
                    @invoiceAmount                       DECIMAL(18,2)
                )
                RETURNS DECIMAL(18,2)
                AS
                BEGIN
                    -- Rule 1: Cancelled and Rejected instruction always returns last invoiced values
                    IF @suggestedBillingInstruction = ''CANCEL'' AND @billingInstructionAcceptReject = ''Rejected''
                         RETURN ISNULL(@currentYearInvoicedTotalToDate, 0);

                    -- Rule 2: Cancelled instruction always returns NULL
                    IF @suggestedBillingInstruction = ''CANCEL'' AND @billingInstructionAcceptReject = ''Accepted''
                        RETURN NULL;

                    -- Rule 3: Rejected INITIAL returns NULL
                    IF @billingInstructionAcceptReject = ''Rejected'' AND @suggestedBillingInstruction = ''INITIAL''
                        RETURN NULL;

                    -- Rule 4: Rejected (but not INITIAL) returns current total as-is
                    IF @billingInstructionAcceptReject = ''Rejected''
                        RETURN ISNULL(@currentYearInvoicedTotalToDate, 0);

                    -- Rule 5: Rebill replaces total with invoice amount
                    IF @suggestedBillingInstruction = ''REBILL''
                        RETURN ISNULL(@invoiceAmount, 0);

                    -- Rule 6: Accepted or any other case adds invoice amount
                    RETURN ISNULL(@currentYearInvoicedTotalToDate, 0) + ISNULL(@invoiceAmount, 0);
                END'
                EXEC(@sql)";

            migrationBuilder.Sql(createCurrentInvoiceYTDTotalSql);

            var createInvoiceAmountFunctionSql = "IF OBJECT_ID(N'[dbo].[GetInvoiceAmount]', 'FN') IS NOT NULL\r\n    DROP FUNCTION [dbo].[GetInvoiceAmount]\r\nGO\r\ndeclare @sql nvarchar(max)\r\nSET @sql = N'\r\nCREATE FUNCTION [dbo].[GetInvoiceAmount] ( \r\n    @billingInstructionAcceptReject VARCHAR(250),\r\n    @suggestedBillingInstruction    VARCHAR(250),\r\n    @totalProducerBillWithBadDebtProvision DECIMAL(18,2),\r\n    @LiabilityDifference           DECIMAL(18,2)\r\n)\r\nRETURNS DECIMAL(18,2)\r\nAS\r\nBEGIN\r\n    IF @billingInstructionAcceptReject <> ''Accepted''\r\n        RETURN NULL;\r\n\r\n    RETURN \r\n        CASE \r\n            WHEN @suggestedBillingInstruction IN (''INITIAL'', ''REBILL'') THEN @totalProducerBillWithBadDebtProvision\r\n            WHEN @suggestedBillingInstruction = ''DELTA'' THEN @LiabilityDifference\r\n            ELSE NULL\r\n        END;\r\nEND'\r\n\r\nEXEC(@sql)";
            migrationBuilder.Sql(createInvoiceAmountFunctionSql);

            var createOutstandingBalanceFunctionSql = "IF OBJECT_ID(N'[dbo].[GetOutstandingBalance]', 'FN') IS NOT NULL  \r\nDROP FUNCTION [dbo].GetOutstandingBalance\r\nGO\r\nDECLARE @sql NVARCHAR(MAX) \r\nSET @sql = N'CREATE FUNCTION [dbo].[GetOutstandingBalance] (\r\n    @billingInstructionAcceptReject        VARCHAR(250),\r\n    @suggestedBillingInstruction           VARCHAR(250),\r\n    @totalProducerBillWithBadDebtProvision DECIMAL(18,2),\r\n    @LiabilityDifference                   DECIMAL(18,2)\r\n)\r\nRETURNS DECIMAL(18,2)\r\nAS\r\nBEGIN\r\n    RETURN \r\n        CASE \r\n            WHEN @billingInstructionAcceptReject <> ''Accepted'' AND @suggestedBillingInstruction = ''INITIAL''\r\n                THEN @totalProducerBillWithBadDebtProvision\r\n\r\n            WHEN @billingInstructionAcceptReject <> ''Accepted''\r\n                THEN @LiabilityDifference\r\n\r\n            ELSE NULL\r\n        END;\r\nEND'\r\nEXEC(@sql)";
            migrationBuilder.Sql(createOutstandingBalanceFunctionSql);

            migrationBuilder.Sql(@"GRANT EXEC ON [dbo].[GetCurrentYearInvoicedTotalAfterThisRun] TO PUBLIC;");
            migrationBuilder.Sql(@"GRANT EXEC ON [dbo].[GetInvoiceAmount] TO PUBLIC;");
            migrationBuilder.Sql(@"GRANT EXEC ON [dbo].[GetOutstandingBalance] TO PUBLIC;");

        }
    }
}