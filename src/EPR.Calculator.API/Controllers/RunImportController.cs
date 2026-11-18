using EPR.Calculator.API.BackgroundService.Features.RunImport;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.API.Controllers;

[ApiController]
[Produces("application/json")]
[Route("v1")]
public class RunImportController(
    IRunImporter runImporter,
    ApplicationDBContext context,
    IOptions<FeatureFlagOptions> featureFlags
) : ControllerBase
{
    // The largest billing CSV is around 40 MB, over Kestrel's default 30 MB limit.
    private const long MaxFileSize = 200 * 1024 * 1024;

    /// <summary>
    ///     Stores a run from its billing CSV so its files can be downloaded from the database. Runs in a year must be
    ///     imported in order: the first is classified Initial and the rest Recalculation, unless a classification is given.
    /// </summary>
    [HttpPost]
    [Route("runs/import")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxFileSize)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxFileSize)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Import(
        IFormFile billingFile,
        [FromForm] RunClassification? classification,
        [FromForm] string? billingFileCreatedBy,
        CancellationToken cancellationToken)
    {
        if (!featureFlags.Value.RunImport)
            return NotFound();

        BillingFileName fileName;
        try
        {
            fileName = BillingFileName.Parse(billingFile.FileName);
        }
        catch (FormatException ex)
        {
            return BadRequest(ex.Message);
        }

        if (await context.CalculatorRuns.AnyAsync(r => r.Id == fileName.RunId, cancellationToken))
            return Conflict($"Run {fileName.RunId} already exists");

        var details = new RunImportDetails { Classification = classification, BillingFileCreatedBy = billingFileCreatedBy };

        try
        {
            using var reader = new StreamReader(billingFile.OpenReadStream());
            var runId = await runImporter.ImportAsync(reader, billingFile.FileName, details, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, new { runId });
        }
        catch (FormatException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
