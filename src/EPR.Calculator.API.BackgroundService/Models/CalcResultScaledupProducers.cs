namespace EPR.Calculator.API.BackgroundService.Models
{
    public record CalcResultScaledupProducers
    {
        public required ImmutableList<CalcResultScaledupProducer> ScaledupProducers { get; set; }

        public CalcResultScaledupProducer? Total { get; set; }
    }
}
