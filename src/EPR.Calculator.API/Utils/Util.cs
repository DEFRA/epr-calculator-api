using System.Text;
using System.Text.RegularExpressions;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;

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
                Description = string.Empty,
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
            else if (IsBadDebt(defaulTemplate))
            {
                sb.Append($"The {defaulTemplate.ParameterType} ");
                sb.Append($"must be between {decimal.Truncate(defaulTemplate.ValidRangeFrom)}% and {Math.Round(defaulTemplate.ValidRangeTo, 2, MidpointRounding.ToZero)}%");
            }
            else if (IsPercentageIncrease(defaulTemplate))
            {
                sb.Append($"The {defaulTemplate.ParameterType} percentage increase ");
                sb.Append($"must be between {decimal.Truncate(defaulTemplate.ValidRangeFrom)}% and {Math.Round(defaulTemplate.ValidRangeTo, 2, MidpointRounding.ToZero)}%");
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
            return defaultTemplate.ParameterType.Contains("tonnage", StringComparison.OrdinalIgnoreCase)
                && !defaultTemplate.ParameterCategory.Contains("amount", StringComparison.OrdinalIgnoreCase)
                && !defaultTemplate.ParameterCategory.Contains("percent", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsTonnageAmountIncrease(DefaultParameterTemplateMaster defaultTemplate)
        {
            return defaultTemplate.ParameterType.Contains("tonnage", StringComparison.OrdinalIgnoreCase)
                && defaultTemplate.ParameterCategory.Contains("amount increase", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsTonnageAmountDecrease(DefaultParameterTemplateMaster defaultTemplate)
        {
            return defaultTemplate.ParameterType.Contains("tonnage", StringComparison.OrdinalIgnoreCase)
                && defaultTemplate.ParameterCategory.Contains("amount decrease", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsBadDebt(DefaultParameterTemplateMaster defaultTemplate)
        {
            return defaultTemplate.ParameterType.Contains("bad debt", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsPercentageIncrease(DefaultParameterTemplateMaster defaultTemplate)
        {
            return ((!string.IsNullOrEmpty(defaultTemplate.ParameterCategory)
                && defaultTemplate.ParameterCategory.Contains("percent", StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(defaultTemplate.ParameterType)
                && defaultTemplate.ParameterType.Contains("percent", StringComparison.OrdinalIgnoreCase))) &&
                defaultTemplate.ValidRangeFrom >= 0;
        }

        public static bool IsPercentageDecrease(DefaultParameterTemplateMaster defaultTemplate)
        {
            return ((!string.IsNullOrEmpty(defaultTemplate.ParameterCategory)
                && defaultTemplate.ParameterCategory.Contains("percent", StringComparison.OrdinalIgnoreCase)) ||
                (!string.IsNullOrEmpty(defaultTemplate.ParameterType)
                && defaultTemplate.ParameterType.Contains("percent", StringComparison.OrdinalIgnoreCase))) &&
                defaultTemplate.ValidRangeFrom < 0;
        }

        public static bool IsNotPercentage(DefaultParameterTemplateMaster defaultTemplate)
        {
            return !string.IsNullOrEmpty(defaultTemplate.ParameterCategory)
                && !defaultTemplate.ParameterCategory.Contains("percent", StringComparison.OrdinalIgnoreCase)
                && !defaultTemplate.ParameterType.Contains("percent", StringComparison.OrdinalIgnoreCase);
        }

        public static CreateLapcapDataErrorDto CreateLapcapDataErrorDto(
            string country,
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
                UniqueReference = uniqueReference,
            };
        }

        /// <summary>
        /// Extracts the first year from a financial year string in the format "YYYY-YY".
        /// </summary>
        /// <param name="value">The financial year string to parse, in the format "YYYY-YY".</param>
        /// <returns>The first year as a string.</returns>
        /// <exception cref="FormatException">Thrown when the format is invalid.</exception>
        public static string GetFinancialYearAsYYYY(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Financial year cannot be null or empty", nameof(value));
            }

            string pattern = @"^\d{4}-\d{2}$";
            TimeSpan regexTimeout = TimeSpan.FromSeconds(1);
            if (!Regex.IsMatch(value, pattern, RegexOptions.None, regexTimeout))
            {
                throw new FormatException("Financial year format is invalid. Expected format is 'YYYY-YY'.");
            }

            var years = value.Split('-');
            return years[0];
        }

        /// <summary>
        /// Converts a financial year string to the previous calendar year as a string.
        /// </summary>
        /// <param name="financialYear">The financial year string to convert, in the format "YYYY-YY".</param>
        /// <returns>The previous calendar year as a string.</returns>
        /// <exception cref="ArgumentException">Thrown when the financial year string is null or empty.</exception>
        public static string GetCalendarYear(string financialYear)
        {
            if (string.IsNullOrWhiteSpace(financialYear))
            {
                throw new ArgumentException("Financial year cannot be null or empty", nameof(financialYear));
            }

            int year = int.Parse(GetFinancialYearAsYYYY(financialYear));
            return (year - 1).ToString();
        }

        public static FormattableString GetFormattedSqlString(string procedureName, int runId, string calendarYear, string createdBy)
        {
            return $"exec {procedureName} @RunId ={runId}, @calendarYear = {calendarYear}, @createdBy = {createdBy}";
        }

        public static FormattableString GetFormattedSqlString(string procedureName, string? instructionConfirmedBy, DateTime? instructionConfirmedDate, int runId)
        {
            return $"exec {procedureName} @instructionConfirmedBy = {instructionConfirmedBy}, @instructionConfirmedDate = {instructionConfirmedDate}, @calculatorRunID ={runId}";
        }

        public static IEnumerable<int> AcceptableRunStatusForBillingInstructions()
        {
            var validRunClassifications = new List<int>
                {
                    (int)RunClassification.INITIAL_RUN,
                    (int)RunClassification.INTERIM_RECALCULATION_RUN,
                    (int)RunClassification.FINAL_RUN,
                    (int)RunClassification.FINAL_RECALCULATION_RUN,
                };

            return validRunClassifications;
        }
    }
}
