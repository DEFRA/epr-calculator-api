namespace EPR.Calculator.API.Models
{
    public class CalcResultLaDisposalCostData
    {
        public string Name { get; set; }
        public IEnumerable<CalcResultParameterCostDetail> CalcResultLaDisposalCostDetails { get; set; } = new List<CalcResultParameterCostDetail>();
    }
}
