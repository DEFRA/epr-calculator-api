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
        private const string NoActionPlaceholder = "-";
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
            ValidateRunClassification(await this.GetRunStatusAsync(runId, cancellationToken), runId);

            var details = await this.GetInstructionDetailsAsync(runId, cancellationToken);

            if (details.Count == 0)
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
                            BillingInstructionAcceptReject =
                                string.IsNullOrWhiteSpace(prsi.BillingInstructionAcceptReject)
                                    ? ((string.IsNullOrWhiteSpace(prsi.SuggestedBillingInstruction) || prsi.SuggestedBillingInstruction.Trim() == NoActionPlaceholder)
                                        ? BillingInstruction.Noaction.ToString()
                                        : BillingStatus.Pending.ToString())
                                    : prsi.BillingInstructionAcceptReject.Trim(),
                            SuggestedBillingInstruction = prsi.SuggestedBillingInstruction,
                            SuggestedInvoiceAmount = (prsi.SuggestedBillingInstruction ?? string.Empty).ToLower() == "cancel" ? prsi.CurrentYearInvoiceTotalToDate : prsi.SuggestedInvoiceAmount,
                        };

            // Group by on BillingInstructionAcceptReject before filtering
            var groupedStatus = applicationDBContext.ProducerResultFileSuggestedBillingInstruction
                .Where(prsi => prsi.CalculatorRunId == runId)
                .AsNoTracking()
                .GroupBy(prsi => string.IsNullOrWhiteSpace(prsi.BillingInstructionAcceptReject)
                    ? ((string.IsNullOrWhiteSpace(prsi.SuggestedBillingInstruction) || prsi.SuggestedBillingInstruction.Trim() == NoActionPlaceholder)
                        ? BillingInstruction.Noaction.ToString()
                        : BillingStatus.Pending.ToString())
                    : prsi.BillingInstructionAcceptReject.Trim())
                .Select(g => new ProducerBillingInstructionsStatus
                {
                    Status = g.Key,
                    TotalRecords = g.Count(),
                });

            // Group by on BillingInstructionAcceptReject before filtering
            var groupedBillingInstruction = query
                        .AsNoTracking()
                        .GroupBy(x => (string.IsNullOrWhiteSpace(x.SuggestedBillingInstruction) || x.SuggestedBillingInstruction.Trim() == NoActionPlaceholder)
                            ? BillingInstruction.Noaction.ToString()
                            : x.SuggestedBillingInstruction.Trim())
                        .Select(g => new ProducerBillingInstructionSuggestion
                        {
                            Suggestion = g.Key,
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

            // Apply BillingInstruction filter if provided and not empty
            if (searchQuery?.BillingInstruction != null && searchQuery.BillingInstruction.Any())
            {
                var billingInstructionList = searchQuery.BillingInstruction.Select(b => b?.Trim()).Where(b => !string.IsNullOrWhiteSpace(b)).ToList();

                bool includeNoAction = billingInstructionList.Any(b => string.Equals(b, BillingInstruction.Noaction.ToString(), StringComparison.OrdinalIgnoreCase));

                if (includeNoAction)
                {
                    query = query.Where(x =>
                        (string.IsNullOrWhiteSpace(x.SuggestedBillingInstruction) || x.SuggestedBillingInstruction.Trim() == NoActionPlaceholder)
                        || (x.SuggestedBillingInstruction != null && billingInstructionList.Contains(x.SuggestedBillingInstruction.Trim())))
                        .AsQueryable();
                }
                else
                {
                    query = query.Where(x => x.SuggestedBillingInstruction != null && billingInstructionList.Contains(x.SuggestedBillingInstruction.Trim())).AsQueryable();
                }
            }

            query = query.Distinct().OrderBy(x => x.ProducerId).AsQueryable();

            requestDto.PageNumber ??= int.TryParse(CommonResources.ProducerBillingInstructionsDefaultPageNumber, out int pageNumber) ? pageNumber : 1;
            requestDto.PageSize ??= int.TryParse(CommonResources.ProducerBillingInstructionsDefaultPageSize, out int pageSize) ? pageSize : 10;

            var response = new ProducerBillingInstructionsResponseDto
            {
                PageNumber = requestDto.PageNumber,
                PageSize = requestDto.PageSize,
                RunName = run.Name,
                CalculatorRunId = run.Id,
            };

            await this.PopulatePagedBillingInstructionsAsync(query, requestDto, run.Id, run.FinancialYearId, response, cancellationToken);
            await this.PopulateBillingStatusCountsAsync(groupedStatus, response, cancellationToken);
            await this.PopulateBillingInstructionCountsAsync(groupedBillingInstruction, response, cancellationToken);

            return response;
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

        public async Task<bool?> IsBillingFileGeneratedLatest(int runId, CancellationToken cancellationToken)
        {
            if (!await applicationDBContext.ProducerResultFileSuggestedBillingInstruction.AnyAsync(
                    x => x.CalculatorRunId == runId, cancellationToken).ConfigureAwait(false)
                ||
                !await applicationDBContext.CalculatorRunBillingFileMetadata.AnyAsync(
                    x => x.CalculatorRunId == runId, cancellationToken).ConfigureAwait(false))
            {
                return null;
            }

            var lastModifiedAcceptReject = await applicationDBContext.ProducerResultFileSuggestedBillingInstruction
                .Where(x => x.CalculatorRunId == runId)
                .OrderByDescending(x => x.LastModifiedAcceptReject)
                .AsNoTracking()
                .Select(x => x.LastModifiedAcceptReject)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            var billingGeneratedDate = await applicationDBContext.CalculatorRunBillingFileMetadata
                .Where(x => x.CalculatorRunId == runId)
                .OrderByDescending(x => x.BillingFileCreatedDate)
                .AsNoTracking()
                .Select(x => x.BillingFileCreatedDate)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            return lastModifiedAcceptReject.HasValue && billingGeneratedDate > lastModifiedAcceptReject.Value;
        }

        private static ProducersInstructionSummary GenerateInstructionSummary(List<ProducersInstructionDetail> details)
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

        private static void ValidateRunClassification(CalculatorRun? runStatus, int runId)
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

        private async Task<CalculatorRun?> GetRunStatusAsync(int runId, CancellationToken cancellationToken)
        {
            return await applicationDBContext.CalculatorRuns
                .SingleOrDefaultAsync(run => run.Id == runId, cancellationToken);
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

        private Task<CalculatorRun?> GetRunAsync(int runId, CancellationToken cancellationToken) =>
            applicationDBContext.CalculatorRuns
                .SingleOrDefaultAsync(x => x.Id == runId, cancellationToken);

        /// <summary>
        /// Get the producer names for the list of producer ids provided.
        /// </summary>
        /// <param name="runId">Current run id.</param>
        /// <param name="financialYear">Financial year of the current run.</param>
        /// <param name="producerIds">List of procuder ids requiring producer name.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Task list of parent producers with producer name.</returns>
        private async Task<List<ParentProducer>> GetParentProducersLatestAsync(
            int runId,
            string financialYear,
            IEnumerable<int> producerIds,
            CancellationToken cancellationToken)
        {
            var runClassificationsToIgnore = new List<int>
            {
                (int)RunClassification.INTHEQUEUE,
                (int)RunClassification.RUNNING,
                (int)RunClassification.TEST_RUN,
                (int)RunClassification.ERROR,
                (int)RunClassification.DELETED,
            };

            var parentProducers = await (from odd in applicationDBContext.CalculatorRunOrganisationDataDetails
                                   join odm in applicationDBContext.CalculatorRunOrganisationDataMaster
                                       on odd.CalculatorRunOrganisationDataMasterId equals odm.Id
                                   join run in applicationDBContext.CalculatorRuns
                                       on odm.Id equals run.CalculatorRunOrganisationDataMasterId
                                   where run.Id == runId
                                       && producerIds.Contains(odd.OrganisationId)
                                       && odd.SubsidiaryId == null
                                   orderby odd.Id descending
                                   select new ParentProducer
                                   {
                                       Id = odd.Id,
                                       ProducerId = odd.OrganisationId,
                                       ProducerName = odd.OrganisationName
                                   }).AsNoTracking().Distinct().ToListAsync(cancellationToken);

            // Get the distinct list of parent producer ids
            var parentProducerIds = parentProducers.Select(pp => pp.ProducerId).Distinct();

            // Identify the producers that still do not have a producer name
            var outstandingProducerIds = producerIds.Where(producerId => !parentProducerIds.Contains(producerId)).ToList();

            // If the list contains any value that means the previous query was not able to identify the producer name
            // This is because for these parent producers, there are no producer detail records because of no pom data submissions
            if (outstandingProducerIds.Any())
            {
                var outstandingParentProducers = await (from p in applicationDBContext.ProducerDetail
                                                  join r in applicationDBContext.CalculatorRuns on p.CalculatorRunId equals r.Id
                                                  where r.FinancialYearId == financialYear
                                                         && !runClassificationsToIgnore.Contains(r.CalculatorRunClassificationId)
                                                         && outstandingProducerIds.Contains(p.ProducerId)
                                                         && p.SubsidiaryId == null
                                                  orderby p.Id descending
                                                  select new ParentProducer
                                                  {
                                                      Id = p.Id,
                                                      ProducerId = p.ProducerId,
                                                      ProducerName = p.ProducerName ?? string.Empty,
                                                  }).AsNoTracking().Distinct().ToListAsync(cancellationToken);

                parentProducers.AddRange(outstandingParentProducers);

                var stillMissingIds = outstandingProducerIds.Where(id => !parentProducers.Any(p => p.ProducerId == id)).ToList();

                // fallback option -- lookup organisation snapshot from previous runs as it was deleted in the pom data and not exists in the producer details
                if (stillMissingIds.Count > 0)
                {
                    var previousRunNames = await (from odd in applicationDBContext.CalculatorRunOrganisationDataDetails
                                            join odm in applicationDBContext.CalculatorRunOrganisationDataMaster on odd.CalculatorRunOrganisationDataMasterId equals odm.Id
                                            join run in applicationDBContext.CalculatorRuns on odm.Id equals run.CalculatorRunOrganisationDataMasterId
                                            where run.FinancialYearId == financialYear
                                                  && stillMissingIds.Contains(odd.OrganisationId)
                                                  && odd.SubsidiaryId == null
                                            orderby odd.Id descending
                                            select new ParentProducer
                                            {
                                                Id = odd.Id,
                                                ProducerId = odd.OrganisationId,
                                                ProducerName = odd.OrganisationName
                                            })
                                            .AsNoTracking()
                                            .Distinct()
                                            .ToListAsync(cancellationToken);

                    parentProducers.AddRange(previousRunNames);
                }
            }

            return parentProducers;
        }

        private async Task PopulatePagedBillingInstructionsAsync(
            IQueryable<ProducerBillingInstructionsDto> query,
            ProducerBillingInstructionsRequestDto requestDto,
            int runId,
            string financialYear,
            ProducerBillingInstructionsResponseDto response,
            CancellationToken cancellationToken)
        {
            var pagedResult = await query
                          .Skip((requestDto.PageNumber!.Value - 1) * requestDto.PageSize!.Value)
                          .Take(requestDto.PageSize.Value)
                          .AsNoTracking()
                          .ToListAsync(cancellationToken);

            var allProducerIdsExcludingIdsWithSuggestedBillingInstructionNoAction = query.Where(x => x.SuggestedBillingInstruction != NoActionPlaceholder).Select(x => x.ProducerId).Distinct();

            var pagedProducerIds = pagedResult.Select(x => x.ProducerId).Distinct();
            var parentProducers = await this.GetParentProducersLatestAsync(runId, financialYear, pagedProducerIds, cancellationToken);

            foreach (var record in pagedResult)
            {
                record.ProducerName = parentProducers
                    .Where(p => p.ProducerId == record.ProducerId)
                    .OrderByDescending(p => p.Id)
                    .FirstOrDefault()?.ProducerName ?? string.Empty;
            }

            response.Records = pagedResult;
            response.AllProducerIds = allProducerIdsExcludingIdsWithSuggestedBillingInstructionNoAction;
        }

        private async Task PopulateBillingStatusCountsAsync(
            IQueryable<ProducerBillingInstructionsStatus> groupedStatus,
            ProducerBillingInstructionsResponseDto response,
            CancellationToken cancellationToken)
        {
            var groupedStatusResult = await groupedStatus.ToListAsync(cancellationToken);

            response.TotalRecords = groupedStatusResult.Sum(s => s.TotalRecords);
            response.TotalAcceptedRecords = groupedStatusResult.FirstOrDefault(s => s.Status == BillingStatus.Accepted.ToString())?.TotalRecords ?? 0;
            response.TotalRejectedRecords = groupedStatusResult.FirstOrDefault(s => s.Status == BillingStatus.Rejected.ToString())?.TotalRecords ?? 0;
            response.TotalPendingRecords = groupedStatusResult.FirstOrDefault(s => s.Status == BillingStatus.Pending.ToString())?.TotalRecords ?? 0;
        }

        private async Task PopulateBillingInstructionCountsAsync(
            IQueryable<ProducerBillingInstructionSuggestion> groupedBillingInstruction,
            ProducerBillingInstructionsResponseDto response,
            CancellationToken cancellationToken)
        {
            var groupedBillingInstructionResult = await groupedBillingInstruction.ToListAsync(cancellationToken);

            response.TotalInitialRecords = groupedBillingInstructionResult.FirstOrDefault(s => string.Equals(s.Suggestion, BillingInstruction.Initial.ToString(), StringComparison.OrdinalIgnoreCase))?.TotalRecords ?? 0;
            response.TotalDeltaRecords = groupedBillingInstructionResult.FirstOrDefault(s => string.Equals(s.Suggestion, BillingInstruction.Delta.ToString(), StringComparison.OrdinalIgnoreCase))?.TotalRecords ?? 0;
            response.TotalRebillRecords = groupedBillingInstructionResult.FirstOrDefault(s => string.Equals(s.Suggestion, BillingInstruction.Rebill.ToString(), StringComparison.OrdinalIgnoreCase))?.TotalRecords ?? 0;
            response.TotalCancelBillRecords = groupedBillingInstructionResult.FirstOrDefault(s => string.Equals(s.Suggestion, BillingInstruction.Cancel.ToString(), StringComparison.OrdinalIgnoreCase))?.TotalRecords ?? 0;
            response.TotalNoActionRecords = groupedBillingInstructionResult.FirstOrDefault(s => string.Equals(s.Suggestion, BillingInstruction.Noaction.ToString(), StringComparison.OrdinalIgnoreCase))?.TotalRecords ?? 0;
        }
    }
}