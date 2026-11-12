namespace EPR.Calculator.API.Data.DataModels;

public class CalcResultOnePlusFourApportionmentEntry
{
    public int Id { get; set; }
    public required int CalculatorRunId { get; set; }
    public required CalcResultOnePlusFourApportionment OnePlusFourApportionment { get; set; }

}

public class CalcResultOnePlusFourApportionment
{
    public required ByCountryCost LaDisposalCost { get; init; }
    public required ByCountryCost LADataPrepCharge { get; init; }

    public required ByCountryCost TotalOnePlusFour { get; init; }
    public required ByCountryApportionment OnePlusFourApportionment { get; init; }

}
