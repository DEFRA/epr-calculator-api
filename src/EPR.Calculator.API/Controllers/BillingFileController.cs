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
                return Results.NotFound(string.Format(CommonResources.NoBillingFileMetadataForRunId, runId));
            }
            else if (string.IsNullOrEmpty(latestBillingFileMetaData.BillingCsvFileName))
            {
                return Results.NotFound(string.Format(CommonResources.NoBillingFileMetadataForRunId, runId));
            }

            var csvFileMetaData = await context.CalculatorRunCsvFileMetadata.
                SingleOrDefaultAsync(x =>
                    x.CalculatorRunId == runId
                    &&
                    x.FileName == latestBillingFileMetaData.BillingCsvFileName);

            if (csvFileMetaData == null)
            {
                return Results.NotFound(string.Format(CommonResources.NoBillingFileMetadataForRunId, runId));
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
