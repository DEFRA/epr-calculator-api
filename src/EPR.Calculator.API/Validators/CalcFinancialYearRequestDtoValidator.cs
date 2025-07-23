using EPR.Calculator.API.Dtos;
using FluentValidation;

namespace EPR.Calculator.API.Validators
{
    public class CalcFinancialYearRequestDtoValidator : AbstractValidator<CalcFinancialYearRequestDto>
    {
        public CalcFinancialYearRequestDtoValidator()
        {
            RuleFor(x => x.FinancialYear)
                .NotEmpty()
                .WithMessage(CommonResources.FinancialYearRequired)
                .Matches(@"^\d{4}-\d{2}$")
                .WithMessage(CommonResources.InvalidFinancialYearFormat);
        }
    }
}
