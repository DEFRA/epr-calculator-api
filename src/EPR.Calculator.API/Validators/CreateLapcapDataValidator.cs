﻿using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Dtos;
using FluentValidation;

namespace EPR.Calculator.API.Validators
{
    public class CreateLapcapDataValidator : AbstractValidator<CreateLapcapDataDto>
    {
        private readonly int MaxFileNameSupported = 256;
        public CreateLapcapDataValidator()
        {
            RuleFor(x => x.ParameterYear).NotEmpty().WithMessage((ErrorMessages.YearRequired));
            RuleFor(x => x.LapcapDataTemplateValues).NotNull().Must(x => x.Count() == LapcapDataUniqueReferences.UniqueReferences.Length)
                .WithMessage((ErrorMessages.LapcapDataTemplateValuesMissing));
            RuleFor(x => x.LapcapFileName).NotEmpty().WithMessage(ErrorMessages.FileNameRequired);
            RuleFor(x => x.LapcapFileName).MaximumLength(MaxFileNameSupported).WithMessage(ErrorMessages.MaxFileNameLength);

        }
    }
}