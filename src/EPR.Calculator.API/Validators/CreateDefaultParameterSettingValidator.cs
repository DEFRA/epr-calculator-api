using EPR.Calculator.API.Dtos;
using FluentValidation;

namespace EPR.Calculator.API.Validators
{
    public class CreateDefaultParameterSettingValidator : AbstractValidator<CreateDefaultParameterSettingDto>
    {
        public CreateDefaultParameterSettingValidator() 
        {
            RuleFor(x => x.ParameterYear).NotEmpty().WithMessage("Parameter Year is required");
            RuleFor(x => x.SchemeParameterTemplateValues).NotNull().Must(x => x.Count() == 41)
                .WithMessage("SchemeParameterTemplateValues should have a count of 41");
        }
    }
}
