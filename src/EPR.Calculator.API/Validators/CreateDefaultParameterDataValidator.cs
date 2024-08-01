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

        private IEnumerable<ErrorDto> ValidateByValues(CreateDefaultParameterSettingDto createDefaultParameterSettingDto)
        {
            IEnumerable<DefaultParameterTemplateMaster> defaultTemplateMasterList = this._context.DefaultParameterTemplateMasterList.ToList();

            var errors = new List<ErrorDto>();
            var schemeParameterTemplateValues = createDefaultParameterSettingDto.SchemeParameterTemplateValues;

            foreach (var defaultParameterTemplateMaster in defaultTemplateMasterList)
            {
                var matchingTemplates = schemeParameterTemplateValues.Where(x => x.ParameterUniqueReferenceId == defaultParameterTemplateMaster.ParameterUniqueReferenceId);

                if (matchingTemplates.Count() > 1)
                {
                    var error = new CreateDefaultParameterSettingErrorDto
                    {
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
                        ParameterType = defaultParameterTemplateMaster.ParameterType,
                        ParameterCategory = defaultParameterTemplateMaster.ParameterCategory,
                        Message = $"Expecting at least One with Parameter Type {this.FormattedErrorString(defaultParameterTemplateMaster)}",
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
                            ParameterType = defaultParameterTemplateMaster.ParameterType,
                            ParameterCategory = defaultParameterTemplateMaster.ParameterCategory,
                            Message = $"{this.FormattedErrorStringForValues(defaultParameterTemplateMaster, matchingTemplate.ParameterValue)}",
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
            sb.Append($"Parameter Type {defaultParameterTemplateMaster.ParameterType}");
            sb.Append($"and Parameter Category {defaultParameterTemplateMaster.ParameterCategory}");
            sb.Append($"and Parameter Unique ref {defaultParameterTemplateMaster.ParameterUniqueReferenceId}");
            return sb.ToString();
        }

        private string FormattedErrorStringForValues(DefaultParameterTemplateMaster defaultParameterTemplateMaster, decimal valueProvided)
        {
            var sb = new StringBuilder();
            sb.Append($"Parameter Value too big or small. Value Provided was {valueProvided} for");
            sb.Append($"Parameter Type {defaultParameterTemplateMaster.ParameterType}");
            sb.Append($"and Parameter Category {defaultParameterTemplateMaster.ParameterCategory}");
            sb.Append($"and Parameter Unique ref {defaultParameterTemplateMaster.ParameterUniqueReferenceId}. ");
            sb.Append($"Value from is {defaultParameterTemplateMaster.ValidRangeFrom} and Value To is {defaultParameterTemplateMaster.ValidRangeTo}.");
            return sb.ToString();
        }
    }
}
