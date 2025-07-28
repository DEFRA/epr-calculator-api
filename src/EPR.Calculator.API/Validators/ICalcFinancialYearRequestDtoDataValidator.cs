using EPR.Calculator.API.Dtos;

namespace EPR.Calculator.API.Validators
{
    public interface ICalcFinancialYearRequestDtoDataValidator
    {
        Task<ValidationResultDto<ErrorDto>> Validate(CalcFinancialYearRequestDto request, CancellationToken cancellationToken = default);
    }
}
