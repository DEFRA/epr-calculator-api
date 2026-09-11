using System.Globalization;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Utils;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Utils;
using EPR.Calculator.API.Utils.Humanizers;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Validators;

public class SetDefaultParametersRequestValidator : AbstractValidator<SetDefaultParametersRequest>
{
    private readonly ApplicationDBContext dbContext;
    private ImmutableDictionary<string, DefaultParameterTemplateMaster> masterTemplate = null!;

    public SetDefaultParametersRequestValidator(ApplicationDBContext dbContext)
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

        RuleFor(r => r.Parameters)
            .NotEmpty()
            .DependentRules(() =>
            {
                RuleFor(r => r.Parameters!)
                    .Custom(MustNotHaveDuplicates)
                    .Custom(AllTemplateValuesMustBePresent);

                RuleForEach(r => r.Parameters)
                    .SetValidator(new ParameterValueValidator(() => masterTemplate));
            });
    }

    public override async Task<ValidationResult> ValidateAsync(
        ValidationContext<SetDefaultParametersRequest> context,
        CancellationToken cancellation = default)
    {
        // Cache the master template for use in validator rules.
        masterTemplate = await dbContext
            .DefaultParameterTemplateMasterList
            .ToImmutableDictionaryAsync(t => t.ParameterUniqueReferenceId, StringComparer.OrdinalIgnoreCase, cancellation);

        return await base.ValidateAsync(context, cancellation);
    }

    private static void MustNotHaveDuplicates(
        IReadOnlyList<SetDefaultParametersRequest.ParameterValue> parameters,
        ValidationContext<SetDefaultParametersRequest> context)
    {
        var duplicates = parameters
            .Select((parameter, index) => new
            {
                Id = parameter.Id ?? "",
                Index = index
            })
            .GroupBy(value => value.Id.ToUpperInvariant())
            .Where(grp => grp.Count() > 1)
            .Select(grp => grp.OrderBy(value => value.Index).Skip(1).First());

        foreach (var duplicate in duplicates)
        {
            context.AddFailure(
                $"{context.PropertyPath}[{duplicate.Index}]",
                $"The parameter {duplicate.Id} is duplicated. Remove any duplicated rows in the file.");
        }
    }

    private void AllTemplateValuesMustBePresent(
        IReadOnlyList<SetDefaultParametersRequest.ParameterValue> parameters,
        ValidationContext<SetDefaultParametersRequest> context)
    {
        var requestKeys = parameters
            .Select(p => p.Id)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missing = masterTemplate
            .Where(entry => !requestKeys.Contains(entry.Key))
            .Select(entry => entry.Value);

        foreach (var templateValue in missing)
            context.AddFailure($"The parameter {templateValue.ParameterUniqueReferenceId} is missing. Add the parameter to the file.");
    }

    private sealed class ParameterValueValidator : AbstractValidator<SetDefaultParametersRequest.ParameterValue>
    {
        private readonly Func<IReadOnlyDictionary<string, DefaultParameterTemplateMaster>> masterTemplateAccessor;

        public ParameterValueValidator(Func<IReadOnlyDictionary<string, DefaultParameterTemplateMaster>> masterTemplateAccessor)
        {
            this.masterTemplateAccessor = masterTemplateAccessor;

            RuleFor(p => p.Id)
                .NotEmpty()
                .MaximumLength(400);

            When(p => !string.IsNullOrWhiteSpace(p.Id), () =>
            {
                RuleFor(p => p.Id!)
                    .Must(ExistInTemplate)
                    .WithMessage(p =>  $"The parameter {p.Id} is an unexpected parameter. Remove it from the file.")
                    .DependentRules(() =>
                    {
                        RuleFor(p => p.Value!)
                            .Cascade(CascadeMode.Stop)
                            .NotEmpty()
                            .WithMessage(p => $"The parameter {p.Id} is blank. Add a value for it.")
                            .MaximumLength(400)
                            .Must(HaveCorrectFormat)
                            .WithMessage("The parameter {Id} is not a valid {Unit} value. {FormatHint}")
                            .Must(BeWithinRange)
                            .WithMessage("The parameter {Id} must be between {Min} and {Max}.");
                    });
            });
        }

        private bool ExistInTemplate(
            SetDefaultParametersRequest.ParameterValue parameter,
            string id,
            ValidationContext<SetDefaultParametersRequest.ParameterValue> context) =>
            masterTemplateAccessor().ContainsKey(id);

        private bool HaveCorrectFormat(
            SetDefaultParametersRequest.ParameterValue parameter,
            string value,
            ValidationContext<SetDefaultParametersRequest.ParameterValue> context)
        {
            var template = masterTemplateAccessor()[context.InstanceToValidate.Id!];

            var (isValid, unit, formatHint) = template.Unit switch
            {
                ParameterUnit.Date => CheckDateFormat(value),
                ParameterUnit.Currency => CheckCurrencyFormat(value),
                ParameterUnit.Percentage => CheckPercentageFormat(value),
                ParameterUnit.Tonnage => CheckTonnageFormat(value),
                _ => CheckDecimalFormat(value)
            };

            context.MessageFormatter.AppendArgument("Id", context.InstanceToValidate.Id);
            context.MessageFormatter.AppendArgument("Unit", unit);
            context.MessageFormatter.AppendArgument("FormatHint", formatHint);

            return isValid;

            static (bool, string, string) CheckDateFormat(string value) =>
            (
                value.Equals("NA", StringComparison.OrdinalIgnoreCase) || DateOnly.TryParse(value, CultureInfo.CurrentCulture, out _),
                "date",
                "Enter a valid date (dd/MM/yyyy) or 'NA'."
            );

            static (bool, string, string) CheckCurrencyFormat(string value) =>
            (
                decimal.TryParse(value, NumberStyles.Currency, CultureInfo.CurrentCulture, out _),
                "currency",
                "It can only include numbers, decimal points and a pound symbol (£)."
            );

            static (bool, string, string) CheckPercentageFormat(string value) =>
            (
                decimal.TryParse(value.TrimEnd('%'), out _),
                "percentage",
                "It can only include numbers, commas, decimal points and a percentage symbol (%)."
            );

            static (bool, string, string) CheckTonnageFormat(string value) =>
            (
                decimal.TryParse(value.TrimEnd('t', 'T'), out _),
                "tonnage",
                "It can only include numbers, decimal points and a tonnage symbol (t)."
            );

            static (bool, string, string) CheckDecimalFormat(string value) =>
            (
                decimal.TryParse(value, out _),
                "decimal",
                "It can only include numbers and decimal points."
            );
        }

        private bool BeWithinRange(
            SetDefaultParametersRequest.ParameterValue parameter,
            string value,
            ValidationContext<SetDefaultParametersRequest.ParameterValue> context)
        {
            var template = masterTemplateAccessor()[context.InstanceToValidate.Id!];

            if (template.Unit == ParameterUnit.Date)
                return true;

            context.MessageFormatter.AppendArgument("Id", context.InstanceToValidate.Id);
            context.MessageFormatter.AppendArgument("Min", GetRangeDisplayValue(template.Unit, template.ValidRangeFrom));
            context.MessageFormatter.AppendArgument("Max", GetRangeDisplayValue(template.Unit, template.ValidRangeTo));

            var decimalValue = decimal.Parse(RegexPatterns.NonDecimalChars().Replace(value, ""));

            return decimalValue >= template.ValidRangeFrom && decimalValue <= template.ValidRangeTo;

            static string GetRangeDisplayValue(ParameterUnit unit, decimal value) =>
                unit switch
                {
                    ParameterUnit.Currency => value.Humanize("C"),
                    ParameterUnit.Percentage => value.Humanize("%"),
                    ParameterUnit.Tonnage => value.Humanize("t"),
                    _ => value.Humanize()
                };
        }
    }
}
