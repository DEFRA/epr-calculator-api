using System.Net;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Services;

public interface IBillingFileService
{
    Task<BillingFileService.Response> StartGeneratingBillingFileAsync(
        int runId,
        string userName,
        CancellationToken cancellationToken);

    Task<BillingFileService.Response> UpdateProducerBillingInstructionsAsync(
        int runId,
        string userName,
        ProduceBillingInstuctionRequestDto produceBillingInstuctionRequestDto,
        CancellationToken cancellationToken);

    /// <summary>
    ///     Gets the billing file name from the calculator_run_billing_file_metadata based on the run ID.
    ///     and then calls blob storage to move the json file to the FSS container.
    /// </summary>
    /// <param name="runId">The calculator run id.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="string" /> containing the name of the json billing file.</returns>
    Task<bool> MoveBillingJsonFile(int runId, CancellationToken cancellationToken);

    /// <summary>
    ///     Gets producer billing instructions for a given calculator run.
    /// </summary>
    /// <param name="runId">The calculator run id.</param>
    /// <param name="requestDto">The request payload.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A <see cref="ProducerBillingInstructionsResponseDto" /> response containing records and pagination data.</returns>
    Task<ProducerBillingInstructionsResponseDto?> GetProducerBillingInstructionsAsync(int runId, ProducerBillingInstructionsRequestDto requestDto, CancellationToken cancellationToken);
}

