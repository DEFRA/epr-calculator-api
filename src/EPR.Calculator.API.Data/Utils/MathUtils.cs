namespace EPR.Calculator.API.Data.Utils;

public static class MathUtils
{
    public static decimal RoundAwayFromZero(decimal value, int decimals = 0) =>
        Math.Round(value, decimals, MidpointRounding.AwayFromZero);
}