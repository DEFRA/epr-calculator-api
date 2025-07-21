using System.Net;
using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Models;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.API.Controllers
{
    /// <summary>
    /// Controller responsible for handling billing file-related operations.
    /// </summary>
    [Route("v1")]
    public class ProducerBillingFileController(IBillingFileService billingFileService, IServiceBusService serviceBusService, IConfiguration configuration) : BaseControllerBase
    {
        /// <summary>
        /// Accept or Reject billing for the matching list of organisation id and run id.
        /// </summary>
        /// <param name="runId">Run Id passed in the URL.</param>
        /// <param name="status">Status.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>Http response on success or failure.</returns>
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
            var claim = this.User.Claims.FirstOrDefault(x => x.Type == "name")!;
            var userName = claim.Value;
            var serviceProcessResponseDto = await billingFileService.StartGeneratingBillingFileAsync(
                runId,
                userName,
                cancellationToken).ConfigureAwait(false);

            if (serviceProcessResponseDto.StatusCode == HttpStatusCode.OK)
            {
                var serviceBusQueueName = configuration.GetSection("ServiceBus").GetSection("QueueName").Value;
                await serviceBusService.SendMessage(serviceBusQueueName, new BillingFileGenerationMessage() { ApprovedBy = userName, CalculatorRunId = runId, MessageType = CommonResources.BillingMessageType });
            }

            return new ObjectResult(serviceProcessResponseDto.Message)
            {
                StatusCode = (int)serviceProcessResponseDto.StatusCode,
            };
        }
    }
}