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
using EPR.Calculator.API.BackgroundService.Telemetry;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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
    private Mock<ITelemetryClient> telemetryClientMock = null!;
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
        telemetryClientMock = new Mock<ITelemetryClient>();

        telemetryClientMock
            .Setup(x => x.TrackDuration(It.IsAny<string>(), It.IsAny<Func<Task<FileExportResult>>>()))
            .Returns((string _, Func<Task<FileExportResult>> func) => func());

        calcResultReaderMock
            .Setup(x => x.ReadPartialData(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CalcResultPartialObligation>());
        calcResultReaderMock
            .Setup(x => x.ReadCancelledProducers(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CalcResultCancelledProducer>());
        calcResultReaderMock
            .Setup(x => x.ReadH1ProjectedData(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CalcResultH1ProjectedProducer>());
        calcResultReaderMock
            .Setup(x => x.ReadH2ProjectedData(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CalcResultH2ProjectedProducer>());
        calcResultReaderMock
            .Setup(x => x.ReadProducerFees(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProducerFees
            {
                CalculatorRunId = RunId,
                Total = new FeeDetail { ProducerId = 0, SubsidiaryId = string.Empty, ProducerName = string.Empty },
                Details = new List<ProducerFeeDetail>()
            });
        rejectedProducersBuilderMock
            .Setup(x => x.ConstructAsync(It.IsAny<RunContext>()))
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
            billingJsonWriterMock.Object,
            telemetryClientMock.Object,
            Mock.Of<ILogger<FileExportService>>());
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
        AddCalculatorRun(RunId, RunClassificationStatusIds.INITIALRUNCOMPLETEDID, BillingRunStatus.None, RunName);
        AddProducerFeeRow(RunId);
        resultsFileExporterMock
            .Setup(x => x.Export(It.IsAny<CalculatorRunContext>(), It.IsAny<CalcResult>()))
            .ReturnsAsync(CsvContent);
        
        var result = await service.Export(RunId, RunType.Calculator, FileExportType.Csv, CancellationToken.None);

        var exported = result.ShouldBeOfType<FileExportResult.Exported>();
        exported.FileName.ShouldBe(RunName + ".csv");
        exported.Content.ShouldBe([.. Encoding.UTF8.GetPreamble(), .. Encoding.UTF8.GetBytes(CsvContent)]);
        errorReportBuilderMock.Verify(x => x.Construct(It.IsAny<RunContext>()), Times.Once);
        rejectedProducersBuilderMock.Verify(x => x.ConstructAsync(It.IsAny<RunContext>()), Times.Never);
    }

    [TestMethod]
    public async Task Export_BillingCsv_ReturnsExported_HasData()
    {
        AddCalculatorRun(RunId, RunClassificationStatusIds.INITIALRUNCOMPLETEDID, BillingRunStatus.Completed, RunName);
        AddBillingFileMetadata(RunId);
        AddProducerFeeRow(RunId);
        billingFileExporterMock
            .Setup(x => x.Export(It.IsAny<BillingRunContext>(), It.IsAny<CalcResult>()))
            .ReturnsAsync(CsvContent);

        var result = await service.Export(RunId, RunType.Billing, FileExportType.Csv, CancellationToken.None);

        var exported = result.ShouldBeOfType<FileExportResult.Exported>();
        exported.FileName.ShouldBe(RunName + ".csv");
        exported.Content.ShouldBe([.. Encoding.UTF8.GetPreamble(), .. Encoding.UTF8.GetBytes(CsvContent)]);
        rejectedProducersBuilderMock.Verify(x => x.ConstructAsync(It.IsAny<RunContext>()), Times.Once);
        errorReportBuilderMock.Verify(x => x.Construct(It.IsAny<RunContext>()), Times.Never);
    }

    [TestMethod]
    public async Task Export_BillingJson_ReturnsExported_HasData()
    {
        AddCalculatorRun(RunId, RunClassificationStatusIds.INITIALRUNCOMPLETEDID, BillingRunStatus.Completed, RunName);
        AddBillingFileMetadata(RunId);
        AddProducerFeeRow(RunId);
        billingJsonWriterMock
            .Setup(x => x.WriteToString(It.IsAny<BillingRunContext>(), It.IsAny<CalcResult>()))
            .ReturnsAsync(JsonContent);

        var result = await service.Export(RunId, RunType.Billing, FileExportType.Json, CancellationToken.None);

        // Assert
        var exported = result.ShouldBeOfType<FileExportResult.Exported>();
        exported.FileName.ShouldBe(RunName + ".json");
        exported.Content.ShouldBe(Encoding.UTF8.GetBytes(JsonContent));
        rejectedProducersBuilderMock.Verify(x => x.ConstructAsync(It.IsAny<RunContext>()), Times.Once);
        errorReportBuilderMock.Verify(x => x.Construct(It.IsAny<RunContext>()), Times.Never);
    }

    [TestMethod]
    public async Task Export_GetResult_NoScalingOrModulation()
    {
        AddCalculatorRun(RunId, RunClassificationStatusIds.INITIALRUNCOMPLETEDID, BillingRunStatus.None, RunName, relativeYear: 2024);
        AddProducerFeeRow(RunId);

        var calcResultDetail = new Mock<CalcResultDetail>().Object;
        var lapcapData = new Mock<CalcResultLapcapData>().Object;
        var lateReportingTonnage = new Mock<CalcResultLateReportingTonnage>().Object;
        var parameterOtherCost = new Mock<CalcResultParameterOtherCost>().Object;
        var onePlusFourApportionment = new Mock<CalcResultOnePlusFourApportionment>().Object;
        var laDisposalCostData = new Mock<CalcResultLaDisposalCostData>().Object;
        var commsCost = new Mock<CalcResultCommsCost>().Object;
        var smcw = new Mock<SelfManagedConsumerWaste>().Object;

        calcResultDetailBuilderMock.Setup(x => x.ConstructAsync(It.IsAny<RunContext>())).ReturnsAsync(calcResultDetail);
        calcResultReaderMock.Setup(x => x.ReadLapcapData(RunId, It.IsAny<CancellationToken>())).ReturnsAsync(lapcapData);
        calcResultReaderMock.Setup(x => x.ReadLateReportingTonnage(RunId, It.IsAny<CancellationToken>())).ReturnsAsync(lateReportingTonnage);
        calcResultReaderMock.Setup(x => x.ReadParameterOtherCost(RunId, It.IsAny<CancellationToken>())).ReturnsAsync(parameterOtherCost);
        calcResultReaderMock.Setup(x => x.ReadOnePlusFourApportionment(RunId, It.IsAny<CancellationToken>())).ReturnsAsync(onePlusFourApportionment);
        calcResultReaderMock.Setup(x => x.ReadLaDisposalCostData(RunId, It.IsAny<CancellationToken>())).ReturnsAsync(laDisposalCostData);
        calcResultReaderMock.Setup(x => x.ReadCommsCost(RunId, It.IsAny<CancellationToken>())).ReturnsAsync(commsCost);
        calcResultReaderMock.Setup(x => x.ReadSmcw(RunId, It.IsAny<CancellationToken>())).ReturnsAsync(smcw);

        CalcResult? capturedResult = null;
        resultsFileExporterMock
            .Setup(x => x.Export(It.IsAny<CalculatorRunContext>(), It.IsAny<CalcResult>()))
            .Callback<CalculatorRunContext, CalcResult>((_, result) => capturedResult = result)
            .ReturnsAsync(CsvContent);

        var result = await service.Export(RunId, RunType.Calculator, FileExportType.Csv, CancellationToken.None);

        result.ShouldBeOfType<FileExportResult.Exported>();
        capturedResult.ShouldNotBeNull();
        capturedResult.CalcResultDetail.ShouldBeSameAs(calcResultDetail);
        capturedResult.CalcResultLapcapData.ShouldBeSameAs(lapcapData);
        capturedResult.CalcResultLateReportingTonnageData.ShouldBeSameAs(lateReportingTonnage);
        capturedResult.CalcResultParameterOtherCost.ShouldBeSameAs(parameterOtherCost);
        capturedResult.CalcResultOnePlusFourApportionment.ShouldBeSameAs(onePlusFourApportionment);
        capturedResult.CalcResultLaDisposalCostData.ShouldBeSameAs(laDisposalCostData);
        capturedResult.CalcResultCommsCostReportDetail.ShouldBeSameAs(commsCost);
        capturedResult.Smcw.ShouldBeSameAs(smcw);

        calcResultReaderMock.Verify(x => x.ReadModulationResult(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        calcResultReaderMock.Verify(x => x.ReadH1ProjectedData(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        calcResultReaderMock.Verify(x => x.ReadH2ProjectedData(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        calcResultReaderMock.Verify(x => x.ReadScaledData(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task Export_GetResult_Modulation()
    {
        AddCalculatorRun(RunId, RunClassificationStatusIds.INITIALRUNCOMPLETEDID, BillingRunStatus.None, RunName, relativeYear: 2026);
        AddProducerFeeRow(RunId);

        var h1Data = new Mock<List<CalcResultH1ProjectedProducer>>().Object;
        var h2Data = new Mock<List<CalcResultH2ProjectedProducer>>().Object;
        var modulationResult = new Mock<ModulationResult>().Object;

        calcResultReaderMock.Setup(x => x.ReadH1ProjectedData(RunId, It.IsAny<CancellationToken>())).ReturnsAsync(h1Data);
        calcResultReaderMock.Setup(x => x.ReadH2ProjectedData(RunId, It.IsAny<CancellationToken>())).ReturnsAsync(h2Data);
        calcResultReaderMock.Setup(x => x.ReadModulationResult(RunId, It.IsAny<CancellationToken>())).ReturnsAsync(modulationResult);

        CalcResult? capturedResult = null;
        resultsFileExporterMock
            .Setup(x => x.Export(It.IsAny<CalculatorRunContext>(), It.IsAny<CalcResult>()))
            .Callback<CalculatorRunContext, CalcResult>((_, result) => capturedResult = result)
            .ReturnsAsync(CsvContent);

        var result = await service.Export(RunId, RunType.Calculator, FileExportType.Csv, CancellationToken.None);

        result.ShouldBeOfType<FileExportResult.Exported>();
        capturedResult.ShouldNotBeNull();
        capturedResult.CalcResultProjectedProducers.H1ProjectedProducers.ShouldBe(h1Data.ToImmutableList());
        capturedResult.CalcResultProjectedProducers.H2ProjectedProducers.ShouldBe(h2Data.ToImmutableList());
        capturedResult.CalcResultModulation.ShouldBeSameAs(modulationResult);

        calcResultReaderMock.Verify(x => x.ReadScaledData(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task Export_GetResult_Scaling()
    {
        AddCalculatorRun(RunId, RunClassificationStatusIds.INITIALRUNCOMPLETEDID, BillingRunStatus.None, RunName, relativeYear: 2025);
        AddProducerFeeRow(RunId);

        var scaledUpData = new Mock<List<CalcResultScaledupProducer>>().Object;
        calcResultReaderMock.Setup(x => x.ReadScaledData(RunId, It.IsAny<CancellationToken>())).ReturnsAsync(scaledUpData);

        CalcResult? capturedResult = null;
        resultsFileExporterMock
            .Setup(x => x.Export(It.IsAny<CalculatorRunContext>(), It.IsAny<CalcResult>()))
            .Callback<CalculatorRunContext, CalcResult>((_, result) => capturedResult = result)
            .ReturnsAsync(CsvContent);

        var result = await service.Export(RunId, RunType.Calculator, FileExportType.Csv, CancellationToken.None);

        result.ShouldBeOfType<FileExportResult.Exported>();
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
    public async Task Export_ResultCsv_ReturnsNotFound_WhenCalculatorRunClassificationIsNotDownloadable()
    {
        AddCalculatorRun(RunId, RunClassificationStatusIds.RUNNINGID, BillingRunStatus.None, RunName);

        var result = await service.Export(RunId, RunType.Calculator, FileExportType.Csv, CancellationToken.None);
        result.ShouldBeOfType<FileExportResult.NotFound>();
    }

    [TestMethod]
    public async Task Export_Billing_ReturnsNotFound_WhenBillingRunStatusIsNotCompleted()
    {
        AddCalculatorRun(RunId, RunClassificationStatusIds.INITIALRUNCOMPLETEDID, BillingRunStatus.None, RunName);
        AddBillingFileMetadata(RunId);

        var csvResult = await service.Export(RunId, RunType.Billing, FileExportType.Csv, CancellationToken.None);
        var jsonResult = await service.Export(RunId, RunType.Billing, FileExportType.Json, CancellationToken.None);
        
        csvResult.ShouldBeOfType<FileExportResult.NotFound>();
        jsonResult.ShouldBeOfType<FileExportResult.NotFound>();
    }

    [TestMethod]
    public async Task Export_Billing_ReturnsNotFound_WhenBillingRunClassificationIsDeleted()
    {
        AddCalculatorRun(RunId, RunClassificationStatusIds.DELETEDID, BillingRunStatus.Completed, RunName);
        AddBillingFileMetadata(RunId);

        var csvResult = await service.Export(RunId, RunType.Billing, FileExportType.Csv, CancellationToken.None);
        var jsonResult = await service.Export(RunId, RunType.Billing, FileExportType.Json, CancellationToken.None);
        
        csvResult.ShouldBeOfType<FileExportResult.NotFound>();
        jsonResult.ShouldBeOfType<FileExportResult.NotFound>();
    }

    [TestMethod]
    public async Task Export_Billing_ReturnsNotFound_WhenBillingRunHasNoBillingFileMetadata()
    {
        AddCalculatorRun(RunId, RunClassificationStatusIds.INITIALRUNCOMPLETEDID, BillingRunStatus.Completed, RunName);

        var csvResult = await service.Export(RunId, RunType.Billing, FileExportType.Csv, CancellationToken.None);
        var jsonResult = await service.Export(RunId, RunType.Billing, FileExportType.Json, CancellationToken.None);

        csvResult.ShouldBeOfType<FileExportResult.NotFound>();
        jsonResult.ShouldBeOfType<FileExportResult.NotFound>();
    }

    [TestMethod]
    public async Task Export_ResultCsv_ReturnsLegacy_WhenCalculatorRunHasNoProducerFeeData()
    {
        AddCalculatorRun(RunId, RunClassificationStatusIds.INITIALRUNCOMPLETEDID, BillingRunStatus.None, RunName);
        var result = await service.Export(RunId, RunType.Calculator, FileExportType.Csv, CancellationToken.None);
        result.ShouldBeOfType<FileExportResult.Legacy>();
    }

    [TestMethod]
    public async Task Export_Billing_ReturnsLegacy_WhenBillingRunHasNoProducerFeeData()
    {
        AddCalculatorRun(RunId, RunClassificationStatusIds.INITIALRUNCOMPLETEDID, BillingRunStatus.Completed, RunName);
        AddBillingFileMetadata(RunId);

        var csvResult = await service.Export(RunId, RunType.Billing, FileExportType.Csv, CancellationToken.None);
        var jsonResult = await service.Export(RunId, RunType.Billing, FileExportType.Json, CancellationToken.None);

        csvResult.ShouldBeOfType<FileExportResult.Legacy>();
        jsonResult.ShouldBeOfType<FileExportResult.Legacy>();
    }

    private void AddCalculatorRun(
        int runId,
        int classificationId,
        BillingRunStatus billingRunStatus,
        string name,
        int relativeYear = 2026)
    {
        dbContext.CalculatorRuns.Add(new CalculatorRun
        {
            Id = runId,
            Name = name,
            RelativeYear = new RelativeYear(relativeYear),
            CreatedBy = "test-user",
            CreatedAt = DateTime.UtcNow,
            CalculatorRunClassificationId = classificationId,
            BillingRunStatus = billingRunStatus,
            BillingRunStartedAt = DateTime.UtcNow,
        });
        dbContext.SaveChanges();
    }

    private void AddBillingFileMetadata(int runId)
    {
        dbContext.CalculatorRunBillingFileMetadata.Add(new CalculatorRunBillingFileMetadata
        {
            CalculatorRunId = runId,
            BillingCsvFileName = "billing.csv",
            BillingJsonFileName = "billing.json",
            BillingFileCreatedBy = "test-user",
            BillingFileCreatedDate = DateTime.UtcNow,
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
