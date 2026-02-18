namespace EPR.Calculator.API.Data.DataModels
{
    public class CalculatorRunPomDataMaster
    {
        public int Id { get; set; }

        public required string RelativeYear { get; set; }

        public required DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        public required string CreatedBy { get; set; }

        public required DateTime CreatedAt { get; set; }

        public virtual ICollection<CalculatorRunPomDataDetail> Details { get; } = new List<CalculatorRunPomDataDetail>();

        public ICollection<CalculatorRun>? RunDetails { get; }
    }
}
