using EPR.Calculator.API.Dtos;
using FluentValidation;

namespace EPR.Calculator.API.Validators
{
    public partial class CreateDefaultParameterSettingValidator : AbstractValidator<CreateDefaultParameterSettingDto>
    {
        public CreateDefaultParameterSettingValidator()
        {
            this.RuleFor(x => x.RelativeYear)
                .NotEmpty()
                .WithMessage(CommonResources.RelativeYearRequired);

            this.RuleFor(x => x.SchemeParameterTemplateValues)
                .NotNull();

            this.RuleFor(x => x.ParameterFileName)
                .NotEmpty()
                .WithMessage(CommonResources.FileNameRequired);

            this.RuleFor(x => x.ParameterFileName)
                .MaximumLength(256)
                .WithMessage(CommonResources.MaxFileNameLength);
        }
    }
}
