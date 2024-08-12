using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Dtos;
using FluentValidation;

namespace EPR.Calculator.API.Validators
{
    public partial class CreateDefaultParameterSettingValidator : AbstractValidator<CreateDefaultParameterSettingDto>
    {
        public CreateDefaultParameterSettingValidator() 
        {
            RuleFor(x => x.ParameterYear).NotEmpty().WithMessage((ErrorMessages.YearRequired));
            RuleFor(x => x.SchemeParameterTemplateValues).NotNull().Must(x => x.Count() == CommonConstants.TemplateCount)
                .WithMessage((ErrorMessages.SchemeParameterTemplateValuesMissing));
            RuleFor(x => x.SchemeParameterTemplateValues).ForEach(x => x.SetValidator(new SchemeParameterTemplateValueValidator()));
        }
    }
}