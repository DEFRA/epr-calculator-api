using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;

namespace EPR.Calculator.API.Validators;

public class CreateDefaultParameterDataValidator(ApplicationDBContext context) : ICreateDefaultParameterDataValidator
{
    private readonly ApplicationDBContext context = context;

    public ValidationResultDto<CreateDefaultParameterSettingErrorDto> Validate(CreateDefaultParameterSettingDto createDefaultParameterSettingDto)
    {
        IEnumerable<DefaultParameterTemplateMaster> defaultTemplateMasterList = this.context.DefaultParameterTemplateMasterList.ToList();
        var templateValues = createDefaultParameterSettingDto.SchemeParameterTemplateValues;

        var errors = ValidateExpectedParameters(defaultTemplateMasterList, templateValues)
            .Concat(ValidateUnexpectedParameters(defaultTemplateMasterList, templateValues))
            .ToList();

        return new ValidationResultDto<CreateDefaultParameterSettingErrorDto>
        {
            Errors    = errors,
            IsInvalid = errors.Count != 0
        };
    }

    private static List<CreateDefaultParameterSettingErrorDto> ValidateExpectedParameters(
        IEnumerable<DefaultParameterTemplateMaster> defaultParameters,
        IEnumerable<SchemeParameterTemplateValueDto> templateValues)
    {
        var errors = new List<CreateDefaultParameterSettingErrorDto>();

        foreach (var defaultParameter in defaultParameters)
        {
            var matchingTemplates = templateValues.Where(x => x.ParameterUniqueReferenceId == defaultParameter.ParameterUniqueReferenceId);

            if (matchingTemplates.Count() > 1)
            {
                errors.Add(CreateErrorDto(defaultParameter, string.Format(CommonResources.DuplicateDefaultParameter, defaultParameter.ParameterUniqueReferenceId)));
            }
            else if (!matchingTemplates.Any())
            {
                errors.Add(CreateErrorDto(defaultParameter, string.Format(CommonResources.MissingDefaultParameter, defaultParameter.ParameterUniqueReferenceId)));
            }
            else
            {
                var parameterValueStr = GetParameterValue(defaultParameter, matchingTemplates.Single().ParameterValue);
                if (string.IsNullOrEmpty(parameterValueStr))
                {
                    errors.Add(CreateErrorDto(defaultParameter, string.Format(CommonResources.EnterDefaultParameter, defaultParameter.ParameterUniqueReferenceId)));
                }
                else if (decimal.TryParse(parameterValueStr, out decimal parameterValue))
                {
                    if (parameterValue < defaultParameter.ValidRangeFrom || parameterValue > defaultParameter.ValidRangeTo)
                    {
                        errors.Add(CreateErrorDto(defaultParameter, FormattedErrorForOutOfRangeValues(defaultParameter)));
                    }
                }
                else
                {
                    errors.Add(CreateErrorDto(defaultParameter,  FormattedErrorForNonDecimalValues(defaultParameter)));
                }
            }
        }

        return errors;
    }

    private static List<CreateDefaultParameterSettingErrorDto> ValidateUnexpectedParameters(
        IEnumerable<DefaultParameterTemplateMaster> defaultParameters,
        IEnumerable<SchemeParameterTemplateValueDto> templateValues)
    {
        var expectedIds = defaultParameters
            .Select(x => x.ParameterUniqueReferenceId)
            .ToHashSet();

        return templateValues
            .Where(x => !expectedIds.Contains(x.ParameterUniqueReferenceId))
            .Select(x => CreateErrorDto(
                parameterUniqueRef: x.ParameterUniqueReferenceId,
                errorMessage      : string.Format(CommonResources.UnexpectedDefaultParameter, x.ParameterUniqueReferenceId)))
            .ToList();
    }

    private static CreateDefaultParameterSettingErrorDto CreateErrorDto(DefaultParameterTemplateMaster defaultParameter, string errorMessage) =>
        new()
        {
            ParameterUniqueRef = defaultParameter.ParameterUniqueReferenceId,
            ParameterType      = defaultParameter.ParameterType,
            ParameterCategory  = defaultParameter.ParameterCategory,
            Message            = errorMessage,
            Description        = string.Empty,
        };

