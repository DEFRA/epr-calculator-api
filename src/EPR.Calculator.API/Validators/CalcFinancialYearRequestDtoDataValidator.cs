using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Validators;
using Microsoft.EntityFrameworkCore;

public class CalcFinancialYearRequestDtoDataValidator : ICalcFinancialYearRequestDtoDataValidator
{
    private readonly ApplicationDBContext context;

    public CalcFinancialYearRequestDtoDataValidator(ApplicationDBContext context)
    {
        this.context = context;
    }

    public async Task<ValidationResultDto<ErrorDto>> Validate(CalcFinancialYearRequestDto request)
    {
        var validationResult = new ValidationResultDto<ErrorDto>();

        // Check if financialYear is empty
        if (string.IsNullOrWhiteSpace(request.FinancialYear))
        {
            validationResult.IsInvalid = true;
            validationResult.Errors.Add(new ErrorDto
            {
                Message = "Financial year is required.",
                Description = null,
            });
            return validationResult;
        }

        // Check if financialYear exists in the database
        var dbYear = await this.context.FinancialYears.SingleOrDefaultAsync(y => y.Name == request.FinancialYear);
        if (dbYear == null)
        {
            validationResult.IsInvalid = true;
            validationResult.Errors.Add(new ErrorDto
            {
                Message = "Financial year not found in the database.",
            });
        }

        var anyMatchingRunWithFinancialYear = await this.context.CalculatorRuns.AnyAsync(x =>
            x.FinancialYearId == request.FinancialYear
            &&
            request.RunId == x.Id);

        if (!anyMatchingRunWithFinancialYear)
        {
            validationResult.IsInvalid = true;
            validationResult.Errors.Add(new ErrorDto
            {
                Message = "No matching run found with the specified financial year.",
            });
        }

        return validationResult;
    }
}
