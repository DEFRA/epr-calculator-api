using EPR.Calculator.API.Dtos;
using FluentValidation;

namespace EPR.Calculator.API.Validators
{
    public class CalcResultsRequestDtoValidator: AbstractValidator<CalcResultsRequestDto>
    {
        public const string ErrorMessage = "RunId should have a value greater than 0";

        public CalcResultsRequestDtoValidator()
        {
            this.RuleFor(x => x.RunId).GreaterThan(0).WithMessage(ErrorMessage);
        }
    }
}
