using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EPR.Calculator.API.Models
{
    public class CalcResultParameterCommunicationCost
    {
        public string Name { get; set; }
        public IEnumerable<CalcResultParameterCommunicationCostDetail1> CalcResultParameterCommunicationCostDetails { get; set; } = 
            new List<CalcResultParameterCommunicationCostDetail1>();
        public IEnumerable<CalcResultParameterCommunicationCostDetail2> CalcResultParameterCommunicationCostDetails2 { get; set; } =
    new List<CalcResultParameterCommunicationCostDetail2>();
        public IEnumerable<CalcResultParameterCommunicationCostDetail3> CalcResultParameterCommunicationCostDetails3 { get; set; } =
    new List<CalcResultParameterCommunicationCostDetail3>();
    }
}
