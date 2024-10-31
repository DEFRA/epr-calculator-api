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
            RuleFor(x => x.SchemeParameterTemplateValues).NotNull().Must(x => x.Count() == DefaultParameterUniqueReferences.UniqueReferences.Length)
                .WithMessage((ErrorMessages.SchemeParameterTemplateValuesMissing));
            RuleFor(x => x.ParameterFileName).NotEmpty().WithMessage(ErrorMessages.FileNamerequired);
            RuleFor(x => x.ParameterFileName).MaximumLength(256).WithMessage(ErrorMessages.MaxFileNameLength);
        }
    }
}