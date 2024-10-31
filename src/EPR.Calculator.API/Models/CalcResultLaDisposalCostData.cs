namespace EPR.Calculator.API.Models
{
    public class CalcResultLaDisposalCostData
    {
        public IEnumerable<CalcResultParameterCostDetail> CalcResultLaDisposalCostDetails { get; set; } = new List<CalcResultParameterCostDetail>();
    }
}
