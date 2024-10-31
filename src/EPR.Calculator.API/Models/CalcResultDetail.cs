﻿namespace EPR.Calculator.API.Models
{
    public class CalcResultDetail
    {
        public string RunName { get; set; } = string.Empty;
        public int RunId { get; set; }
        public DateTime RunDdate { get; set; }
        public string RunBy { get; set; } = string.Empty;
        public string FinancialYear { get; set; } = string.Empty;
        public string RpdFile { get; set; } = string.Empty;
        public string LapcapFile { get; set; } = string.Empty;
        public string ParametersFile { get; set; } = string.Empty;
    }
}