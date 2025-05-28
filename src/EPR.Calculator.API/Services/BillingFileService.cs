using System.Net;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Exceptions;
using EPR.Calculator.API.Services.Abstractions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
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

        public async Task<ProducersInstructionResponse?> GetProducersInstructionResponseAsync(int runId, CancellationToken cancellationToken)
        {
            var validRunClassifications = new[]
                {
                    (int)RunClassification.INITIAL_RUN,
                    (int)RunClassification.INTERIM_RECALCULATION_RUN,
                    (int)RunClassification.FINAL_RUN,
                    (int)RunClassification.FINAL_RECALCULATION_RUN,
                };

            var runStatus = await applicationDBContext.CalculatorRuns
                .Where(run => run.Id == runId)
                .FirstOrDefaultAsync(cancellationToken);

            if (runStatus == null)
            {
                throw new KeyNotFoundException($"Run ID {runId} was not found.");
            }

            if (!validRunClassifications.Contains(runStatus.CalculatorRunClassificationId))
            {
                throw new UnprocessableEntityException(string.Format(CommonResources.NotAValidClassificationStatus, runId));
            }

            var instructions = await (
             from billing in applicationDBContext.ProducerResultFileSuggestedBillingInstruction
             join producer in applicationDBContext.ProducerDetail
                 on new { billing.ProducerId, billing.CalculatorRunId }
                 equals new { producer.ProducerId, producer.CalculatorRunId }
             where billing.CalculatorRunId == runId
             select new
             {
                 OrganisationId = producer.ProducerId,
                 OrganisationName = producer.ProducerName ?? producer.TradingName ?? string.Empty,
                 BillingInstruction = billing.SuggestedBillingInstruction,
                 InvoiceAmount = billing.SuggestedInvoiceAmount,
                 Status = billing.BillingInstructionAcceptReject,
             }).ToListAsync(cancellationToken);

            if (instructions == null || !instructions.Any())
            {
                return null;
            }

            var details = instructions.Select(i => new ProducersInstructionDetail
            {
                organisationId = i.OrganisationId,
                organisationName = i.OrganisationName,
                billingInstruction = i.BillingInstruction,
                invoiceAmount = i.InvoiceAmount.ToString("F2"),
                status = MapToBillingStatus(i.Status),
            }).ToList();

            var summary = new ProducersInstructionSummary
            {
                Statuses = details
                    .GroupBy(d => d.status)
                    .ToDictionary(g => g.Key, g => g.Count()),
            };

            return new ProducersInstructionResponse
            {
                ProducersInstructionDetails = details,
                ProducersInstructionSummary = summary,
            };
        }

        private BillingStatus MapToBillingStatus(string? status)
        {
            return status?.ToLowerInvariant() switch
            {
                "accepted" => BillingStatus.Accepted,
                "rejected" => BillingStatus.Rejected,
                "pending" => BillingStatus.Pending,
                _ => BillingStatus.Noaction,
            };
        }
    }
}
