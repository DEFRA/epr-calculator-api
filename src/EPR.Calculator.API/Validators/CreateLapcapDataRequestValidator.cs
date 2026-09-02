using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Utils;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Utils;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Validators;

public class CreateLapcapDataRequestValidator : AbstractValidator<CreateLapcapDataRequest>
{
    private readonly ApplicationDBContext dbContext;
    private ImmutableDictionary<string, LapcapDataTemplateMaster> masterTemplate = null!;

    public CreateLapcapDataRequestValidator(ApplicationDBContext dbContext)
    {
        this.dbContext = dbContext;

        RuleFor(r => r.RelativeYear)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .MustAsync((requestYear, ct) => dbContext.CalculatorRunRelativeYears.AnyAsync(dbYear => dbYear.Value == requestYear!.Value, ct))
            .WithMessage(CommonResources.NoDataForSpecifiedYear);

        RuleFor(r => r.Filename)
            .NotEmpty()
            .WithMessage(CommonResources.FileNameRequired)
            .MaximumLength(256)
            .WithMessage(CommonResources.MaxFileNameLength);

        RuleFor(r => r.Values)
            .NotEmpty()
            .DependentRules(() =>
            {
                RuleFor(r => r.Values!)
                    .Custom(MustNotHaveDuplicates)
                    .Custom(AllMasterCombinationsMustBePresent)
                    .Custom(CountriesMustNotBeNegative)
                    .Custom(MaterialsMustNotBeNegative);

                RuleForEach(r => r.Values)
                    .SetValidator(new LapcapValueValidator(() => masterTemplate));
            });
    }


    public override async Task<ValidationResult> ValidateAsync(ValidationContext<CreateLapcapDataRequest> context, CancellationToken cancellation = default)
    {
        // Cache the master template for use in validator rules.
        masterTemplate = await dbContext
            .LapcapDataTemplateMaster
            .ToImmutableDictionaryAsync(LapcapKeyHelper.KeyFor, cancellation);

        return await base.ValidateAsync(context, cancellation);
    }

    private static void MustNotHaveDuplicates(ImmutableList<CreateLapcapDataRequest.LapcapValue> values, ValidationContext<CreateLapcapDataRequest> context)
    {
        var duplicates = values
            .Select((value, index) => new
            {
                Key = LapcapKeyHelper.KeyFor(value),
                Index = index,
                value.Country,
                value.Material
            })
            .GroupBy(value => value.Key)
            .Where(grp => grp.Count() > 1)
            .Select(grp => grp.OrderBy(value => value.Index).Skip(1).First());

        foreach (var duplicate in duplicates)
        {
            context.AddFailure(
                $"{context.PropertyPath}[{duplicate.Index}]",
                $"You have entered the total cost for {duplicate.Material} in {duplicate.Country} more than once." +
                $" Make sure there is only one entry for {duplicate.Material} in {duplicate.Country}.");
        }
    }

    private void AllMasterCombinationsMustBePresent(ImmutableList<CreateLapcapDataRequest.LapcapValue> values, ValidationContext<CreateLapcapDataRequest> context)
    {
        var requestKeys = values
            .Select(LapcapKeyHelper.KeyFor)
            .ToHashSet();

        var missing = masterTemplate
            .Where(entry => !requestKeys.Contains(entry.Key))
            .Select(entry => entry.Value)
            .OrderBy(entry => entry.Country)
            .ThenBy(entry => entry.Material);

        foreach (var master in missing)
        {
            context.AddFailure($"The total cost for {master.Material} in {master.Country} is missing." +
                               $" Enter the total cost for {master.Material} in {master.Country}.");
        }
    }

    private void CountriesMustNotBeNegative(ImmutableList<CreateLapcapDataRequest.LapcapValue> values, ValidationContext<CreateLapcapDataRequest> context)
        => OverallTotalMustNotBeNegative(values, value => value.Country, context);

    private void MaterialsMustNotBeNegative(ImmutableList<CreateLapcapDataRequest.LapcapValue> values, ValidationContext<CreateLapcapDataRequest> context)
        => OverallTotalMustNotBeNegative(values, value => value.Material, context);

    private void OverallTotalMustNotBeNegative(
        IEnumerable<CreateLapcapDataRequest.LapcapValue> values,
        Func<CreateLapcapDataRequest.LapcapValue, string?> groupSelector,
        ValidationContext<CreateLapcapDataRequest> context)
    {
        var negativeTotals = values
            .Where(value => masterTemplate.ContainsKey(LapcapKeyHelper.KeyFor(value)))
            .GroupBy(value => groupSelector(value)?.ToUpperInvariant())
            .Select(grp => new
            {
                Name = groupSelector(grp.First()),
                TotalCost = grp.Sum(value => value.TotalCost)
            })
            .Where(grp => grp.TotalCost < 0);

        foreach (var negative in negativeTotals)
            context.AddFailure($"The overall total disposal cost for {negative.Name} is negative ({negative.TotalCost:C}).");
    }

    private sealed class LapcapValueValidator : AbstractValidator<CreateLapcapDataRequest.LapcapValue>
    {
        private readonly Func<IReadOnlyDictionary<string, LapcapDataTemplateMaster>> masterTemplateAccessor;

        public LapcapValueValidator(Func<IReadOnlyDictionary<string, LapcapDataTemplateMaster>> masterTemplateAccessor)
        {
            this.masterTemplateAccessor = masterTemplateAccessor;

            RuleFor(lv => lv.Country)
                .NotEmpty()
                .MaximumLength(400);

            RuleFor(lv => lv.Material)
                .NotEmpty()
                .MaximumLength(400);

            RuleFor(lv => lv.TotalCost)
                .NotNull();

            When(lv => !string.IsNullOrWhiteSpace(lv.Country) && !string.IsNullOrWhiteSpace(lv.Material), () =>
            {
                RuleFor(lv => lv)
                    .Must(ExistInMaster)
                    .WithMessage("The country and material combination {Country}/{Material} does not exist in the master template.");

                When(lv => lv.TotalCost.HasValue, () =>
                {
                    RuleFor(lv => lv.TotalCost)
                        .Must(BeWithinRange)
                        .WithMessage("The total cost for {Material} in {Country} is invalid. Enter a total cost between {Min} and {Max}.");
                });
            });
        }

        private bool ExistInMaster(CreateLapcapDataRequest.LapcapValue lapcapValue, CreateLapcapDataRequest.LapcapValue _, ValidationContext<CreateLapcapDataRequest.LapcapValue> context)
        {
            context.MessageFormatter.AppendArgument("Material", lapcapValue.Material);
            context.MessageFormatter.AppendArgument("Country", lapcapValue.Country);

            return masterTemplateAccessor().ContainsKey(LapcapKeyHelper.KeyFor(lapcapValue));
        }

        private bool BeWithinRange(CreateLapcapDataRequest.LapcapValue lapcapValue, decimal? totalCost, ValidationContext<CreateLapcapDataRequest.LapcapValue> context)
        {
            if (masterTemplateAccessor().TryGetValue(LapcapKeyHelper.KeyFor(lapcapValue), out var master))
            {
                context.MessageFormatter.AppendArgument("Material", lapcapValue.Material);
                context.MessageFormatter.AppendArgument("Country", lapcapValue.Country);
                context.MessageFormatter.AppendArgument("Min", master.TotalCostFrom.ToString("C"));
                context.MessageFormatter.AppendArgument("Max", master.TotalCostTo.ToString("C"));

                return totalCost >= master.TotalCostFrom && totalCost <= master.TotalCostTo;
            }

            return true;
        }
    }
}
