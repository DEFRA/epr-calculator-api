using System.Runtime.Serialization.Formatters;

namespace EPR.Calculator.API.Models
{

    public class CalcResultParameterCostDetail
    {
        public required string Material { get; set; }
        public required decimal Cost { get; set; }
        public required string Country { get; set; }
        public CalcResultFormatterType CalcResultFormatterType { get; set; }
        public int OrderId { get; set; }
    }
}
