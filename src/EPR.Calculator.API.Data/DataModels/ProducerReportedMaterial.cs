namespace EPR.Calculator.API.Data.DataModels
{
    public class ProducerReportedMaterial
    {
        public int Id { get; set; }

        public int MaterialId { get; set; }

        public int ProducerDetailId { get; set; }

        public required string PackagingType { get; set; }

        public decimal PackagingTonnage { get; set; }

        public virtual ProducerDetail? ProducerDetail { get; set; }

        public virtual Material? Material { get; set; }
    }
}