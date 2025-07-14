using System.ComponentModel.DataAnnotations;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Exceptions;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Controllers
{
    /// <summary>
    /// Controller responsible for handling billing file-related operations.
    /// </summary>
    public class BillingFileController(IBillingFileService billingFileService, IStorageService storageService, ApplicationDBContext context) : BaseControllerBase
    {
        private const string ObsoleteError = "This endpoint is no longer needed for Accept/Reject";

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
        [Obsolete(ObsoleteError)]
        public async Task<IActionResult> GenerateBillingFile(
            [FromBody][Required] GenerateBillingFileRequestDto generateBillingFileRequestDto,
            CancellationToken cancellationToken = default)
        {
            if (generateBillingFileRequestDto.CalculatorRunId <= 0)
            {
                return this.NotFound(CommonResources.ResourceNotFoundErrorMessage);
            }

            ServiceProcessResponseDto serviceProcessResponseDto = await billingFileService.GenerateBillingFileAsync(
                   generateBillingFileRequestDto,
                   cancellationToken).ConfigureAwait(false);

            return new ObjectResult(serviceProcessResponseDto.Message)
            {
                StatusCode = (int)serviceProcessResponseDto.StatusCode,
            };
        }

        [HttpGet("ProducerBillingInstructions/{runId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProducersInstructionResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ProducerBillingInstructions(
            int runId, CancellationToken cancellationToken = default)
        {
            if (runId <= 0)
            {
                return this.StatusCode(StatusCodes.Status400BadRequest, $"Invalid Run Id {runId}");
            }

            try
            {
                var responseDto = await billingFileService.GetProducersInstructionResponseAsync(
                    runId, cancellationToken).ConfigureAwait(false);

                if (responseDto == null || (responseDto.ProducersInstructionDetails == null && responseDto.ProducersInstructionSummary == null))
                {
                    return this.StatusCode(StatusCodes.Status404NotFound, $"No billing instructions found for Run Id {runId}");
                }

                return Ok(responseDto);
            }
            catch (Exception ex)
            {
                return ex switch
                {
                    UnprocessableEntityException unEx => this.StatusCode(StatusCodes.Status422UnprocessableEntity, unEx.Message),
                    KeyNotFoundException keyEx => this.StatusCode(StatusCodes.Status404NotFound, keyEx.Message),
                    _ => throw ex,
                };
            }
        }

        /// <summary>
        /// Downloads the CSV billing file for the specified calculator run.
        /// </summary>
        /// <param name="runId">The unique identifier of the calculator run.</param>
        /// <returns>The CSV billing file as a downloadable response, or an error result if not found or invalid.</returns>
        /// <response code="200">Returns the CSV billing file for the specified run.</response>
        /// <response code="400">Returned when the request is invalid.</response>
        /// <response code="404">Returned when the billing file is not found for the specified run id.</response>
        [HttpGet]
        [Route("downloadBillingFile/{runId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IResult> DownloadCsvBillingFile(int runId)
        {
            if (!this.ModelState.IsValid)
            {
                var badRequest = Results.BadRequest(this.ModelState.Values.SelectMany(x => x.Errors));
                return badRequest;
            }

            var latestBillingFileMetaData = await context.CalculatorRunBillingFileMetadata.Where(
                x => x.CalculatorRunId == runId).OrderByDescending(x => x.BillingFileCreatedDate).
                FirstOrDefaultAsync();

            if (latestBillingFileMetaData == null)
            {
                return Results.NotFound($"No billing file metadata for Run Id {runId}");
            }
            else if (string.IsNullOrEmpty(latestBillingFileMetaData.BillingCsvFileName))
            {
                return Results.NotFound($"No billing file name found for Run Id {runId}");
            }

            var csvFileMetaData = await context.CalculatorRunCsvFileMetadata.
                SingleOrDefaultAsync(x =>
                    x.CalculatorRunId == runId
                    &&
                    x.FileName == latestBillingFileMetaData.BillingCsvFileName);

            if (csvFileMetaData == null)
            {
                return Results.NotFound($"No billing file uri found for Run Id {runId}");
            }

            try
            {
                return await storageService.DownloadFile(csvFileMetaData.FileName, csvFileMetaData.BlobUri);
            }
            catch (Exception e)
            {
                return Results.Problem(e.Message);
            }
        }
    }
}
