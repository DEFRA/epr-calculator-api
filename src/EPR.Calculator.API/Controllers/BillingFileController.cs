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
    ApplicationDBContext context
) : ControllerBase
{
    /// <summary>
    ///     Downloads the CSV billing file for the specified calculator run.
    /// </summary>
    /// <param name="runId">The unique identifier of the calculator run.</param>
    /// <returns>The CSV billing file as a downloadable response, or an error result if not found or invalid.</returns>
    /// <response code="200">Returns the CSV billing file for the specified run.</response>
    /// <response code="400">Returned when the request is invalid.</response>
    /// <response code="404">Returned when the billing file is not found for the specified run id.</response>
    [HttpGet]
    [Route("downloadBillingFile/{runId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IResult> DownloadCsvBillingFile(int runId)
    {
        var latestBillingFileMetaData = await context.CalculatorRunBillingFileMetadata
            .Where(x => x.CalculatorRunId == runId)
            .OrderByDescending(x => x.BillingFileCreatedDate)
            .FirstOrDefaultAsync();

        if (latestBillingFileMetaData == null)
            return Results.NotFound(string.Format(CommonResources.NoBillingFileMetadataForRunId, runId));

        if (string.IsNullOrEmpty(latestBillingFileMetaData.BillingCsvFileName))
            return Results.NotFound(string.Format(CommonResources.NoBillingFileMetadataForRunId, runId));

        var csvFileMetadata = await context.CalculatorRunCsvFileMetadata
            .Where(x =>
                x.CalculatorRunId == runId
                && x.FileName == latestBillingFileMetaData.BillingCsvFileName)
            .SingleOrDefaultAsync();

        if (csvFileMetadata == null)
            return Results.NotFound(string.Format(CommonResources.NoBillingFileMetadataForRunId, runId));

        var stream = await blobStorage.OpenBillingCsvStream(csvFileMetadata.FileName);

        if (stream == null)
            return Results.NotFound(string.Format(CommonResources.NoBillingFileMetadataForRunId, runId));

        return Results.File(stream, "text/csv", csvFileMetadata.FileName);
    }
}
