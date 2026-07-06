namespace EPR.Calculator.API.Data.DataModels;

public class FeeWithBadDebt
{
    public decimal FeeWithoutBadDebt { get; set; }

    public decimal BadDebt { get; set; }

    public required ByCountryCost ByCountry { get; set; }

    public static FeeWithBadDebt Empty => new()
    {
        FeeWithoutBadDebt = 0,
        BadDebt           = 0,
        ByCountry    = ByCountryCost.Empty
    };

    public static FeeWithBadDebt operator +(FeeWithBadDebt a, FeeWithBadDebt b) =>
        new()
        {
            FeeWithoutBadDebt = a.FeeWithoutBadDebt + b.FeeWithoutBadDebt,
            BadDebt           = a.BadDebt           + b.BadDebt,
            ByCountry      = a.ByCountry      + b.ByCountry,
        };
}

public static class CalcResultSummaryBadDebtProvisionExtensions
{
    public static FeeWithBadDebt Sum(this IEnumerable<FeeWithBadDebt> source) =>
        source.Aggregate(FeeWithBadDebt.Empty, (acc, r) => acc + r);
}
