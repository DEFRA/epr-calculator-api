namespace EPR.Calculator.API.BackgroundService.Models
{
    public record CalcResultPartialObligations
    {
        public required ImmutableList<CalcResultPartialObligation> PartialObligations { get; set; }
    }
}
