namespace EPR.Calculator.API.Validators
{
    public class ValidationResult
    {
        public ValidationResult()
        {
            this.ErrorMessages = new List<string>();
        }

        public bool IsValid { get; set; }

        public IEnumerable<string> ErrorMessages { get; set; }
    }
}