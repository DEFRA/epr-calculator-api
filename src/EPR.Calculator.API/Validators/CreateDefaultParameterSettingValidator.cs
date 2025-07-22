using EPR.Calculator.API.Dtos;
using FluentValidation;

namespace EPR.Calculator.API.Validators
{
    public partial class CreateDefaultParameterSettingValidator : AbstractValidator<CreateDefaultParameterSettingDto>
    {
        public CreateDefaultParameterSettingValidator()
        {
            this.RuleFor(x => x.ParameterYear)
                .NotEmpty()
                .WithMessage(CommonResources.YearRequired);
            this.RuleFor(x => x.SchemeParameterTemplateValues)
                .NotNull()
                .Must(x => x.Count() == CommonResources.DefaultParameterUniqueReferences.Split(',').Length)
                .WithMessage(string.Format(CommonResources.SchemeParameterTemplateValuesMissing, CommonResources.DefaultParameterUniqueReferences.Split(',').Length));
            this.RuleFor(x => x.ParameterFileName)
                .NotEmpty()
                .WithMessage(CommonResources.FileNameRequired);
            this.RuleFor(x => x.ParameterFileName)
                .MaximumLength(256)
                .WithMessage(CommonResources.MaxFileNameLength);
        }
    }
}