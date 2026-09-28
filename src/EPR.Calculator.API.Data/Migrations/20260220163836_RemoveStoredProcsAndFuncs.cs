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
            // CREATE PROCEDURE/FUNCTION can't sit inside the IF block that idempotent scripts wrap each statement in,
            // hence EXEC. Pass the SQL to EXEC directly rather than through a declared variable: EF Core 9+ scripts emit
            // a whole migration as one batch (a GO inside Sql() no longer separates batches), so Sql() calls that each
            // DECLARE @sql fail with "The variable name '@sql' has already been declared".
            migrationBuilder.Sql(
                """
                IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreateRunOrganization]') AND type = N'P')
                    DROP PROCEDURE [dbo].[CreateRunOrganization];
                EXEC(N'CREATE PROCEDURE [dbo].[CreateRunOrganization]
                (
                    @RunId int,
                    @calendarYear varchar(400),
                    @createdBy varchar(400)
                )
                AS
                BEGIN
                    SET NOCOUNT ON

                    declare @DateNow datetime, @orgDataMasterid int
                    SET @DateNow = GETDATE()

                    declare @oldCalcRunOrgMasterId int
                    SET @oldCalcRunOrgMasterId = (select top 1 id from dbo.calculator_run_organization_data_master order by id desc)
                    Update calculator_run_organization_data_master SET effective_to = @DateNow WHERE id = @oldCalcRunOrgMasterId

                    INSERT into dbo.calculator_run_organization_data_master
                        (calendar_year, created_at, created_by, effective_from, effective_to)
                    values
                        (@calendarYear, @DateNow, @createdBy, @DateNow, NULL)

                    SET @orgDataMasterid = CAST(scope_identity() AS int);

                    INSERT into dbo.calculator_run_organization_data_detail
                        (calculator_run_organization_data_master_id,
                        load_ts,
                        organisation_id,
                        organisation_name,
                        trading_name,
                        subsidiary_id,
                        obligation_status,
                        submitter_id,
                        status_code,
                        num_days_obligated,
                        error_code)
                    SELECT @orgDataMasterid,
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
                    from dbo.organisation_data

                    Update dbo.calculator_run Set calculator_run_organization_data_master_id = @orgDataMasterid where id = @RunId
                END')
                """);
            migrationBuilder.Sql(@"GRANT EXEC ON [dbo].[CreateRunOrganization] TO PUBLIC;");

            migrationBuilder.Sql(
                """
                IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreateRunPom]') AND type = N'P')
                    DROP PROCEDURE [dbo].[CreateRunPom];
                EXEC(N'CREATE PROCEDURE [dbo].[CreateRunPom]
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

                    SET @pomDataMasterid = CAST(scope_identity() AS int);

                    INSERT into dbo.calculator_run_pom_data_detail
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
                    SELECT @pomDataMasterid,
                        load_ts,
                        organisation_id,
                        packaging_activity,
                        packaging_type,
                        packaging_class,
                        packaging_material,
                        packaging_material_weight,
                        submission_period,
                        submission_period_desc,
                        CASE WHEN LTRIM(RTRIM(subsidiary_id)) = '''' THEN NULL ELSE subsidiary_id END as subsidiary_id,
                        submitter_id
                    from dbo.pom_data

                    Update dbo.calculator_run Set calculator_run_pom_data_master_id = @pomDataMasterid where id = @RunId
                END')
                """);
            migrationBuilder.Sql(@"GRANT EXEC ON [dbo].[CreateRunPom] TO PUBLIC;");

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[dbo].[InsertInvoiceDetailsAtProducerLevel]', 'P') IS NOT NULL
                    DROP PROCEDURE [dbo].[InsertInvoiceDetailsAtProducerLevel];
                EXEC(N'CREATE PROCEDURE [dbo].[InsertInvoiceDetailsAtProducerLevel]
                (
                    @instructionConfirmedBy NVARCHAR(4000),
                    @instructionConfirmedDate DATETIME2(7),
                    @calculatorRunID INT
                )
                AS
                BEGIN
                    SET NOCOUNT OFF
                    -- Temp table to hold calculated values
                    CREATE TABLE #CalculatedValues (
                        calculator_run_id INT,
                        producer_id INT,
                        total_producer_bill_with_bad_debt DECIMAL(18,2),
                        current_year_invoice_total_to_date DECIMAL(18,2),
                        amount_liability_difference_calc_vs_prev DECIMAL(18,2),
                        suggested_billing_instruction NVARCHAR(4000),
                        billing_instruction_accept_reject NVARCHAR(4000),
                        invoice_amount DECIMAL(18,2),
                        instruction_confirmed_by NVARCHAR(4000),
                        instruction_confirmed_date DATETIME2(7),
                        billing_instruction_id NVARCHAR(4000)
                    );

                    -- Insert into temp table
                    INSERT INTO #CalculatedValues
                    SELECT
                        prsbi.calculator_run_id,
                        prsbi.producer_id,
                        prsbi.total_producer_bill_with_bad_debt,
                        prsbi.current_year_invoice_total_to_date,
                        prsbi.amount_liability_difference_calc_vs_prev,
                        prsbi.suggested_billing_instruction,
                        prsbi.billing_instruction_accept_reject,
                        dbo.GetInvoiceAmount(
                            prsbi.billing_instruction_accept_reject,
                            prsbi.suggested_billing_instruction,
                            prsbi.total_producer_bill_with_bad_debt,
                            prsbi.amount_liability_difference_calc_vs_prev
                        ) AS invoice_amount,
                        @instructionConfirmedBy AS instruction_confirmed_by,
                        @instructionConfirmedDate AS instruction_confirmed_date,
                        CONCAT(prsbi.calculator_run_id, ''_'', prsbi.producer_id) AS billing_instruction_id
                    FROM dbo.producer_resultfile_suggested_billing_instruction AS prsbi
                    WHERE prsbi.calculator_run_id = @calculatorRunID

                    -- First SELECT using the temp table
                    INSERT INTO [dbo].[producer_designated_run_invoice_instruction] (
                        producer_id,
                        calculator_run_id,
                        current_year_invoiced_total_after_this_run,
                        invoice_amount,
                        outstanding_balance,
                        billing_instruction_id,
                        instruction_confirmed_date,
                        instruction_confirmed_by
                    )
                    SELECT
                        cv.producer_id,
                        cv.calculator_run_id,
                        dbo.GetCurrentYearInvoicedTotalAfterThisRun(
                            cv.billing_instruction_accept_reject,
                            cv.suggested_billing_instruction,
                            cv.current_year_invoice_total_to_date,
                            cv.invoice_amount
                        ) AS current_year_invoiced_total_after_this_run,
                        cv.invoice_amount,
                        dbo.GetOutstandingBalance(
                            cv.billing_instruction_accept_reject,
                            cv.suggested_billing_instruction,
                            cv.total_producer_bill_with_bad_debt,
                            cv.amount_liability_difference_calc_vs_prev
                        ) AS outstanding_balance,
                        cv.billing_instruction_id,
                        cv.instruction_confirmed_date,
                        cv.instruction_confirmed_by
                    FROM #CalculatedValues AS cv;

                    DROP TABLE #CalculatedValues;
                END')
                """);
            migrationBuilder.Sql(@"GRANT EXEC ON [dbo].[InsertInvoiceDetailsAtProducerLevel] TO PUBLIC;");

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[dbo].[GetCurrentYearInvoicedTotalAfterThisRun]', 'FN') IS NOT NULL
                    DROP FUNCTION [dbo].GetCurrentYearInvoicedTotalAfterThisRun;
                EXEC(N'CREATE FUNCTION [dbo].[GetCurrentYearInvoicedTotalAfterThisRun] (
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
                END')
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[dbo].[GetInvoiceAmount]', 'FN') IS NOT NULL
                    DROP FUNCTION [dbo].[GetInvoiceAmount];
                EXEC(N'CREATE FUNCTION [dbo].[GetInvoiceAmount] (
                    @billingInstructionAcceptReject VARCHAR(250),
                    @suggestedBillingInstruction    VARCHAR(250),
                    @totalProducerBillWithBadDebtProvision DECIMAL(18,2),
                    @LiabilityDifference           DECIMAL(18,2)
                )
                RETURNS DECIMAL(18,2)
                AS
                BEGIN
                    IF @billingInstructionAcceptReject <> ''Accepted''
                        RETURN NULL;

                    RETURN
                        CASE
                            WHEN @suggestedBillingInstruction IN (''INITIAL'', ''REBILL'') THEN @totalProducerBillWithBadDebtProvision
                            WHEN @suggestedBillingInstruction = ''DELTA'' THEN @LiabilityDifference
                            ELSE NULL
                        END;
                END')
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'[dbo].[GetOutstandingBalance]', 'FN') IS NOT NULL
                    DROP FUNCTION [dbo].GetOutstandingBalance;
                EXEC(N'CREATE FUNCTION [dbo].[GetOutstandingBalance] (
                    @billingInstructionAcceptReject        VARCHAR(250),
                    @suggestedBillingInstruction           VARCHAR(250),
                    @totalProducerBillWithBadDebtProvision DECIMAL(18,2),
                    @LiabilityDifference                   DECIMAL(18,2)
                )
                RETURNS DECIMAL(18,2)
                AS
                BEGIN
                    RETURN
                        CASE
                            WHEN @billingInstructionAcceptReject <> ''Accepted'' AND @suggestedBillingInstruction = ''INITIAL''
                                THEN @totalProducerBillWithBadDebtProvision

                            WHEN @billingInstructionAcceptReject <> ''Accepted''
                                THEN @LiabilityDifference

                            ELSE NULL
                        END;
                END')
                """);

            migrationBuilder.Sql(@"GRANT EXEC ON [dbo].[GetCurrentYearInvoicedTotalAfterThisRun] TO PUBLIC;");
            migrationBuilder.Sql(@"GRANT EXEC ON [dbo].[GetInvoiceAmount] TO PUBLIC;");
            migrationBuilder.Sql(@"GRANT EXEC ON [dbo].[GetOutstandingBalance] TO PUBLIC;");
        }
    }
}
