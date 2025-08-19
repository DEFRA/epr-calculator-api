using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Utils;

namespace EPR.Calculator.API.Validators
{
    public class CreateDefaultParameterDataValidator : ICreateDefaultParameterDataValidator
    {
        private readonly ApplicationDBContext context;

        public CreateDefaultParameterDataValidator(ApplicationDBContext context)
        {
            this.context = context;
        }

        public ValidationResultDto<CreateDefaultParameterSettingErrorDto> Validate(CreateDefaultParameterSettingDto createDefaultParameterSettingDto)
        {
            var validationResult = new ValidationResultDto<CreateDefaultParameterSettingErrorDto>();

            var errors = this.ValidateByValues(createDefaultParameterSettingDto);
            validationResult.Errors.AddRange(errors);
            validationResult.IsInvalid = validationResult.Errors.Count != 0;
            return validationResult;
        }

        private List<CreateDefaultParameterSettingErrorDto> ValidateByValues(CreateDefaultParameterSettingDto createDefaultParameterSettingDto)
        {
            IEnumerable<DefaultParameterTemplateMaster> defaultTemplateMasterList = this.context.DefaultParameterTemplateMasterList.ToList();

            var errors = new List<CreateDefaultParameterSettingErrorDto>();
            var schemeParameterTemplateValues = createDefaultParameterSettingDto.SchemeParameterTemplateValues;

            foreach (var defaultParameterTemplateMaster in defaultTemplateMasterList)
            {
                var matchingTemplates = schemeParameterTemplateValues.Where(x => x.ParameterUniqueReferenceId == defaultParameterTemplateMaster.ParameterUniqueReferenceId);

                if (matchingTemplates.Count() > 1)
                {
                    var errorMessage = string.Format(CommonResources.ExpectingParameterType, Util.FormattedErrorForMoreThanOneUniqueRefs(defaultParameterTemplateMaster));
                    var error = Util.CreateErrorDto(defaultParameterTemplateMaster, errorMessage);
                    errors.Add(error);
                }
                else if (!matchingTemplates.Any())
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
                    if (string.IsNullOrEmpty(parameterValueStr))
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