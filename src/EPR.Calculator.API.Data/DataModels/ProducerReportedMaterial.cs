namespace EPR.Calculator.API.Data.DataModels
{
    public class ProducerReportedMaterial
    {
        public int Id { get; set; }

        public int MaterialId { get; set; }

        public int ProducerDetailId { get; set; }

        public required string PackagingType { get; set; }

        public decimal PackagingTonnage { get; set; }

        public decimal? RedRamRagRating { get; set; }

        public decimal? AmberRamRagRating { get; set; }

        public decimal? GreenRamRagRating { get; set; }

        public decimal? RedMedicalRamRagRating { get; set; }

        public decimal? AmberMedicalRamRagRating { get; set; }

        public decimal? GreenMedicalRamRagRating { get; set; }

        public virtual ProducerDetail? ProducerDetail { get; set; }

        public virtual Material? Material { get; set; }
    }
}