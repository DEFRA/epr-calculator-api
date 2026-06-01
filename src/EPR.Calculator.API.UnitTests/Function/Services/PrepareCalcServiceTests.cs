using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Builder;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Exporter.CsvExporter;
using EPR.Calculator.Service.Function.Exporter.JsonExporter;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.Service.Function.UnitTests.Services;

[TestClass]
public class PrepareCalcServiceTests
{
    private IFixture fixture = null!;
    private ApplicationDBContext dbContext = null!;
    private Mock<ICalcResultBuilder> builder = null!;
    private Mock<ICalcResultsExporter> csvResultsExporter = null!;
    private Mock<IBillingFileExporter> csvBillingExporter = null!;
    private Mock<ICalcBillingJsonExporter> jsonBillingExporter = null!;
    private Mock<IStorageService> storageService = null!;
    private PrepareCalcService sut = null!;

    [TestInitialize]
    public void Init()
    {
        fixture = TestFixtures.New();
        dbContext = fixture.Freeze<ApplicationDBContext>();
        builder = fixture.Freeze<Mock<ICalcResultBuilder>>();

        csvResultsExporter = fixture.Freeze<Mock<ICalcResultsExporter>>();
        csvResultsExporter
            .Setup(x => x.Export(It.IsAny<CalcResult>(), It.IsAny<IImmutableList<MaterialDetail>>()))
            .Returns("Some value");

        storageService = fixture.Freeze<Mock<IStorageService>>();
        storageService
            .Setup(x => x.UploadFileContentAsync(It.IsAny<(string, string, string, string, bool)>()))
            .ReturnsAsync("http://testuri");

        csvBillingExporter = fixture.Freeze<Mock<IBillingFileExporter>>();
        jsonBillingExporter = fixture.Freeze<Mock<ICalcBillingJsonExporter>>();

        SeedDatabase();

        var calcResult = new CalcResult
        {
            ApplyModulation = true,
            CalcResultScaledupProducers = new CalcResultScaledupProducers(),
            CalcResultPartialObligations = new CalcResultPartialObligations(),
            CalcResultDetail = new CalcResultDetail
            {
                RunId = 4,
                RunDate = DateTime.UtcNow,
                RunName = "RunName",
                RelativeYear = new RelativeYear(2024)
            },
            CalcResultLapcapData = new CalcResultLapcapData
            {
                ByMaterial = []
            },
            CalcResultParameterOtherCost = new CalcResultParameterOtherCost
            {
                SchemeSetupCost = new ByCountryCost
                {
                    England = 0,
                    Wales = 0,
                    Scotland = 0,
                    NorthernIreland = 0
                }
            },
            CalcResultLateReportingTonnageData = new() { ByMaterial = [] },
            CalcResultProjectedProducers = new CalcResultProjectedProducers()
        };

        builder
            .Setup(b => b.BuildAsync(It.IsAny<CalcResultsRequestDto>(), It.IsAny<IImmutableList<MaterialDetail>>()))
            .ReturnsAsync(calcResult);

        sut = fixture.Create<PrepareCalcService>();
    }

    [TestCleanup]
    public void TearDown()
    {
        dbContext.Database.EnsureDeleted();
        dbContext.Dispose();
    }

