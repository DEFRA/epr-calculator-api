using System.Net;
using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Services.Abstractions;
using EPR.Calculator.API.Utils;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Services
{
    /// <summary>
    /// Service responsible for handling billing file operations <seealso cref="IBillingFileService"/>.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="BillingFileService"/> class.
    /// </remarks>
    /// <param name="applicationDBContext">The database context <seealso cref="ApplicationDBContext"/>.</param>
    /// <param name="storageService">The storage service <seealso cref="IStorageService"/>.</param>
    public class BillingFileService(ApplicationDBContext applicationDBContext, IStorageService storageService) : IBillingFileService
    {
        /// <inheritdoc/>
        public async Task<ServiceProcessResponseDto> GenerateBillingFileAsync(
            GenerateBillingFileRequestDto generateBillingFileRequestDto,
            CancellationToken cancellationToken)
        {
            CalculatorRun? calculatorRun = await applicationDBContext.CalculatorRuns
                                                                     .FirstOrDefaultAsync(x => x.Id == generateBillingFileRequestDto.CalculatorRunId, cancellationToken)
                                                                     .ConfigureAwait(false);

            if (calculatorRun is null)
            {
                return new ServiceProcessResponseDto
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Message = CommonResources.ResourceNotFoundErrorMessage,
                };
            }
            else if (calculatorRun.CalculatorRunClassificationId != (int)RunClassification.INITIAL_RUN)
            {
                return new ServiceProcessResponseDto
                {
                    StatusCode = HttpStatusCode.UnprocessableContent,
                    Message = string.Format(CommonResources.NotAValidClassificationStatus, generateBillingFileRequestDto.CalculatorRunId),
                };
            }
            else if (calculatorRun.HasBillingFileGenerated)
            {
                return new ServiceProcessResponseDto
                {
                    StatusCode = HttpStatusCode.UnprocessableContent,
                    Message = string.Format(CommonResources.GenerateBillingFileAlreadyRequest, generateBillingFileRequestDto.CalculatorRunId),
                };
            }
            else
            {
                CalculatorRunCsvFileMetadata? calculatorRunCsvFileMetadata = await applicationDBContext.CalculatorRunCsvFileMetadata
                                                                     .FirstOrDefaultAsync(x => x.CalculatorRunId == generateBillingFileRequestDto.CalculatorRunId, cancellationToken)
                                                                     .ConfigureAwait(false);

                if (calculatorRunCsvFileMetadata is null)
                {
                    return new ServiceProcessResponseDto
                    {
                        StatusCode = HttpStatusCode.UnprocessableContent,
                        Message = string.Format(CommonResources.CsvFileMetadataNotFoundErrorMessage, generateBillingFileRequestDto.CalculatorRunId),
                    };
                }
                else if (!await storageService.IsBlobExistsAsync(
                    calculatorRunCsvFileMetadata.FileName,
                    calculatorRunCsvFileMetadata.BlobUri,
                    cancellationToken).ConfigureAwait(false))
                {
                    return new ServiceProcessResponseDto
                    {
                        StatusCode = HttpStatusCode.UnprocessableContent,
                        Message = string.Format(CommonResources.BlobNotFoundErrorMessage, generateBillingFileRequestDto.CalculatorRunId),
                    };
                }
                else
                {
                    calculatorRun.HasBillingFileGenerated = true;
                    await applicationDBContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                    return new ServiceProcessResponseDto
                    {
                        StatusCode = HttpStatusCode.Accepted,
                        Message = CommonResources.RequestAcceptedMessage,
                    };
                }
            }
        }

        public async Task<ServiceProcessResponseDto> UpdateProducerBillingInstructionsAsync(
            int runId,
            string userName,
            ProduceBillingInstuctionRequestDto produceBillingInstuctionRequestDto,
            CancellationToken cancellationToken)
        {
            try
            {
                var calculatorRun = await applicationDBContext.CalculatorRuns
                            .SingleOrDefaultAsync(x => x.Id == runId && Util.AcceptableRunStatusForBillingInstructions().Contains(x.CalculatorRunClassificationId), cancellationToken)
                            .ConfigureAwait(false);

                if (calculatorRun is null)
                {
                    return new ServiceProcessResponseDto
                    {
                        StatusCode = HttpStatusCode.UnprocessableContent,
                        Message = ErrorMessages.InvalidRunId,
                    };
                }

                var rows = await applicationDBContext.ProducerResultFileSuggestedBillingInstruction
                            .Where(x => produceBillingInstuctionRequestDto.OrganisationIds.Contains(x.ProducerId) && x.CalculatorRunId == runId)
                            .ToListAsync(cancellationToken)
                            .ConfigureAwait(false);

                if (rows.Count < produceBillingInstuctionRequestDto.OrganisationIds.Count())
                {
                    return new ServiceProcessResponseDto
                    {
                        StatusCode = HttpStatusCode.UnprocessableContent,
                        Message = ErrorMessages.InvalidOrganisationId,
                    };
                }

                foreach (var row in rows)
                {
                    UpdateBillingInstruction(userName, produceBillingInstuctionRequestDto, row);
                }

                await applicationDBContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                return new ServiceProcessResponseDto
                {
                    StatusCode = HttpStatusCode.NoContent,
                };
            }
            catch (Exception exception)
            {
                return new ServiceProcessResponseDto
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Message = exception.Message,
                };
            }
        }

        private static void UpdateBillingInstruction(
            string userName,
            ProduceBillingInstuctionRequestDto produceBillingInstuctionRequestDto,
            ProducerResultFileSuggestedBillingInstruction row)
        {
            row.BillingInstructionAcceptReject = produceBillingInstuctionRequestDto.Status;

            if (string.Equals(produceBillingInstuctionRequestDto.Status, BillingStatus.Rejected.ToString()))
            {
                row.ReasonForRejection = produceBillingInstuctionRequestDto.ReasonForRejection;
            }

            row.LastModifiedAcceptReject = DateTime.UtcNow;
            row.LastModifiedAcceptRejectBy = userName;
        }
    }
}