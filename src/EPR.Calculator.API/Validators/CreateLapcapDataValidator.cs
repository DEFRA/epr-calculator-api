using EPR.Calculator.API.Dtos;
using FluentValidation;

namespace EPR.Calculator.API.Validators
{
    public class CreateLapcapDataValidator : AbstractValidator<CreateLapcapDataDto>
    {
        public CreateLapcapDataValidator()
        {
            this.RuleFor(x => x.ParameterYear)
                .NotEmpty()
                .WithMessage(CommonResources.ParameterYearRequired);
            this.RuleFor(x => x.LapcapDataTemplateValues).NotNull()
                .Must(x => x.Count() == CommonResources.LapcapDataUniqueReferences.Split(',').Length)
                .WithMessage(string.Format(CommonResources.LapcapDataTemplateValuesMissing, CommonResources.DefaultParameterUniqueReferences.Split(',').Length));
            this.RuleFor(x => x.LapcapFileName)
                .NotEmpty()
                .WithMessage(CommonResources.FileNameRequired);
            this.RuleFor(x => x.LapcapFileName)
                .MaximumLength(int.TryParse(CommonResources.MaxFileNameSupported, out int maxFileNameSupported) ? maxFileNameSupported : 256)
                .WithMessage(CommonResources.MaxFileNameLength);
        }
    }
}