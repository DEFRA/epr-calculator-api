using System.Text;
using EPR.Calculator.API.BackgroundService.Builder.Detail;
using EPR.Calculator.API.BackgroundService.Builder.ErrorReport;
using EPR.Calculator.API.BackgroundService.Builder.RejectedProducers;
using EPR.Calculator.API.BackgroundService.Exporter.CsvExporter;
using EPR.Calculator.API.BackgroundService.Exporter.JsonExporter;
using EPR.Calculator.API.BackgroundService.Features.BillingRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Features.Common;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.Services;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.UnitTests.Services;

[TestClass]
public class FileExportServiceTests
{
    private const int RunId = 100;
    private const string RunName = "Run 1";
    private const string CsvContent = "hello,bye";
    private const string JsonContent = "{\"billing\":true}";
    private ApplicationDBContext dbContext = null!;
    private Mock<ICalcResultDetailBuilder> calcResultDetailBuilderMock = null!;
    private Mock<ICalcResultErrorReportBuilder> errorReportBuilderMock = null!;
    private Mock<ICalcResultRejectedProducersBuilder> rejectedProducersBuilderMock = null!;
    private Mock<ICalcResultReader> calcResultReaderMock = null!;
    private Mock<IParameterService> parameterServiceMock = null!;
    private Mock<ICalcResultsExporter> resultsFileExporterMock = null!;
    private Mock<IBillingFileExporter> billingFileExporterMock = null!;
    private Mock<IBillingFileJsonWriter> billingJsonWriterMock = null!;
    private FileExportService service = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        var options = new DbContextOptionsBuilder<ApplicationDBContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        dbContext = new ApplicationDBContext(options);
        dbContext.Database.EnsureCreated();

        calcResultDetailBuilderMock = new Mock<ICalcResultDetailBuilder>();
        errorReportBuilderMock = new Mock<ICalcResultErrorReportBuilder>();
        rejectedProducersBuilderMock = new Mock<ICalcResultRejectedProducersBuilder>();
        calcResultReaderMock = new Mock<ICalcResultReader>();
        parameterServiceMock = new Mock<IParameterService>();
        resultsFileExporterMock = new Mock<ICalcResultsExporter>();
        billingFileExporterMock = new Mock<IBillingFileExporter>();
        billingJsonWriterMock = new Mock<IBillingFileJsonWriter>();

