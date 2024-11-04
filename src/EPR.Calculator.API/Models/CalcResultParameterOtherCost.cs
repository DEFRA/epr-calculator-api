namespace EPR.Calculator.API.Models
{
    public class CalcResultParameterOtherCost
    {
        public string Name { get; set; }
        public IEnumerable<CalcResultParameterOtherCostDetail> CalcResultParameterCommunicationCostDetails1 { get; set; } =
            new List<CalcResultParameterOtherCostDetail>();
        public IEnumerable<CalcResultParameterOtherCostDetail> CalcResultParameterCommunicationCostDetails2 { get; set; } =
            new List<CalcResultParameterOtherCostDetail>();

        public IEnumerable<CalcResultParameterOtherCostDetail> CalcResultParameterCommunicationCostDetails3 { get; set; } =
            new List<CalcResultParameterOtherCostDetail>();

        public KeyValuePair<string, string> CalcResultParameterCommunicationCostDetails4 { get; set; } = new KeyValuePair<string, string>();

        public IEnumerable<CalcResultParameterOtherCostDetail5> CalcResultParameterCommunicationCostDetails5 { get; set; } =
            new List<CalcResultParameterOtherCostDetail5>();
    }
}
