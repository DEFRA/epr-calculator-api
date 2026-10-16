using EPR.Calculator.API.BackgroundService.Builder.ErrorReport;
using EPR.Calculator.API.BackgroundService.Constants;
using EPR.Calculator.API.BackgroundService.Enums;
using EPR.Calculator.API.BackgroundService.Features.Common;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.API.BackgroundService.UnitTests.Builder.ErrorReport;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class CalcResultErrorReportBuilderTests : TestsFor<CalcResultErrorReportBuilder>
{
    private readonly RunContext runContext = TestDataHelper.CalculatorRun2025;

    protected override void TestInitialize() => TestDataHelper.SeedDatabaseForInitialRun(dbContext, runContext);

    [TestMethod]
    public async Task ConstructAsync_ReturnsMappedErrorReport()
    {
        // Arrange
        var synapseError = "Conflicting Obligations(blanks)";
        var testErrorCode = "Test Error code";

        dbContext.ErrorReports.AddRange(
            new Data.DataModels.ErrorReport
            {
                Id = 1,
                CalculatorRunId = runContext.RunId,
                ProducerId = 1,
                SubsidiaryId = null,
                ErrorCode = ErrorCodes.MissingPOMData,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Test user"
            },
            new Data.DataModels.ErrorReport
            {
                Id = 2,
                CalculatorRunId = runContext.RunId,
                ProducerId = 1,
                SubsidiaryId = "Sub 1",
                ErrorCode = ErrorCodes.MissingPOMData,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Test user"
            },
            new Data.DataModels.ErrorReport
            {
                Id = 3,
                CalculatorRunId = runContext.RunId,
                ProducerId = 1,
                SubsidiaryId = "Sub 2",
                ErrorCode = ErrorCodes.MissingPOMData,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Test user"
            },
            new Data.DataModels.ErrorReport
            {
                Id = 5,
                CalculatorRunId = runContext.RunId,
                ProducerId = 2,
                SubsidiaryId = "Sub 2",
                ErrorCode = synapseError,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Test user"
            },
            new Data.DataModels.ErrorReport
            {
                Id = 6,
                CalculatorRunId = 2,
                ProducerId = 1,
                SubsidiaryId = null,
                ErrorCode = ErrorCodes.MissingRegistrationData,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Test user"
            },
            new Data.DataModels.ErrorReport
            {
                Id = 7,
                CalculatorRunId = 2,
                ProducerId = 1,
                SubsidiaryId = "Sub 1",
                ErrorCode = ErrorCodes.MissingPOMData,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Test user"
            },
            new Data.DataModels.ErrorReport
            {
                Id = 8,
                CalculatorRunId = 2,
                ProducerId = 1,
                SubsidiaryId = "Sub 2",
                ErrorCode = ErrorCodes.MissingPOMData,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Test user"
            },
            new Data.DataModels.ErrorReport
            {
                Id = 9,
                CalculatorRunId = runContext.RunId,
                ProducerId = 2,
                SubsidiaryId = null,
                ErrorCode = testErrorCode,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Test user"
            }
        );

        await dbContext.SaveChangesAsync();

        // Act
        var result = testSubject.Construct(runContext).ToList();

        Assert.AreEqual(5, result.Count);
        var report = result[0];
        Assert.AreEqual(1, report.ProducerId);
        Assert.AreEqual(CommonConstants.Hyphen, report.SubsidiaryId);
        Assert.AreEqual("Allied Packaging", report.ProducerName);
        Assert.AreEqual("Allied Trading", report.TradingName);
        Assert.AreEqual(CommonConstants.Hyphen, report.LeaverCode);
        Assert.AreEqual(ErrorCodes.MissingPOMData, report.ErrorCodeText);

        var report1 = result[1];
        Assert.AreEqual(1, report1.ProducerId);
        Assert.AreEqual("Sub 1", report1.SubsidiaryId);
        Assert.AreEqual("Allied Packaging sub 1", report1.ProducerName);
        Assert.AreEqual("Allied Trading sub 1", report1.TradingName);
        Assert.AreEqual(CommonConstants.Hyphen, report1.LeaverCode);
        Assert.AreEqual(ErrorCodes.MissingPOMData, report1.ErrorCodeText);

        var report2 = result[2];
        Assert.AreEqual(1, report2.ProducerId);
        Assert.AreEqual("Sub 2", report2.SubsidiaryId);
        Assert.AreEqual("Allied Packaging sub 2", report2.ProducerName);
        Assert.AreEqual("Allied Trading sub 2", report2.TradingName);
        Assert.AreEqual(CommonConstants.Hyphen, report2.LeaverCode);
        Assert.AreEqual(ErrorCodes.MissingPOMData, report2.ErrorCodeText);

        var report3 = result[3];
        Assert.AreEqual(2, report3.ProducerId);
        Assert.AreEqual(CommonConstants.Hyphen, report3.SubsidiaryId);
        Assert.AreEqual(CommonConstants.Hyphen, report3.ProducerName);
        Assert.AreEqual(CommonConstants.Hyphen, report3.TradingName);
        Assert.AreEqual(CommonConstants.Hyphen, report3.LeaverCode);
        Assert.AreEqual(testErrorCode, report3.ErrorCodeText);

        var report4 = result[4];
        Assert.AreEqual(2, report4.ProducerId);
        Assert.AreEqual("Sub 2", report4.SubsidiaryId);
        Assert.AreEqual(CommonConstants.Hyphen, report4.ProducerName);
        Assert.AreEqual(CommonConstants.Hyphen, report4.TradingName);
        Assert.AreEqual(CommonConstants.Hyphen, report4.LeaverCode);
        Assert.AreEqual(synapseError, report4.ErrorCodeText);
    }

    [TestMethod]
    public async Task ConstructAsync_NullSubsidiaryId_MapsToEmptyString()
    {
        // Arrange
        dbContext.ErrorReports.Add(new Data.DataModels.ErrorReport
        {
            Id = 1,
            CalculatorRunId = runContext.RunId,
            ProducerId = 1,
            SubsidiaryId = null,
            ErrorCode = ErrorCodes.MissingRegistrationData,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "Test user"
        });

        await dbContext.SaveChangesAsync();

        // Act
        var result = testSubject.Construct(runContext).ToList();

        // Assert
        Assert.AreEqual(1, result.Count);
        var report = result[0];
        Assert.AreEqual(1, report.ProducerId);
        Assert.AreEqual(CommonConstants.Hyphen, report.SubsidiaryId);
        Assert.AreEqual("Allied Packaging", report.ProducerName);
        Assert.AreEqual("Allied Trading", report.TradingName);
        Assert.AreEqual(CommonConstants.Hyphen, report.LeaverCode);
        Assert.AreEqual(ErrorCodes.MissingRegistrationData, report.ErrorCodeText);
    }

    [TestMethod]
    public async Task ConstructAsync_ReturnsProducerNameMappedErrorReport()
    {
        // Arrange
        dbContext.ErrorReports.Add(new Data.DataModels.ErrorReport
        {
            Id = 1,
            CalculatorRunId = runContext.RunId,
            ProducerId = 2,
            SubsidiaryId = "SUB-2",
            ErrorCode = ErrorCodes.MissingRegistrationData,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "Test user"
        });

        await dbContext.SaveChangesAsync();

        // Act
        var result = testSubject.Construct(runContext).ToList();

        // Assert
        var report = result.Single(x => x.SubsidiaryId == "SUB-2");
        Assert.AreEqual(2, report.ProducerId);
        Assert.AreEqual("SUB-2", report.SubsidiaryId);
        Assert.AreEqual(CommonConstants.Hyphen, report.ProducerName);
        Assert.AreEqual(CommonConstants.Hyphen, report.TradingName);
        Assert.AreEqual(CommonConstants.Hyphen, report.LeaverCode);
        Assert.AreEqual(ErrorCodes.MissingRegistrationData, report.ErrorCodeText);
    }

    [TestMethod]
    public async Task ConstructAsync_WhenNoOrganisationDetailExists_ReturnsHyphens()
    {
        // Arrange
        dbContext.ErrorReports.Add(new Data.DataModels.ErrorReport
        {
            Id = 999,
            CalculatorRunId = runContext.RunId,
            ProducerId = 999, // no matching OrganisationDataDetail
            SubsidiaryId = "MISSING-SUB",
            ErrorCode = ErrorCodes.MissingRegistrationData,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "Test user"
        });

        await dbContext.SaveChangesAsync();

        // Act
        var result = testSubject.Construct(runContext).ToList();

        // Assert
        var report = result.Single(x => x.ProducerId == 999 && x.SubsidiaryId == "MISSING-SUB");

        Assert.AreEqual(CommonConstants.Hyphen, report.ProducerName);
        Assert.AreEqual(CommonConstants.Hyphen, report.TradingName);
    }

    [TestMethod]
    public async Task ConstructAsync_AddsHoldingCompanyRow_WhenAllErrorsAreSubsidiaryScoped()
    {
        // Arrange
        dbContext.ErrorReports.Add(new Data.DataModels.ErrorReport
        {
            Id = 1,
            CalculatorRunId = runContext.RunId,
            ProducerId = 1,
            SubsidiaryId = "Sub 1",
            ErrorCode = ErrorCodes.MissingPOMData,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "Test user"
        });

        await dbContext.SaveChangesAsync();

        // Act
        var result = testSubject.Construct(runContext).ToList();

        // Assert
        Assert.AreEqual(2, result.Count, "Expected the subsidiary error plus a holding-company row.");
        var holding = result[0];
        Assert.AreEqual(1, holding.ProducerId);
        Assert.AreEqual(CommonConstants.Hyphen, holding.SubsidiaryId);
        Assert.AreEqual("Allied Packaging", holding.ProducerName);
        Assert.AreEqual("Allied Trading", holding.TradingName);
        Assert.AreEqual(string.Empty, holding.LeaverCode);
        Assert.AreEqual(string.Empty, holding.ErrorCodeText);
        Assert.AreEqual("Sub 1", result[1].SubsidiaryId);
    }

    [TestMethod]
    public async Task ConstructAsync_HoldingCompanyRowUsesHyphens_WhenNoOrganisationDetailExists()
    {
        // Arrange
        dbContext.ErrorReports.Add(new Data.DataModels.ErrorReport
        {
            Id = 1,
            CalculatorRunId = runContext.RunId,
            ProducerId = 999,
            SubsidiaryId = "MISSING-SUB",
            ErrorCode = ErrorCodes.MissingRegistrationData,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "Test user"
        });

        await dbContext.SaveChangesAsync();

        // Act
        var result = testSubject.Construct(runContext).ToList();

        // Assert
        var holding = result.Single(x => x.ProducerId == 999 && x.SubsidiaryId == CommonConstants.Hyphen);
        Assert.AreEqual(CommonConstants.Hyphen, holding.ProducerName);
        Assert.AreEqual(CommonConstants.Hyphen, holding.TradingName);
    }

    [TestMethod]
    public async Task ConstructAsync_DoesNotAddHoldingCompanyRow_WhenAHoldingLevelErrorExists()
    {
        // Arrange
        dbContext.ErrorReports.AddRange(
            new Data.DataModels.ErrorReport
            {
                Id = 1,
                CalculatorRunId = runContext.RunId,
                ProducerId = 1,
                SubsidiaryId = "Sub 1",
                ErrorCode = ErrorCodes.MissingPOMData,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Test user"
            },
            new Data.DataModels.ErrorReport
            {
                Id = 2,
                CalculatorRunId = runContext.RunId,
                ProducerId = 1,
                SubsidiaryId = null,
                ErrorCode = ErrorCodes.MissingRegistrationData,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Test user"
            });

        await dbContext.SaveChangesAsync();

        // Act
        var result = testSubject.Construct(runContext).ToList();

        // Assert
        Assert.AreEqual(2, result.Count, "Should not add a holding-company row on top of an existing holding-level error.");
        Assert.AreEqual(ErrorCodes.MissingRegistrationData, result.Single(x => x.SubsidiaryId == CommonConstants.Hyphen).ErrorCodeText);
    }
}
