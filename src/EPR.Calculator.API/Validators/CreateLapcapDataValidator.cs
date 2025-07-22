using EPR.Calculator.API.Dtos;
using FluentValidation;

namespace EPR.Calculator.API.Validators
{
    public class CreateLapcapDataValidator : AbstractValidator<CreateLapcapDataDto>
    {
        private const int MaxFileNameSupported = 256;

        public CreateLapcapDataValidator()
        {
            this.RuleFor(x => x.ParameterYear)
                .NotEmpty()
                .WithMessage(CommonResources.YearRequired);
            this.RuleFor(x => x.LapcapDataTemplateValues).NotNull()
                .Must(x => x.Count() == CommonResources.LapcapDataUniqueReferences.Split(',').Length)
                .WithMessage(string.Format(CommonResources.LapcapDataTemplateValuesMissing, CommonResources.DefaultParameterUniqueReferences.Split(',').Length));
            this.RuleFor(x => x.LapcapFileName)
                .NotEmpty()
                .WithMessage(CommonResources.FileNameRequired);
            this.RuleFor(x => x.LapcapFileName)
                .MaximumLength(MaxFileNameSupported)
                .WithMessage(CommonResources.MaxFileNameLength);
        }
    }
}