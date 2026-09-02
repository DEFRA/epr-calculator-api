using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.API.BackgroundService.Utils;

[ExcludeFromCodeCoverage]
public static class DefaultParameterExtensions
{
    public static decimal ToDecimal(this string value)
    {
        if (decimal.TryParse(value, out var result))
        {
            return result;
        }

        throw new FormatException($"'{value}' is not a valid decimal.");
    }
}
