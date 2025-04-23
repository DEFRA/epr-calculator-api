namespace EPR.Calculator.API.Data.DataModels
{
    public class CalculatorRunFinancialYear
    {
        public required string Name { get; set; }

        public string? Description { get; set; }

        public override string ToString() => Name;

        public ICollection<CalculatorRun> CalculatorRuns { get; }
            = new List<CalculatorRun>();

        public ICollection<DefaultParameterSettingMaster> DefaultParameterSettingMasters { get; }
            = new List<DefaultParameterSettingMaster>();

        public ICollection<LapcapDataMaster> LapcapDataMasters { get; }
            = new List<LapcapDataMaster>();
    }
}
