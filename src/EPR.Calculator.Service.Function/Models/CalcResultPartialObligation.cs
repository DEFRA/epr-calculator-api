namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultPartialObligation
    {
        public int ProducerId { get; set; }

        public string? SubsidiaryId { get; set; }

        public string? ProducerName { get; set; }

        public string? TradingName { get; set; }

        public required string Level { get; set; }

        public required int SubmissionYear { get; set; }

        public required int DaysInSubmissionYear { get; set; }

        public string? JoiningDate { get; set; }

        public int? DaysObligated { get; set; }

        public required decimal ObligatedFactor { get; set; }

        public Dictionary<string, CalcResultPartialObligationTonnage> PartialObligationTonnageByMaterial { get; set; } = new Dictionary<string, CalcResultPartialObligationTonnage>();
    }
}
