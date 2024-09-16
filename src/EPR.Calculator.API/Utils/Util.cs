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

            if (IsTonnage(defaulTemplate))
            {
                sb.Append($"{defaulTemplate.ParameterType} for {defaulTemplate.ParameterCategory} ");
                sb.Append($"must be between {decimal.Truncate(defaulTemplate.ValidRangeFrom)} and {Math.Round(defaulTemplate.ValidRangeTo, 3, MidpointRounding.ToZero)} ");
                sb.Append("tons");
            }
            else if (IsTonnageAmountIncrease(defaulTemplate) || IsTonnageAmountDecrease(defaulTemplate))
            {
                sb.Append($"{defaulTemplate.ParameterType} for {defaulTemplate.ParameterCategory} ");
                sb.Append($"must be between £{Math.Round(defaulTemplate.ValidRangeFrom, 2, MidpointRounding.ToZero)} and £{Math.Round(defaulTemplate.ValidRangeTo, 2, MidpointRounding.ToZero)}");
            }
            else if (IsBadDebt(defaulTemplate))
            {
                sb.Append($"The {defaulTemplate.ParameterType} ");
                sb.Append($"must be between {decimal.Truncate(defaulTemplate.ValidRangeFrom)}% and {Math.Round(defaulTemplate.ValidRangeTo, 2, MidpointRounding.ToZero)}%");
            }
            else if (IsPercentageIncrease(defaulTemplate))
            {
                sb.Append($"The {defaulTemplate.ParameterType} percentage increase ");
                sb.Append($"must be between { decimal.Truncate(defaulTemplate.ValidRangeFrom)}% and {Math.Round(defaulTemplate.ValidRangeTo, 2, MidpointRounding.ToZero)}%");
            }
            else if (IsPercentageDecrease(defaulTemplate))
            {
                sb.Append($"The {defaulTemplate.ParameterType} percentage decrease ");
                sb.Append($"must be between {Math.Round(defaulTemplate.ValidRangeFrom, 2, MidpointRounding.ToZero)}% and {decimal.Truncate(defaulTemplate.ValidRangeTo)}%");
            }
            else
            {
                sb.Append($"{defaulTemplate.ParameterType} for {defaulTemplate.ParameterCategory} ");
                sb.Append($"must be between £{Math.Round(defaulTemplate.ValidRangeFrom, 2, MidpointRounding.ToZero)} and £{Math.Round(defaulTemplate.ValidRangeTo, 2, MidpointRounding.ToZero)}");
            }

            return sb.ToString();
        }

        public static bool IsTonnage(DefaultParameterTemplateMaster defaultTemplate)
        {
            return defaultTemplate.ParameterType.ToLower().Contains("tonnage")
                && !defaultTemplate.ParameterCategory.ToLower().Contains("amount")
                && !defaultTemplate.ParameterCategory.ToLower().Contains("percent");
        }

        public static bool IsTonnageAmountIncrease(DefaultParameterTemplateMaster defaultTemplate)
        {
            return defaultTemplate.ParameterType.ToLower().Contains("tonnage")
                && defaultTemplate.ParameterCategory.ToLower().Contains("amount increase");
        }

        public static bool IsTonnageAmountDecrease(DefaultParameterTemplateMaster defaultTemplate)
        {
            return defaultTemplate.ParameterType.ToLower().Contains("tonnage")
                && defaultTemplate.ParameterCategory.ToLower().Contains("amount decrease");
        }

        public static bool IsBadDebt(DefaultParameterTemplateMaster defaultTemplate)
        {
            return defaultTemplate.ParameterType.ToLower().Contains("bad debt");
        }

        public static bool IsPercentageIncrease(DefaultParameterTemplateMaster defaultTemplate)
        {
            return ((!string.IsNullOrEmpty(defaultTemplate.ParameterCategory)
                && defaultTemplate.ParameterCategory.ToLower().Contains("percent")) ||
                (!string.IsNullOrEmpty(defaultTemplate.ParameterType)
                && defaultTemplate.ParameterType.ToLower().Contains("percent"))) &&
                defaultTemplate.ValidRangeFrom >= 0;
        }

        public static bool IsPercentageDecrease(DefaultParameterTemplateMaster defaultTemplate)
        {
            return ((!string.IsNullOrEmpty(defaultTemplate.ParameterCategory)
                && defaultTemplate.ParameterCategory.ToLower().Contains("percent")) ||
                (!string.IsNullOrEmpty(defaultTemplate.ParameterType)
                && defaultTemplate.ParameterType.ToLower().Contains("percent"))) &&
                defaultTemplate.ValidRangeFrom < 0;
        }

        public static bool IsNotPercentage(DefaultParameterTemplateMaster defaultTemplate)
        {
            return !string.IsNullOrEmpty(defaultTemplate.ParameterCategory)
                && !defaultTemplate.ParameterCategory.ToLower().Contains("percent")
                && !defaultTemplate.ParameterType.ToLower().Contains("percent");
        }

        public static CreateLapcapDataErrorDto CreateLapcapDataErrorDto(string country,
                                                                        string material,
                                                                        string message,
                                                                        string description,
                                                                        string uniqueReference)
        {
            return new CreateLapcapDataErrorDto
            {
                Country = country,
                Material = material,
                Message = message,
                Description = description,
                UniqueReference = uniqueReference
            };
        }
    }
}
