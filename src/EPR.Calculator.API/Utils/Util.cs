using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using System.Text;

namespace EPR.Calculator.API.Utils
{
    public static class Util
    {
        public static CreateDefaultParameterSettingErrorDto CreateErrorDto(DefaultParameterTemplateMaster template, string errorMessage)
        {
            return new CreateDefaultParameterSettingErrorDto
            {
                ParameterUniqueRef = template.ParameterUniqueReferenceId,
                ParameterType = template.ParameterType,
                ParameterCategory = template.ParameterCategory,
                Message = errorMessage,
                Description = ""
            };
        }

        public static string GetParameterValue(DefaultParameterTemplateMaster defaultTemplate, string parameterValue)
        {
            return IsNotPercentage(defaultTemplate) ? parameterValue.Replace("£", string.Empty) : parameterValue.TrimEnd('%');
        }

        public static string FormattedErrorForEmptyValue(DefaultParameterTemplateMaster defaultTemplate)
        {
            var sb = new StringBuilder();
            if (IsNotPercentage(defaultTemplate))
            {
                sb.Append($"Enter the {defaultTemplate.ParameterType} for {defaultTemplate.ParameterCategory}");
            }
            else if (IsPercentageIncrease(defaultTemplate))
            {
                sb.Append($"Enter the {defaultTemplate.ParameterType} percentage increase");
            }
            else
            {
                sb.Append($"Enter the {defaultTemplate.ParameterType} percentage decrease");
            }
            return sb.ToString();
        }

        public static string FormattedErrorForMoreThanOneUniqueRefs(DefaultParameterTemplateMaster defaultParameterTemplateMaster)
        {
            var sb = new StringBuilder();
            sb.Append($"Parameter Type {defaultParameterTemplateMaster.ParameterType} ");
            sb.Append($"and Parameter Category {defaultParameterTemplateMaster.ParameterCategory} ");
            sb.Append($"and Parameter Unique ref {defaultParameterTemplateMaster.ParameterUniqueReferenceId}");
            return sb.ToString();
        }

        public static string FormattedErrorForNonDecimalValues(DefaultParameterTemplateMaster defaulTemplate)
        {
            var sb = new StringBuilder();
            if (IsNotPercentage(defaulTemplate))
            {
                sb.Append($"{defaulTemplate.ParameterType} for {defaulTemplate.ParameterCategory} ");
                sb.Append($"can only include numbers, commas and decimal points");
            }
            else if (IsPercentageIncrease(defaulTemplate))
            {
                sb.Append($"The {defaulTemplate.ParameterType} percentage increase ");
                sb.Append($"can only include numbers, commas, decimal points and a percentage symbol (%)");
            }
            else
            {
                sb.Append($"The {defaulTemplate.ParameterType} percentage decrease ");
                sb.Append($"can only include numbers, commas, decimal points and a percentage symbol (%)");
            }

            return sb.ToString();
        }

        public static string FormattedErrorForMissingValues(DefaultParameterTemplateMaster defaulTemplate)
        {
            var sb = new StringBuilder();
            if (IsNotPercentage(defaulTemplate))
            {
                sb.Append($"Enter the {defaulTemplate.ParameterType} ");
                sb.Append($"for {defaulTemplate.ParameterCategory}");
            }
            else if (IsPercentageIncrease(defaulTemplate))
            {
                sb.Append($"Enter the {defaulTemplate.ParameterType} increase");
            }
            else
            {
                sb.Append($"Enter the {defaulTemplate.ParameterType} decrease");
            }

            return sb.ToString();
        }

        public static string FormattedErrorForOutOfRangeValues(DefaultParameterTemplateMaster defaulTemplate)
        {
            var sb = new StringBuilder();
            if (IsNotPercentage(defaulTemplate))
            {
                sb.Append($"{defaulTemplate.ParameterType} for {defaulTemplate.ParameterCategory} ");
                sb.Append($"must be between {defaulTemplate.ValidRangeFrom} and {defaulTemplate.ValidRangeTo}");
            }
            else if (IsPercentageIncrease(defaulTemplate))
            {
                sb.Append($"The {defaulTemplate.ParameterType} percentage increase ");
                sb.Append($"must be between {defaulTemplate.ValidRangeFrom}% and {defaulTemplate.ValidRangeTo}%");
            }
            else
            {
                sb.Append($"The {defaulTemplate.ParameterType} percentage decrease ");
                sb.Append($"must be between {defaulTemplate.ValidRangeFrom}% and {defaulTemplate.ValidRangeTo}%");
            }

            return sb.ToString();
        }

        public static bool IsPercentageIncrease(DefaultParameterTemplateMaster defaultTemplate)
        {
            return ((!string.IsNullOrEmpty(defaultTemplate.ParameterCategory)
                && defaultTemplate.ParameterCategory.ToLower().Contains("percent")) ||
                (!string.IsNullOrEmpty(defaultTemplate.ParameterType)
                && defaultTemplate.ParameterType.ToLower().Contains("percent"))) &&
                defaultTemplate.ValidRangeFrom >= 0;
        }

        public static bool IsNotPercentage(DefaultParameterTemplateMaster defaultTemplate)
        {
            return !string.IsNullOrEmpty(defaultTemplate.ParameterCategory)
                && !defaultTemplate.ParameterCategory.ToLower().Contains("percent")
                && !defaultTemplate.ParameterType.ToLower().Contains("percent");
        }
    }
}
