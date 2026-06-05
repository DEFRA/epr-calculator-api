using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Exporter.CsvExporter;
using EPR.Calculator.Service.Function.Exporter.JsonExporter;
using EPR.Calculator.Service.Function.Features.BillingRun.Contexts;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Options;
using EPR.Calculator.Service.Function.Services;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.Service.Function.Features.BillingRun.Outputs;

public interface IBillingFileGenerator
{
    /// <summary>
    ///     Serializes the calcResult to CSV/JSON billing files and exports them.
    /// </summary>
    Task<BillingFileResult> SerializeAndExport(BillingRunContext runContext, CalcResult calcResult, CancellationToken cancellationToken);
}

public class BillingFileGenerator(
    IOptions<BlobStorageOptions> blobStorageOptions,
    IBillingFileExporter exporter,
    IBillingFileJsonWriter jsonWriter,
    IStorageService storageService,
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

        var csvBlobUri = await storageService.UploadFileContentAsync((
            FileName: csvFilename,
            Content: csvContent,
            runContext.RunName,
            ContainerName: blobStorageOptions.Value.BillingFileCsvContainer,
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

        await storageService.UploadFileContentAsync((
            FileName: jsonFilename,
            Content: jsonContent,
            runContext.RunName,
            ContainerName: blobStorageOptions.Value.BillingFileJsonContainer,
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
