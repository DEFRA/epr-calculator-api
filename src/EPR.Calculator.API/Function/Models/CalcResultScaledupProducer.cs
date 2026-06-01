namespace EPR.Calculator.Service.Function.Models
{
    public record ScaledupPomEntry(
        int MaterialId,
        string? PackagingType,
        decimal Tonnage,
        decimal ScaledTonnage
    );

    public class CalcResultScaledupProducer
    {
        public int ProducerId { get; set; }

        public string? SubsidiaryId { get; set; }

        public string? ProducerName { get; set; }

        public string? TradingName { get; set; }

        public string? Level { get; set; }

        public bool IsSubtotalRow { get; set; }

        public string? SubmissionPeriodCode { get; set; }

        public int DaysInSubmissionPeriod { get; set; }

        public int DaysInWholePeriod { get; set; }

        public decimal ScaleupFactor { get; set; }

        public IReadOnlyList<ScaledupPomEntry> PomData { get; set; } = [];

        public Dictionary<string, CalcResultScaledupProducerTonnage> ScaledupProducerTonnageByMaterial { get; set; } = new Dictionary<string, CalcResultScaledupProducerTonnage>();
    }
}
