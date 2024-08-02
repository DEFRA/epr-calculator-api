using EPR.Calculator.API.Dtos;
using FluentValidation;

namespace EPR.Calculator.API.Validators
{
    public partial class CreateDefaultParameterSettingValidator
    {
        public class SchemeParameterTemplateValueValidator : AbstractValidator<SchemeParameterTemplateValueDto>
        {
            public SchemeParameterTemplateValueValidator()
            {
                RuleFor(x => x.ParameterValue).NotEmpty().WithMessage("ParameterValue is missing");
                RuleFor(x => x.ParameterUniqueReferenceId).NotEmpty().WithMessage("ParameterUniqueReferenceId is missing");
            }
        }
    }
}
