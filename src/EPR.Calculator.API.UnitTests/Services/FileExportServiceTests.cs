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
            .Setup(x => x.ReadProducerFees(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProducerFees
            {
                CalculatorRunId = RunId,
                Total = new FeeDetail { ProducerId = 0, SubsidiaryId = string.Empty, ProducerName = string.Empty },
                Details = new List<ProducerFeeDetail>()
            });
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
        exported.Content.ShouldBe(Encoding.UTF8.GetBytes(CsvContent));
        errorReportBuilderMock.Verify(x => x.Construct(It.IsAny<RunContext>()), Times.Once);
        rejectedProducersBuilderMock.Verify(x => x.ConstructAsync(It.IsAny<RunContext>(), It.IsAny<CancellationToken>()), Times.Never);
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
        exported.Content.ShouldBe(Encoding.UTF8.GetBytes(CsvContent));
        rejectedProducersBuilderMock.Verify(x => x.ConstructAsync(It.IsAny<RunContext>(), It.IsAny<CancellationToken>()), Times.Once);
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
        rejectedProducersBuilderMock.Verify(x => x.ConstructAsync(It.IsAny<RunContext>(), It.IsAny<CancellationToken>()), Times.Once);
        errorReportBuilderMock.Verify(x => x.Construct(It.IsAny<RunContext>()), Times.Never);
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
    public async Task Export_ResultCsv_ReturnsNotCached_WhenCalculatorRunHasNoCachedData()
    {
        AddCalculatorRun(RunId, RunClassificationStatusIds.INITIALRUNCOMPLETEDID, BillingRunStatus.None, RunName);
        var result = await service.Export(RunId, RunType.Calculator, FileExportType.Csv, CancellationToken.None);
        result.ShouldBeOfType<FileExportResult.NotCached>();
    }

    [TestMethod]
    public async Task Export_Billing_ReturnsNotCached_WhenBillingRunHasNoProducerFeeData()
    {
        AddCalculatorRun(RunId, RunClassificationStatusIds.INITIALRUNCOMPLETEDID, BillingRunStatus.Completed, RunName);
        AddBillingFileMetadata(RunId);

        var csvResult = await service.Export(RunId, RunType.Billing, FileExportType.Csv, CancellationToken.None);
        var jsonResult = await service.Export(RunId, RunType.Billing, FileExportType.Json, CancellationToken.None);

        csvResult.ShouldBeOfType<FileExportResult.NotCached>();
        jsonResult.ShouldBeOfType<FileExportResult.NotCached>();
    }

    private void AddCalculatorRun(
        int runId,
        int classificationId,
        BillingRunStatus billingRunStatus,
        string name)
    {
        dbContext.CalculatorRuns.Add(new CalculatorRun
        {
            Id = runId,
            Name = name,
            RelativeYear = new RelativeYear(2026),
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
