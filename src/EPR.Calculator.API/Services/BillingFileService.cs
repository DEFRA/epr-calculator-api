using System.Net;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Exceptions;
using EPR.Calculator.API.Mappers;
using EPR.Calculator.API.Models;
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
    public class BillingFileService(ApplicationDBContext applicationDBContext, IStorageService storageService, IBlobStorageService2 blobStorageService2, IConfiguration configuration) : IBillingFileService
    {
        /// <summary>
        /// Validates the run ID for accepting all billing instructions.
        /// </summary>
        /// <param name="run">calculation Run</param>
        /// <returns>bool</returns>
        public static bool ValidateRunForAcceptAllBillingInstructions(CalculatorRun run)
        {
            return Util.AcceptableRunStatusForBillingInstructions().Contains(run.CalculatorRunClassificationId)
                   &&
                   !run.IsBillingFileGenerating.GetValueOrDefault();
        }

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
                        Message = CommonResources.InvalidRunId,
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
                        Message = CommonResources.InvalidOrganisationId,
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

        public async Task<ProducersInstructionResponse?> GetProducersInstructionResponseAsync(int runId, CancellationToken cancellationToken)
        {
            this.ValidateRunClassification(await this.GetRunStatusAsync(runId, cancellationToken), runId);

            var details = await this.GetInstructionDetailsAsync(runId, cancellationToken);

            if (!details.Any())
            {
                return null;
            }

            var summary = GenerateInstructionSummary(details);

            return new ProducersInstructionResponse
            {
                ProducersInstructionDetails = details,
                ProducersInstructionSummary = summary,
            };
        }

        public async Task<bool> MoveBillingJsonFile(int runId, CancellationToken cancellationToken)
        {
            var billingFileMetaData =
                await applicationDBContext.CalculatorRunBillingFileMetadata.Where(m => m.CalculatorRunId == runId)
                .OrderByDescending(m => m.BillingFileCreatedDate)
                    .FirstOrDefaultAsync(cancellationToken)
                    .ConfigureAwait(false);

            if (billingFileMetaData == null || string.IsNullOrEmpty(billingFileMetaData.BillingJsonFileName))
            {
                return false;
            }

            var blobStorageSettings = new BlobStorageSettings();
            configuration.GetSection("BlobStorage").Bind(blobStorageSettings);

            var sourceContainer = blobStorageSettings.BillingFileJsonContainerName;
            var targetContainer = blobStorageSettings.BillingFileJsonForFssContainerName;

            var result = await blobStorageService2.MoveBlobAsync(sourceContainer, targetContainer, billingFileMetaData.BillingJsonFileName);

            return result;
        }

        public async Task<ProducerBillingInstructionsResponseDto?> GetProducerBillingInstructionsAsync(
            int runId,
            ProducerBillingInstructionsRequestDto requestDto,
            CancellationToken cancellationToken)
        {
            var run = await this.GetRunAsync(runId, cancellationToken);

            if (run == null)
            {
                return null;
            }

            var searchQuery = requestDto.SearchQuery;

            var query = from prsi in applicationDBContext.ProducerResultFileSuggestedBillingInstruction
                        where prsi.CalculatorRunId == runId
                        select new ProducerBillingInstructionsDto()
                        {
                            ProducerId = prsi.ProducerId,
                            BillingInstructionAcceptReject = prsi.BillingInstructionAcceptReject ?? BillingStatus.Pending.ToString(),
                            SuggestedBillingInstruction = prsi.SuggestedBillingInstruction,
                            SuggestedInvoiceAmount = prsi.SuggestedInvoiceAmount,
                        };

            // Group by on BillingInstructionAcceptReject
            var groupedStatus = query
                        .GroupBy(x => string.IsNullOrWhiteSpace(x.BillingInstructionAcceptReject) ? BillingStatus.Pending.ToString() : x.BillingInstructionAcceptReject.Trim())
                        .Select(g => new ProducerBillingInstructionsStatus
                        {
                            Status = g.Key,
                            TotalRecords = g.Count(),
                        });

            // Apply OrganisationId filter if provided
            if (searchQuery?.OrganisationId.HasValue == true)
            {
                var orgId = searchQuery.OrganisationId.Value;
                query = query.Where(x => x.ProducerId == orgId).AsQueryable();
            }

            // Apply Status filter if provided and not empty
            if (searchQuery?.Status != null && searchQuery.Status.Any())
            {
                var statusList = searchQuery.Status.ToList();

                query = query.Where(x => x.BillingInstructionAcceptReject != null && statusList.Contains(x.BillingInstructionAcceptReject)).AsQueryable();
            }

            query = query.Distinct().OrderBy(x => x.ProducerId).AsQueryable();

            requestDto.PageNumber ??= int.TryParse(CommonResources.ProducerBillingInstructionsDefaultPageNumber, out int pageNumber) ? pageNumber : 1;
            requestDto.PageSize ??= int.TryParse(CommonResources.ProducerBillingInstructionsDefaultPageSize, out int pageSize) ? pageSize : 10;

            var pagedResult = await query
                          .Skip((requestDto.PageNumber.Value - 1) * requestDto.PageSize.Value)
                          .Take(requestDto.PageSize.Value)
                          .AsNoTracking()
                          .ToListAsync(cancellationToken);

            var allProducerIds = query.Select(x => x.ProducerId).Distinct();
            var pagedProducerIds = pagedResult.Select(x => x.ProducerId).Distinct();
            var parentProducers = await this.GetParentProducersLatestAsync(runId, pagedProducerIds, cancellationToken);

            foreach (var record in pagedResult)
            {
                record.ProducerName = parentProducers.FirstOrDefault(p => p.ProducerId == record.ProducerId)?.ProducerName ?? string.Empty;
            }

            var groupedStatusResult = await groupedStatus.ToListAsync(cancellationToken);

            var totalAcceptedRecords = groupedStatusResult.FirstOrDefault(s => s.Status == BillingStatus.Accepted.ToString())?.TotalRecords ?? 0;
            var totalRejectedRecords = groupedStatusResult.FirstOrDefault(s => s.Status == BillingStatus.Rejected.ToString())?.TotalRecords ?? 0;
            var totalPendingRecords = groupedStatusResult.FirstOrDefault(s => s.Status == BillingStatus.Pending.ToString())?.TotalRecords ?? 0;

            return new ProducerBillingInstructionsResponseDto
            {
                Records = pagedResult,
                PageNumber = requestDto.PageNumber,
                PageSize = requestDto.PageSize,
                TotalRecords = groupedStatusResult.Sum(s => s.TotalRecords),
                TotalAcceptedRecords = totalAcceptedRecords,
                TotalRejectedRecords = totalRejectedRecords,
                TotalPendingRecords = totalPendingRecords,
                RunName = run.Name,
                CalculatorRunId = run.Id,
                AllProducerIds = allProducerIds,
            };
        }

        public async Task<ServiceProcessResponseDto> UpdateProducerBillingInstructionsAcceptAllAsync(
            int runId,
            string userName,
            CancellationToken cancellationToken)
        {
            try
            {
                var calculatorRun = await applicationDBContext.CalculatorRuns
                            .SingleOrDefaultAsync(run => run.Id == runId, cancellationToken)
                            .ConfigureAwait(false);

                if (calculatorRun is null)
                {
                    return new ServiceProcessResponseDto
                    {
                        StatusCode = HttpStatusCode.UnprocessableContent,
                        Message = CommonResources.InvalidRunId,
                    };
                }

                if (!ValidateRunForAcceptAllBillingInstructions(calculatorRun))
                {
                    return new ServiceProcessResponseDto
                    {
                        StatusCode = HttpStatusCode.UnprocessableContent,
                        Message = CommonResources.InvalidRunStatusForAcceptAll,
                    };
                }

                var rows = await applicationDBContext.ProducerResultFileSuggestedBillingInstruction
                            .Where(x => x.CalculatorRunId == runId)
                            .ToListAsync(cancellationToken)
                            .ConfigureAwait(false);

                if (rows.Count <= 0)
                {
                    return new ServiceProcessResponseDto
                    {
                        StatusCode = HttpStatusCode.UnprocessableContent,
                        Message = CommonResources.InvalidOrganisationId,
                    };
                }

                rows.ForEach(x =>
                {
                    x.BillingInstructionAcceptReject = BillingStatus.Accepted.ToString();
                    x.LastModifiedAcceptReject = DateTime.UtcNow;
                    x.LastModifiedAcceptRejectBy = userName;
                });

                calculatorRun.IsBillingFileGenerating = true;

                await applicationDBContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                return new ServiceProcessResponseDto
                {
                    StatusCode = HttpStatusCode.OK,
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

        public async Task<ServiceProcessResponseDto> StartGeneratingBillingFileAsync(
            int runId,
            string userName,
            CancellationToken cancellationToken)
        {
            try
            {
                var calculatorRun = await applicationDBContext.CalculatorRuns
                            .SingleOrDefaultAsync(run => run.Id == runId, cancellationToken)
                            .ConfigureAwait(false);

                if (calculatorRun is null)
                {
                    return new ServiceProcessResponseDto
                    {
                        StatusCode = HttpStatusCode.UnprocessableContent,
                        Message = CommonResources.InvalidRunId,
                    };
                }

                if (!ValidateRunForAcceptAllBillingInstructions(calculatorRun))
                {
                    return new ServiceProcessResponseDto
                    {
                        StatusCode = HttpStatusCode.UnprocessableContent,
                        Message = CommonResources.InvalidRunStatusForAcceptAll,
                    };
                }

                calculatorRun.IsBillingFileGenerating = true;

                await applicationDBContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                return new ServiceProcessResponseDto
                {
                    StatusCode = HttpStatusCode.OK,
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

        public async Task<bool> IsBillingFileGeneratedLatest(int runId)
        {
            var lastModifiedAcceptreject = await applicationDBContext.ProducerResultFileSuggestedBillingInstruction
                .Where(x => x.CalculatorRunId == runId)
                .OrderByDescending(x => x.LastModifiedAcceptReject)
                .AsNoTracking()
                .Select(x => x.LastModifiedAcceptReject)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            var billingGeneratedDate = await applicationDBContext.CalculatorRunBillingFileMetadata
                .Where(x => x.CalculatorRunId == runId)
                .OrderByDescending(x => x.BillingFileCreatedDate)
                .AsNoTracking()
                .Select(x => x.BillingFileCreatedDate)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);

            return lastModifiedAcceptreject != null && billingGeneratedDate > lastModifiedAcceptreject;
        }

        private async Task<CalculatorRun?> GetRunStatusAsync(int runId, CancellationToken cancellationToken)
        {
            return await applicationDBContext.CalculatorRuns
                .SingleOrDefaultAsync(run => run.Id == runId, cancellationToken);
        }

        private void ValidateRunClassification(CalculatorRun? runStatus, int runId)
        {
            if (runStatus == null)
            {
                throw new KeyNotFoundException(string.Format(CommonResources.RunINotFound, runId));
            }

            var validRunClassifications = new HashSet<int>
                    {
                        (int)RunClassification.INITIAL_RUN,
                        (int)RunClassification.INTERIM_RECALCULATION_RUN,
                        (int)RunClassification.FINAL_RUN,
                        (int)RunClassification.FINAL_RECALCULATION_RUN,
                    };

            if (!validRunClassifications.Contains(runStatus.CalculatorRunClassificationId))
            {
                throw new UnprocessableEntityException(string.Format(CommonResources.NotAValidClassificationStatus, runId));
            }
        }

        private async Task<List<ProducersInstructionDetail>> GetInstructionDetailsAsync(int runId, CancellationToken cancellationToken)
        {
            return await (
                from billing in applicationDBContext.ProducerResultFileSuggestedBillingInstruction
                join producer in applicationDBContext.ProducerDetail
                    on new { billing.ProducerId, billing.CalculatorRunId }
                    equals new { producer.ProducerId, producer.CalculatorRunId }
                where billing.CalculatorRunId == runId && producer.SubsidiaryId == null
                select new ProducersInstructionDetail
                {
                    OrganisationId = producer.ProducerId,
                    OrganisationName = producer.ProducerName,
                    BillingInstruction = billing.SuggestedBillingInstruction,
                    InvoiceAmount = $"£{billing.SuggestedInvoiceAmount:N2}",
                    Status = string.IsNullOrWhiteSpace(billing.BillingInstructionAcceptReject) ? string.Empty : billing.BillingInstructionAcceptReject,
                }).Distinct().ToListAsync(cancellationToken);
        }

        private ProducersInstructionSummary GenerateInstructionSummary(List<ProducersInstructionDetail> details)
        {
            var statusGroups = details
                .GroupBy(d => string.IsNullOrWhiteSpace(d.Status) ? string.Empty : d.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            var orderedStatuses = new Dictionary<string, int> { { "All", details.Count }, };

            foreach (var kvp in statusGroups)
            {
                orderedStatuses[kvp.Key] = kvp.Value;
            }

            return new ProducersInstructionSummary
            {
                Statuses = orderedStatuses,
            };
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
            else
            {
                row.ReasonForRejection = null;
            }

            row.LastModifiedAcceptReject = DateTime.UtcNow;
            row.LastModifiedAcceptRejectBy = userName;
        }

        private Task<CalculatorRun?> GetRunAsync(int runId, CancellationToken cancellationToken) =>
            applicationDBContext.CalculatorRuns
                .SingleOrDefaultAsync(x => x.Id == runId, cancellationToken);

        private Task<List<ParentProducer>> GetParentProducersLatestAsync(int runId, IEnumerable<int> producerIds, CancellationToken cancellationToken) =>
            (from odd in applicationDBContext.CalculatorRunOrganisationDataDetails
            join crdm in applicationDBContext.CalculatorRunOrganisationDataMaster
            on odd.CalculatorRunOrganisationDataMasterId equals crdm.Id
            join run in applicationDBContext.CalculatorRuns on crdm.Id equals run.CalculatorRunOrganisationDataMasterId
             where run.Id == runId && producerIds.ToList().Contains(odd.OrganisationId ?? 0) && odd.SubsidaryId == null
             select new
            ParentProducer
            {
                ProducerId = odd.OrganisationId ?? 0,
                ProducerName = odd.OrganisationName,
            }).ToListAsync(cancellationToken);
    }
}