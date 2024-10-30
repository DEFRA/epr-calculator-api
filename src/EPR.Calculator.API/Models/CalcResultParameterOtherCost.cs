namespace EPR.Calculator.API.Models
{
    public class CalcResultParameterOtherCost
    {
        public IEnumerable<CalcResultParameterCostDetail> CalcResultParameterCommunicationCostDetails { get; set; } = 
            new List<CalcResultParameterCostDetail>();
    }
}
