using System.Diagnostics.CodeAnalysis;
using System.Text;
using EPR.Calculator.API.BackgroundService.Builder.Detail;
using EPR.Calculator.API.BackgroundService.Builder.ErrorReport;
using EPR.Calculator.API.BackgroundService.Builder.RejectedProducers;
using EPR.Calculator.API.BackgroundService.Constants;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter;
using EPR.Calculator.API.BackgroundService.Exporter.JsonExporter;
using EPR.Calculator.API.BackgroundService.Features.BillingRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Features.Common;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.BackgroundService.Exporter;

/// <summary>
///     A run's file, ready to be written: its name, and a function that writes its complete content (including any byte
///     order mark) to a stream.
/// </summary>
public sealed record RunFile(string FileName, Func<Stream, CancellationToken, Task> WriteTo);

/// <summary>
///     Builds a run's results/billing files. Shared by the runs (which write them to blob storage) and file downloads, so
///     both produce identical files.
/// </summary>
public interface IRunFileExporter
{
    /// <summary>
    ///     The results CSV for a result that's already in memory (i.e. from the calculator run that built it).
    /// </summary>
    RunFile GetResultsCsv(CalculatorRunContext runContext, CalcResult calcResult, IEnumerable<FeeDetail> producerFeeDetails);

    /// <returns>The results CSV from the run's stored results, or null if it has none (i.e. a legacy run).</returns>
    Task<RunFile?> GetResultsCsv(CalculatorRunContext runContext, CancellationToken cancellationToken);

    /// <summary>
    ///     The run's stored results filtered for billing (to its accepted producers) - everything both billing files need,
    ///     so the billing run can build it once and write both from it.
    /// </summary>
    /// <returns>The billing result, or null if the run has no stored results (i.e. a legacy run).</returns>
    Task<CalcResult?> GetBillingResult(BillingRunContext runContext, CancellationToken cancellationToken);

    /// <summary>
    ///     The billing CSV for a billing result that's already in memory (i.e. from <see cref="GetBillingResult" />).
    /// </summary>
    RunFile GetBillingCsv(BillingRunContext runContext, CalcResult billingResult, IEnumerable<FeeDetail> acceptedFeeDetails);

    /// <summary>
    ///     The billing JSON for a billing result that's already in memory (i.e. from <see cref="GetBillingResult" />).
    /// </summary>
    RunFile GetBillingJson(BillingRunContext runContext, CalcResult billingResult, IEnumerable<FeeDetail> acceptedFeeDetails);

    /// <returns>The billing CSV from the run's stored results, or null if it has none (i.e. a legacy run).</returns>
    Task<RunFile?> GetBillingCsv(BillingRunContext runContext, CancellationToken cancellationToken);

    /// <returns>The billing JSON from the run's stored results, or null if it has none (i.e. a legacy run).</returns>
    Task<RunFile?> GetBillingJson(BillingRunContext runContext, CancellationToken cancellationToken);
}

