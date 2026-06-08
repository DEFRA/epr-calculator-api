namespace EPR.Calculator.API.Data.DataModels;

public class CountryApportionment
{
    public int Id { get; set; }
    public int CalculatorRunId { get; set; }
    public decimal Apportionment { get; set; }
    public int CountryId { get; set; }
    public int CostTypeId { get; set; }

    #region EF navigational properties

    public virtual Country Country { get; set; } = null!;
    public virtual CostType CostType { get; set; } = null!;
    public virtual CalculatorRun CalculatorRun { get; set; } = null!;

    #endregion
}
