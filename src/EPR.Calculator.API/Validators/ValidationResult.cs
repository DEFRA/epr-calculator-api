namespace EPR.Calculator.API.Validators
{
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public IEnumerable<string> ErrorMessages { get; set; }
    }
}