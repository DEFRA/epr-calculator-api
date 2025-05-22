namespace EPR.Calculator.API.Data.DataModels
{
    public class Material
    {
        public int Id { get; set; }

        public required string Code { get; set; }

        public required string Name { get; set; }

        public string? Description { get; set; }

        public virtual ICollection<ProducerReportedMaterial> ProducerReportedMaterials { get; } = new List<ProducerReportedMaterial>();

        public ICollection<ProducerInvoicedMaterialNetTonnage> ProducerInvoicedMaterialNetTonnage { get; } = [];
    }
}