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
                .WithMessage("Organisation Id is required.");

            this.RuleFor(x => x.Status)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Status is required.")
                .Must(value => Enum.TryParse(typeof(BillingStatus), value, true, out _))
                .WithMessage("Invalid status value.");

            this.RuleFor(x => x.ReasonForRejection)
                .NotEmpty()
                .When(x => x.Status.ToLower() == BillingStatus.Rejected.ToString().ToLower())
                .WithMessage("Reason for rejection is required.");
        }
    }
}