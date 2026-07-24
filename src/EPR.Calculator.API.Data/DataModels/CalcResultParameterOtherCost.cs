namespace EPR.Calculator.API.Data.DataModels;

public class CalcResultParameterOtherCostEntry
{
    public int Id { get; set; }
    public required int CalculatorRunId { get; set; }
    public required CalcResultParameterOtherCost ParameterOtherCost { get; set; }
}

public class CalcResultParameterOtherCost
{
    public ByCountryCost SaOperatingCost { get; set; } = ByCountryCost.Empty;
    public ByCountryCost LaDataPrepCharge { get; set; } = ByCountryCost.Empty;
    public ByCountryApportionment CountryApportionment { get; set; } = ByCountryApportionment.Empty;
    public ByCountryCost SchemeSetupCost { get; set; } = ByCountryCost.Empty;
    public Materiality MaterialityIncrease { get; set; } = new Materiality { Amount = 0, Percentage = 0 };
    public Materiality MaterialityDecrease { get; set; } = new Materiality { Amount = 0, Percentage = 0 };
    public Materiality TonnageChangeIncrease { get; set; } = new Materiality { Amount = 0, Percentage = 0 };
    public Materiality TonnageChangeDecrease { get; set; } = new Materiality { Amount = 0, Percentage = 0 };
    public decimal BadDebtValue { get; set; }
    public DateTime? CutOffDate { get; set; }
}

public record Materiality
{
    public required decimal Amount  { get; init; }
    public required decimal Percentage { get; init; }
}
