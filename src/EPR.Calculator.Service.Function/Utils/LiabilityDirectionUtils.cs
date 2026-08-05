using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Constants;

namespace EPR.Calculator.Service.Function.Utils;

public static class LiabilityDirectionUtils
{
    /// <summary>
    /// Converts a threshold breach direction to the "+ve"/"-ve"/"-" format used in exports and reports.
    /// </summary>
    public static string ToThresholdBreachedString(this LiabilityDirection? direction) =>
        direction switch
        {
            LiabilityDirection.Positive => CommonConstants.Positive,
            LiabilityDirection.Negative => CommonConstants.Negative,
            _ => CommonConstants.Hyphen
        };
}
