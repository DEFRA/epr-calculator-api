BEGIN TRANSACTION;
GO


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

                    -- Rule 5: Accepted or any other case adds invoice amount
                    RETURN ISNULL(@currentYearInvoicedTotalToDate, 0) + ISNULL(@invoiceAmount, 0);
                END'
                EXEC(@sql)
GO

GRANT EXEC ON [dbo].[GetCurrentYearInvoicedTotalAfterThisRun] TO PUBLIC;
GO

DELETE FROM [__EFMigrationsHistory]
WHERE [MigrationId] = N'20260218155412_UpdateGetCurrentYearInvoicedTotalAfterThisRun';
GO

COMMIT;
GO