    private static CreateDefaultParameterSettingErrorDto CreateErrorDto(string parameterUniqueRef, string errorMessage) =>
        new()
        {
            ParameterUniqueRef = parameterUniqueRef,
            ParameterType      = string.Empty,
            ParameterCategory  = string.Empty,
            Message            = errorMessage,
            Description        = string.Empty,
        };

    private static string GetParameterValue(DefaultParameterTemplateMaster defaultParameter, string parameterValue) =>
        IsPercentage(defaultParameter)
            ? parameterValue.TrimEnd('%')
            : parameterValue.Replace("£", string.Empty);

    private static string FormattedErrorForNonDecimalValues(DefaultParameterTemplateMaster defaultParameter) =>
        IsPercentage(defaultParameter)
            ? string.Format(CommonResources.CanOnlyIncudeNumbersAndPercentage, defaultParameter.ParameterUniqueReferenceId)
            : string.Format(CommonResources.CanOnlyIncludeNumbers            , defaultParameter.ParameterUniqueReferenceId);

    private static string FormattedErrorForOutOfRangeValues(DefaultParameterTemplateMaster defaultParameter)
    {
        var id   = defaultParameter.ParameterUniqueReferenceId;
        var from = defaultParameter.ValidRangeFrom;
        var to   = defaultParameter.ValidRangeTo;

        if (IsTonnage(defaultParameter))
        {
            return string.Format(CommonResources.MustBeBetweenWithTons      , id, decimal.Truncate(from), Math.Round(to, 3, MidpointRounding.ToZero));
        }
        else if (IsBadDebt(defaultParameter))
        {
            return string.Format(CommonResources.MustBeBetweenWithPercentage, id, decimal.Truncate(from), Math.Round(to, 2, MidpointRounding.ToZero));
        }
        else if (IsPercentageIncrease(defaultParameter))
        {
            return string.Format(CommonResources.MustBeBetweenWithPercentage, id, decimal.Truncate(from), Math.Round(to, 2, MidpointRounding.ToZero));
        }
        else if (IsPercentageDecrease(defaultParameter))
        {
            return string.Format(CommonResources.MustBeBetweenWithPercentage, id, Math.Round(from, 2, MidpointRounding.ToZero), decimal.Truncate(to));
        }
        else if (IsFactor(defaultParameter))
        {
            return string.Format(CommonResources.MustBeBetween              , id, Math.Round(from, 3, MidpointRounding.ToZero), Math.Round(to, 3, MidpointRounding.ToZero));
        }
        else
        {
            return string.Format(CommonResources.MustBeBetweenWithCurrency  , id, Math.Round(from, 2, MidpointRounding.ToZero), Math.Round(to, 2, MidpointRounding.ToZero));
        }
    }

    private static bool IsTonnage(DefaultParameterTemplateMaster defaultParameter) =>
         defaultParameter.ParameterType    .Contains("tonnage", StringComparison.OrdinalIgnoreCase) &&
        !defaultParameter.ParameterCategory.Contains("amount" , StringComparison.OrdinalIgnoreCase) &&
        !defaultParameter.ParameterCategory.Contains("percent", StringComparison.OrdinalIgnoreCase);

    private static bool IsFactor(DefaultParameterTemplateMaster defaultParameter) =>
        defaultParameter.ParameterType    .Contains("factor", StringComparison.OrdinalIgnoreCase) &&
        defaultParameter.ParameterCategory.Contains("factor", StringComparison.OrdinalIgnoreCase);

    private static bool IsBadDebt(DefaultParameterTemplateMaster defaultParameter) =>
        defaultParameter.ParameterType.Contains("bad debt", StringComparison.OrdinalIgnoreCase);

    private static bool IsPercentage(DefaultParameterTemplateMaster defaultParameter) =>
        (!string.IsNullOrEmpty(defaultParameter.ParameterCategory) && defaultParameter.ParameterCategory.Contains("percent", StringComparison.OrdinalIgnoreCase)) ||
        (!string.IsNullOrEmpty(defaultParameter.ParameterType    ) && defaultParameter.ParameterType    .Contains("percent", StringComparison.OrdinalIgnoreCase));

    private static bool IsPercentageIncrease(DefaultParameterTemplateMaster defaultParameter) =>
        IsPercentage(defaultParameter) && defaultParameter.ValidRangeFrom >= 0;

    private static bool IsPercentageDecrease(DefaultParameterTemplateMaster defaultParameter) =>
        IsPercentage(defaultParameter) && defaultParameter.ValidRangeFrom < 0;
}
