using EPR.Calculator.API.Constants;
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
                .WithMessage(ErrorMessages.YearRequired);
            this.RuleFor(x => x.LapcapDataTemplateValues).NotNull()
                .Must(x => x.Count() == LapcapDataUniqueReferences.UniqueReferences.Length)
                .WithMessage(string.Format(CommonResources.LapcapDataTemplateValuesMissing, CommonResources.UniqueReferences.Split(',').Length));
            this.RuleFor(x => x.LapcapFileName)
                .NotEmpty()
                .WithMessage(ErrorMessages.FileNameRequired);
            this.RuleFor(x => x.LapcapFileName)
                .MaximumLength(MaxFileNameSupported)
                .WithMessage(ErrorMessages.MaxFileNameLength);
        }
    }
}