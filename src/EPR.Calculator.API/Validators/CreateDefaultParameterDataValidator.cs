using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using System.Text;

namespace api.Validators
{
    public class CreateDefaultParameterDataValidator
    {
        private readonly ApplicationDBContext _context;
        public CreateDefaultParameterDataValidator(ApplicationDBContext context)
        {
            _context = context;
        }
        public ValidationResultDto Validate(CreateDefaultParameterSettingDto createDefaultParameterSettingDto)
        {
            var validationResult = new ValidationResultDto();

            var errors = ValidateByValues(createDefaultParameterSettingDto);
            validationResult.Errors.AddRange(errors);
            validationResult.IsInvalid = validationResult.Errors.Any();
            return validationResult;
        }

        private IEnumerable<CreateDefaultParameterSettingErrorDto> ValidateByValues(CreateDefaultParameterSettingDto createDefaultParameterSettingDto)
        {
            IEnumerable<DefaultParameterTemplateMaster> defaultTemplateMasterList = this._context.DefaultParameterTemplateMasterList.ToList();

            var errors = new List<CreateDefaultParameterSettingErrorDto>();
            var schemeParameterTemplateValues = createDefaultParameterSettingDto.SchemeParameterTemplateValues;

            foreach (var defaultParameterTemplateMaster in defaultTemplateMasterList)
            {
                var matchingTemplates = schemeParameterTemplateValues.Where(x => x.ParameterUniqueReferenceId == defaultParameterTemplateMaster.ParameterUniqueReferenceId);

                if (matchingTemplates.Count() > 1)
                {
                    var errorMessage = $"Expecting only One with Parameter Type {this.FormattedErrorForMoreThanOneUniqueRefs(defaultParameterTemplateMaster)}";
                    var error = this.CreateErrorDto(defaultParameterTemplateMaster, errorMessage);
                    errors.Add(error);
                }
                else if (matchingTemplates.Count() == 0)
                {
                    var errorMessage = this.FormattedErrorForMissingValues(defaultParameterTemplateMaster);
                    var error = this.CreateErrorDto(defaultParameterTemplateMaster, errorMessage);
                    errors.Add(error);
                }
                else
                {
                    decimal parameterValue;
                    var matchingTemplate = matchingTemplates.Single();
                    var parameterValueStr = this.GetParameterValue(defaultParameterTemplateMaster, matchingTemplate.ParameterValue);
                    if (decimal.TryParse(parameterValueStr, out parameterValue))
                    {
                        if (parameterValue < defaultParameterTemplateMaster.ValidRangeFrom ||
                            parameterValue > defaultParameterTemplateMaster.ValidRangeTo)
                        {
                            var errorMessage = $"{this.FormattedErrorForOutOfRangeValues(defaultParameterTemplateMaster)}";
                            var error = this.CreateErrorDto(defaultParameterTemplateMaster, errorMessage);
                            errors.Add(error);
                        }
                    }
                    else
                    {
                        var errorMessage = $"{this.FormattedErrorForNonDecimalValues(defaultParameterTemplateMaster)}";
                        var error = this.CreateErrorDto(defaultParameterTemplateMaster, errorMessage);
                        errors.Add(error);
                    }
                }
            }

            return errors;
        }

        private CreateDefaultParameterSettingErrorDto CreateErrorDto(DefaultParameterTemplateMaster template, string errorMessage)
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

        private string GetParameterValue(DefaultParameterTemplateMaster defaultTemplate, string parameterValue)
        {
            return IsNotPercentage(defaultTemplate) ? parameterValue.TrimStart('£') : parameterValue.TrimEnd('%');
        }

        private string FormattedErrorForMoreThanOneUniqueRefs(DefaultParameterTemplateMaster defaultParameterTemplateMaster)
        {
            var sb = new StringBuilder();
            sb.Append($"Parameter Type {defaultParameterTemplateMaster.ParameterType} ");
            sb.Append($"and Parameter Category {defaultParameterTemplateMaster.ParameterCategory} ");
            sb.Append($"and Parameter Unique ref {defaultParameterTemplateMaster.ParameterUniqueReferenceId}");
            return sb.ToString();
        }

        private string FormattedErrorForNonDecimalValues(DefaultParameterTemplateMaster defaulTemplate)
        {
            var sb = new StringBuilder();
            if (IsNotPercentage(defaulTemplate))
            {
                sb.Append($"{defaulTemplate.ParameterType} for {defaulTemplate.ParameterCategory} ");
                sb.Append($"can only include numbers, commas and decimal points.");
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

        private string FormattedErrorForMissingValues(DefaultParameterTemplateMaster defaulTemplate)
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

        private string FormattedErrorForOutOfRangeValues(DefaultParameterTemplateMaster defaulTemplate)
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
                sb.Append($"must be between {defaulTemplate.ValidRangeFrom} and {defaulTemplate.ValidRangeTo}");
            }
            else
            {
                sb.Append($"The {defaulTemplate.ParameterType} percentage decrease ");
                sb.Append($"must be between {defaulTemplate.ValidRangeFrom} and {defaulTemplate.ValidRangeTo}");
            }

            return sb.ToString();
        }

        private bool IsPercentageIncrease(DefaultParameterTemplateMaster defaultTemplate)
        {
            return ((!string.IsNullOrEmpty(defaultTemplate.ParameterCategory)
                && defaultTemplate.ParameterCategory.ToLower().Contains("percent")) ||
                (!string.IsNullOrEmpty(defaultTemplate.ParameterType)
                && defaultTemplate.ParameterType.ToLower().Contains("percent"))) &&
                defaultTemplate.ValidRangeFrom >= 0;
        }

        private bool IsNotPercentage(DefaultParameterTemplateMaster defaultTemplate)
        {
            return !string.IsNullOrEmpty(defaultTemplate.ParameterCategory)
                && !defaultTemplate.ParameterCategory.ToLower().Contains("percent")
                && !defaultTemplate.ParameterType.ToLower().Contains("percent");
        }
    }
}