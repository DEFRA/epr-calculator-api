using System.Globalization;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.Summary.Common
{
    internal static class TonnageChangeUtil
    {
        public static decimal? ComputePerMaterialChange(
            string level,
            decimal? netReportedTonnage,
            decimal? previousInvoicedTonnage)
        {
            if (level != CommonConstants.LevelOne.ToString() || netReportedTonnage is null)
                return null;

            if (!previousInvoicedTonnage.HasValue)
                return null;

            if (previousInvoicedTonnage.Value == 0m)
                return 0m;

            return netReportedTonnage - previousInvoicedTonnage.Value;
        }

        public static decimal? GetOverallChangeTotal(
            IReadOnlyList<CalcResultSummaryProducerDisposalFees> producerDisposalFees,
            string materialCode)
        {
            if (producerDisposalFees == null || string.IsNullOrWhiteSpace(materialCode))
            {
                return 0m;
            }

            var level1 = producerDisposalFees
                .Where(r => r != null && r.Level == CommonConstants.LevelOne.ToString());

            decimal total = 0m;

            foreach (var row in level1.Select(r => r.ProducerDisposalFeesByMaterial))
            {
                if (row != null &&
                    row.TryGetValue(materialCode, out var mat) &&
                    mat?.TonnageChange != null)
                {
                    total += mat.TonnageChange.Value;
                }
                // else: contribute 0 to the sum
            }

            return total;
        }

        public static (string? Count, string? Advice) ComputeCountAndAdvice(
            string level,
            IDictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial> byMaterial)
        {
            if (level != CommonConstants.LevelOne.ToString())
            {
                return (null, null);
            }

            int count = byMaterial?.Values
                .Count(m => m?.TonnageChange.HasValue == true && m.TonnageChange.Value != 0m) ?? 0;

            return (
                count.ToString(CultureInfo.InvariantCulture),
                count > 0 ? "CHANGE" : string.Empty
            );
        }
    }
}
