namespace EPR.Calculator.API.BackgroundService.Models
{
    public record CalcResultScaledupProducers
    {
        public required ImmutableList<CalcResultScaledupProducer> ScaledupProducers { get; set; }
    }
}