        calcResultReaderMock
            .Setup(x => x.ReadPartialData(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        calcResultReaderMock
            .Setup(x => x.ReadCancelledProducers(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        calcResultReaderMock
            .Setup(x => x.ReadH1ProjectedData(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        calcResultReaderMock
            .Setup(x => x.ReadH2ProjectedData(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        calcResultReaderMock
            .Setup(x => x.ReadProducerFeesTotal(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FeeDetail { ProducerId = 0, SubsidiaryId = string.Empty, ProducerName = string.Empty });
        calcResultReaderMock
            .Setup(x => x.StreamProducerFeeDetails(It.IsAny<int>()))
            .Returns(Enumerable.Empty<FeeDetail>());
        rejectedProducersBuilderMock
            .Setup(x => x.ConstructAsync(It.IsAny<RunContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CalcResultRejectedProducer>());

        service = new FileExportService(
            calcResultDetailBuilderMock.Object,
            errorReportBuilderMock.Object,
            rejectedProducersBuilderMock.Object,
            calcResultReaderMock.Object,
            dbContext,
            parameterServiceMock.Object,
            resultsFileExporterMock.Object,
            billingFileExporterMock.Object,
            billingJsonWriterMock.Object);
    }

    private static async Task<byte[]> WrittenBytes(FileExportResult.Streamed streamed)
    {
        using var stream = new MemoryStream();
        await streamed.WriteTo(stream, CancellationToken.None);
        return stream.ToArray();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        dbContext.Database.EnsureDeleted();
        dbContext.Dispose();
    }

    [TestMethod]
    public async Task Export_ResultCsv_ReturnsExported_HasData()
    {
        var createdAt = new DateTime(2026, 3, 15, 9, 30, 0, DateTimeKind.Utc);
        AddCalculatorRun(RunId, RunClassification.Initial, BillingRunStatus.None, RunName, createdAt: createdAt);
        AddProducerFeeRow(RunId);
        resultsFileExporterMock
            .Setup(x => x.Export(It.IsAny<CalculatorRunContext>(), It.IsAny<CalcResult>(), It.IsAny<IEnumerable<FeeDetail>>(), It.IsAny<TextWriter>()))
            .Returns<CalculatorRunContext, CalcResult, IEnumerable<FeeDetail>, TextWriter>((_, _, _, writer) => writer.WriteAsync(CsvContent));

        var result = await service.Export(RunId, RunType.Calculator, FileExportType.Csv, CancellationToken.None);

        var streamed = result.ShouldBeOfType<FileExportResult.Streamed>();
        streamed.FileName.ShouldBe(new CalcResultsAndBillingFileName(RunId, RunName, createdAt).ToString());
        (await WrittenBytes(streamed)).ShouldBe([.. Encoding.UTF8.GetPreamble(), .. Encoding.UTF8.GetBytes(CsvContent)]);
        errorReportBuilderMock.Verify(x => x.Construct(It.IsAny<RunContext>()), Times.Once);
        rejectedProducersBuilderMock.Verify(x => x.ConstructAsync(It.IsAny<RunContext>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task Export_BillingCsv_ReturnsExported_HasData()
    {
        var billingFileCreatedDate = new DateTime(2026, 3, 15, 9, 30, 0, DateTimeKind.Utc);
        AddCalculatorRun(RunId, RunClassification.Initial, BillingRunStatus.Completed, RunName);
        AddBillingFileMetadata(RunId, createdDate: billingFileCreatedDate);
        AddProducerFeeRow(RunId);
        billingFileExporterMock
            .Setup(x => x.Export(It.IsAny<BillingRunContext>(), It.IsAny<CalcResult>(), It.IsAny<IEnumerable<FeeDetail>>(), It.IsAny<TextWriter>()))
            .Returns<BillingRunContext, CalcResult, IEnumerable<FeeDetail>, TextWriter>((_, _, _, writer) => writer.WriteAsync(CsvContent));

        var result = await service.Export(RunId, RunType.Billing, FileExportType.Csv, CancellationToken.None);

        var streamed = result.ShouldBeOfType<FileExportResult.Streamed>();
        streamed.FileName.ShouldBe(new CalcResultsAndBillingFileName(RunId, RunName, billingFileCreatedDate, isDraftBillingFile: true).ToString());
        (await WrittenBytes(streamed)).ShouldBe([.. Encoding.UTF8.GetPreamble(), .. Encoding.UTF8.GetBytes(CsvContent)]);
        rejectedProducersBuilderMock.Verify(x => x.ConstructAsync(It.IsAny<RunContext>(), It.IsAny<CancellationToken>()), Times.Once);
        errorReportBuilderMock.Verify(x => x.Construct(It.IsAny<RunContext>()), Times.Never);
        VerifyCsvOnlyReads(Times.Once());
    }

    [TestMethod]
    public async Task Export_BillingJson_ReturnsExported_HasData()
    {
        AddCalculatorRun(RunId, RunClassification.Initial, BillingRunStatus.Completed, RunName);
        AddBillingFileMetadata(RunId);
        AddProducerFeeRow(RunId);
        billingJsonWriterMock
            .Setup(x => x.WriteTo(It.IsAny<Stream>(), It.IsAny<BillingRunContext>(), It.IsAny<CalcResult>(), It.IsAny<IEnumerable<FeeDetail>>(), It.IsAny<CancellationToken>()))
            .Returns<Stream, BillingRunContext, CalcResult, IEnumerable<FeeDetail>, CancellationToken>((stream, _, _, _, cancellationToken) =>
                stream.WriteAsync(Encoding.UTF8.GetBytes(JsonContent), cancellationToken).AsTask());

        var result = await service.Export(RunId, RunType.Billing, FileExportType.Json, CancellationToken.None);

        // Assert
        var exported = result.ShouldBeOfType<FileExportResult.Exported>();
        exported.FileName.ShouldBe(new CalcResultsAndBillingFileName(RunId).ToString());
        exported.Content.ShouldBe(Encoding.UTF8.GetBytes(JsonContent));
        rejectedProducersBuilderMock.Verify(x => x.ConstructAsync(It.IsAny<RunContext>(), It.IsAny<CancellationToken>()), Times.Once);
        errorReportBuilderMock.Verify(x => x.Construct(It.IsAny<RunContext>()), Times.Never);
        VerifyCsvOnlyReads(Times.Never());
    }

    [TestMethod]
    public async Task Export_BillingJson_LeavesRejectedProducersOutOfCancelledProducers()
    {
        AddCalculatorRun(RunId, RunClassification.Initial, BillingRunStatus.Completed, RunName);
        AddBillingFileMetadata(RunId);
        AddProducerFeeRow(RunId);
        calcResultReaderMock
            .Setup(x => x.ReadCancelledProducers(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new CalcResultCancelledProducer { ProducerId = 201001 }, new CalcResultCancelledProducer { ProducerId = 201007 }]);
        rejectedProducersBuilderMock
            .Setup(x => x.ConstructAsync(It.IsAny<RunContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new CalcResultRejectedProducer
            {
                ProducerId = 201007, ProducerName = "Rejected", TradingName = "", SuggestedBillingInstruction = "REBILL",
                InstructionConfirmedBy = "user", ReasonForRejection = "reason"
            }]);
        CalcResult? written = null;
        billingJsonWriterMock
            .Setup(x => x.WriteTo(It.IsAny<Stream>(), It.IsAny<BillingRunContext>(), It.IsAny<CalcResult>(), It.IsAny<IEnumerable<FeeDetail>>(), It.IsAny<CancellationToken>()))
            .Callback<Stream, BillingRunContext, CalcResult, IEnumerable<FeeDetail>, CancellationToken>((_, _, calcResult, _, _) => written = calcResult)
            .Returns(Task.CompletedTask);

        (await service.Export(RunId, RunType.Billing, FileExportType.Json, CancellationToken.None)).ShouldBeOfType<FileExportResult.Exported>();

        written.ShouldNotBeNull();
        written.CalcResultCancelledProducers.Select(p => p.ProducerId).ShouldBe([201001]);
    }

    private void VerifyCsvOnlyReads(Times times)
    {
        calcResultReaderMock.Verify(x => x.ReadH1ProjectedData(RunId, It.IsAny<CancellationToken>()), times);
        calcResultReaderMock.Verify(x => x.ReadH2ProjectedData(RunId, It.IsAny<CancellationToken>()), times);
        calcResultReaderMock.Verify(x => x.ReadPartialData(RunId, It.IsAny<CancellationToken>()), times);
        calcResultReaderMock.Verify(x => x.ReadSmcwByMaterial(RunId, It.IsAny<CancellationToken>()), times);
    }

    [TestMethod]
    public async Task Export_GetResult_NoScalingOrModulation()
    {
        AddCalculatorRun(RunId, RunClassification.Initial, BillingRunStatus.None, RunName, relativeYear: 2024);
        AddProducerFeeRow(RunId);

        var calcResultDetail = new Mock<CalcResultDetail>().Object;
        var lapcapData = new Mock<CalcResultLapcapData>().Object;
        var lateReportingTonnage = new Mock<CalcResultLateReportingTonnage>().Object;
        var parameterOtherCost = new Mock<CalcResultParameterOtherCost>().Object;
        var onePlusFourApportionment = new Mock<CalcResultOnePlusFourApportionment>().Object;
        var laDisposalCostData = new Mock<CalcResultLaDisposalCostData>().Object;
        var commsCost = new Mock<CalcResultCommsCost>().Object;
        IReadOnlyDictionary<string, SelfManagedConsumerWasteData> smcwByMaterial = new Dictionary<string, SelfManagedConsumerWasteData>();

        calcResultDetailBuilderMock.Setup(x => x.ConstructAsync(It.IsAny<RunContext>(), It.IsAny<CancellationToken>())).ReturnsAsync(calcResultDetail);
        calcResultReaderMock.Setup(x => x.ReadLapcapData(RunId, It.IsAny<CancellationToken>())).ReturnsAsync(lapcapData);
        calcResultReaderMock.Setup(x => x.ReadLateReportingTonnage(RunId, It.IsAny<CancellationToken>())).ReturnsAsync(lateReportingTonnage);
        calcResultReaderMock.Setup(x => x.ReadParameterOtherCost(RunId, It.IsAny<CancellationToken>())).ReturnsAsync(parameterOtherCost);
        calcResultReaderMock.Setup(x => x.ReadOnePlusFourApportionment(RunId, It.IsAny<CancellationToken>())).ReturnsAsync(onePlusFourApportionment);
        calcResultReaderMock.Setup(x => x.ReadLaDisposalCostData(RunId, It.IsAny<CancellationToken>())).ReturnsAsync(laDisposalCostData);
        calcResultReaderMock.Setup(x => x.ReadCommsCost(RunId, It.IsAny<CancellationToken>())).ReturnsAsync(commsCost);
        calcResultReaderMock.Setup(x => x.ReadSmcwByMaterial(RunId, It.IsAny<CancellationToken>())).ReturnsAsync(smcwByMaterial);

        CalcResult? capturedResult = null;
        resultsFileExporterMock
            .Setup(x => x.Export(It.IsAny<CalculatorRunContext>(), It.IsAny<CalcResult>(), It.IsAny<IEnumerable<FeeDetail>>(), It.IsAny<TextWriter>()))
            .Callback<CalculatorRunContext, CalcResult, IEnumerable<FeeDetail>, TextWriter>((_, result, _, _) => capturedResult = result)
            .Returns(Task.CompletedTask);

        var result = await service.Export(RunId, RunType.Calculator, FileExportType.Csv, CancellationToken.None);

        await WrittenBytes(result.ShouldBeOfType<FileExportResult.Streamed>());
        capturedResult.ShouldNotBeNull();
        capturedResult.CalcResultDetail.ShouldBeSameAs(calcResultDetail);
        capturedResult.CalcResultLapcapData.ShouldBeSameAs(lapcapData);
        capturedResult.CalcResultLateReportingTonnageData.ShouldBeSameAs(lateReportingTonnage);
        capturedResult.CalcResultParameterOtherCost.ShouldBeSameAs(parameterOtherCost);
        capturedResult.CalcResultOnePlusFourApportionment.ShouldBeSameAs(onePlusFourApportionment);
        capturedResult.CalcResultLaDisposalCostData.ShouldBeSameAs(laDisposalCostData);
        capturedResult.CalcResultCommsCostReportDetail.ShouldBeSameAs(commsCost);
        capturedResult.SmcwByMaterial.ShouldBeSameAs(smcwByMaterial);

        calcResultReaderMock.Verify(x => x.ReadModulationResult(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        calcResultReaderMock.Verify(x => x.ReadH1ProjectedData(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        calcResultReaderMock.Verify(x => x.ReadH2ProjectedData(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        calcResultReaderMock.Verify(x => x.ReadScaledData(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task Export_GetResult_Modulation()
    {
        AddCalculatorRun(RunId, RunClassification.Initial, BillingRunStatus.None, RunName, relativeYear: 2026);
        AddProducerFeeRow(RunId);

        var h1Data = new Mock<List<CalcResultH1ProjectedProducer>>().Object;
        var h2Data = new Mock<List<CalcResultH2ProjectedProducer>>().Object;
        var modulationResult = new Mock<ModulationResult>().Object;

        calcResultReaderMock.Setup(x => x.ReadH1ProjectedData(RunId, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        calcResultReaderMock.Setup(x => x.ReadH2ProjectedData(RunId, It.IsAny<CancellationToken>())).ReturnsAsync([]);
        calcResultReaderMock.Setup(x => x.ReadModulationResult(RunId, It.IsAny<CancellationToken>())).ReturnsAsync(modulationResult);

        CalcResult? capturedResult = null;
        resultsFileExporterMock
            .Setup(x => x.Export(It.IsAny<CalculatorRunContext>(), It.IsAny<CalcResult>(), It.IsAny<IEnumerable<FeeDetail>>(), It.IsAny<TextWriter>()))
            .Callback<CalculatorRunContext, CalcResult, IEnumerable<FeeDetail>, TextWriter>((_, result, _, _) => capturedResult = result)
            .Returns(Task.CompletedTask);

        var result = await service.Export(RunId, RunType.Calculator, FileExportType.Csv, CancellationToken.None);

        await WrittenBytes(result.ShouldBeOfType<FileExportResult.Streamed>());
        capturedResult.ShouldNotBeNull();
        capturedResult.CalcResultProjectedProducers.H1ProjectedProducers.ShouldBe(h1Data.ToImmutableList());
        capturedResult.CalcResultProjectedProducers.H2ProjectedProducers.ShouldBe(h2Data.ToImmutableList());
        capturedResult.CalcResultModulation.ShouldBeSameAs(modulationResult);

        calcResultReaderMock.Verify(x => x.ReadScaledData(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task Export_GetResult_Scaling()
    {
        AddCalculatorRun(RunId, RunClassification.Initial, BillingRunStatus.None, RunName, relativeYear: 2025);
        AddProducerFeeRow(RunId);

        var scaledUpData = new Mock<List<CalcResultScaledupProducer>>().Object;
        calcResultReaderMock.Setup(x => x.ReadScaledData(RunId, It.IsAny<CancellationToken>())).ReturnsAsync([]);

        CalcResult? capturedResult = null;
        resultsFileExporterMock
            .Setup(x => x.Export(It.IsAny<CalculatorRunContext>(), It.IsAny<CalcResult>(), It.IsAny<IEnumerable<FeeDetail>>(), It.IsAny<TextWriter>()))
            .Callback<CalculatorRunContext, CalcResult, IEnumerable<FeeDetail>, TextWriter>((_, result, _, _) => capturedResult = result)
            .Returns(Task.CompletedTask);

        var result = await service.Export(RunId, RunType.Calculator, FileExportType.Csv, CancellationToken.None);

        await WrittenBytes(result.ShouldBeOfType<FileExportResult.Streamed>());
        capturedResult.ShouldNotBeNull();
        capturedResult.CalcResultScaledupProducers.ScaledupProducers.ShouldBe(scaledUpData.ToImmutableList());

        calcResultReaderMock.Verify(x => x.ReadModulationResult(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        calcResultReaderMock.Verify(x => x.ReadH1ProjectedData(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        calcResultReaderMock.Verify(x => x.ReadH2ProjectedData(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task Export_ReturnsNotFound_WhenCalculatorRunDoesNotExist()
    {
        var calcResult = await service.Export(RunId, RunType.Calculator, FileExportType.Csv, CancellationToken.None);
        var billingCsv = await service.Export(RunId, RunType.Billing, FileExportType.Csv, CancellationToken.None);
        var billingJson = await service.Export(RunId, RunType.Billing, FileExportType.Json, CancellationToken.None);

        calcResult.ShouldBeOfType<FileExportResult.NotFound>();
        billingCsv.ShouldBeOfType<FileExportResult.NotFound>();
        billingJson.ShouldBeOfType<FileExportResult.NotFound>();
    }

    [TestMethod]
    public async Task Export_ResultCsv_ReturnsNotFound_WhenCalculationHasNotCompleted()
    {
        AddCalculatorRun(RunId, RunClassification.None, BillingRunStatus.None, RunName, calculationRunStatus: CalculationRunStatus.Started);

        var result = await service.Export(RunId, RunType.Calculator, FileExportType.Csv, CancellationToken.None);
        result.ShouldBeOfType<FileExportResult.NotFound>();
    }

    [TestMethod]
    public async Task Export_Billing_ReturnsNotFound_WhenBillingRunStatusIsNotCompleted()
    {
        AddCalculatorRun(RunId, RunClassification.Initial, BillingRunStatus.None, RunName);
        AddBillingFileMetadata(RunId);

        var csvResult = await service.Export(RunId, RunType.Billing, FileExportType.Csv, CancellationToken.None);
        var jsonResult = await service.Export(RunId, RunType.Billing, FileExportType.Json, CancellationToken.None);

        csvResult.ShouldBeOfType<FileExportResult.NotFound>();
        jsonResult.ShouldBeOfType<FileExportResult.NotFound>();
    }

    [TestMethod]
    public async Task Export_Billing_ReturnsNotFound_WhenBillingRunClassificationIsDeleted()
    {
        AddCalculatorRun(RunId, RunClassification.Deleted, BillingRunStatus.Completed, RunName);
        AddBillingFileMetadata(RunId);

        var csvResult = await service.Export(RunId, RunType.Billing, FileExportType.Csv, CancellationToken.None);
        var jsonResult = await service.Export(RunId, RunType.Billing, FileExportType.Json, CancellationToken.None);

        csvResult.ShouldBeOfType<FileExportResult.NotFound>();
        jsonResult.ShouldBeOfType<FileExportResult.NotFound>();
    }

    [TestMethod]
    public async Task Export_Billing_ReturnsNotFound_WhenBillingRunHasNoBillingFileMetadata()
    {
        AddCalculatorRun(RunId, RunClassification.Initial, BillingRunStatus.Completed, RunName);

        var csvResult = await service.Export(RunId, RunType.Billing, FileExportType.Csv, CancellationToken.None);
        var jsonResult = await service.Export(RunId, RunType.Billing, FileExportType.Json, CancellationToken.None);

        csvResult.ShouldBeOfType<FileExportResult.NotFound>();
        jsonResult.ShouldBeOfType<FileExportResult.NotFound>();
    }

    [TestMethod]
    public async Task Export_ResultCsv_ReturnsLegacy_WhenCalculatorRunHasNoProducerFeeData()
    {
        AddCalculatorRun(RunId, RunClassification.Initial, BillingRunStatus.None, RunName);
        var result = await service.Export(RunId, RunType.Calculator, FileExportType.Csv, CancellationToken.None);
        result.ShouldBeOfType<FileExportResult.Legacy>();
    }

    [TestMethod]
    public async Task Export_Billing_ReturnsLegacy_WhenBillingRunHasNoProducerFeeData()
    {
        AddCalculatorRun(RunId, RunClassification.Initial, BillingRunStatus.Completed, RunName);
        AddBillingFileMetadata(RunId);

        var csvResult = await service.Export(RunId, RunType.Billing, FileExportType.Csv, CancellationToken.None);
        var jsonResult = await service.Export(RunId, RunType.Billing, FileExportType.Json, CancellationToken.None);

        csvResult.ShouldBeOfType<FileExportResult.Legacy>();
        jsonResult.ShouldBeOfType<FileExportResult.Legacy>();
    }

    private void AddCalculatorRun(
        int runId,
        RunClassification classification,
        BillingRunStatus billingRunStatus,
        string name,
        int relativeYear = 2026,
        DateTime? createdAt = null,
        CalculationRunStatus calculationRunStatus = CalculationRunStatus.Completed)
    {
        dbContext.CalculatorRuns.Add(new CalculatorRun
        {
            Id = runId,
            Name = name,
            RelativeYear = new RelativeYear(relativeYear),
            CreatedBy = "test-user",
            CreatedAt = createdAt ?? DateTime.UtcNow,
            Classification = classification,
            CalculationRunStatus = calculationRunStatus,
            BillingRunStatus = billingRunStatus,
            BillingRunStartedAt = DateTime.UtcNow,
        });
        dbContext.SaveChanges();
    }

    private void AddBillingFileMetadata(int runId, DateTime? createdDate = null)
    {
        dbContext.CalculatorRunBillingFileMetadata.Add(new CalculatorRunBillingFileMetadata
        {
            CalculatorRunId = runId,
            BillingCsvFileName = "billing.csv",
            BillingJsonFileName = "billing.json",
            BillingFileCreatedBy = "test-user",
            BillingFileCreatedDate = createdDate ?? DateTime.UtcNow,
        });
        dbContext.SaveChanges();
    }

    private void AddProducerFeeRow(int runId)
    {
        dbContext.ProducerDisposalFee.Add(new ProducerFees
        {
            CalculatorRunId = runId,
            Total = new FeeDetail { ProducerId = 0, SubsidiaryId = string.Empty, ProducerName = string.Empty },
        });
        dbContext.SaveChanges();
    }
}
