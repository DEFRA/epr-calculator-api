using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers.Fixtures;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers.TestData;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.CommonDataService.DataApi.Alignment;

namespace EPR.Calculator.API.BackgroundService.UnitTests.Services;

/// <summary>
///     Unit tests for <see cref="ProducerDataTransposer" />, focused on how a single unified
///     <see cref="ProducerRecord" /> list is split across the three tables it feeds:
///     <see cref="CalculatorRunOrganisation" /> (every record), <see cref="ProducerDetail" /> (only
///     records with reported materials), and <see cref="IErrorReportService" /> (every record's
///     errors/warnings, flattened back into a keyed list).
/// </summary>
[TestClass]
public class ProducerDataTransposerTests
{
    private ApplicationDBContext dbContext = null!;
    private Mock<IErrorReportService> errorReportService = null!;
    private ProducerDataTransposer sut = null!;

    [TestInitialize]
    public void Setup()
    {
        var fixture = TestFixtures.New();
        dbContext = fixture.Freeze<ApplicationDBContext>();
        errorReportService = fixture.Freeze<Mock<IErrorReportService>>();

        dbContext.CalculatorRuns.Add(new CalculatorRun { Id = TestDataHelper.CalculatorRun2024.RunId, Name = "Test run" });
        dbContext.Material.Add(new Material { Id = 1, Code = "PL", Name = "Plastic" });
        dbContext.SaveChanges();

        sut = fixture.Create<ProducerDataTransposer>();
    }

    [TestMethod]
    public async Task Transpose_WritesCalculatorRunOrganisation_ForEveryRecord()
    {
        var records = new[]
        {
            ProducerRecordWithMaterials(organisationId: 1),
            ProducerRecordWithNoMaterials(organisationId: 2)
        };

        await sut.Transpose(TestDataHelper.CalculatorRun2024, records, CancellationToken.None);

        var organisations = dbContext.CalculatorRunOrganisations.ToList();
        organisations.Count.ShouldBe(2);
        organisations.ShouldContain(o => o.OrganisationId == 1);
        organisations.ShouldContain(o => o.OrganisationId == 2);
    }

    [TestMethod]
    public async Task Transpose_WritesProducerDetail_OnlyForRecordsWithReportedMaterials()
    {
        var records = new[]
        {
            ProducerRecordWithMaterials(organisationId: 1),
            ProducerRecordWithNoMaterials(organisationId: 2)
        };

        await sut.Transpose(TestDataHelper.CalculatorRun2024, records, CancellationToken.None);

        var producerDetails = dbContext.ProducerDetail.ToList();
        producerDetails.Count.ShouldBe(1);
        producerDetails[0].ProducerId.ShouldBe(1);
    }

    [TestMethod]
    public async Task Transpose_FlattensErrorsAndWarnings_IntoKeyedErrorList()
    {
        var error = new ProducerCalculationError { ErrorCode = "some error", LeaverCode = "01", IsWarning = false, HasPomMatch = true };
        var warning = new ProducerCalculationError { ErrorCode = "some warning", LeaverCode = "", IsWarning = true, HasPomMatch = true };

        var records = new[]
        {
            ProducerRecordWithNoMaterials(organisationId: 1) with { Errors = [error] },
            ProducerRecordWithMaterials(organisationId: 2, subsidiaryId: "SUB") with { Warnings = [warning] }
        };

        await sut.Transpose(TestDataHelper.CalculatorRun2024, records, CancellationToken.None);

        errorReportService.Verify(s => s.PersistErrors(
            It.Is<IReadOnlyList<OrganisationCalculationError>>(errors =>
                errors.Count == 2 &&
                errors.Any(e => e.OrganisationId == 1 && e.SubsidiaryId == null && e.Error == error) &&
                errors.Any(e => e.OrganisationId == 2 && e.SubsidiaryId == "SUB" && e.Error == warning)),
            TestDataHelper.CalculatorRun2024.RunId,
            It.IsAny<string>(),
            TestDataHelper.CalculatorRun2024.RelativeYear,
            It.IsAny<CancellationToken>()));
    }

    private static ProducerRecord ProducerRecordWithMaterials(int organisationId, string? subsidiaryId = null) => new()
    {
        OrganisationId = organisationId,
        SubsidiaryId = subsidiaryId,
        ProducerName = "Org Co",
        ObligationStatus = "O",
        Errors = [],
        Warnings = [],
        ReportedMaterials =
        [
            new AlignedReportedMaterial
            {
                MaterialCode = "PL",
                PackagingType = "HH",
                SubmissionPeriod = "2024-P1",
                TotalWeight = 100d,
                RedWeight = 0d,
                AmberWeight = 0d,
                GreenWeight = 100d,
                RedMedicalWeight = 0d,
                AmberMedicalWeight = 0d,
                GreenMedicalWeight = 0d
            }
        ]
    };

    private static ProducerRecord ProducerRecordWithNoMaterials(int organisationId, string? subsidiaryId = null) => new()
    {
        OrganisationId = organisationId,
        SubsidiaryId = subsidiaryId,
        ProducerName = "Holding Co",
        ObligationStatus = "O",
        Errors = [],
        Warnings = [],
        ReportedMaterials = []
    };
}
