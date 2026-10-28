
using EPR.Calculator.API.BackgroundService.Exporter;
using EPR.Calculator.API.BackgroundService.Features.BillingRuns.Constants;
using EPR.Calculator.API.BackgroundService.Features.BillingRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.BackgroundService.Telemetry;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataTypes;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Services;

public interface IFileExportService
{
    Task<FileExportResult> Export(int runId, FileExportType fileType, CancellationToken cancellationToken);
}

public enum FileExportType { ResultsCsv, BillingCsv, BillingJson }


public abstract record FileExportResult
{
    private FileExportResult() { }
    public sealed record Streamed(Func<Stream, CancellationToken, Task> WriteTo, string FileName) : FileExportResult;
    public sealed record NotFound() : FileExportResult;
    public sealed record Legacy() : FileExportResult;
}

public class FileExportService(
    IRunFileExporter runFileExporter,
    ApplicationDBContext dbContext,
    IParameterService parameterService
)  : IFileExportService
{
    [ActivityMetric(nameof(Metrics.FileExportDuration), threshold: "00:00:10")]
    public Task<FileExportResult> Export(int runId, FileExportType fileType, CancellationToken cancellationToken)
    {
        return fileType switch
        {
            FileExportType.ResultsCsv => ExportResultCsv(runId, cancellationToken),
            FileExportType.BillingCsv or FileExportType.BillingJson => ExportBilling(runId, fileType, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null)
        };
    }

    private async Task<FileExportResult> ExportResultCsv(int runId, CancellationToken cancellationToken)
    {
        var runContext = await GetCalculatorRunContext(runId, cancellationToken);

        if (runContext is null)
            return new FileExportResult.NotFound();

        return ToExportResult(await runFileExporter.GetResultsCsv(runContext, cancellationToken));
    }

    private async Task<FileExportResult> ExportBilling(int runId, FileExportType billingFileType, CancellationToken cancellationToken)
    {
        var runContext = await GetBillingRunContext(runId, cancellationToken);

        if (runContext is null)
            return new FileExportResult.NotFound();

        var file = billingFileType switch
        {
            FileExportType.BillingCsv => await runFileExporter.GetBillingCsv(runContext, cancellationToken),
            FileExportType.BillingJson => await runFileExporter.GetBillingJson(runContext, cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(billingFileType), billingFileType, null)
        };

        return ToExportResult(file);
    }

    private static FileExportResult ToExportResult(RunFile? file) =>
        file is null
            ? new FileExportResult.Legacy()
            : new FileExportResult.Streamed(file.WriteTo, file.FileName);

    private async Task<CalculatorRunContext?> GetCalculatorRunContext(int runId, CancellationToken cancellationToken)
    {
        var run = await dbContext.CalculatorRuns.AsNoTracking().SingleOrDefaultAsync(x => x.Id == runId, cancellationToken);

        if (run is null || run.CalculationRunStatus != CalculationRunStatus.Completed || run.Classification == RunClassification.Deleted)
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
            .SingleOrDefaultAsync(r => r.Id == runId, cancellationToken);

        if (run is null || run.BillingRunStatus != BillingRunStatus.Completed || run.Classification == RunClassification.Deleted)
            return null;

        var billingFileMetadata = await dbContext.CalculatorRunBillingFileMetadata
            .AsNoTracking()
            .Where(m => m.CalculatorRunId == runId)
            .OrderByDescending(m => m.BillingFileCreatedDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (billingFileMetadata is null)
            return null;

        var acceptedProducerIds = await dbContext.ProducerResultFileSuggestedBillingInstruction
            .AsNoTracking()
            .Where(p =>
                p.CalculatorRunId == runId
                && p.BillingInstructionAcceptReject == BillingConstants.Action.Accepted
                && p.SuggestedBillingInstruction != BillingConstants.Suggestion.Cancel)
            .Select(p => p.ProducerId)
            .Distinct()
            .ToListAsync(cancellationToken);

        return new BillingRunContext
        {
            RunId = run.Id,
            RunName = run.Name.Trim(),
            ProcessingStartedAt = new DateTimeOffset(DateTime.SpecifyKind(billingFileMetadata.BillingFileCreatedDate, DateTimeKind.Utc)),
            RelativeYear = run.RelativeYear,
            User = billingFileMetadata.BillingFileCreatedBy,
            AcceptedProducerIds = acceptedProducerIds.ToImmutableHashSet(),
            DefaultParameters = await parameterService.GetDefaultParameters(run.Id)
        };
    }
}
