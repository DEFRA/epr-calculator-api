namespace EPR.Calculator.API.Models
{
    public class CalcResultParameterOtherCost
    {
        public string Name { get; set; }
        public IEnumerable<CalcResultParameterOtherCostDetail> SaOperatingCost { get; set; } = new List<CalcResultParameterOtherCostDetail>();
        public IEnumerable<CalcResultParameterOtherCostDetail> Details { get; set; } = new List<CalcResultParameterOtherCostDetail>();
        public CalcResultParameterOtherCostDetail SchemeSetupCost { get; set; } = new CalcResultParameterOtherCostDetail();
        public KeyValuePair<string, string> BadDebtProvision { get; set; } = new KeyValuePair<string, string>();
        public IEnumerable<CalcResultMateriality> Materiality { get; set; } = new List<CalcResultMateriality>();
        public decimal BadDebtValue { get; set; }
    }
}
