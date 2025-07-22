using EPR.Calculator.API.Dtos;
using FluentValidation;

namespace EPR.Calculator.API.Validators
{
    public class CalcResultsRequestDtoValidator : AbstractValidator<CalcResultsRequestDto>
    {
        public CalcResultsRequestDtoValidator()
        {
            this.RuleFor(x => x.RunId).GreaterThan(0).WithMessage(CommonResources.RunIdGreaterThan0);
        }
    }
}
