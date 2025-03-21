using EPR.Calculator.API.Constants;
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
                .WithMessage(ErrorMessages.YearRequired);
            this.RuleFor(x => x.SchemeParameterTemplateValues)
                .NotNull()
                .Must(x => x.Count() == DefaultParameterUniqueReferences.UniqueReferences.Length)
                .WithMessage(ErrorMessages.SchemeParameterTemplateValuesMissing);
            this.RuleFor(x => x.ParameterFileName)
                .NotEmpty()
                .WithMessage(ErrorMessages.FileNameRequired);
            this.RuleFor(x => x.ParameterFileName)
                .MaximumLength(256)
                .WithMessage(ErrorMessages.MaxFileNameLength);
        }
    }
}