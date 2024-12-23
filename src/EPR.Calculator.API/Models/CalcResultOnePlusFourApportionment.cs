namespace EPR.Calculator.API.Models
{
    public class CalcResultOnePlusFourApportionment
    {
        public required string Name { get; set; }
        public IEnumerable<CalcResultOnePlusFourApportionmentDetail> CalcResultOnePlusFourApportionmentDetails { get; set; } = new List<CalcResultOnePlusFourApportionmentDetail>();
    }

}
