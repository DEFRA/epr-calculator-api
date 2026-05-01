namespace EPR.Calculator.API.Data.DataModels;

public class Material
{
    public int Id { get; set; }

    public required string Code { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    #region EF navigational properties

    public virtual ICollection<ProducerReportedMaterial> ProducerReportedMaterials { get; } = [];
    public virtual ICollection<ProducerReportedMaterialProjected> ProducerReportedMaterialProjecteds { get; } = [];
    public virtual ICollection<ProducerInvoicedMaterialNetTonnage> ProducerInvoicedMaterialNetTonnage { get; } = [];

    #endregion
}
