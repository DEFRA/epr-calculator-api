using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Services.Abstractions;
using EPR.Calculator.API.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.API.Controllers
{
    /// <summary>
    /// Controller responsible for handling producer billing instruction operations.
    /// </summary>
    [AllowAnonymous]
    [Route("v1")]
    public class ProducerBillingInstructionsController(
        IBillingFileService billingFileService,
        IServiceBusService serviceBusService,
        IConfiguration configuration,
        IProducerBillingInstructionsRequestDtoDataValidator validator) : BaseControllerBase
    {
        /// <summary>
        /// Retrieve producer billing instructions for a specific calculator run.
        /// </summary>
        /// <param name="requestDto" type="ProducerBillingInstructionsRequestDto">Request object.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Http response on success or failure.</returns>
        [HttpPost]
        [Route("producerBillingInstructions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ProducerBillingInstructions(
            [FromBody] ProducerBillingInstructionsRequestDto requestDto,
            CancellationToken cancellationToken = default)
        {
            // TODO Unit test 
            // If there are no producer records associated to the calculator run
            // We are still sending a 200 but with an empty records field

            var validationResult = validator.Validate(requestDto);
            if (validationResult.IsInvalid)
            {
                return new ObjectResult(validationResult)
                {
                    StatusCode = validationResult.StatusCode != null ? (int)validationResult.StatusCode : StatusCodes.Status400BadRequest,
                };
            }

            var serviceProcessResponseDto = await billingFileService.GetProducerBillingInstructionsAsync(
                requestDto,
                cancellationToken).ConfigureAwait(false);

            return new ObjectResult(serviceProcessResponseDto)
            {
                StatusCode = (int)serviceProcessResponseDto.StatusCode,
            };
        }
    }
}