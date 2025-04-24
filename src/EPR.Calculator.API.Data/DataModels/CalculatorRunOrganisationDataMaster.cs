namespace EPR.Calculator.API.Data.DataModels
{
    public class CalculatorRunOrganisationDataMaster
    {
        public int Id { get; set; }

        public required string CalendarYear { get; set; }

        public required DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        public required string CreatedBy { get; set; }

        public required DateTime CreatedAt { get; set; }

        public ICollection<CalculatorRunOrganisationDataDetail> Details { get; } = new List<CalculatorRunOrganisationDataDetail>();

        public ICollection<CalculatorRun>? RunDetails { get; }
    }
}
