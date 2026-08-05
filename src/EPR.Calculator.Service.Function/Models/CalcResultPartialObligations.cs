namespace EPR.Calculator.Service.Function.Models
{
    public record CalcResultPartialObligations
    {
        public required ImmutableList<CalcResultPartialObligation> PartialObligations { get; set; }
    }
}
