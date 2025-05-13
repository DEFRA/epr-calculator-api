namespace EPR.Calculator.API.Data.DataModels
{
    public class ProducerDetail
    {
        public int Id { get; set; }

        public int ProducerId { get; set; }

        public string? TradingName { get; set; }

        public string? SubsidiaryId { get; set; }

        public string? ProducerName { get; set; }

        public int CalculatorRunId { get; set; }

        public ICollection<ProducerReportedMaterial> ProducerReportedMaterials { get; } = new List<ProducerReportedMaterial>();

        public virtual CalculatorRun? CalculatorRun { get; set; }
    }
}