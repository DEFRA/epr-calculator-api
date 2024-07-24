using api.Dtos;

namespace api.Validators
{
    public class CreateDefaultParameterSettingDtoValidator
    {
        public ValidationResultDto Validate(CreateDefaultParameterSettingDto createDefaultParameterSettingDto)
        {
            var validationResult = new ValidationResultDto();

            if (string.IsNullOrEmpty(createDefaultParameterSettingDto?.ParameterYear))
            {
                var error = new ErrorDto
                {
                    Message = "ParameterYear is mandatory",
                    Description = ""
                };
                validationResult.IsInvalid = true;
            }

            if(createDefaultParameterSettingDto?.SchemeParameterTemplateValues.Count() != 41)
            {
                var error = new ErrorDto
                {
                    Message = "SchemeParameterTemplateValues should have a count of 41",
                    Description = ""
                };
                validationResult.Errors.Add(error);
                validationResult.IsInvalid = true;
            }
            return validationResult;
        }
    }
}
