using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Services;

public interface IInvoiceDetailsService
{
    Task<int> InsertInvoiceDetailsAtProducerLevel(int runId, DateTime instructionConfirmedDate, string instructionConfirmedBy, CancellationToken cancellationToken);
}

public class InvoiceDetailsService (ApplicationDBContext context)
    : IInvoiceDetailsService
{
    public async Task<int> InsertInvoiceDetailsAtProducerLevel(int runId, DateTime instructionConfirmedDate, string instructionConfirmedBy, CancellationToken cancellationToken)
    {
        var sourceRows = await context.ProducerResultFileSuggestedBillingInstruction
            .Where(x => x.CalculatorRunId == runId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var entitiesToInsert = sourceRows.Select(row =>
        {
            var invoiceAmount = GetInvoiceAmount(row.BillingInstructionAcceptReject, row.SuggestedBillingInstruction, row.TotalProducerBillWithBadDebt, row.AmountLiabilityDifferenceCalcVsPrev);
            var currentYearInvoicedTotalAfterRun = GetCurrentYearInvoicedTotalAfterThisRun(row.BillingInstructionAcceptReject, row.SuggestedBillingInstruction, row.CurrentYearInvoiceTotalToDate, invoiceAmount);
            var outstandingBalance = GetOutstandingBalance(row.BillingInstructionAcceptReject, row.SuggestedBillingInstruction, row.TotalProducerBillWithBadDebt, row.AmountLiabilityDifferenceCalcVsPrev);

            return new ProducerDesignatedRunInvoiceInstruction
            {
                ProducerId = row.ProducerId,
                CalculatorRunId = row.CalculatorRunId,
                CurrentYearInvoicedTotalAfterThisRun = currentYearInvoicedTotalAfterRun,
                InvoiceAmount = invoiceAmount,
                OutstandingBalance = outstandingBalance,
                BillingInstructionId = $"{row.CalculatorRunId}_{row.ProducerId}",
                InstructionConfirmedBy = instructionConfirmedBy,
                InstructionConfirmedDate = instructionConfirmedDate
            };
        }).ToList();

        await context.ProducerDesignatedRunInvoiceInstruction.AddRangeAsync(entitiesToInsert, cancellationToken);

        return await context.SaveChangesAsync(cancellationToken);
    }

    public static decimal? GetCurrentYearInvoicedTotalAfterThisRun(
        string? billingInstructionAcceptReject,
        string suggestedBillingInstruction,
        decimal? currentYearInvoicedTotalToDate,
        decimal? invoiceAmount)
    {
        var currentTotalYTD = currentYearInvoicedTotalToDate ?? 0m;
        var invoiceAmt = invoiceAmount ?? 0m;

        return (suggestedBillingInstruction, billingInstructionAcceptReject) switch
        {
            ("CANCEL", "Rejected") => currentTotalYTD, // Rule 1: Cancelled and Rejected instruction always returns last invoiced values
            ("CANCEL", "Accepted") => null, // Rule 2: Cancelled instruction always returns NULL
            ("INITIAL", "Rejected") => null, // Rule 3: Rejected INITIAL returns NULL
            (_, "Rejected") => currentTotalYTD, // Rule 4: Rejected (but not INITIAL) returns current total as-is
            ("REBILL", _) => invoiceAmt, // Rule 5: Rebill replaces total with invoice amount
            _ => currentTotalYTD + invoiceAmt // Rule 6: Accepted or any other case adds invoice amount
        };
    }

    public static decimal? GetInvoiceAmount(
        string? billingInstructionAcceptReject,
        string suggestedBillingInstruction,
        decimal? totalProducerBillWithBadDebtProvision,
        decimal? liabilityDifference)
    {
        if (billingInstructionAcceptReject != "Accepted")
            return null;

        return suggestedBillingInstruction switch
        {
            "INITIAL" or "REBILL" => totalProducerBillWithBadDebtProvision,
            "DELTA" => liabilityDifference,
            _ => null
        };
    }

    public static decimal? GetOutstandingBalance(
        string? billingInstructionAcceptReject,
        string suggestedBillingInstruction,
        decimal? totalProducerBillWithBadDebtProvision,
        decimal? liabilityDifference)
    {
        if (billingInstructionAcceptReject is null or "Accepted")
            return null;

        return suggestedBillingInstruction == "INITIAL"
            ? totalProducerBillWithBadDebtProvision
            : liabilityDifference;
    }
}
