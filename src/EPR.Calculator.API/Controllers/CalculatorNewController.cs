using System.Collections.Immutable;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Mappers;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Services.Abstractions;
using EPR.Calculator.API.Validators;
using EPR.Calculator.Service.Function.Services;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Controllers;

[Route("V2")]
[ApiController]
public class CalculatorNewController(
    ApplicationDBContext dbContext,
    ICalculatorRunStatusDataValidator calculatorRunStatusDataValidator,
    IBillingFileService billingFileService,
    IInvoiceDetailsService invoiceDetailsService,
    TelemetryClient telemetryClient,
    ICalculationRunService calculationRunService)
    : ControllerBase
{
    [HttpPut]
    [Route("calculatorRuns")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PutCalculatorRunStatus([FromBody] CalculatorRunStatusUpdateDto runStatusUpdateDto)
    {
        var claim = User.Claims.FirstOrDefault(x => x.Type == "name");
        if (claim == null)
            return new ObjectResult(CommonResources.NoClaimInRequest) { StatusCode = StatusCodes.Status401Unauthorized };

        var userName = claim.Value;

        var classification = await dbContext.CalculatorRunClassifications.SingleOrDefaultAsync(x =>
            x.Id == runStatusUpdateDto.ClassificationId);

        if (classification == null)
        {
            return new ObjectResult(string.Format(CommonResources.UnableToFindClassificationId, runStatusUpdateDto.ClassificationId))
                { StatusCode = StatusCodes.Status422UnprocessableEntity };
        }

        var calculatorRun = await dbContext.CalculatorRuns.SingleOrDefaultAsync(x => x.Id == runStatusUpdateDto.RunId);
        if (calculatorRun == null)
        {
            return new ObjectResult(string.Format(CommonResources.UnableToFindRun, runStatusUpdateDto.RunId))
                { StatusCode = StatusCodes.Status422UnprocessableEntity };
        }

        // Perform basic validation on classification status
        var genericValidationResultDto = calculatorRunStatusDataValidator.Validate(calculatorRun, runStatusUpdateDto);

        if (genericValidationResultDto.IsInvalid)
        {
            return new ObjectResult(genericValidationResultDto.Errors)
                { StatusCode = StatusCodes.Status422UnprocessableEntity };
        }

        // Perform validation to check other designated runs are not in progress and not already completed for the same relative year
        var designatedRuns = await calculationRunService.GetDesignatedRunsByFinanialYear(calculatorRun.RelativeYear);

        genericValidationResultDto = calculatorRunStatusDataValidator.Validate(designatedRuns, calculatorRun, runStatusUpdateDto);

        if (genericValidationResultDto.IsInvalid)
        {
            return new ObjectResult(genericValidationResultDto.Errors)
                { StatusCode = StatusCodes.Status422UnprocessableEntity };
        }

        calculatorRun.CalculatorRunClassificationId = runStatusUpdateDto.ClassificationId;
        calculatorRun.UpdatedAt = DateTime.UtcNow;
        calculatorRun.UpdatedBy = userName;

        dbContext.CalculatorRuns.Update(calculatorRun);
        await dbContext.SaveChangesAsync();

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
    public async Task<ActionResult> PrepareBillingFileSendToFSS(int runId, CancellationToken cancellationToken = default)
    {
        var claim = User.Claims.FirstOrDefault(x => x.Type == "name");

        if (claim == null)
            return StatusCode(StatusCodes.Status401Unauthorized, CommonResources.NoClaimInRequest);

        var runDto = await dbContext.CalculatorRuns
            .Where(run => run.Id == runId)
            .Select(CalcRunMapper.ToDto)
            .SingleOrDefaultAsync(cancellationToken);

        if (runDto == null)
            return StatusCode(StatusCodes.Status422UnprocessableEntity, string.Format(CommonResources.UnableToFindRun, runId));

        if (runDto.BillingFile is not { IsLatest: true })
            return StatusCode(StatusCodes.Status422UnprocessableEntity, string.Format(CommonResources.BillingFileOutdated, runId));

        var validTransitions = ImmutableDictionary.CreateRange(new KeyValuePair<RunClassification, int>[]
        {
            new(RunClassification.INITIAL_RUN,               (int) RunClassification.INITIAL_RUN_COMPLETED),
            new(RunClassification.INTERIM_RECALCULATION_RUN, (int) RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED),
            new(RunClassification.FINAL_RECALCULATION_RUN,   (int) RunClassification.FINAL_RECALCULATION_RUN_COMPLETED),
            new(RunClassification.FINAL_RUN,                 (int) RunClassification.FINAL_RUN_COMPLETED)
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

            var sentBy = claim.Value;
            var sentAt = DateTime.UtcNow;

            calculatorRun.CalculatorRunClassificationId = newClassification;
            metadata.BillingFileAuthorisedBy = sentBy;
            metadata.BillingFileAuthorisedDate = sentAt;

            var affectedRows = await invoiceDetailsService
                .InsertInvoiceDetailsAtProducerLevel(runId, sentAt, sentBy, cancellationToken);

            telemetryClient.TrackEvent("InsertInvoiceDetailsAtProducerLevel", new Dictionary<string, string>
            {
                { "RunId", runId.ToString() },
                { "RowsAffected", affectedRows.ToString() }
            });

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
