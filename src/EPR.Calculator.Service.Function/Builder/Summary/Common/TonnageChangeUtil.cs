using System.Globalization;
using EPR.Calculator.API.Data.DataModels;
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

        public static (string? Count, string? Advice) ComputeCountAndAdvice(
            string level,
            IDictionary<string, DisposalFee> byMaterial)
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
