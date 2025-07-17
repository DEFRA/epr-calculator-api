using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Validators;

public class CalcFinancialYearRequestDtoDataValidator : ICalcFinancialYearRequestDtoDataValidator
{
    private readonly ApplicationDBContext context;

    public CalcFinancialYearRequestDtoDataValidator(ApplicationDBContext context)
    {
        this.context = context;
    }

    public ValidationResultDto<ErrorDto> Validate(CalcFinancialYearRequestDto request)
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
        var dbYear = this.context.FinancialYears
            .AsNoTracking()
            .SingleOrDefault(y => y.Name == request.FinancialYear);

        if (dbYear == null)
        {
            validationResult.IsInvalid = true;
            validationResult.Errors.Add(new ErrorDto
            {
                Message = "Financial year not found in the database.",
            });
            return validationResult;
        }

        var currentRun = this.context.CalculatorRuns
            .AsNoTracking()
            .SingleOrDefault(run => run.Id == request.RunId);

        // Check that the run esists
        if (currentRun == null)
        {
            validationResult.IsInvalid = true;
            validationResult.Errors.Add(new ErrorDto
            {
                Message = "Run not found in the database.",
            });
            return validationResult;
        }

        // Check that the run is unclassified
        if (currentRun.CalculatorRunClassificationId != (int)RunClassification.UNCLASSIFIED)
        {
            validationResult.IsInvalid = true;
            validationResult.Errors.Add(new ErrorDto
            {
                Message = "Run is already classified.",
            });
        }

        return validationResult;
    }
}
