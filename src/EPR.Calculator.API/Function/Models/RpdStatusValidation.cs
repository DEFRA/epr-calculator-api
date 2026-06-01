namespace EPR.Calculator.Service.Function.Models
{
    public record RpdStatusValidation
    {
        public bool isValid { get; set; }

        public int StatusCode { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
