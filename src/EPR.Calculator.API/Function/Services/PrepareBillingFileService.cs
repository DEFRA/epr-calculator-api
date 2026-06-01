using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Logging;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Utils;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Services;

public interface IPrepareBillingFileService
{
    Task<PreparedResult<(string CsvFileName, string JsonFileName)>> PrepareBillingFileAsync(int calculatorRunId, string runName, string approvedBy);
}

public class PrepareBillingFileService(
    ApplicationDBContext dbContext,
    IPrepareCalcService prepareCalcService,
    ILogger<PrepareBillingFileService> logger)
    : IPrepareBillingFileService
{
    public Task<PreparedResult<(string CsvFileName, string JsonFileName)>> PrepareBillingFileAsync(int calculatorRunId, string runName, string approvedBy) =>
        logger.LogDuration(async () =>
        {
            try
            {
                var run = await dbContext.CalculatorRuns
                    .Where(run => run.Id == calculatorRunId && (run.IsBillingFileGenerating ?? false))
                    .SingleAsync(x => x.Id == calculatorRunId);

                var acceptedProducerIds = await GetAcceptedProducerIdsAsync(run.Id);
                var request = new CalcResultsRequestDto
                {
                    RunId = calculatorRunId,
                    AcceptedProducerIds = acceptedProducerIds,
                    IsBillingFile = true,
                    ApprovedBy = approvedBy,
                    RelativeYear = run.RelativeYear
                };

                return await prepareCalcService.PrepareBillingResultsAsync(request, runName, CancellationToken.None);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Error occurred while preparing the billing file");
                return PreparedResult.Failure<(string, string)>();
            }
        });

    private async Task<ImmutableHashSet<int>> GetAcceptedProducerIdsAsync(int calculatorRunId)
    {
        return await dbContext.ProducerResultFileSuggestedBillingInstruction
            .Where(x => x.CalculatorRunId == calculatorRunId
                        && x.BillingInstructionAcceptReject == PrepareBillingFileConstants.BillingInstructionAccepted
                        && x.SuggestedBillingInstruction.Trim().ToLower() != PrepareBillingFileConstants.SuggestedBillingInstructionCancelBill.Trim().ToLower())
            .Select(x => x.ProducerId)
            .Distinct()
            .ToImmutableHashSetAsync();
    }
}