public class BillingFileService(
    ApplicationDBContext dbContext,
    IBlobStorageService blobStorage
) : IBillingFileService
{
    private const string NoActionPlaceholder = "-";

    public async Task<Response> UpdateProducerBillingInstructionsAsync(
        int runId,
        string userName,
        ProduceBillingInstuctionRequestDto produceBillingInstuctionRequestDto,
        CancellationToken cancellationToken)
    {
        var calculatorRun = await dbContext.CalculatorRuns
            .SingleOrDefaultAsync(x => x.Id == runId && AcceptableRunStatusForBillingInstructions().Contains(x.CalculatorRunClassificationId), cancellationToken)
            .ConfigureAwait(false);

        if (calculatorRun is null)
        {
            return new Response
            {
                StatusCode = HttpStatusCode.UnprocessableContent,
                Message = CommonResources.InvalidRunId
            };
        }

        var rows = await dbContext.ProducerResultFileSuggestedBillingInstruction
            .Where(x => produceBillingInstuctionRequestDto.OrganisationIds.Contains(x.ProducerId) && x.CalculatorRunId == runId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        if (rows.Count < produceBillingInstuctionRequestDto.OrganisationIds.Count())
        {
            return new Response
            {
                StatusCode = HttpStatusCode.UnprocessableContent,
                Message = CommonResources.InvalidOrganisationId
            };
        }

        foreach (var row in rows)
            UpdateBillingInstruction(userName, produceBillingInstuctionRequestDto, row);

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new Response
        {
            StatusCode = HttpStatusCode.NoContent
        };
    }

    public async Task<bool> MoveBillingJsonFile(int runId, CancellationToken cancellationToken)
    {
        var billingFileMetaData =
            await dbContext.CalculatorRunBillingFileMetadata.Where(m => m.CalculatorRunId == runId)
                .OrderByDescending(m => m.BillingFileCreatedDate)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

        if (billingFileMetaData == null || string.IsNullOrEmpty(billingFileMetaData.BillingJsonFileName))
            return false;

        return await blobStorage.MoveBillingJsonToFss(billingFileMetaData.BillingJsonFileName, cancellationToken);
    }

    public async Task<ProducerBillingInstructionsResponseDto?> GetProducerBillingInstructionsAsync(
        int runId,
        ProducerBillingInstructionsRequestDto requestDto,
        CancellationToken cancellationToken)
    {
        var run = await GetRunAsync(runId, cancellationToken);

        if (run == null)
            return null;

        var searchQuery = requestDto.SearchQuery;

        var baseQuery = dbContext.ProducerResultFileSuggestedBillingInstruction
            .Where(prsi => prsi.CalculatorRunId == runId)
            .Select(prsi => new
            {
                prsi,
                PendingStatus = (string.IsNullOrWhiteSpace(prsi.SuggestedBillingInstruction) || prsi.SuggestedBillingInstruction.Trim() == NoActionPlaceholder)
                    ? BillingInstructionAction.Noaction.ToString()
                    : BillingStatus.Pending.ToString()
            });

        var query = from x in baseQuery
            select new ProducerBillingInstructionsDto
            {
                ProducerId = x.prsi.ProducerId,
                BillingInstructionAcceptReject =
                    string.IsNullOrWhiteSpace(x.prsi.BillingInstructionAcceptReject)
                        ? x.PendingStatus
                        : x.prsi.BillingInstructionAcceptReject.Trim(),
                SuggestedBillingInstruction = x.prsi.SuggestedBillingInstruction,
                SuggestedInvoiceAmount = (x.prsi.SuggestedBillingInstruction ?? string.Empty).Equals("cancel", StringComparison.CurrentCultureIgnoreCase) ? x.prsi.CurrentYearInvoiceTotalToDate : x.prsi.SuggestedInvoiceAmount
            };

        // Group by on BillingInstructionAcceptReject before filtering
        var groupedStatus = baseQuery
            .AsNoTracking()
            .GroupBy(x => string.IsNullOrWhiteSpace(x.prsi.BillingInstructionAcceptReject)
                ? x.PendingStatus
                : x.prsi.BillingInstructionAcceptReject.Trim())
            .Select(g => new ProducerBillingInstructionsStatus
            {
                Status = g.Key,
                TotalRecords = g.Count()
            });

            // Group by on BillingInstructionAcceptReject before filtering
        var groupedBillingInstruction = query
                    .AsNoTracking()
                    .GroupBy(x => string.IsNullOrWhiteSpace(x.SuggestedBillingInstruction) || x.SuggestedBillingInstruction.Trim() == NoActionPlaceholder
                        ? BillingInstructionAction.Noaction.ToString()
                        : x.SuggestedBillingInstruction.Trim())
                    .Select(g => new ProducerBillingInstructionSuggestion
                    {
                        Suggestion = g.Key,
                        TotalRecords = g.Count(),
                    });

        // Apply OrganisationId filter if provided
        query = query.ApplyOrganisationIdFilter(searchQuery);

        // Apply Status filter if provided and not empty
        query = query.ApplyStatusFilter(searchQuery);

        // Apply BillingInstruction filter if provided and not empty
        query = query.ApplyBillingInstructionFilter(searchQuery);

        query = query.OrderBy(x => x.ProducerId).AsQueryable();

        requestDto.PageNumber ??= DetermineProducerBillingInstructionsPageNumber();
        requestDto.PageSize ??= DetermineProducerBillingInstructionsPageSize();

        var response = new ProducerBillingInstructionsResponseDto
        {
            PageNumber = requestDto.PageNumber,
            PageSize = requestDto.PageSize,
            RunName = run.Name,
            CalculatorRunId = run.Id,
            TotalRecords = await query.CountAsync(cancellationToken)
        };

        await PopulatePagedBillingInstructionsAsync(query, requestDto, run.Id, run.RelativeYear, response, cancellationToken);
        await PopulateBillingStatusCountsAsync(groupedStatus, response, cancellationToken);
        await PopulateBillingInstructionCountsAsync(groupedBillingInstruction, response, cancellationToken);

        return response;
    }

    public async Task<Response> StartGeneratingBillingFileAsync(
        int runId,
        string userName,
        CancellationToken cancellationToken)
    {
        var calculatorRun = await dbContext.CalculatorRuns
            .SingleOrDefaultAsync(run => run.Id == runId, cancellationToken)
            .ConfigureAwait(false);

        if (calculatorRun is null)
        {
            return new Response
            {
                StatusCode = HttpStatusCode.UnprocessableContent,
                Message = CommonResources.InvalidRunId
            };
        }

        if (!ValidateRunForAcceptAllBillingInstructions(calculatorRun))
        {
            return new Response
            {
                StatusCode = HttpStatusCode.UnprocessableContent,
                Message = CommonResources.InvalidRunStatusForAcceptAll
            };
        }

        calculatorRun.BillingRunStatus = BillingRunStatus.Running;
        calculatorRun.BillingRunStartedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return new Response
        {
            StatusCode = HttpStatusCode.OK
        };
    }

    /// <summary>
    ///     Validates the run ID for accepting all billing instructions.
    /// </summary>
    /// <param name="run">calculation Run</param>
    /// <returns>bool</returns>
    private static bool ValidateRunForAcceptAllBillingInstructions(CalculatorRun run)
    {
        // Valid if:
        // * Billing file has not been sent to FSS (i.e. classification is not 'Completed')
        // * Not already Running OR has been for more than 1 hour (i.e. 'stuck' due to unclean shutdown of the processor)
        return AcceptableRunStatusForBillingInstructions().Contains(run.CalculatorRunClassificationId)
               && (run.BillingRunStatus != BillingRunStatus.Running
                   || run.BillingRunStartedAt?.AddHours(1) < DateTime.UtcNow);
    }

    private static int DetermineProducerBillingInstructionsPageSize() => int.TryParse(CommonResources.ProducerBillingInstructionsDefaultPageSize, out var pageSize) ? pageSize : 10;

    private static int DetermineProducerBillingInstructionsPageNumber() => int.TryParse(CommonResources.ProducerBillingInstructionsDefaultPageNumber, out var pageNumber) ? pageNumber : 1;

    private static void UpdateBillingInstruction(
        string userName,
        ProduceBillingInstuctionRequestDto produceBillingInstuctionRequestDto,
        ProducerResultFileSuggestedBillingInstruction row)
    {
        row.BillingInstructionAcceptReject = produceBillingInstuctionRequestDto.Status;

        if (string.Equals(produceBillingInstuctionRequestDto.Status, BillingStatus.Rejected.ToString()))
            row.ReasonForRejection = produceBillingInstuctionRequestDto.ReasonForRejection;
        else
            row.ReasonForRejection = null;

        row.LastModifiedAcceptReject = DateTime.UtcNow;
        row.LastModifiedAcceptRejectBy = userName;
    }


    private Task<CalculatorRun?> GetRunAsync(int runId, CancellationToken cancellationToken) =>
        dbContext.CalculatorRuns
            .SingleOrDefaultAsync(x => x.Id == runId, cancellationToken);

    /// <summary>
    ///     Get the producer names for the list of producer ids provided.
    /// </summary>
    /// <param name="runId">Current run id.</param>
    /// <param name="relativeYear">year of the current run.</param>
    /// <param name="producerIds">List of procuder ids requiring producer name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task list of parent producers with producer name.</returns>
    private async Task<List<ParentProducer>> GetParentProducersLatestAsync(
        int runId,
        RelativeYear relativeYear,
        IEnumerable<int> producerIds,
        CancellationToken cancellationToken)
    {
        var runClassificationsToIgnore = new List<int>
        {
            (int)RunClassification.INTHEQUEUE,
            (int)RunClassification.RUNNING,
            (int)RunClassification.TEST_RUN,
            (int)RunClassification.ERROR,
            (int)RunClassification.DELETED
        };

        var parentProducers = await (from odd in dbContext.CalculatorRunOrganisationDataDetails
            join odm in dbContext.CalculatorRunOrganisationDataMaster
                on odd.CalculatorRunOrganisationDataMasterId equals odm.Id
            join run in dbContext.CalculatorRuns
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
        if (outstandingProducerIds.Count > 0)
        {
            var outstandingParentProducers = await (from p in dbContext.ProducerDetail
                join r in dbContext.CalculatorRuns on p.CalculatorRunId equals r.Id
                where r.RelativeYear == relativeYear
                      && !runClassificationsToIgnore.Contains(r.CalculatorRunClassificationId)
                      && outstandingProducerIds.Contains(p.ProducerId)
                      && p.SubsidiaryId == null
                orderby p.Id descending
                select new ParentProducer
                {
                    Id = p.Id,
                    ProducerId = p.ProducerId,
                    ProducerName = p.ProducerName ?? string.Empty
                }).AsNoTracking().Distinct().ToListAsync(cancellationToken);

            parentProducers.AddRange(outstandingParentProducers);

            var stillMissingIds = outstandingProducerIds.Where(id => !parentProducers.Exists(p => p.ProducerId == id)).ToList();

            // fallback option -- lookup organisation snapshot from previous runs as it was deleted in the pom data and not exists in the producer details
            if (stillMissingIds.Count > 0)
            {
                var previousRunNames = await (from odd in dbContext.CalculatorRunOrganisationDataDetails
                        join odm in dbContext.CalculatorRunOrganisationDataMaster on odd.CalculatorRunOrganisationDataMasterId equals odm.Id
                        join run in dbContext.CalculatorRuns on odm.Id equals run.CalculatorRunOrganisationDataMasterId
                        where run.RelativeYear == relativeYear.Value
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
        RelativeYear relativeYear,
        ProducerBillingInstructionsResponseDto response,
        CancellationToken cancellationToken)
    {
        var pagedResult = await query
            .Skip((requestDto.PageNumber!.Value - 1) * requestDto.PageSize!.Value)
            .Take(requestDto.PageSize.Value)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var allProducerIdsExcludingIdsWithSuggestedBillingInstructionNoAction =
            await query
                .Where(x => x.SuggestedBillingInstruction != NoActionPlaceholder)
                .Select(x => x.ProducerId)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync(cancellationToken);

        var pagedProducerIds = pagedResult.Select(x => x.ProducerId).Distinct();
        var parentProducers = await GetParentProducersLatestAsync(runId, relativeYear, pagedProducerIds, cancellationToken);

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

    private static async Task PopulateBillingStatusCountsAsync(
        IQueryable<ProducerBillingInstructionsStatus> groupedStatus,
        ProducerBillingInstructionsResponseDto response,
        CancellationToken cancellationToken)
    {
        var groupedStatusResult = await groupedStatus.ToListAsync(cancellationToken);

        response.TotalAcceptedRecords = groupedStatusResult.Find(s => s.Status == BillingStatus.Accepted.ToString())?.TotalRecords ?? 0;
        response.TotalRejectedRecords = groupedStatusResult.Find(s => s.Status == BillingStatus.Rejected.ToString())?.TotalRecords ?? 0;
        response.TotalPendingRecords = groupedStatusResult.Find(s => s.Status == BillingStatus.Pending.ToString())?.TotalRecords ?? 0;
    }

    private static async Task PopulateBillingInstructionCountsAsync(
        IQueryable<ProducerBillingInstructionSuggestion> groupedBillingInstruction,
        ProducerBillingInstructionsResponseDto response,
        CancellationToken cancellationToken)
    {
        var groupedBillingInstructionResult = await groupedBillingInstruction.ToListAsync(cancellationToken);

        response.TotalInitialRecords = groupedBillingInstructionResult.Find(s => string.Equals(s.Suggestion, BillingInstructionAction.Initial.ToString(), StringComparison.OrdinalIgnoreCase))?.TotalRecords ?? 0;
        response.TotalDeltaRecords = groupedBillingInstructionResult.Find(s => string.Equals(s.Suggestion, BillingInstructionAction.Delta.ToString(), StringComparison.OrdinalIgnoreCase))?.TotalRecords ?? 0;
        response.TotalRebillRecords = groupedBillingInstructionResult.Find(s => string.Equals(s.Suggestion, BillingInstructionAction.Rebill.ToString(), StringComparison.OrdinalIgnoreCase))?.TotalRecords ?? 0;
        response.TotalCancelBillRecords = groupedBillingInstructionResult.Find(s => string.Equals(s.Suggestion, BillingInstructionAction.Cancel.ToString(), StringComparison.OrdinalIgnoreCase))?.TotalRecords ?? 0;
        response.TotalNoActionRecords = groupedBillingInstructionResult.Find(s => string.Equals(s.Suggestion, BillingInstructionAction.Noaction.ToString(), StringComparison.OrdinalIgnoreCase))?.TotalRecords ?? 0;
    }

    private static IEnumerable<int> AcceptableRunStatusForBillingInstructions() =>
        [
            (int)RunClassification.INITIAL_RUN,
            (int)RunClassification.INTERIM_RECALCULATION_RUN,
            (int)RunClassification.FINAL_RUN,
            (int)RunClassification.FINAL_RECALCULATION_RUN,
        ];

    public record Response
    {
        /// <summary>
        ///     The HTTP status code representing the result of the service process.
        /// </summary>
        public required HttpStatusCode StatusCode { get; init; }

        /// <summary>
        ///     The message providing details about the service process.
        /// </summary>
        public string? Message { get; init; }
    }
}
