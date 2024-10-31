namespace EPR.Calculator.API.Models
{
    public class CalcResultParameterCommunicationCost
    {
        public string Name { get; set; }
        public IEnumerable<CalcResultParameterCostDetail> CalcResultParameterCommunicationCostDetails { get; set; } = 
            new List<CalcResultParameterCostDetail>();
    }
}
