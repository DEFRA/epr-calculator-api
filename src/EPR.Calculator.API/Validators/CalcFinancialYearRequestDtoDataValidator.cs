using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Validators;
using Microsoft.EntityFrameworkCore;

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
        var dbYear = this.context.FinancialYears.SingleOrDefault(y => y.Name == request.FinancialYear);
        if (dbYear == null)
        {
            validationResult.IsInvalid = true;
            validationResult.Errors.Add(new ErrorDto
            {
                Message = "Financial year not found in the database.",
            });
        }

        // Check that the current run is unclassified
        var currentRun = this.context.CalculatorRuns
            .Where(run => run.Id == request.RunId)
            .AsNoTracking()
            .SingleOrDefault();

        if (currentRun?.CalculatorRunClassificationId != (int)RunClassification.UNCLASSIFIED)
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
