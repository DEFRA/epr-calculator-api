namespace EPR.Calculator.API.Data.DataModels
{
    public class LapcapDataMaster
    {
        public int Id { get; set; }

        public string ProjectionYearId { get; set; } = null!;

        public required CalculatorRunFinancialYear ProjectionYear { get; set; }

        public DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        public string CreatedBy { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string LapcapFileName { get; set; } = string.Empty;

        public ICollection<LapcapDataDetail> Details { get; } = new List<LapcapDataDetail>();

        public ICollection<CalculatorRun>? RunDetails { get; }
    }
}
