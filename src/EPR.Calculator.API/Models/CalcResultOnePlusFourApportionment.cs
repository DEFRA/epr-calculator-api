namespace EPR.Calculator.API.Models
{
    public class CalcResultOnePlusFourApportionment
    {
        public string Name { get; set; }
        public IEnumerable<CalcResultParameterCostDetail> CalcResultOnePlusFourApportionmentDetails { get; set; } = new List<CalcResultParameterCostDetail>();
    }

}
