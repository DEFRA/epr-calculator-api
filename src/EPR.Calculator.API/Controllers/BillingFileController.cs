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
    public class BillingFileController(IBillingFileService billingFileService) : BaseControllerBase
    {
        /// <summary>
        /// Generates a billing file based on the provided request.
        /// </summary>
        /// <param name="generateBillingFileRequestDto">The request containing the details for generating the billing file.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>An <see cref="IActionResult"/> containing the result of the operation.</returns>
        /// <response code="202">The request has been accepted for processing.</response>
        /// <response code="400">The request is invalid.</response>
        /// <response code="404">The specified resource was not found.</response>
        /// <response code="422">The request could not be processed due to semantic errors.</response>
        /// <response code="500">An internal server error occurred.</response>
        [HttpPost("generateBillingFile")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GenerateBillingFile(
            [FromBody][Required] GenerateBillingFileRequestDto generateBillingFileRequestDto,
            CancellationToken cancellationToken = default)
        {
            if (generateBillingFileRequestDto.CalculatorRunId <= 0)
            {
                return this.NotFound(CommonResources.ResourceNotFoundErrorMessage);
            }
            else
            {
                ServiceProcessResponseDto serviceProcessResponseDto = await billingFileService.GenerateBillingFileAsync(
                    generateBillingFileRequestDto,
                    cancellationToken).ConfigureAwait(false);

                return new ObjectResult(serviceProcessResponseDto.Message)
                {
                    StatusCode = (int)serviceProcessResponseDto.StatusCode,
                };
            }
        }
    }
}
