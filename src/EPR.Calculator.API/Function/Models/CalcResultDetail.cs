using EPR.Calculator.API.Data.Models;

namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultDetail
    {
        public string RunName { get; set; } = string.Empty;
        public required int RunId { get; set; }
        public required RelativeYear RelativeYear { get; set; }
        public DateTime RunDate { get; set; }
        public string RunBy { get; set; } = string.Empty;
        public string RpdFileORG { get; set; } = string.Empty;
        public string RpdFilePOM { get; set; } = string.Empty;
        public string LapcapFile { get; set; } = string.Empty;
        public string ParametersFile { get; set; } = string.Empty;
        public string CountryApportionmentFile { get; set; } = string.Empty;
    }
}