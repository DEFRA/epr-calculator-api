using System.Globalization;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;

namespace EPR.Calculator.API.Validators;

public class CreateDefaultParameterDataValidator(ApplicationDBContext context) : ICreateDefaultParameterDataValidator
{
    private readonly ApplicationDBContext context = context;

    public ValidationResultDto<CreateDefaultParameterSettingErrorDto> Validate(CreateDefaultParameterSettingDto createDefaultParameterSettingDto)
    {
        IEnumerable<DefaultParameterTemplateMaster> masterList = this.context.DefaultParameterTemplateMasterList.ToList();

        var errors = masterList
            .SelectMany(master => ValidateParameter(master, createDefaultParameterSettingDto.SchemeParameterTemplateValues))
            .Concat(ValidateUnexpectedParameters(masterList, createDefaultParameterSettingDto.SchemeParameterTemplateValues))
            .ToList();

        return new ValidationResultDto<CreateDefaultParameterSettingErrorDto>
        {
            Errors    = errors,
            IsInvalid = errors.Count != 0
        };
    }

    private static IEnumerable<CreateDefaultParameterSettingErrorDto> ValidateParameter(
        DefaultParameterTemplateMaster master,
        IEnumerable<SchemeParameterTemplateValueDto> values)
    {
        var matches = values
            .Where(x => x.ParameterUniqueReferenceId == master.ParameterUniqueReferenceId)
            .ToList();

        return matches.Count switch
        {
            0 => [CreateErrorDto(master, $"The parameter {master.ParameterUniqueReferenceId} is missing. Add the parameter to the file.")],
            1 => ValidateValue(master, matches.Single().ParameterValue),
            _ => [CreateErrorDto(master, $"The parameter {master.ParameterUniqueReferenceId} is duplicated. Remove the duplicated row in the file.")],
        };
    }

    private static IEnumerable<CreateDefaultParameterSettingErrorDto> ValidateValue(
        DefaultParameterTemplateMaster master,
        string rawValue)
    {
        var id    = master.ParameterUniqueReferenceId;
        var value = IsPercentage(master)
            ? rawValue.TrimEnd('%')
            : rawValue.Replace("£", string.Empty);

        if (string.IsNullOrWhiteSpace(value))
        {
            yield return CreateErrorDto(master, $"The value for {id} is blank. Add a value for {id}.");
        }
        else if (master.ParameterCategory == "Optional Date")
        {
            if (!value.Equals("NA", StringComparison.OrdinalIgnoreCase) && !DateTime.TryParseExact(value, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            {
                yield return CreateErrorDto(master, $"The parameter {id} value is invalid. Enter a valid date or 'NA'.");
            }
        }
        else if (!decimal.TryParse(value, out var decimalValue))
        {
            yield return IsPercentage(master)
                ? CreateErrorDto(master, $"The parameter {id} can only include numbers, commas, decimal points and a percentage symbol (%).")
                : CreateErrorDto(master, $"The parameter {id} can only include numbers, commas and decimal points.");
        }
        else if (decimalValue < master.ValidRangeFrom ||
                 decimalValue > master.ValidRangeTo)
        {
            yield return CreateErrorDto(master, OutOfRangeValues(master));
        }
    }

    private static List<CreateDefaultParameterSettingErrorDto> ValidateUnexpectedParameters(
        IEnumerable<DefaultParameterTemplateMaster> masterList,
        IEnumerable<SchemeParameterTemplateValueDto> templateValues)
    {
        var expectedIds = masterList
            .Select(x => x.ParameterUniqueReferenceId)
            .ToHashSet();

        return templateValues
            .Where(x => !expectedIds.Contains(x.ParameterUniqueReferenceId))
            .Select(x => CreateErrorDto(
                parameterUniqueRef: x.ParameterUniqueReferenceId,
                errorMessage      : $"The parameter {x.ParameterUniqueReferenceId} is an unexpected parameter. Remove it from the file."))
            .ToList();
    }

    private static CreateDefaultParameterSettingErrorDto CreateErrorDto(DefaultParameterTemplateMaster master, string errorMessage) =>
        new()
        {
            ParameterUniqueRef = master.ParameterUniqueReferenceId,
            ParameterType      = master.ParameterType,
            ParameterCategory  = master.ParameterCategory,
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

    private static string OutOfRangeValues(DefaultParameterTemplateMaster master)
    {
        var id  = master.ParameterUniqueReferenceId;

        if (IsTonnage(master))
        {
            var min = decimal.Truncate(master.ValidRangeFrom);
            var max = Math.Round(master.ValidRangeTo, 3, MidpointRounding.ToZero).ToString("N3");
            return $"The parameter {id} must be between {min} and {max} tons.";
        }
        else if (IsBadDebt(master) || IsPercentageIncrease(master))
        {
            var min = decimal.Truncate(master.ValidRangeFrom);
            var max = Math.Round(master.ValidRangeTo, 2, MidpointRounding.ToZero);
            return $"The parameter {id} must be between {min}% and {max}%.";
        }
        else if (IsPercentageDecrease(master))
        {
            var min = Math.Round(master.ValidRangeFrom, 2, MidpointRounding.ToZero);
            var max = decimal.Truncate(master.ValidRangeTo);
            return $"The parameter {id} must be between {min}% and {max}%.";
        }
        else if (IsFactor(master))
        {
            var min = Math.Round(master.ValidRangeFrom, 3, MidpointRounding.ToZero);
            var max = Math.Round(master.ValidRangeTo  , 3, MidpointRounding.ToZero);
            return $"The parameter {id} must be between {min} and {max}.";
        }
        else
        {
            var min = Math.Round(master.ValidRangeFrom, 2, MidpointRounding.ToZero).ToString("C");
            var max = Math.Round(master.ValidRangeTo  , 2, MidpointRounding.ToZero).ToString("C");
            return $"The parameter {id} must be between {min} and {max}.";
        }
    }

    private static bool IsTonnage(DefaultParameterTemplateMaster master) =>
         master.ParameterType    .Contains("tonnage", StringComparison.OrdinalIgnoreCase) &&
        !master.ParameterCategory.Contains("amount" , StringComparison.OrdinalIgnoreCase) &&
        !master.ParameterCategory.Contains("percent", StringComparison.OrdinalIgnoreCase);

    private static bool IsFactor(DefaultParameterTemplateMaster master) =>
        master.ParameterType    .Contains("factor", StringComparison.OrdinalIgnoreCase) &&
        master.ParameterCategory.Contains("factor", StringComparison.OrdinalIgnoreCase);

    private static bool IsBadDebt(DefaultParameterTemplateMaster master) =>
        master.ParameterType.Contains("bad debt", StringComparison.OrdinalIgnoreCase);

    private static bool IsPercentage(DefaultParameterTemplateMaster master) =>
        (!string.IsNullOrEmpty(master.ParameterCategory) && master.ParameterCategory.Contains("percent", StringComparison.OrdinalIgnoreCase)) ||
        (!string.IsNullOrEmpty(master.ParameterType    ) && master.ParameterType    .Contains("percent", StringComparison.OrdinalIgnoreCase));

    private static bool IsPercentageIncrease(DefaultParameterTemplateMaster master) =>
        IsPercentage(master) && master.ValidRangeFrom >= 0;

    private static bool IsPercentageDecrease(DefaultParameterTemplateMaster master) =>
        IsPercentage(master) && master.ValidRangeFrom < 0;
}
