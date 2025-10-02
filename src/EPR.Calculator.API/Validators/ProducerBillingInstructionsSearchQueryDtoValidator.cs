using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using FluentValidation;

namespace EPR.Calculator.API.Validators;

public class ProducerBillingInstructionsSearchQueryDtoValidator : AbstractValidator<ProducerBillingInstructionsSearchQueryDto>
{
    private static readonly HashSet<string> AllowedStatuses = Enum.GetNames<BillingStatus>().ToHashSet(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> AllowedInstructionSuggestions = Enum.GetNames<BillingInstruction>().ToHashSet(StringComparer.OrdinalIgnoreCase);

    public ProducerBillingInstructionsSearchQueryDtoValidator()
    {
        this.RuleFor(x => x.OrganisationId)
            .Must(id => !id.HasValue || id > 0)
            .WithMessage(CommonResources.OrganisationIdGreaterThan0)
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
                        context.AddFailure(string.Format(CommonResources.StatusCanOnlyContain, string.Join(", ", AllowedStatuses)));
                    }

                    // No duplicates (case-insensitive)
                    if (list.Count != list.Distinct(System.StringComparer.OrdinalIgnoreCase).Count())
                    {
                        context.AddFailure(CommonResources.StatusDuplicateValues);
                    }
                }
            })
            .When(x => x != null && x.Status != null && x.Status.Any());

        this.RuleFor(x => x.BillingInstruction)
            .Custom((instructionList, context) =>
            {
                if (instructionList != null)
                {
                    var list = instructionList.ToList();

                    // Only allowed values
                    var invalidInstructions = list.Where(s => !AllowedInstructionSuggestions.Contains(s)).Distinct().ToList();
                    if (invalidInstructions.Count > 0)
                    {
                        context.AddFailure(string.Format(CommonResources.BillingInstructionCanOnlyContain, string.Join(", ", AllowedInstructionSuggestions)));
                    }

                    // No duplicates (case-insensitive)
                    if (list.Count != list.Distinct(System.StringComparer.OrdinalIgnoreCase).Count())
                    {
                        context.AddFailure(CommonResources.BillingInstructionDuplicateValues);
                    }
                }
            })
            .When(x => x != null && x.BillingInstruction != null && x.BillingInstruction.Any());
    }
}