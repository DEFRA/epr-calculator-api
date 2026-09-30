using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Extensions;
using EPR.Calculator.API.Mappers;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Validators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Controllers;

[ApiController]
[Produces("application/json")]
[Route("V2")]
public class CalculatorNewController(
    ApplicationDBContext dbContext,
    IRunClassificationValidator runClassificationValidator,
    IBillingFileService billingFileService,
    IInvoiceDetailsService invoiceDetailsService,
    ILogger<CalculatorNewController> logger
) : ControllerBase
{
    [HttpPut]
    [Route("calculatorRuns")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PutCalculatorRunStatus([FromBody] SetRunClassificationRequest request, CancellationToken cancellationToken = default)
    {
        var run = await dbContext.CalculatorRuns.SingleOrDefaultAsync(x => x.Id == request.RunId, cancellationToken: cancellationToken);

        if (run == null)
        {
            return new ObjectResult(string.Format(CommonResources.UnableToFindRun, request.RunId))
                { StatusCode = StatusCodes.Status422UnprocessableEntity };
        }

        var validationResult = await runClassificationValidator.ValidateAsync(run, request.Classification, cancellationToken);

        if (validationResult.IsInvalid)
            return new ObjectResult(validationResult.Errors) { StatusCode = StatusCodes.Status422UnprocessableEntity };

        run.Classification = request.Classification;
        run.UpdatedAt = DateTime.UtcNow;
        run.UpdatedBy = User.GetName();

        dbContext.CalculatorRuns.Update(run);
        await dbContext.SaveChangesAsync(cancellationToken);

        return StatusCode(201);
    }

    [HttpPost]
    [Route("prepareBillingFileSendToFSS/{runId}")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PrepareBillingFileSendToFSS(int runId, CancellationToken cancellationToken = default)
    {
        var runDto = await dbContext.CalculatorRuns
            .Where(run => run.Id == runId)
            .Select(CalcRunMapper.ToDto)
            .SingleOrDefaultAsync(cancellationToken);

        if (runDto == null)
            return StatusCode(StatusCodes.Status422UnprocessableEntity, string.Format(CommonResources.UnableToFindRun, runId));

        if (runDto.BillingFile is not { IsLatest: true })
            return StatusCode(StatusCodes.Status422UnprocessableEntity, string.Format(CommonResources.BillingFileOutdated, runId));

        var validTransitions = ImmutableDictionary.CreateRange(new KeyValuePair<RunClassification, RunClassification>[]
        {
            new(RunClassification.Initial,       RunClassification.InitialCompleted),
            new(RunClassification.Recalculation, RunClassification.RecalculationCompleted)
        });

        if (!validTransitions.TryGetValue(runDto.RunClassification, out var newClassification))
            return StatusCode(StatusCodes.Status422UnprocessableEntity, string.Format(CommonResources.UnableToChangeStatusToCompleted, runDto.RunClassification));

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var calculatorRun = await dbContext.CalculatorRuns
                .Include(run => run.CalculatorRunBillingFileMetadata)
                .SingleAsync(x => x.Id == runId, cancellationToken);

            var metadata = calculatorRun.CalculatorRunBillingFileMetadata
                .Single(m => m.Id == runDto.BillingFile.Id);

            var sentBy = User.GetName();
            var sentAt = DateTime.UtcNow;

            calculatorRun.Classification = newClassification;
            metadata.BillingFileAuthorisedBy = sentBy;
            metadata.BillingFileAuthorisedDate = sentAt;

            var affectedRows = await invoiceDetailsService
                .InsertInvoiceDetailsAtProducerLevel(runId, sentAt, sentBy, cancellationToken);

            logger.LogDebug("Inserting {RowsAffected} invoice details at producer level for run {RunId}", affectedRows, runId);

            dbContext.CalculatorRuns.Update(calculatorRun);
            await dbContext.SaveChangesAsync(cancellationToken);

            var result = await billingFileService.MoveBillingJsonFile(runId, cancellationToken);

            if (!result)
                return StatusCode(StatusCodes.Status422UnprocessableEntity, string.Format(CommonResources.UnableToMoveBillingFile, runId));

            await transaction.CommitAsync(cancellationToken);

            return StatusCode(StatusCodes.Status202Accepted);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
