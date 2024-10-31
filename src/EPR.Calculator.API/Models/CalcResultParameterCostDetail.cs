using System.Runtime.Serialization.Formatters;

namespace EPR.Calculator.API.Models
{

    public class CalcResultParameterCostDetail
    {
        /// <summary>
        /// KeyName can be a Material name or Cost name
        /// </summary>
        public string KeyName { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        /// <summary>
        /// Category Name can be the Country name
        /// </summary>
        public string CategoryName { get; set; } = string.Empty;
        public CalcResultFormatterType CalcResultFormatterType { get; set; }
        public int OrderId { get; set; }

        public int Precision { get; set; }
    }
}
