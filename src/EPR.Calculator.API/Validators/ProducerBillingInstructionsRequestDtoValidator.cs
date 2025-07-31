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
                .WithMessage(CommonResources.PageNumberGreaterThan1);

            this.RuleFor(x => x.PageSize)
                .GreaterThanOrEqualTo(1)
                .WithMessage(CommonResources.PageSizeGreaterThan1);

            this.RuleFor(x => x.SearchQuery)
                .SetValidator(new ProducerBillingInstructionsSearchQueryDtoValidator()!)
                .When(x => x.SearchQuery != null);
        }
    }
}