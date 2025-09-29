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

    public async Task<ValidationResultDto<ErrorDto>> Validate(CalcFinancialYearRequestDto request, CancellationToken cancellationToken = default)
    {
        var validationResult = new ValidationResultDto<ErrorDto>();

        // Check if financialYear is empty
        if (string.IsNullOrWhiteSpace(request.FinancialYear))
        {
            validationResult.IsInvalid = true;
            validationResult.Errors.Add(new ErrorDto
            {
                Message = CommonResources.FinancialYearRequired,
                Description = null,
            });
            return validationResult;
        }

        // Check if financialYear exists in the database
        var dbYear = await this.context.FinancialYears
            .AsNoTracking()
            .SingleOrDefaultAsync(y => y.Name == request.FinancialYear, cancellationToken);

        if (dbYear == null)
        {
            validationResult.IsInvalid = true;
            validationResult.Errors.Add(new ErrorDto
            {
                Message = CommonResources.FinancialYearNotInDatabase,
            });
            return validationResult;
        }

        var currentRun = await this.context.CalculatorRuns
            .AsNoTracking()
            .SingleOrDefaultAsync(run => run.Id == request.RunId, cancellationToken);

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

        if (currentRun.FinancialYearId != request.FinancialYear)
        {
            validationResult.IsInvalid = true;
            validationResult.Errors.Add(new ErrorDto
            {
                Message = CommonResources.NoMatchingRunFound,
            });
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