using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Extensions;
using EPR.Calculator.API.Mappers;
using EPR.Calculator.API.Options;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Validators;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.API.Controllers;

[ApiController]
[Produces("application/json")]
[Route("V2")]
[SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
public class CalculatorNewController(
    ApplicationDBContext dbContext,
    IRunClassificationValidator runClassificationValidator,
    IInvoiceDetailsService invoiceDetailsService,
    ILogger<CalculatorNewController> logger,
    IFileExportService fileExportService,
    IBlobStorageService blobStorage,
    IStorageUploadService storageUploadService,
    IOptions<BlobStorageOptions> blobStorageOptions,
    IOptions<FeatureFlagOptions> featureFlags
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
        // Tracked: the run is updated and saved below.
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
        // AsNoTracking() not needed: projects to a DTO.
        var runDto = await dbContext.CalculatorRuns
            .Where(run => run.Id == runId)
            .Select(CalcRunMapper.ToDto)
            .SingleOrDefaultAsync(cancellationToken);

        if (runDto == null)
            return StatusCode(StatusCodes.Status422UnprocessableEntity, string.Format(CommonResources.UnableToFindRun, runId));

        if (runDto.BillingFile is not { IsLatest: true })
            return StatusCode(StatusCodes.Status422UnprocessableEntity, string.Format(CommonResources.BillingFileOutdated, runId));

        if (runDto.BillingFile is not { IsShared: false })
            return StatusCode(StatusCodes.Status422UnprocessableEntity, string.Format(CommonResources.BillingFileAlreadyShared, runId));

        if (runDto.RunClassification is not (RunClassification.Initial or RunClassification.Recalculation))
            return StatusCode(StatusCodes.Status422UnprocessableEntity, string.Format(CommonResources.UnableToChangeStatusToCompleted, runDto.RunClassification));

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            // Tracked: the run is updated and saved below.
            var run = await dbContext.CalculatorRuns
                .SingleAsync(x => x.Id == runId, cancellationToken);

            run.IsBillingFileShared = true;
            run.BillingFileSharedBy = User.GetName();
            run.BillingFileSharedAt = DateTime.UtcNow;

            var affectedRows = await invoiceDetailsService
                .InsertInvoiceDetailsAtProducerLevel(runId, run.BillingFileSharedAt.Value, run.BillingFileSharedBy, cancellationToken);

            logger.LogDebug("Inserting {RowsAffected} invoice details at producer level for run {RunId}", affectedRows, runId);

            dbContext.CalculatorRuns.Update(run);
            await dbContext.SaveChangesAsync(cancellationToken);

            if (featureFlags.Value.UploadFssBillingFileToBlobStorage)
            {
                var success = await fileExportService.Export(runId, FileExportType.BillingJson, cancellationToken) switch
                {
                    FileExportResult.Streamed s => await UploadBillingJson(s, runId, cancellationToken),
                    FileExportResult.NotFound _ => false,
                    FileExportResult.Legacy _ => await blobStorage.MoveBillingJsonToFss(await GetBillingJsonFileName(runDto.BillingFile.Id, cancellationToken), cancellationToken),
                    _ => false
                };

                if (!success)
                    return StatusCode(StatusCodes.Status422UnprocessableEntity, string.Format("Unable to export billing file for run {0}", runId));
            }

            await transaction.CommitAsync(cancellationToken);

            return StatusCode(StatusCodes.Status202Accepted);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private Task<string> GetBillingJsonFileName(int billingFileId, CancellationToken cancellationToken) =>
        dbContext.CalculatorRunBillingFileMetadata
            .AsNoTracking()
            .Where(m => m.Id == billingFileId)
            .Select(m => m.BillingJsonFileName)
            .SingleAsync(cancellationToken);

    private async Task<bool> UploadBillingJson(FileExportResult.Streamed billingJson, int calculatorRunId, CancellationToken cancellationToken)
    {
        using var content = new MemoryStream();
        await billingJson.WriteTo(content, cancellationToken);

        await storageUploadService.UploadFileContentAsync(
            new IStorageUploadService.Request
            {
                FileName = string.Format(CultureInfo.CurrentCulture, CompositeFormat.Parse("{0}billing.json"), calculatorRunId),
                Content = content.ToArray(),
                ContainerName = blobStorageOptions.Value.FssContainer,
                Overwrite = true
            },
            cancellationToken);

        return true;
    }
}
