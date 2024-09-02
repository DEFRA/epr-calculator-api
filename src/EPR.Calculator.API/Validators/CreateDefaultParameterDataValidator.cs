using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Utils;

namespace api.Validators
{
    public class CreateDefaultParameterDataValidator
    {
        private readonly ApplicationDBContext _context;
        public CreateDefaultParameterDataValidator(ApplicationDBContext context)
        {
            _context = context;
        }
        public ValidationResultDto Validate(IEnumerable<SchemeParameterTemplateValueDto> schemeParameterTemplateValues)
        {
            var validationResult = new ValidationResultDto();

            var errors = ValidateByValues(schemeParameterTemplateValues);
            validationResult.Errors.AddRange(errors);
            validationResult.IsInvalid = validationResult.Errors.Any();
            return validationResult;
        }

        private IEnumerable<CreateDefaultParameterSettingErrorDto> ValidateByValues(IEnumerable<SchemeParameterTemplateValueDto> schemeParameterTemplateValues)
        {
            IEnumerable<DefaultParameterTemplateMaster> defaultTemplateMasterList = this._context.DefaultParameterTemplateMasterList.ToList();

            var errors = new List<CreateDefaultParameterSettingErrorDto>();

            foreach (var defaultParameterTemplateMaster in defaultTemplateMasterList)
            {
                var matchingTemplates = schemeParameterTemplateValues.Where(x => x.ParameterUniqueReferenceId == defaultParameterTemplateMaster.ParameterUniqueReferenceId);

                if (matchingTemplates.Count() > 1)
                {
                    var errorMessage = $"Expecting only One with Parameter Type {Util.FormattedErrorForMoreThanOneUniqueRefs(defaultParameterTemplateMaster)}";
                    var error = Util.CreateErrorDto(defaultParameterTemplateMaster, errorMessage);
                    errors.Add(error);
                }
                else if (matchingTemplates.Count() == 0)
                {
                    var errorMessage = Util.FormattedErrorForMissingValues(defaultParameterTemplateMaster);
                    var error = Util.CreateErrorDto(defaultParameterTemplateMaster, errorMessage);
                    errors.Add(error);
                }
                else
                {
                    decimal parameterValue;
                    var matchingTemplate = matchingTemplates.Single();
                    var parameterValueStr = Util.GetParameterValue(defaultParameterTemplateMaster, matchingTemplate.ParameterValue);
                    if(string.IsNullOrEmpty(parameterValueStr))
                    {
                        var errorMessage = $"{Util.FormattedErrorForEmptyValue(defaultParameterTemplateMaster)}";
                        var error = Util.CreateErrorDto(defaultParameterTemplateMaster, errorMessage);
                        errors.Add(error);
                    }
                    else if (decimal.TryParse(parameterValueStr, out parameterValue))
                    {
                        if (parameterValue < defaultParameterTemplateMaster.ValidRangeFrom ||
                            parameterValue > defaultParameterTemplateMaster.ValidRangeTo)
                        {
                            var errorMessage = $"{Util.FormattedErrorForOutOfRangeValues(defaultParameterTemplateMaster)}";
                            var error = Util.CreateErrorDto(defaultParameterTemplateMaster, errorMessage);
                            errors.Add(error);
                        }
                    }
                    else
                    {
                        var errorMessage = $"{Util.FormattedErrorForNonDecimalValues(defaultParameterTemplateMaster)}";
                        var error = Util.CreateErrorDto(defaultParameterTemplateMaster, errorMessage);
                        errors.Add(error);
                    }
                }
            }

            return errors;
        }

        
    }
}