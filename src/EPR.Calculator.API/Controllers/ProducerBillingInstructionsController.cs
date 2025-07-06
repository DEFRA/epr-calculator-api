using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.API.Controllers
{
    /// <summary>
    /// Controller responsible for handling producer billing instruction operations.
    /// </summary>
    [Route("v1")]
    public class ProducerBillingInstructionsController(
        IBillingFileService billingFileService) : BaseControllerBase
    {
        /// <summary>
        /// Retrieve producer billing instructions for a specific calculator run.
        /// </summary>
        /// <param name="requestDto" type="ProducerBillingInstructionsRequestDto">Request object.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Http response on success or failure.</returns>
        [HttpPost]
        [Route("producerBillingInstructions/{runId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ProducerBillingInstructions(
            [FromRoute] int runId,
            [FromBody] ProducerBillingInstructionsRequestDto requestDto,
            CancellationToken cancellationToken = default)
        {
            if (!this.ModelState.IsValid)
            {
                return this.StatusCode(StatusCodes.Status400BadRequest, this.ModelState.Values.SelectMany(x => x.Errors));
            }

            try
            {
                var serviceProcessResponseDto = await billingFileService.GetProducerBillingInstructionsAsync(
                    runId,
                    requestDto,
                    cancellationToken).ConfigureAwait(false);

                if (serviceProcessResponseDto == null)
                {
                    return this.NotFound(new ErrorDto
                    {
                        Message = ErrorMessages.RunNotFound,
                        Description = ErrorMessages.RunNotFound,
                    });
                }

                return this.Ok(serviceProcessResponseDto);
            }
            catch (Exception e)
            {
                return this.StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}