    [TestMethod]
    public async Task PrepareCalcResults_ShouldReturnTrueStatus()
    {
        // Arrange
        var resultsRequestDto = new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2025) };
        var runName = fixture.Create<string>();

        storageService.Setup(x => x.UploadFileContentAsync(
                It.IsAny<(string, string, string, string, bool)>()))
            .ReturnsAsync("expected result");
        // Act
        var result = await sut.PrepareCalcResultsAsync(resultsRequestDto, runName, CancellationToken.None);

        // Assert
        Assert.AreEqual(true, result.IsSuccess);

        storageService.Verify(x => x.UploadFileContentAsync(
            It.Is<(string, string, string, string, bool)>(y => y.Item5 == false)), Times.Once);
    }

    [TestMethod]
    public async Task PrepareCalcResults_Exported_Exception_ShouldReturnFalseStatus()
    {
        // Arrange
        var resultsRequestDto = new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2025) };
        csvResultsExporter.Setup(x => x.Export(It.IsAny<CalcResult>(), It.IsAny<IImmutableList<MaterialDetail>>())).Throws(new Exception("Custom exception message"));
        var runName = fixture.Create<string>();

        // Act
        var result = await sut.PrepareCalcResultsAsync(resultsRequestDto, runName, CancellationToken.None);

        // Assert
        Assert.AreEqual(false, result.IsSuccess);
    }

    [TestMethod]
    public async Task PrepareCalcResults_Exported_Operation_Exception_ShouldReturnFalseStatus()
    {
        // Arrange
        var resultsRequestDto = new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2025) };
        csvResultsExporter.Setup(x => x.Export(It.IsAny<CalcResult>(), It.IsAny<IImmutableList<MaterialDetail>>())).Throws(new OperationCanceledException("Operation canceled exception message"));
        var runName = fixture.Create<string>();

        // Act
        var result = await sut.PrepareCalcResultsAsync(resultsRequestDto, runName, CancellationToken.None);

        // Assert
        Assert.AreEqual(false, result.IsSuccess);
    }

    [TestMethod]
    public async Task PrepareCalcResults_ShouldReturnFalseStatus()
    {
        // Arrange
        var resultsRequestDto = new CalcResultsRequestDto { RunId = 2, RelativeYear = new RelativeYear(2025) };
        var runName = "test";

        // Act
        var result = await sut.PrepareCalcResultsAsync(resultsRequestDto, runName, CancellationToken.None);

        // Assert
        Assert.AreEqual(false, result.IsSuccess);
    }

    [TestMethod]
    public async Task PrepareCalcResults_CalcRun_ShouldReturnFalseStatus()
    {
        // Arrange
        var resultsRequestDto = new CalcResultsRequestDto { RunId = 10, RelativeYear = new RelativeYear(2025) };
        var runName = "test";

        // Act
        var result = await sut.PrepareCalcResultsAsync(resultsRequestDto, runName, CancellationToken.None);

        // Assert
        Assert.AreEqual(false, result.IsSuccess);
    }

    [TestMethod]
    public async Task PrepareBillingResults_Test()
    {
        var calcRun = dbContext.CalculatorRuns.Single(x => x.Id == 1);
        calcRun.IsBillingFileGenerating = true;
        await dbContext.SaveChangesAsync();

        jsonBillingExporter
            .Setup(t => t.Export(It.IsAny<CalcResult>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<List<int>>()))
            .Returns(fixture.Create<string>());

        csvBillingExporter
            .Setup(t => t.Export(It.IsAny<CalcResult>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<ImmutableHashSet<int>>()))
            .Returns(fixture.Create<string>());

        storageService
            .Setup(x => x.UploadFileContentAsync(It.IsAny<(string, string, string, string, bool)>()))
            .ReturnsAsync("fileName");

        var calcResultsRequestDto = new CalcResultsRequestDto
        {
            RunId = 1,
            RelativeYear = new RelativeYear(2025),
            IsBillingFile = true,
            AcceptedProducerIds = [1, 2],
            ApprovedBy = "Test User 234"
        };

        var billingResult = await sut.PrepareBillingResultsAsync(calcResultsRequestDto, "TestRun", CancellationToken.None);

        Assert.IsTrue(billingResult.IsSuccess);
        calcRun = dbContext.CalculatorRuns.Single(x => x.Id == 1);
        Assert.IsFalse(calcRun.IsBillingFileGenerating);

        builder
            .Verify(b => b.BuildAsync(It.Is<CalcResultsRequestDto>(x => x.RunId == 1 && x.IsBillingFile), It.IsAny<IImmutableList<MaterialDetail>>()), Times.Once);

        var billingFileMetaData = dbContext.CalculatorRunBillingFileMetadata.SingleOrDefault(x => x.CalculatorRunId == 1);

        Assert.IsNotNull(billingFileMetaData);

        billingFileMetaData.BillingFileCreatedBy = "Test User 234";
        var fileNamePart = $"1-TestRun_Billing File_{DateTime.Today:yyyyMMdd}";
        Assert.IsTrue(billingFileMetaData.BillingCsvFileName?.StartsWith(fileNamePart));
        Assert.AreEqual("1billing.json", billingFileMetaData.BillingJsonFileName);

        storageService.Verify(x => x.UploadFileContentAsync(
            It.IsAny<(string, string, string, string, bool)>()), Times.Exactly(2));

        storageService.Verify(x => x.UploadFileContentAsync(
            It.Is<(string, string, string, string, bool)>(y => !y.Item5)), Times.Once);

        storageService.Verify(x => x.UploadFileContentAsync(
            It.Is<(string, string, string, string, bool)>(y => y.Item5)), Times.Once);
    }

    private void SeedDatabase()
    {
        dbContext.CalculatorRunOrganisationDataMaster.AddRange(GetCalculatorRunOrganisationDataMaster());
        dbContext.CalculatorRunOrganisationDataDetails.AddRange(GetCalculatorRunOrganisationDataDetails());

        dbContext.CalculatorRunPomDataMaster.AddRange(GetCalculatorRunPomDataMaster());
        dbContext.CalculatorRunPomDataDetails.AddRange(GetCalculatorRunPomDataDetails());

        dbContext.CalculatorRuns.AddRange(GetCalculatorRuns());
        dbContext.Material.AddRange(GetMaterials());

        dbContext.SaveChanges();
    }

    private static IEnumerable<CalculatorRunOrganisationDataMaster> GetCalculatorRunOrganisationDataMaster()
    {
        var list = new List<CalculatorRunOrganisationDataMaster>
        {
            new()
            {
                Id = 1,
                RelativeYear = new RelativeYear(2024),
                EffectiveFrom = DateTime.UtcNow,
                CreatedBy = "Test user",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = 2,
                RelativeYear = new RelativeYear(2024),
                EffectiveFrom = DateTime.UtcNow,
                CreatedBy = "Test user",
                CreatedAt = DateTime.UtcNow
            }
        };
        return list;
    }

    private static IEnumerable<CalculatorRunOrganisationDataDetail> GetCalculatorRunOrganisationDataDetails()
    {
        var list = new List<CalculatorRunOrganisationDataDetail>();
        list.AddRange(new List<CalculatorRunOrganisationDataDetail>
        {
            new()
            {
                Id = 1,
                OrganisationId = 1,
                OrganisationName = "UPU LIMITED",
                SubsidiaryId = "1",
                LoadTimeStamp = DateTime.UtcNow,
                CalculatorRunOrganisationDataMasterId = 1
            },
            new()
            {
                Id = 2,
                OrganisationId = 1,
                OrganisationName = "Test LIMITED",
                LoadTimeStamp = DateTime.UtcNow,
                CalculatorRunOrganisationDataMasterId = 1
            },
            new()
            {
                Id = 3,
                OrganisationId = 2,
                SubsidiaryId = "1",
                OrganisationName = "Subsid2",
                LoadTimeStamp = DateTime.UtcNow,
                CalculatorRunOrganisationDataMasterId = 2
            }
        });
        return list;
    }

    private static IEnumerable<Material> GetMaterials()
    {
        var list = new List<Material>
        {
            new()
            {
                Id = 1,
                Code = "AL",
                Name = "Aluminium",
                Description = "Aluminium"
            },
            new()
            {
                Id = 2,
                Code = "FC",
                Name = "Fibre composite",
                Description = "Fibre composite"
            },
            new()
            {
                Id = 3,
                Code = "GL",
                Name = "Glass",
                Description = "Glass"
            },
            new()
            {
                Id = 4,
                Code = "PC",
                Name = "Paper or card",
                Description = "Paper or card"
            },
            new()
            {
                Id = 5,
                Code = "PL",
                Name = "Plastic",
                Description = "Plastic"
            },
            new()
            {
                Id = 6,
                Code = "ST",
                Name = "Steel",
                Description = "Steel"
            },
            new()
            {
                Id = 7,
                Code = "WD",
                Name = "Wood",
                Description = "Wood"
            },
            new()
            {
                Id = 8,
                Code = "OT",
                Name = "Other materials",
                Description = "Other materials"
            }
        };
        return list;
    }

    private static IEnumerable<CalculatorRunPomDataMaster> GetCalculatorRunPomDataMaster()
    {
        var list = new List<CalculatorRunPomDataMaster>
        {
            new()
            {
                Id = 1,
                RelativeYear = new RelativeYear(2024),
                EffectiveFrom = DateTime.UtcNow,
                CreatedBy = "Test user",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = 2,
                RelativeYear = new RelativeYear(2024),
                EffectiveFrom = DateTime.UtcNow,
                CreatedBy = "Test user",
                CreatedAt = DateTime.UtcNow
            }
        };
        return list;
    }

    private static IEnumerable<CalculatorRunPomDataDetail> GetCalculatorRunPomDataDetails()
    {
        var list = new List<CalculatorRunPomDataDetail>
        {
            new()
            {
                Id = 1,
                OrganisationId = 1,
                SubsidiaryId = "1",
                SubmissionPeriod = "2023-P2",
                PackagingActivity = null,
                PackagingType = "CW",
                PackagingClass = "O1",
                PackagingMaterial = "PC",
                PackagingMaterialWeight = 1000,
                LoadTimeStamp = DateTime.UtcNow,
                CalculatorRunPomDataMasterId = 1,
                SubmissionPeriodDesc = "July to December 2023"
            },
            new()
            {
                Id = 2,
                OrganisationId = 1,
                SubmissionPeriod = "2023-P2",
                PackagingActivity = null,
                PackagingType = "CW",
                PackagingClass = "O1",
                PackagingMaterial = "PC",
                PackagingMaterialWeight = 1000,
                LoadTimeStamp = DateTime.UtcNow,
                CalculatorRunPomDataMasterId = 1,
                SubmissionPeriodDesc = "July to December 2023"
            },
            new()
            {
                Id = 3,
                OrganisationId = 1,
                SubsidiaryId = "1",
                SubmissionPeriod = "2023-P1",
                PackagingActivity = null,
                PackagingType = "CW",
                PackagingClass = "O1",
                PackagingMaterial = "PC",
                PackagingMaterialWeight = 1000,
                LoadTimeStamp = DateTime.UtcNow,
                CalculatorRunPomDataMasterId = 1,
                SubmissionPeriodDesc = "January to June 2023"
            },
            new()
            {
                Id = 4,
                OrganisationId = 2,
                SubsidiaryId = "1",
                SubmissionPeriod = "2024-P1",
                PackagingActivity = null,
                PackagingType = "CW",
                PackagingClass = "O1",
                PackagingMaterial = "PC",
                PackagingMaterialWeight = 1000,
                LoadTimeStamp = DateTime.UtcNow,
                CalculatorRunPomDataMasterId = 2,
                SubmissionPeriodDesc = "January to June 2024"
            }
        };
        return list;
    }

    private static IEnumerable<CalculatorRun> GetCalculatorRuns()
    {
        var list = new List<CalculatorRun>
        {
            new()
            {
                Id = 1,
                CalculatorRunClassificationId = (int)RunClassification.RUNNING,
                Name = "Test Run",
                RelativeYear = new RelativeYear(2024),
                CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                CreatedBy = "Test User",
                CalculatorRunOrganisationDataMasterId = 2,
                CalculatorRunPomDataMasterId = 2,
                DefaultParameterSettingMasterId = 5,
                LapcapDataMasterId = 6
            },
            new()
            {
                Id = 2,
                CalculatorRunClassificationId = (int)RunClassification.RUNNING,
                Name = "Test Calculated Result",
                RelativeYear = new RelativeYear(2025),
                CreatedAt = new DateTime(2024, 8, 21, 14, 16, 27, DateTimeKind.Utc),
                CreatedBy = "Test User",
                DefaultParameterSettingMasterId = 5,
                LapcapDataMasterId = 6
            },
            new()
            {
                Id = 3,
                CalculatorRunClassificationId = (int)RunClassification.RUNNING,
                Name = "Test Run",
                RelativeYear = new RelativeYear(2025),
                CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                CreatedBy = "Test User",
                CalculatorRunOrganisationDataMasterId = 1,
                CalculatorRunPomDataMasterId = 1,
                DefaultParameterSettingMasterId = 5,
                LapcapDataMasterId = 6
            },
            new()
            {
                Id = 4,
                CalculatorRunClassificationId = (int)RunClassification.RUNNING,
                Name = "Test Calculated Result",
                RelativeYear = new RelativeYear(2025),
                CreatedAt = new DateTime(2024, 8, 21, 14, 16, 27, DateTimeKind.Utc),
                CreatedBy = "Test User",
                CalculatorRunOrganisationDataMasterId = 2,
                CalculatorRunPomDataMasterId = 2,
                DefaultParameterSettingMasterId = 5,
                LapcapDataMasterId = 6
            }
        };
        return list;
    }
}
