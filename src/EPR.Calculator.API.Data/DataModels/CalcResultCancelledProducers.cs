namespace EPR.Calculator.API.Data.DataModels;


public class CalcResultCancelledProducerEntry
{
    public int Id { get; set; }
    public required int CalculatorRunId { get; set; }
    public required CalcResultCancelledProducer CancelledProducer { get; set; }
}

public class CalcResultCancelledProducer
{
    public int ProducerId { get; set; }
    public string? SubsidiaryId { get; set; }
    public string? ProducerOrSubsidiaryName { get; set; }
    public string? TradingName { get; set; }
    public LastTonnage? LastTonnage { get; set; }
    public LatestInvoice? LatestInvoice { get; set; }
}

public class LastTonnage
{
    public decimal? Aluminium { get; set; }
    public decimal? FibreComposite { get; set; }
    public decimal? Glass { get; set; }
    public decimal? PaperOrCard { get; set; }
    public decimal? Plastic { get; set; }
    public decimal? Steel { get; set; }
    public decimal? Wood { get; set; }
    public decimal? OtherMaterials { get; set; }
}

public class LatestInvoice
{
    public decimal? CurrentYearInvoicedTotalToDate { get; set; }
    public string? RunNumber { get; set; }
    public string? RunName { get; set; }
    public string? BillingInstructionId { get; set; }
}
