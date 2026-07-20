namespace EPR.Calculator.API.Data.DataModels;

public class ProducerFees
{
    public int Id { get; set; }

    public required int CalculatorRunId { get; set; }

    public required FeeDetail Total { get; set; } = new FeeDetail { ProducerId = 0, SubsidiaryId = string.Empty, ProducerName = string.Empty };

    #region EF navigational properties
    public virtual ICollection<ProducerFeeDetail> Details { get; set; } = new List<ProducerFeeDetail>();

    #endregion
}
