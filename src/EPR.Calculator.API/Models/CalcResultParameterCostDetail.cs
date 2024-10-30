using System.Runtime.Serialization.Formatters;

namespace EPR.Calculator.API.Models
{

    public class CalcResultParameterCostDetail
    {
        public string Material { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public string Country { get; set; } = string.Empty;
        public CalcResultFormatterType CalcResultFormatterType { get; set; }
        public int OrderId { get; set; }
    }
}
