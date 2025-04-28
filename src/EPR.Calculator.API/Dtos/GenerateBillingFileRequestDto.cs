using System.ComponentModel.DataAnnotations;

namespace EPR.Calculator.API.Dtos
{
    public class GenerateBillingFileRequestDto : IValidatableObject
    {
        public int CalculatorRunId { get; set; }

        [Required]
        public required string BillingFileRequestedBy { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            throw new NotImplementedException();
        }
    }
}
