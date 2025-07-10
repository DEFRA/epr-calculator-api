using EPR.Calculator.API.Dtos;
using FluentValidation;

namespace EPR.Calculator.API.Validators;

public class ProducerBillingInstructionsSearchQueryDtoValidator : AbstractValidator<ProducerBillingInstructionsSearchQueryDto>
{
    private static readonly HashSet<string> AllowedStatuses = new() { "Accepted", "Rejected", "Pending" };

    public ProducerBillingInstructionsSearchQueryDtoValidator()
    {
        this.RuleFor(x => x.OrganisationId)
            .Must(id => !id.HasValue || id > 0)
            .WithMessage("OrganisationId must be greater than 0 if provided.")
            .When(x => x != null && x.OrganisationId.HasValue);

        this.RuleFor(x => x.Status)
            .Custom((statusList, context) =>
            {
                if (statusList != null)
                {
                    var list = statusList.ToList();

                    // Only allowed values
                    var invalidStatuses = list.Where(s => !AllowedStatuses.Contains(s)).Distinct().ToList();
                    if (invalidStatuses.Count > 0)
                    {
                        context.AddFailure($"Status can only contain: {string.Join(", ", AllowedStatuses)}.");
                    }

                    // No duplicates (case-insensitive)
                    if (list.Count != list.Distinct(System.StringComparer.OrdinalIgnoreCase).Count())
                    {
                        context.AddFailure("Status cannot contain duplicate values.");
                    }
                }
            })
            .When(x => x != null && x.Status != null && x.Status.Any());
    }
}