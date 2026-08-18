using System.ComponentModel.DataAnnotations;
using EPR.Calculator.API.BackgroundService.Features.Common;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Extensions;
using EPR.Calculator.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.API.Controllers;

[ApiController]
[Produces("application/json")]
[Route("v2")]
public class BillingFileNewController(
    IBillingFileService billingFileService,
    IFileExportService fileExportService
) : ControllerBase
{
    /// <summary>
    ///     Accept or Reject billing for the matching list of organisation id and run id.
    /// </summary>
    /// <param name="runId">Run Id passed in the URL.</param>
    /// <param name="produceBillingInstuctionRequestDto">Organisation id and status passed as request body.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>Http response on success or failure.</returns>
    [HttpPut]
    [Route("producerBillingInstructions/{runId}")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ProducerBillingInstructions(
        int runId,
        [FromBody] [Required] ProduceBillingInstuctionRequestDto produceBillingInstuctionRequestDto,
        CancellationToken cancellationToken = default)
    {
        var serviceProcessResponseDto = await billingFileService.UpdateProducerBillingInstructionsAsync(
            runId,
            User.GetName(),
            produceBillingInstuctionRequestDto,
            cancellationToken).ConfigureAwait(false);

        return new ObjectResult(serviceProcessResponseDto.Message)
        {
            StatusCode = (int)serviceProcessResponseDto.StatusCode
        };
    }
  
    [HttpGet]
    [Route("downloadBillingCsv/{runId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadBillingCsv(int runId, CancellationToken cancellationToken = default)
    {
        var export = await fileExportService.Export(runId, RunType.Billing, FileExportType.Csv, cancellationToken);

        if (export is null)
            return NotFound();

        return File(export.Content, "text/csv", export.FileName);
    }

    [HttpGet]
    [Route("downloadBillingJson/{runId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadBillingJson(int runId, CancellationToken cancellationToken = default)
    {
        var export = await fileExportService.Export(runId, RunType.Billing, FileExportType.Json, cancellationToken);

        if (export is null)
            return NotFound();

        return File(export.Content, "application/json", export.FileName);
    }
}
