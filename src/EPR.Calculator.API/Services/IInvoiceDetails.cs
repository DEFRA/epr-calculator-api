namespace EPR.Calculator.Service.Function.Services {
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using Microsoft.EntityFrameworkCore;

    public interface IInvoiceDetails
    {
        Task<int> InsertInvoiceDetailsAtProducerLevel(int runId, DateTime instructionConfirmedDate, string instructionConfirmedBy, CancellationToken cancellationToken);
    }

    public class InvoiceDetails : IInvoiceDetails
    {
        private readonly ApplicationDBContext _context;

        public InvoiceDetails(ApplicationDBContext context)
        {
            _context = context;
        }

        public static decimal? GetCurrentYearInvoicedTotalAfterThisRun(
            string? billingInstructionAcceptReject,
            string suggestedBillingInstruction,
            decimal? currentYearInvoicedTotalToDate,
            decimal? invoiceAmount)
        {
            var currentTotalYTD = currentYearInvoicedTotalToDate ?? 0m;

            return (suggestedBillingInstruction, billingInstructionAcceptReject) switch
            {
                ("CANCEL", "Rejected") => currentTotalYTD, // Rule 1: Cancelled and Rejected instruction always returns last invoiced values
                ("CANCEL", "Accepted") => null, // Rule 2: Cancelled instruction always returns NULL
                ("INITIAL", "Rejected") => null, // Rule 3: Rejected INITIAL returns NULL
                (_, "Rejected") => currentTotalYTD, // Rule 4: Rejected (but not INITIAL) returns current total as-is
                _ => currentTotalYTD + (invoiceAmount ?? 0m) // Rule 5: Accepted or any other case adds invoice amount
            };
        }

        public static decimal? GetInvoiceAmount(
            string? billingInstructionAcceptReject,
            string suggestedBillingInstruction,
            decimal? totalProducerBillWithBadDebtProvision,
            decimal? liabilityDifference)
        {
            if (billingInstructionAcceptReject != "Accepted")
            {
                return null;
            }

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
            return (billingInstructionAcceptReject, suggestedBillingInstruction) switch
            {
                (not "Accepted", "INITIAL") => totalProducerBillWithBadDebtProvision,
                (not "Accepted", _) => liabilityDifference,
                _ => null
            };
        }

        public async Task<int> InsertInvoiceDetailsAtProducerLevel(int runId, DateTime instructionConfirmedDate, string instructionConfirmedBy, CancellationToken cancellationToken)
        {
            var sourceRows = await _context.ProducerResultFileSuggestedBillingInstruction
                                    .Where(x => x.CalculatorRunId == runId)
                                    .AsNoTracking()
                                    .ToListAsync();

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

            await _context.ProducerDesignatedRunInvoiceInstruction.AddRangeAsync(entitiesToInsert);

            return await _context.SaveChangesAsync();
        }
    }
}