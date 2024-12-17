namespace EPR.Calculator.API.Models
{
    public class CalcResultLaDisposalCostData
    {
        public required string Name { get; set; }
        public IEnumerable<CalcResultLaDisposalCostDataDetail> CalcResultLaDisposalCostDetails { get; set; } = new List<CalcResultLaDisposalCostDataDetail>();
    }
}
