namespace EPR.Calculator.API.Models
{
    public class CalcResult1Plus4Apportionment
    {
        public IEnumerable<CalcResultParameterCostDetail> CalcResultParameterCommunicationCostDetails { get; set; } =
            new List<CalcResultParameterCostDetail>();
    }
}
