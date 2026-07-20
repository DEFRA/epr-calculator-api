namespace EPR.Calculator.API.Data.DataModels;

public class ProducerDetail
{
    public int Id { get; set; }
    public int ProducerId { get; set; }
    public int CalculatorRunId { get; set; }
    public string? TradingName { get; set; }
    public string? SubsidiaryId { get; set; }
    public string? ProducerName { get; set; }


    #region EF navigational properties

    public virtual ICollection<ProducerReportedMaterial> ProducerReportedMaterials { get; } = [];
    public virtual ICollection<ProducerMaterialPackaging> ProducerMaterialPackaging { get; } = [];
    public virtual CalculatorRun CalculatorRun { get; set; } = null!;

    #endregion
}
