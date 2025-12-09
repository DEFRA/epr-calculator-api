namespace EPR.Calculator.API.Data.DataModels;

public class ErrorReport
{
    public int Id { get; set; }

    public int ProducerId { get; set; }

    public string? SubsidiaryId { get; set; }

    public int CalculatorRunId { get; set; }

    public string? LeaverCode { get; set; }

    public required string ErrorCode { get; set; }

    public DateTime CreatedAt { get; set; }

    public required string CreatedBy { get; set; }

    public virtual CalculatorRun? CalculatorRun { get; set; }
}
