namespace EPR.Calculator.API.Models
{
    public class CalcResultDetail
    {
        public required string RunName { get; set; }
        public int RunId { get; set; }
        public DateTime RunDdate { get; set; }
        public required string RunBy { get; set; }
        public required string FinancialYear { get; set; }
        public required string RpdFile { get; set; }
        public required string LapcapFile { get; set; }
        public required string ParametersFile { get; set; }
    }
}
