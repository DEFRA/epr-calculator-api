namespace EPR.Calculator.API.Models
{
    public class ScaledupProducer
    {
        public int? ProducerId { get; set; }

        public string? SubmissionPeriod { get; set; }

        public decimal ScaleupFactor { get; set; }

        public int DaysInSubmissionPeriod { get; set; }

        public int DaysInWholePeriod { get; set; }
    }
}
