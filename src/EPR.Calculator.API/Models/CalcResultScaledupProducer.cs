namespace EPR.Calculator.API.Models
{
    public class CalcResultScaledupProducer
    {
        public required string ProducerId { get; set; }

        public required string SubsidiaryId { get; set; }

        public required string ProducerName { get; set; }

        public required string Level { get; set; }

        public bool isTotalRow { get; set; } = false;

        public required string SubmissonPeriodCode { get; set; }

        public required int DaysInSubmissionPeriod { get; set; }

        public required int DaysInWholePeriod { get; set; }

        public required decimal ScaleupFactor { get; set; }

        public required Dictionary<string, CalcResultScaledupProducerTonnage> ScaledupProducerTonnageByMaterial { get; set; }
    }
}
