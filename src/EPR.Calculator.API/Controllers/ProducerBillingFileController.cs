using System.Net;
using EPR.Calculator.API.Extensions;
using EPR.Calculator.API.Models;
using EPR.Calculator.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;

namespace EPR.Calculator.API.Controllers;

[ApiController]
[Produces("application/json")]
[Route("v1")]
public class ProducerBillingFileController(
    IBillingFileService billingFileService,
    IServiceBusService serviceBus
) : ControllerBase
{
    /// <summary>
    ///     Accept or Reject billing for the matching list of organisation id and run id.
    /// </summary>
    [HttpPut]
    [Route("producerBillingInstructionsAccept/{runId}")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ProducerBillingInstructions(
        int runId,
        CancellationToken cancellationToken = default)
    {
        var serviceProcessResponseDto = await billingFileService.StartGeneratingBillingFileAsync(
            runId,
            User.GetName(),
            cancellationToken).ConfigureAwait(false);

        if (serviceProcessResponseDto.StatusCode == HttpStatusCode.OK)
        {
            var serviceBusMessage = new BillingFileGenerationMessage
            {
                CalculatorRunId = runId,
                ApprovedBy = User.GetName()
            };

            await serviceBus.SendMessage(serviceBusMessage);
        }

        return new ObjectResult(serviceProcessResponseDto.Message)
        {
            StatusCode = (int)serviceProcessResponseDto.StatusCode
        };
    }
}
