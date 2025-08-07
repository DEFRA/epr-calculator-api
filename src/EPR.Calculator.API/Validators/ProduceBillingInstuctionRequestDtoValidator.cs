using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using FluentValidation;

namespace EPR.Calculator.API.Validators
{
    public class ProduceBillingInstuctionRequestDtoValidator : AbstractValidator<ProduceBillingInstuctionRequestDto>
    {
        public ProduceBillingInstuctionRequestDtoValidator()
        {
            this.RuleFor(x => x.OrganisationIds)
                .Must(ids => ids != null && ids.Any())
                .WithMessage(CommonResources.OrganisationIdRequired);

            this.RuleFor(x => x.Status)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage(CommonResources.StatusRequired)
                .Must(value => Enum.TryParse(typeof(BillingStatus), value, true, out _))
                .WithMessage(CommonResources.InvalidStatusValue);

            this.RuleFor(x => x.ReasonForRejection)
                .NotEmpty()
                .When(x => string.Equals(x.Status, BillingStatus.Rejected.ToString()))
                .WithMessage(CommonResources.ReasonForRejectionRequired);
        }
    }
}