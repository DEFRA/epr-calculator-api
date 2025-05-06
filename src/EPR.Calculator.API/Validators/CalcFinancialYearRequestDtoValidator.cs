using EPR.Calculator.API.Dtos;
using FluentValidation;

namespace EPR.Calculator.API.Validators
{
    public class CalcFinancialYearRequestDtoValidator : AbstractValidator<CalcFinancialYearRequestDto>
    {
        public const string ErrorMessage = "Invalid financial year format. Expected format: YYYY-YY (e.g., 2024-25).";

        public CalcFinancialYearRequestDtoValidator()
        {
            RuleFor(x => x.FinancialYear)
                .NotEmpty()
                .WithMessage("Financial year is required.")
                .Matches(@"^\d{4}-\d{2}$")
                .WithMessage(ErrorMessage);
        }
    }
}
