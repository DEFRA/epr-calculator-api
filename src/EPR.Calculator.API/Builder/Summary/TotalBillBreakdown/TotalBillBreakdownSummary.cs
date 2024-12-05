using EPR.Calculator.API.Builder.Summary.SaSetupCosts;
using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder.Summary.TotalProducerBillBreakdown
{
    public static class TotalBillBreakdownSummary
    {
        public static readonly int ColumnIndex = 231;

        public static IEnumerable<CalcResultSummaryHeader> GetHeaders()
        {
            return [
                new CalcResultSummaryHeader { Name = $"{TotalBillBreakdownHeaders.TotalProducerBillBreakdown}", ColumnIndex = ColumnIndex }
            ];
        }
    }
}