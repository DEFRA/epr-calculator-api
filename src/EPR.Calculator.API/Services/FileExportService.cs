
using System.Diagnostics.CodeAnalysis;
using System.Text;
using EPR.Calculator.API.BackgroundService.Builder.Detail;
using EPR.Calculator.API.BackgroundService.Builder.ErrorReport;
using EPR.Calculator.API.BackgroundService.Builder.RejectedProducers;
using EPR.Calculator.API.BackgroundService.Constants;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter;
using EPR.Calculator.API.BackgroundService.Exporter.JsonExporter;
using EPR.Calculator.API.BackgroundService.Features.BillingRuns.Constants;
using EPR.Calculator.API.BackgroundService.Features.BillingRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Features.Common;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.BackgroundService.Telemetry;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Services;

public interface IFileExportService
{
    Task<FileExportResult> Export(int runId, RunType runType, FileExportType fileType, CancellationToken cancellationToken);
}

public enum FileExportType { Csv, Json }


public abstract record FileExportResult
{
    private FileExportResult() { }
    public sealed record Exported(byte[] Content, string FileName) : FileExportResult;
    public sealed record NotFound() : FileExportResult;
    public sealed record NotCached() : FileExportResult;
}

[SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
public class FileExportService(
    ICalcResultDetailBuilder calcResultDetailBuilder,
    ICalcResultErrorReportBuilder errorReportBuilder,
    ICalcResultRejectedProducersBuilder rejectedProducersBuilder,
    ICalcResultReader calcResultReader,
    ApplicationDBContext dbContext,
    IParameterService parameterService,
    ICalcResultsExporter resultsFileExporter,
    IBillingFileExporter billingFileExporter,
    IBillingFileJsonWriter billingJsonWriter
)  : IFileExportService
{
    private static readonly ImmutableHashSet<int> NonDownloadableClassifications = [
        RunClassificationStatusIds.INTHEQUEUEID,
        RunClassificationStatusIds.RUNNINGID,
        RunClassificationStatusIds.ERRORID,
        RunClassificationStatusIds.DELETEDID
    ];

    [ActivityMetric(nameof(Metrics.FileExportDuration), threshold: "00:00:10")]
    public Task<FileExportResult> Export(int runId, RunType runType, FileExportType fileType, CancellationToken cancellationToken)
    {
        return runType switch
        {
            RunType.Calculator => ExportResultCsv(runId, cancellationToken),
            RunType.Billing => ExportBilling(runId, fileType, cancellationToken),
            _ => Task.FromResult<FileExportResult>(new FileExportResult.NotFound())
        };
    }

    private async Task<FileExportResult> ExportResultCsv(int runId, CancellationToken cancellationToken)
    {
        var runContext = await GetCalculatorRunContext(runId, cancellationToken);

        if(runContext is null)
            return new FileExportResult.NotFound();

        var result = await GetResult(runContext, cancellationToken);

        if (result is null)
            return new FileExportResult.NotCached();

        var content = await resultsFileExporter.Export(runContext, result);
        return new FileExportResult.Exported(Encoding.UTF8.GetBytes(content), $"{runContext.RunName}.csv");
    }

    private async Task<FileExportResult> ExportBilling(int runId, FileExportType billingFileType, CancellationToken cancellationToken)
    {
        var runContext = await GetBillingRunContext(runId, cancellationToken);

        if(runContext is null)
            return new FileExportResult.NotFound();

        var result = await GetResult(runContext, cancellationToken);

        if (result is null)
            return new FileExportResult.NotCached();

        var filteredResult = FilterResult(runId, result, runContext.AcceptedProducerIds);
        return billingFileType switch
        {
            FileExportType.Csv => new FileExportResult.Exported(
                Encoding.UTF8.GetBytes(await billingFileExporter.Export(runContext, filteredResult)),
                $"{runContext.RunName}.csv"
            ),
            FileExportType.Json => new FileExportResult.Exported(
                Encoding.UTF8.GetBytes(await billingJsonWriter.WriteToString(runContext, filteredResult)),
                $"{runContext.RunName}.json"
            ),
            _ => throw new ArgumentOutOfRangeException(nameof(billingFileType), billingFileType, null)
        };
    }

    private async Task<CalculatorRunContext?> GetCalculatorRunContext(int runId, CancellationToken cancellationToken)
    {
        var run = await dbContext.CalculatorRuns.AsNoTracking().SingleOrDefaultAsync(x => x.Id == runId, cancellationToken);

        if(run is null || NonDownloadableClassifications.Contains(run.CalculatorRunClassificationId))
            return null;

        return new CalculatorRunContext
        {
            RunId = run.Id,
            RunName = run.Name.Trim(),
            ProcessingStartedAt = run.CreatedAt,
            RelativeYear = run.RelativeYear,
            User = run.CreatedBy,
            DefaultParameters = await parameterService.GetDefaultParameters(runId)
        };
    }

    private async Task<BillingRunContext?> GetBillingRunContext(int runId, CancellationToken cancellationToken) {
        var run = await dbContext
            .CalculatorRuns
            .AsNoTracking()
            .Include(r => r.ProducerResultFileSuggestedBillingInstruction)
            .Include(r => r.CalculatorRunBillingFileMetadata)
            .SingleOrDefaultAsync(r => r.Id == runId, cancellationToken);

        if (run is null || run.BillingRunStatus != BillingRunStatus.Completed || run.CalculatorRunClassificationId == RunClassificationStatusIds.DELETEDID)
            return null;

        var billingFileMetadata = run.CalculatorRunBillingFileMetadata
            .OrderByDescending(m => m.BillingFileCreatedDate)
            .FirstOrDefault();

        if (billingFileMetadata is null)
            return null;

        return new BillingRunContext
        {
            RunId = run.Id,
            RunName = run.Name.Trim(),
            ProcessingStartedAt = new DateTimeOffset(DateTime.SpecifyKind(billingFileMetadata.BillingFileCreatedDate, DateTimeKind.Utc)),
            RelativeYear = run.RelativeYear,
            User = billingFileMetadata.BillingFileCreatedBy,
            AcceptedProducerIds = run.ProducerResultFileSuggestedBillingInstruction
                                    .Where(p =>
                                        p.BillingInstructionAcceptReject == BillingConstants.Action.Accepted
                                        && p.SuggestedBillingInstruction != BillingConstants.Suggestion.Cancel)
                                    .Select(p => p.ProducerId)
                                    .Distinct()
                                    .ToImmutableHashSet(),
            DefaultParameters = await parameterService.GetDefaultParameters(run.Id)
        };
    }

    private async Task<CalcResult?> GetResult(RunContext runContext, CancellationToken cancellationToken)
    {
        var hasData = await dbContext.ProducerDisposalFee.AnyAsync(f => f.CalculatorRunId == runContext.RunId, cancellationToken);

        if(!hasData)
            return null;

        var result = CalcResult.Empty;

        result.CalcResultDetail = await calcResultDetailBuilder.ConstructAsync(runContext, cancellationToken);
        result.CalcResultLapcapData = await calcResultReader.ReadLapcapData(runContext.RunId, cancellationToken);
        result.CalcResultLateReportingTonnageData = await calcResultReader.ReadLateReportingTonnage(runContext.RunId, cancellationToken);
        result.CalcResultParameterOtherCost = await calcResultReader.ReadParameterOtherCost(runContext.RunId, cancellationToken);
        result.CalcResultOnePlusFourApportionment = await calcResultReader.ReadOnePlusFourApportionment(runContext.RunId, cancellationToken);

        if (runContext.RequiresModulation)
        {
            result.CalcResultProjectedProducers.H1ProjectedProducers = await calcResultReader.ReadH1ProjectedData(runContext.RunId, cancellationToken);
            result.CalcResultProjectedProducers.H2ProjectedProducers = await  calcResultReader.ReadH2ProjectedData(runContext.RunId, cancellationToken);
        }

        if (runContext.RequiresScaling)
            result.CalcResultScaledupProducers.ScaledupProducers = await calcResultReader.ReadScaledData(runContext.RunId, cancellationToken);

        result.CalcResultPartialObligations.PartialObligations = await calcResultReader.ReadPartialData(runContext.RunId, cancellationToken);
        result.CalcResultCancelledProducers = await calcResultReader.ReadCancelledProducers(runContext.RunId, cancellationToken);
        result.Smcw = await calcResultReader.ReadSmcw(runContext.RunId, cancellationToken);
        result.CalcResultLaDisposalCostData = await calcResultReader.ReadLaDisposalCostData(runContext.RunId, cancellationToken);
        result.CalcResultCommsCostReportDetail = await calcResultReader.ReadCommsCost(runContext.RunId, cancellationToken);

        if(runContext.RunType is RunType.Billing)
            result.CalcResultRejectedProducers = await rejectedProducersBuilder.ConstructAsync(runContext, cancellationToken);

        if (runContext.RequiresModulation)
            result.CalcResultModulation = await calcResultReader.ReadModulationResult(runContext.RunId, cancellationToken);

        result.ProducerFees = await calcResultReader.ReadProducerFees(runContext.RunId, cancellationToken);

        if (runContext.RunType is RunType.Calculator)
            result.CalcResultErrorReports = errorReportBuilder.Construct(runContext);

        return result;
    }

    private static CalcResult FilterResult(int runId, CalcResult calcResult, ImmutableHashSet<int> acceptedProducerIds)
    {
        ImmutableList<T> FilterAccepted<T>(IEnumerable<T> producers, Func<T, int> producerId) =>
                producers.Where(producer => acceptedProducerIds.Contains(producerId(producer))).ToImmutableList();

        var rejectedProducerIds = calcResult.CalcResultRejectedProducers.Select(r => r.ProducerId).ToHashSet();

        return calcResult with
        {
            CalcResultProjectedProducers = calcResult.CalcResultProjectedProducers with
            {
                H1ProjectedProducers = FilterAccepted(calcResult.CalcResultProjectedProducers.H1ProjectedProducers, p => p.ProducerId),
                H2ProjectedProducers = FilterAccepted(calcResult.CalcResultProjectedProducers.H2ProjectedProducers, p => p.ProducerId)
            },
            CalcResultScaledupProducers = calcResult.CalcResultScaledupProducers with { ScaledupProducers = FilterAccepted(calcResult.CalcResultScaledupProducers.ScaledupProducers, p => p.ProducerId) },
            CalcResultPartialObligations = calcResult.CalcResultPartialObligations with { PartialObligations = FilterAccepted(calcResult.CalcResultPartialObligations.PartialObligations, p => p.ProducerId) },
            ProducerFees = new ProducerFees
            {
                CalculatorRunId = runId,
                Details         = FilterAccepted(calcResult.ProducerFees.Details, p => p.FeeDetail.ProducerId),
                Total           = BillingTotal(calcResult.ProducerFees.Total)
            },
            CalcResultCancelledProducers = calcResult.CalcResultCancelledProducers.Where(p => !rejectedProducerIds.Contains(p.ProducerId)).ToList()
        };
    }

    private static FeeDetail BillingTotal(FeeDetail total) => new()
    {
        ProducerId                               = 0,
        SubsidiaryId                             = string.Empty,
        ProducerName                             = string.Empty,
        TradingName                              = string.Empty,
        StatusCode                               = string.Empty,
        JoinerDate                               = string.Empty,
        LeaverDate                               = CommonConstants.Totals,
        TonnageChangeCount                       = string.Empty,
        TonnageChangeAdvice                      = string.Empty,
        BillingInstruction                       = new BillingInstruction { SuggestedBillingInstruction = string.Empty },
        LADisposalCostsSection1                  = total.LADisposalCostsSection1,
        CommsCostsSection2a                      = total.CommsCostsSection2a,
        CommsCostsSection2b                      = total.CommsCostsSection2b,
        CommsCostsSection2c                      = total.CommsCostsSection2c,
        SaOperatingCostsSection3                 = total.SaOperatingCostsSection3,
        LaDataPrepSection4                       = total.LaDataPrepSection4,
        SaSetupCostsSection5                     = total.SaSetupCostsSection5,
        TotalOnePlus2A2B2CWithBadDebtPercentage  = total.TotalOnePlus2A2B2CWithBadDebtPercentage
    };

}
