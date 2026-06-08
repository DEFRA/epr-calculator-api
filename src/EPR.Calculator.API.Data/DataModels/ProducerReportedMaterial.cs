namespace EPR.Calculator.API.Data.DataModels;

public class ProducerReportedMaterial
{
    public int Id { get; set; }
    public int MaterialId { get; set; }
    public int ProducerDetailId { get; set; }
    public required string PackagingType { get; set; }
    public decimal PackagingTonnage { get; set; }
    public decimal? PackagingTonnageRed { get; set; }
    public decimal? PackagingTonnageAmber { get; set; }
    public decimal? PackagingTonnageGreen { get; set; }
    public decimal? PackagingTonnageRedMedical { get; set; }
    public decimal? PackagingTonnageAmberMedical { get; set; }
    public decimal? PackagingTonnageGreenMedical { get; set; }
    public required string SubmissionPeriod { get; set; }

    #region EF navigational properties

    public virtual ProducerDetail ProducerDetail { get; set; } = null!;
    public virtual Material Material { get; set; } = null!;

    #endregion
}
