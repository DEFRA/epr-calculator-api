namespace EPR.Calculator.API.Models
{
    public class RpdStatusValidation
    {
        public bool isValid { get; set; }

        public int StatusCode { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
