using EPR.Calculator.API.Dtos;

namespace EPR.Calculator.API.Validators
{
    public interface ICalcRelativeYearRequestDtoDataValidator
    {
        Task<ValidationResultDto<ErrorDto>> Validate(CalcRelativeYearRequestDto request, CancellationToken cancellationToken = default);
    }
}
