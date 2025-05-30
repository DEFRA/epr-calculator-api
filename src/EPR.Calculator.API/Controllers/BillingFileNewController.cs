using System.ComponentModel.DataAnnotations;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.API.Controllers
{
    /// <summary>
    /// Controller responsible for handling billing file-related operations.
    /// </summary>
    [Route("v2")]
    public class BillingFileNewController(IBillingFileService billingFileService) : BaseControllerBase
    {
        /// <summary>
        /// Accept or Reject billing instruction based on the provided request.
        /// </summary>
        /// <param name="runId">The request containing.</param>
        /// <param name="produceBillingInstuctionRequestDto">The request containing the details for billing instruction.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>Http response on success or failure.</returns>
        [HttpPut]
        [Route("producerBillingInstructions/{runId}")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ProducerBillingInstructions(
            int runId,
            [FromBody][Required] ProduceBillingInstuctionRequestDto produceBillingInstuctionRequestDto,
            CancellationToken cancellationToken = default)
        {
            var claim = this.User.Claims.FirstOrDefault(x => x.Type == "name");
            var userName = claim.Value;
            var serviceProcessResponseDto = await billingFileService.UpdateProducerBillingInstructionsAsync(
                runId,
                userName,
                produceBillingInstuctionRequestDto,
                cancellationToken).ConfigureAwait(false);

            return new ObjectResult(serviceProcessResponseDto.Message)
            {
                StatusCode = (int)serviceProcessResponseDto.StatusCode,
            };
        }
    }
}
