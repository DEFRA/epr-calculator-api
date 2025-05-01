using EPR.Calculator.API.Dtos;

namespace EPR.Calculator.API.Validators
{
    public interface ICalcFinancialYearRequestDtoDataValidator
    {
        ValidationResultDto<ErrorDto> Validate(CalcFinancialYearRequestDto request);
    }
}
