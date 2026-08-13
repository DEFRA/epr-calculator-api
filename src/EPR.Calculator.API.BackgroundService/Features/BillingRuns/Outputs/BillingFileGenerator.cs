using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter;
using EPR.Calculator.API.BackgroundService.Exporter.JsonExporter;
using EPR.Calculator.API.BackgroundService.Features.BillingRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.BackgroundService.Options;
using EPR.Calculator.API.BackgroundService.Services;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.API.BackgroundService.Features.BillingRuns.Outputs;

public interface IBillingFileGenerator
{
    /// <summary>
    ///     Serializes the calcResult to CSV/JSON billing files and exports them.
    /// </summary>
    Task<BillingFileResult> SerializeAndExport(BillingRunContext runContext, CalcResult calcResult, CancellationToken cancellationToken);
}

public class BillingFileGenerator(
    IOptions<BlobStorageUploadOptions> blobStorageUploadOptions,
    IBillingFileExporter exporter,
    IBillingFileJsonWriter jsonWriter,
    IStorageUploadService storageUploadService,
    ILogger<BillingFileGenerator> logger)
    : IBillingFileGenerator
{
    public async Task<BillingFileResult> SerializeAndExport(BillingRunContext runContext, CalcResult calcResult, CancellationToken cancellationToken)
    {
        var csvMetaData = await HandleCsvFile(runContext, calcResult, cancellationToken);
        logger.LogInformation($"{nameof(HandleCsvFile)} Completed. File: {{Filename}}", csvMetaData.FileName);

        var jsonMetaData = await HandleJsonFile(runContext, calcResult, csvMetaData, cancellationToken);
        logger.LogInformation($"{nameof(HandleJsonFile)} Completed. File: {{Filename}}", jsonMetaData.BillingJsonFileName);

        return new BillingFileResult
        {
            CsvMetadata = csvMetaData,
            JsonMetadata = jsonMetaData
        };
    }

    private async Task<CalculatorRunCsvFileMetadata> HandleCsvFile(
        BillingRunContext runContext,
        CalcResult calcResults,
        CancellationToken ct)
    {
        var csvFilename = new CalcResultsAndBillingFileName(runContext.RunId, runContext.RunName, runContext.ProcessingStartedAt.UtcDateTime, true);
        var csvContent = await exporter.Export(runContext, calcResults);

        var csvBlobUri = await storageUploadService.UploadFileContentAsync((
            FileName: csvFilename,
            Content: csvContent,
            runContext.RunName,
            ContainerName: blobStorageUploadOptions.Value.BillingFileCsvContainer,
            Overwrite: true), ct);

        return new CalculatorRunCsvFileMetadata
        {
            BlobUri = csvBlobUri,
            CalculatorRunId = runContext.RunId,
            FileName = csvFilename
        };
    }

    private async Task<CalculatorRunBillingFileMetadata> HandleJsonFile(
        BillingRunContext runContext,
        CalcResult calcResults,
        CalculatorRunCsvFileMetadata csvMetaData,
        CancellationToken ct)
    {
        var jsonFilename = new CalcResultsAndBillingFileName(runContext.RunId);
        var jsonContent = await jsonWriter.WriteToString(runContext, calcResults);

        await storageUploadService.UploadFileContentAsync((
            FileName: jsonFilename,
            Content: jsonContent,
            runContext.RunName,
            ContainerName: blobStorageUploadOptions.Value.BillingFileJsonContainer,
            Overwrite: true), ct);

        return new CalculatorRunBillingFileMetadata
        {
            CalculatorRunId = runContext.RunId,
            BillingCsvFileName = csvMetaData.FileName,
            BillingFileCreatedBy = runContext.User,
            BillingFileCreatedDate = runContext.ProcessingStartedAt.UtcDateTime,
            BillingJsonFileName = jsonFilename
        };
    }
}
