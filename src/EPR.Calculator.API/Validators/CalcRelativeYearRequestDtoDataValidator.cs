using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Validators;

public class CalcRelativeYearRequestDtoDataValidator : ICalcRelativeYearRequestDtoDataValidator
{
    private readonly ApplicationDBContext context;

    public CalcRelativeYearRequestDtoDataValidator(ApplicationDBContext context)
    {
        this.context = context;
    }

    public async Task<ValidationResultDto<ErrorDto>> Validate(CalcRelativeYearRequestDto request, CancellationToken cancellationToken = default)
    {
        var validationResult = new ValidationResultDto<ErrorDto>();

        // Check if relativeYear exists in the database
        var dbYear = await this.context.FindRelativeYearAsync(request.RelativeYearValue);
        if (dbYear == null)
        {
            validationResult.IsInvalid = true;
            validationResult.Errors.Add(new ErrorDto
            {
                Message = CommonResources.RelativeYearNotInDatabase,
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

        if (currentRun.RelativeYear.Value != request.RelativeYearValue)
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