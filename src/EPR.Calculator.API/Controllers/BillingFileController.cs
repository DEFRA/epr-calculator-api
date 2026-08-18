using EPR.Calculator.API.BackgroundService.Features.Common;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Controllers;

[ApiController]
[Produces("application/json")]
[Route("v1")]
public class BillingFileController(
    IBlobStorageService blobStorage,
    IFileExportService fileExportService,
    ApplicationDBContext context
) : ControllerBase
{
    [HttpGet]
    [Route("downloadBillingCsv/{runId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadBillingCsv(int runId, CancellationToken cancellationToken = default)
    {
        return await fileExportService.Export(runId, RunType.Billing, FileExportType.Csv, cancellationToken) switch
        {
            FileExportResult.Exported s => File(s.Content, "text/csv", s.FileName),
            FileExportResult.NotFound _ => NotFound(),
            FileExportResult.NotCached _ => await DownloadBillingCsvFromBlobStorage(runId),
            _ => throw new InvalidOperationException($"Unexpected {nameof(FileExportResult)}")
        };
    }

    [HttpGet]
    [Route("downloadBillingJson/{runId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadBillingJson(int runId, CancellationToken cancellationToken = default)
    {
        return await fileExportService.Export(runId, RunType.Billing, FileExportType.Json, cancellationToken) switch
        {
            FileExportResult.Exported s => File(s.Content, "application/json", s.FileName),
            FileExportResult.NotFound _ => NotFound(),
            FileExportResult.NotCached _ => await DownloadBillingJsonFromBlobStorage(runId),
            _ => throw new InvalidOperationException($"Unexpected {nameof(FileExportResult)}")
        };
    }

    private async Task<IActionResult> DownloadBillingCsvFromBlobStorage(int runId)
    {
        var latestBillingFileMetaData = await context.CalculatorRunBillingFileMetadata
            .Where(x => x.CalculatorRunId == runId)
            .OrderByDescending(x => x.BillingFileCreatedDate)
            .FirstOrDefaultAsync();

        if (latestBillingFileMetaData == null)
            return NotFound(string.Format(CommonResources.NoBillingFileMetadataForRunId, runId));

        if (string.IsNullOrEmpty(latestBillingFileMetaData.BillingCsvFileName))
            return NotFound(string.Format(CommonResources.NoBillingFileMetadataForRunId, runId));

        var csvFileMetadata = await context.CalculatorRunCsvFileMetadata
            .Where(x =>
                x.CalculatorRunId == runId
                && x.FileName == latestBillingFileMetaData.BillingCsvFileName)
            .SingleOrDefaultAsync();

        if (csvFileMetadata == null)
            return NotFound(string.Format(CommonResources.NoBillingFileMetadataForRunId, runId));

        var stream = await blobStorage.OpenBillingCsvStream(csvFileMetadata.FileName);

        if (stream == null)
            return NotFound(string.Format(CommonResources.NoBillingFileMetadataForRunId, runId));

        return File(stream, "text/csv", csvFileMetadata.FileName);
    }

    private async Task<IActionResult> DownloadBillingJsonFromBlobStorage(int runId)
    {
        var latestBillingFileMetaData = await context.CalculatorRunBillingFileMetadata
            .Where(x => x.CalculatorRunId == runId)
            .OrderByDescending(x => x.BillingFileCreatedDate)
            .FirstOrDefaultAsync();

        if (latestBillingFileMetaData == null)
            return NotFound(string.Format(CommonResources.NoBillingFileMetadataForRunId, runId));

        var billingJsonFileName = latestBillingFileMetaData.BillingJsonFileName;

        if (string.IsNullOrEmpty(billingJsonFileName))
            return NotFound(string.Format(CommonResources.NoBillingFileMetadataForRunId, runId));

        var stream = await blobStorage.OpenBillingJsonStream(billingJsonFileName);

        if (stream == null)
            return NotFound(string.Format(CommonResources.NoBillingFileMetadataForRunId, runId));

        return File(stream, "application/json", billingJsonFileName);
    }
}
