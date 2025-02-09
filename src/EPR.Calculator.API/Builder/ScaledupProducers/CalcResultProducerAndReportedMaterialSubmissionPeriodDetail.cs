using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.Builder.ScaledupProducers
{
    public class CalcResultProducerAndReportedMaterialSubmissionPeriodDetail
    {
        public required ProducerDetail ProducerDetail { get; set; }
        public required ProducerReportedMaterial ProducerReportedMaterial { get; set; }
        public required string SubmissionPeriodLookup { get; set; }
    }
}
