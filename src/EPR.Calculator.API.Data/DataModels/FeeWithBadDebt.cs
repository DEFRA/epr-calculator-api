namespace EPR.Calculator.API.Data.DataModels;

public record FeeWithBadDebt
{
    public decimal FeeWithoutBadDebt { get; init; }
    public decimal BadDebt { get; init; }
    public required ByCountryCost ByCountry { get; init; }

    public static FeeWithBadDebt Empty => new()
    {
        FeeWithoutBadDebt = 0,
        BadDebt           = 0,
        ByCountry         = ByCountryCost.Empty
    };

    public static FeeWithBadDebt operator +(FeeWithBadDebt a, FeeWithBadDebt b) =>
        new()
        {
            FeeWithoutBadDebt = a.FeeWithoutBadDebt + b.FeeWithoutBadDebt,
            BadDebt           = a.BadDebt           + b.BadDebt,
            ByCountry         = a.ByCountry         + b.ByCountry
        };
}

public static class CalcResultSummaryBadDebtProvisionExtensions
{
    public static FeeWithBadDebt Sum(this IEnumerable<FeeWithBadDebt> source) =>
        source.Aggregate(FeeWithBadDebt.Empty, (acc, r) => acc + r);
}
