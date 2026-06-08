using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Exporter.CsvExporter;
using EPR.Calculator.Service.Function.Features.CalculatorRun.Contexts;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Options;
using EPR.Calculator.Service.Function.Services;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.Service.Function.Features.CalculatorRun.Outputs;

public interface ICalculatorFileGenerator
{
    /// <summary>
    ///     Serializes the calcResult to a CSV result file and exports it.
    /// </summary>
    Task<CalculatorFileResult> SerializeAndExport(CalculatorRunContext runContext, CalcResult calcResult, CancellationToken cancellationToken);
}

public class CalculatorFileGenerator(
    IOptions<BlobStorageOptions> blobStorageOptions,
    ICalcResultsExporter csvWriter,
    IStorageUploadService storageUploadService,
    ILogger<CalculatorFileGenerator> logger)
    : ICalculatorFileGenerator
{
    public async Task<CalculatorFileResult> SerializeAndExport(CalculatorRunContext runContext, CalcResult calcResult, CancellationToken cancellationToken)
    {
        var csvMetaData = await HandleCsvFile(runContext, calcResult, cancellationToken);
        logger.LogInformation($"{nameof(HandleCsvFile)} Completed. File: {{Filename}}", csvMetaData.FileName);

        return new CalculatorFileResult
        {
            CsvMetadata = csvMetaData
        };
    }

    public async Task<CalculatorRunCsvFileMetadata> HandleCsvFile(CalculatorRunContext runContext, CalcResult calcResult, CancellationToken cancellationToken)
    {
        var content = await csvWriter.Export(runContext, calcResult);

        var csvFilename = new CalcResultsAndBillingFileName(
            calcResult.CalcResultDetail.RunId,
            calcResult.CalcResultDetail.RunName,
            calcResult.CalcResultDetail.RunDate);

        var csvBlobUri = await storageUploadService.UploadFileContentAsync(
            (FileName: csvFilename,
                Content: content,
                runContext.RunName,
                ContainerName: blobStorageOptions.Value.ResultFileCsvContainerName,
                Overwrite: false), cancellationToken);

        return new CalculatorRunCsvFileMetadata
        {
            BlobUri = csvBlobUri,
            CalculatorRunId = runContext.RunId,
            FileName = csvFilename
        };
    }
}
