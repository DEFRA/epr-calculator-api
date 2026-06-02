namespace EPR.Calculator.API.Data.DataModels;

public class CalculatorRunBillingFileMetadata
{
    public int Id { get; set; }
    public int CalculatorRunId { get; set; }
    public string? BillingCsvFileName { get; set; }
    public string? BillingJsonFileName { get; set; }
    public required DateTime BillingFileCreatedDate { get; set; } = DateTime.UtcNow;
    public required string BillingFileCreatedBy { get; set; }
    public DateTime? BillingFileAuthorisedDate { get; set; }
    public string? BillingFileAuthorisedBy { get; set; }

    #region EF navigational properties

    public virtual CalculatorRun CalculatorRun { get; set; } = null!;

    #endregion
}
