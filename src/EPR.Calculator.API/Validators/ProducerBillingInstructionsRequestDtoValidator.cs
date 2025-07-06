using EPR.Calculator.API.Dtos;
using FluentValidation;

namespace EPR.Calculator.API.Validators
{
    public class ProducerBillingInstructionsRequestDtoValidator : AbstractValidator<ProducerBillingInstructionsRequestDto>
    {
        public ProducerBillingInstructionsRequestDtoValidator()
        {
            this.RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1)
                .WithMessage("PageNumber must be 1 or greater.");

            this.RuleFor(x => x.PageSize)
                .GreaterThanOrEqualTo(1)
                .WithMessage("PageSize must be at least 1 if provided.");

            this.RuleFor(x => x.SearchQuery)
                .SetValidator(new ProducerBillingInstructionsSearchQueryDtoValidator())
                .When(x => x.SearchQuery != null);
        }
    }
}