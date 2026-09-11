using System.Text;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter;
using EPR.Calculator.API.BackgroundService.Exporter.JsonExporter;
using EPR.Calculator.API.BackgroundService.Features.BillingRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.BackgroundService.Options;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.API.BackgroundService.Features.BillingRuns.Outputs;

public interface IBillingFileGenerator
{
    /// <summary>
    ///     Serializes the calcResult to CSV/JSON billing files and exports them.
    /// </summary>
    /// <param name="producerFeeDetails">
    ///     Deferred, streamed per-producer fee rows - enumerated once by the CSV export and once by the
    ///     JSON export, so the whole fee graph is never held in memory.
    /// </param>
    Task<BillingFileResult> SerializeAndExport(BillingRunContext runContext, CalcResult calcResult, IEnumerable<FeeDetail> producerFeeDetails, CancellationToken cancellationToken);
}

public class BillingFileGenerator(
    IOptions<BlobStorageUploadOptions> blobStorageUploadOptions,
    IBillingFileExporter exporter,
    IBillingFileJsonWriter jsonWriter,
    IStorageUploadService storageUploadService,
    ILogger<BillingFileGenerator> logger)
    : IBillingFileGenerator
{
    public async Task<BillingFileResult> SerializeAndExport(BillingRunContext runContext, CalcResult calcResult, IEnumerable<FeeDetail> producerFeeDetails, CancellationToken cancellationToken)
    {
        var csvMetaData = await HandleCsvFile(runContext, calcResult, producerFeeDetails, cancellationToken);
        logger.LogInformation($"{nameof(HandleCsvFile)} Completed. File: {{Filename}}", csvMetaData.FileName);

        var jsonMetaData = await HandleJsonFile(runContext, calcResult, csvMetaData, producerFeeDetails, cancellationToken);
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
        IEnumerable<FeeDetail> producerFeeDetails,
        CancellationToken ct)
    {
        var csvFilename = new CalcResultsAndBillingFileName(runContext.RunId, runContext.RunName, runContext.ProcessingStartedAt.UtcDateTime, true);

        var request = new IStorageUploadService.StreamRequest
        {
            FileName = csvFilename,
            ContainerName = blobStorageUploadOptions.Value.BillingFileCsvContainer,
            Overwrite = true
        };

        var csvBlobUri = await storageUploadService.UploadFileStreamAsync(
            request,
            async (stream, token) =>
            {
                await using var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), leaveOpen: true);
                await exporter.Export(runContext, calcResults, writer, producerFeeDetails);
                await writer.FlushAsync(token);
            },
            ct);

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
        IEnumerable<FeeDetail> producerFeeDetails,
        CancellationToken ct)
    {
        var jsonFilename = new CalcResultsAndBillingFileName(runContext.RunId);

        var request = new IStorageUploadService.StreamRequest
        {
            FileName = jsonFilename,
            ContainerName = blobStorageUploadOptions.Value.BillingFileJsonContainer,
            Overwrite = true,
            UseUtf8Bom = false
        };

        await storageUploadService.UploadFileStreamAsync(
            request,
            (stream, token) => jsonWriter.WriteTo(stream, runContext, calcResults, producerFeeDetails, token),
            ct);

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
