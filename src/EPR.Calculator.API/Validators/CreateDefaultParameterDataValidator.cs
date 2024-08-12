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
                    var error = new CreateDefaultParameterSettingErrorDto
                    {
                        ParameterUniqueRef = defaultParameterTemplateMaster.ParameterUniqueReferenceId,
                        ParameterType = defaultParameterTemplateMaster.ParameterType,
                        ParameterCategory = defaultParameterTemplateMaster.ParameterCategory,
                        Message = $"Expecting only One with Parameter Type {this.FormattedErrorString(defaultParameterTemplateMaster)}",
                        Description = ""
                    };
                    errors.Add(error);
                }
                else if (matchingTemplates.Count() == 0)
                {
                    var error = new CreateDefaultParameterSettingErrorDto
                    {
                        ParameterUniqueRef = defaultParameterTemplateMaster.ParameterUniqueReferenceId,
                        ParameterType = defaultParameterTemplateMaster.ParameterType,
                        ParameterCategory = defaultParameterTemplateMaster.ParameterCategory,
                        Message = $"Expecting at least One with {this.FormattedErrorString(defaultParameterTemplateMaster)}",
                        Description = ""
                    };
                    errors.Add(error);
                }
                else
                {
                    var matchingTemplate = matchingTemplates.Single();
                    if (matchingTemplate.ParameterValue < defaultParameterTemplateMaster.ValidRangeFrom ||
                        matchingTemplate.ParameterValue > defaultParameterTemplateMaster.ValidRangeTo)
                    {
                        var error = new CreateDefaultParameterSettingErrorDto
                        {
                            ParameterUniqueRef = defaultParameterTemplateMaster.ParameterUniqueReferenceId,
                            ParameterType = defaultParameterTemplateMaster.ParameterType,
                            ParameterCategory = defaultParameterTemplateMaster.ParameterCategory,
                            Message = $"{this.FormattedErrorStringForValues(defaultParameterTemplateMaster)}",
                            Description = ""
                        };
                        errors.Add(error);
                    }
                }
            }

            return errors;
        }

        private string FormattedErrorString(DefaultParameterTemplateMaster defaultParameterTemplateMaster)
        {
            var sb = new StringBuilder();
            sb.Append($"Parameter Type {defaultParameterTemplateMaster.ParameterType} ");
            sb.Append($"and Parameter Category {defaultParameterTemplateMaster.ParameterCategory} ");
            sb.Append($"and Parameter Unique ref {defaultParameterTemplateMaster.ParameterUniqueReferenceId}");
            return sb.ToString();
        }

        private string FormattedErrorStringForValues(DefaultParameterTemplateMaster defaulTemplate)
        {
            var sb = new StringBuilder();
            if (IsNotPercentage(defaulTemplate))
            {
                sb.Append($"{defaulTemplate.ParameterType} for {defaulTemplate.ParameterCategory} ");
                sb.Append($"must be between {defaulTemplate.ValidRangeFrom} and {defaulTemplate.ValidRangeTo}");
            }
            else if(IsPercentageIncrease(defaulTemplate))
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