[SuppressMessage("Major Code Smell", "S107:Methods should not have too many parameters", Justification = "This is suppressed for now and will be refactored later.")]
public class RunFileExporter(
    ICalcResultDetailBuilder calcResultDetailBuilder,
    ICalcResultErrorReportBuilder errorReportBuilder,
    ICalcResultRejectedProducersBuilder rejectedProducersBuilder,
    ICalcResultReader calcResultReader,
    ApplicationDBContext dbContext,
    ICalcResultsExporter resultsFileExporter,
    IBillingFileExporter billingFileExporter,
    IBillingFileJsonWriter billingJsonWriter
) : IRunFileExporter
{
    private enum FileType { ResultsCsv, BillingCsv, BillingJson }

    private static readonly UTF8Encoding Utf8WithBom = new(encoderShouldEmitUTF8Identifier: true);

    public RunFile GetResultsCsv(CalculatorRunContext runContext, CalcResult calcResult, IEnumerable<FeeDetail> producerFeeDetails)
    {
        var detail = calcResult.CalcResultDetail;
        var fileName = new CalcResultsAndBillingFileName(detail.RunId, detail.RunName.Trim(), detail.RunDate);

        return new RunFile(
            fileName,
            (stream, ct) => WriteCsv(stream, writer => resultsFileExporter.Export(runContext, calcResult, producerFeeDetails, writer), ct));
    }

    public async Task<RunFile?> GetResultsCsv(CalculatorRunContext runContext, CancellationToken cancellationToken)
    {
        var result = await GetResult(runContext, FileType.ResultsCsv, cancellationToken);

        return result is null
            ? null
            : GetResultsCsv(runContext, result, calcResultReader.StreamProducerFeeDetails(runContext.RunId));
    }

    // The billing CSV needs a superset of the billing JSON's data, so this serves both
    public Task<CalcResult?> GetBillingResult(BillingRunContext runContext, CancellationToken cancellationToken) =>
        GetFilteredResult(runContext, FileType.BillingCsv, cancellationToken);

    public RunFile GetBillingCsv(BillingRunContext runContext, CalcResult billingResult, IEnumerable<FeeDetail> acceptedFeeDetails)
    {
        var fileName = new CalcResultsAndBillingFileName(runContext.RunId, runContext.RunName, runContext.ProcessingStartedAt.UtcDateTime, isDraftBillingFile: true);

        return new RunFile(
            fileName,
            (stream, ct) => WriteCsv(stream, writer => billingFileExporter.Export(runContext, billingResult, acceptedFeeDetails, writer), ct));
    }

    public RunFile GetBillingJson(BillingRunContext runContext, CalcResult billingResult, IEnumerable<FeeDetail> acceptedFeeDetails) =>
        new(
            new CalcResultsAndBillingFileName(runContext.RunId),
            (stream, ct) => billingJsonWriter.WriteTo(stream, runContext, billingResult, acceptedFeeDetails, ct));

    public async Task<RunFile?> GetBillingCsv(BillingRunContext runContext, CancellationToken cancellationToken)
    {
        var result = await GetFilteredResult(runContext, FileType.BillingCsv, cancellationToken);

        return result is null ? null : GetBillingCsv(runContext, result, StreamAcceptedFeeDetails(runContext));
    }

    public async Task<RunFile?> GetBillingJson(BillingRunContext runContext, CancellationToken cancellationToken)
    {
        // Reads only what the billing JSON needs, rather than GetBillingResult's superset
        var result = await GetFilteredResult(runContext, FileType.BillingJson, cancellationToken);

        return result is null ? null : GetBillingJson(runContext, result, StreamAcceptedFeeDetails(runContext));
    }

    private IEnumerable<FeeDetail> StreamAcceptedFeeDetails(BillingRunContext runContext) =>
        calcResultReader.StreamProducerFeeDetails(runContext.RunId)
            .Where(d => runContext.AcceptedProducerIds.Contains(d.ProducerId));

    private static async Task WriteCsv(Stream stream, Func<TextWriter, Task> export, CancellationToken cancellationToken)
    {
        await using var writer = new StreamWriter(stream, Utf8WithBom, bufferSize: 64 * 1024, leaveOpen: true);
        await export(writer);
        await writer.FlushAsync(cancellationToken);
    }

    private async Task<CalcResult?> GetFilteredResult(BillingRunContext runContext, FileType fileType, CancellationToken cancellationToken)
    {
        var result = await GetResult(runContext, fileType, cancellationToken);
        return result is null ? null : FilterResult(result, runContext.AcceptedProducerIds);
    }

    private async Task<CalcResult?> GetResult(RunContext runContext, FileType fileType, CancellationToken cancellationToken)
    {
        // AsNoTracking() not needed: AnyAsync returns a bool.
        var hasData = await dbContext.ProducerDisposalFee.AnyAsync(f => f.CalculatorRunId == runContext.RunId, cancellationToken);

        if (!hasData)
            return null;

        var result = CalcResult.Empty;

        result.CalcResultDetail = await calcResultDetailBuilder.ConstructAsync(runContext, cancellationToken);
        result.CalcResultLapcapData = await calcResultReader.ReadLapcapData(runContext.RunId, cancellationToken);
        result.CalcResultLateReportingTonnageData = await calcResultReader.ReadLateReportingTonnage(runContext.RunId, cancellationToken);
        result.CalcResultParameterOtherCost = await calcResultReader.ReadParameterOtherCost(runContext.RunId, cancellationToken);
        result.CalcResultOnePlusFourApportionment = await calcResultReader.ReadOnePlusFourApportionment(runContext.RunId, cancellationToken);

        if (runContext.RequiresModulation && fileType is not FileType.BillingJson)
        {
            result.CalcResultProjectedProducers.H1ProjectedProducers = await calcResultReader.ReadH1ProjectedData(runContext.RunId, cancellationToken);
            result.CalcResultProjectedProducers.H2ProjectedProducers = await  calcResultReader.ReadH2ProjectedData(runContext.RunId, cancellationToken);
        }

        if (runContext.RequiresScaling)
            result.CalcResultScaledupProducers.ScaledupProducers = await calcResultReader.ReadScaledData(runContext.RunId, cancellationToken);

        if (fileType is not FileType.BillingJson)
        {
            result.CalcResultPartialObligations.PartialObligations = await calcResultReader.ReadPartialData(runContext.RunId, cancellationToken);
            result.SmcwByMaterial = await calcResultReader.ReadSmcwByMaterial(runContext.RunId, cancellationToken);
        }

        result.CalcResultCancelledProducers = await calcResultReader.ReadCancelledProducers(runContext.RunId, cancellationToken);
        result.CalcResultLaDisposalCostData = await calcResultReader.ReadLaDisposalCostData(runContext.RunId, cancellationToken);
        result.CalcResultCommsCostReportDetail = await calcResultReader.ReadCommsCost(runContext.RunId, cancellationToken);

        // The billing JSON doesn't show rejected producers, but FilterResult needs them to remove them from its cancelled producers.
        if (fileType is FileType.BillingCsv or FileType.BillingJson)
            result.CalcResultRejectedProducers = await rejectedProducersBuilder.ConstructAsync(runContext, cancellationToken);

        if (runContext.RequiresModulation)
            result.CalcResultModulation = await calcResultReader.ReadModulationResult(runContext.RunId, cancellationToken);

        result.ProducerFeesTotal = await calcResultReader.ReadProducerFeesTotal(runContext.RunId, cancellationToken);

        if (fileType is FileType.ResultsCsv)
            result.CalcResultErrorReports = errorReportBuilder.Construct(runContext);

        return result;
    }

    private static CalcResult FilterResult(CalcResult calcResult, ImmutableHashSet<int> acceptedProducerIds)
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
            ProducerFeesTotal = BillingTotal(calcResult.ProducerFeesTotal),
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
