namespace EPR.Calculator.API.Data.DataModels;

public class ProducerFees
{
    public int Id { get; set; }

    public required int CalculatorRunId { get; set; }

    public IEnumerable<ProducerFeeDetail> Details { get; set; } = new List<ProducerFeeDetail>();

    public required ProducerFeeDetail Total { get; set; } = new ProducerFeeDetail { ProducerId = 0, SubsidiaryId = string.Empty, ProducerName = string.Empty };
}
