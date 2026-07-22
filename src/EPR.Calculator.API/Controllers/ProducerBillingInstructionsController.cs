using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.API.Controllers;

[ApiController]
[Produces("application/json")]
[Route("v1")]
public class ProducerBillingInstructionsController(
    IBillingFileService billingFileService
) : ControllerBase
{
    /// <summary>
    ///     Retrieve producer billing instructions for a specific calculator run.
    /// </summary>
    [HttpPost]
    [Route("producerBillingInstructions/{runId}")]
    [ProducesResponseType(typeof(ProducerBillingInstructionsResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ProducerBillingInstructions(
        [FromRoute] int runId,
        [FromBody] ProducerBillingInstructionsRequestDto requestDto,
        CancellationToken cancellationToken = default)
    {
        var serviceProcessResponseDto = await billingFileService.GetProducerBillingInstructionsAsync(
            runId,
            requestDto,
            cancellationToken).ConfigureAwait(false);

        if (serviceProcessResponseDto == null)
        {
            return NotFound(new ErrorDto
            {
                Message = CommonResources.RunNotFound,
                Description = CommonResources.RunNotFound
            });
        }

        return Ok(serviceProcessResponseDto);
    }
}
