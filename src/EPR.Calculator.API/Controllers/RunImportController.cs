using EPR.Calculator.API.Services.RunImport;
using EPR.Calculator.API.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.API.Controllers;

[ApiController]
[Produces("application/json")]
[Route("v1")]
public class RunImportController(
    IRunImportService runImportService,
    IOptions<FeatureFlagOptions> featureFlags
) : ControllerBase
{
    // A zip of a year's runs can be several hundred MB uncompressed; compressed it's far smaller.
    private const long MaxFileSize = 500 * 1024 * 1024;

    /// <summary>
    ///     Stores runs from a zip of their files, so their files can be downloaded from the database. For each run the zip
    ///     needs its results file and its authorised billing file; producer-names.csv, run-details.csv and
    ///     billing-instructions.csv add what those files can't hold. Runs are imported in run ID order, each in its own
    ///     transaction, and runs already stored are skipped. With checkOnly, nothing is stored.
    /// </summary>
    [HttpPost]
    [Route("runs/import")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxFileSize)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxFileSize)]
    [ProducesResponseType(typeof(IReadOnlyList<RunImportOutcome>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Import(IFormFile runsZip, [FromForm] bool checkOnly, CancellationToken cancellationToken)
    {
        if (!featureFlags.Value.RunImport)
            return NotFound();

        try
        {
            await using var zip = runsZip.OpenReadStream();
            return Ok(await runImportService.ImportAsync(zip, checkOnly, cancellationToken));
        }
        catch (RunImportPackageException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
