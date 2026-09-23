using System.Runtime.CompilerServices;
using EPR.Calculator.Api.DataApi.AcceptedFileSelection;
using EPR.Calculator.Api.DataApi.Alignment;
using EPR.Calculator.Api.DataApi.CommonDataApi;
using EPR.Calculator.Api.DataApi.CommonDataApi.Entities;
using EPR.Calculator.Api.DataApi.CommonDataApi.LoadTables;
using EPR.Calculator.Api.DataApi.Models;
using EPR.Calculator.Api.DataApi.ObligationDetermination;
using EPR.Calculator.Api.DataApi.PomEligibility;
using EPR.Calculator.Api.DataApi.Services;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.API.DataApi.UnitTests.Services;

/// <summary>
///     Unit tests for <see cref="ProducerDataService" /> - the single entry point that replaces separate
///     org/POM streaming calls plus in-process alignment/error-detection. Uses real
///     <see cref="ProducerErrorDetector" />/<see cref="ProducerPomAligner" /> instances (cheap, pure
///     components) alongside pass-through fakes for the rest, so these tests exercise the actual
///     ordering: streaming -> file selection -> obligation/eligibility/flags -> mapping -> error
///     detection -> excluding unmatched org/subs from alignment -> dedupe -> align.
/// </summary>
[TestClass]
public class ProducerDataServiceTests
{
    [TestMethod]
    public async Task GetProducerData_HappyPath_ReturnsProducerRecord()
    {
        var submitterId = Guid.NewGuid().ToString();

        var org = new PayCalOrganisation
        {
            OrganisationId = 1,
            OrganisationName = "Org Co",
            ObligationStatus = "O",
            SubmitterId = submitterId,
            SubmissionPeriodYear = 2024,
            RegulatorStatus = "Accepted",
            HasH1 = true,
            HasH2 = true
        };

        var pom = new PayCalPom
        {
            OrganisationId = 1,
            SubmitterId = submitterId,
            PackagingType = "HH",
            PackagingMaterial = "PL",
            SubmissionPeriod = "2024-P1",
            PackagingMaterialWeight = 1000
        };

        var service = CreateService(orgs: [org], poms: [pom]);

        var result = await service.GetProducerData(2024, cutOffDate: null, materialCodes: ["PL"]);

        result.Count.ShouldBe(1);
        result[0].OrganisationId.ShouldBe(1);
        result[0].ReportedMaterials.Count.ShouldBe(1);
        result[0].ReportedMaterials[0].MaterialCode.ShouldBe("PL");
        result[0].Errors.ShouldBeEmpty();
        result[0].Warnings.ShouldBeEmpty();
        result[0].IsError.ShouldBeFalse();
    }

    [TestMethod]
    public async Task GetProducerData_ObligationDeterminationRunsBeforeMapping()
    {
        var submitterId = Guid.NewGuid().ToString();
        var rawOrganisation = new PayCalOrganisation { OrganisationId = 1, OrganisationName = "Org Co", SubmitterId = submitterId, SubmissionPeriodYear = 2024, RegulatorStatus = "Accepted" };

        var mockDeterminer = new Mock<IProducerObligationDeterminer>();
        mockDeterminer
            .Setup(d => d.Determine(It.Is<IReadOnlyList<PayCalOrganisation>>(l => l.Count == 1 && l[0] == rawOrganisation)))
            .Returns([rawOrganisation with { ObligationStatus = "O", NumDaysObligated = 42 }]);

        var service = CreateService(orgs: [rawOrganisation], poms: [], determiner: mockDeterminer.Object);

        var result = await service.GetProducerData(2024, null, []);

        // The org only becomes a record at all because the determiner returned ObligationStatus "O" -
        // that's what the aligner requires - so its presence here, with the determiner's DaysObligated,
        // is itself evidence obligation determination ran before mapping.
        result.Count.ShouldBe(1);
        result[0].DaysObligated.ShouldBe(42);
        mockDeterminer.VerifyAll();
    }

    [TestMethod]
    public async Task GetProducerData_HardErroredOrganisation_HasNoReportedMaterials_ButHasError()
    {
        var submitterId = Guid.NewGuid().ToString();

        var org = new PayCalOrganisation
        {
            OrganisationId = 1,
            OrganisationName = "Org Co",
            ObligationStatus = "E",
            ErrorCode = "some synapse error",
            SubmitterId = submitterId,
            SubmissionPeriodYear = 2024,
            RegulatorStatus = "Accepted"
        };

        var pom = new PayCalPom
        {
            OrganisationId = 1,
            SubmitterId = submitterId,
            PackagingType = "HH",
            PackagingMaterial = "PL",
            SubmissionPeriod = "2024-P1",
            PackagingMaterialWeight = 1000
        };

        var service = CreateService(orgs: [org], poms: [pom]);

        var result = await service.GetProducerData(2024, null, ["PL"]);

        // The org is obligation-status "E" and has a matching POM, so it's a hard error - it should
        // never reach Align, even though a matching POM exists.
        result.Count.ShouldBe(1);
        result[0].OrganisationId.ShouldBe(1);
        result[0].ReportedMaterials.ShouldBeEmpty();
        result[0].Errors.Count.ShouldBe(1);
        result[0].Errors[0].ErrorCode.ShouldBe("some synapse error");
        result[0].Errors[0].IsWarning.ShouldBeFalse();
        result[0].Errors[0].HasPomMatch.ShouldBeTrue();
        result[0].IsError.ShouldBeTrue();
    }

    [TestMethod]
    public async Task GetProducerData_HardErroredOrganisation_WithNoPomMatch_IsStillReturnedAsError()
    {
        // DataApi can't see billing history, so it can't decide whether a no-POM-match error is still
        // worth surfacing (e.g. the organisation was invoiced in a previous run) - it always includes
        // it, flagged with HasPomMatch = false, and leaves that decision to the caller.
        var org = new PayCalOrganisation
        {
            OrganisationId = 1,
            OrganisationName = "Org Co",
            ObligationStatus = "E",
            ErrorCode = "some synapse error",
            SubmitterId = Guid.NewGuid().ToString(),
            SubmissionPeriodYear = 2024,
            RegulatorStatus = "Accepted"
        };

        var service = CreateService(orgs: [org], poms: []);

        var result = await service.GetProducerData(2024, null, []);

        result.Count.ShouldBe(1);
        result[0].OrganisationId.ShouldBe(1);
        result[0].ReportedMaterials.ShouldBeEmpty();
        result[0].Errors.Count.ShouldBe(1);
        result[0].Errors[0].HasPomMatch.ShouldBeFalse();
    }

    [TestMethod]
    public async Task GetProducerData_ObligatedWarning_IsIncludedInBothProducersAndErrors()
    {
        var submitterId = Guid.NewGuid().ToString();

        var org = new PayCalOrganisation
        {
            OrganisationId = 1,
            OrganisationName = "Org Co",
            ObligationStatus = "O",
            ErrorCode = "some warning",
            SubmitterId = submitterId,
            SubmissionPeriodYear = 2024,
            RegulatorStatus = "Accepted",
            HasH1 = true,
            HasH2 = true
        };

        var pom = new PayCalPom
        {
            OrganisationId = 1,
            SubmitterId = submitterId,
            PackagingType = "HH",
            PackagingMaterial = "PL",
            SubmissionPeriod = "2024-P1",
            PackagingMaterialWeight = 1000
        };

        var service = CreateService(orgs: [org], poms: [pom]);

        var result = await service.GetProducerData(2024, null, ["PL"]);

        // A warning is kept in calculation - the org/sub should get both its POM data and the warning
        // on the same record.
        result.Count.ShouldBe(1);
        result[0].OrganisationId.ShouldBe(1);
        result[0].ReportedMaterials.Count.ShouldBe(1);
        result[0].Errors.ShouldBeEmpty();

        result[0].Warnings.Count.ShouldBe(1);
        result[0].Warnings[0].IsWarning.ShouldBeTrue();
        result[0].Warnings[0].ErrorCode.ShouldBe("some warning");
        result[0].Warnings[0].HasPomMatch.ShouldBeTrue();

        // A warning alone must not flip IsError - it's the two-way split consumers rely on to tell a
        // genuine calculation participant apart from a row that exists only to carry error data.
        result[0].IsError.ShouldBeFalse();
    }

    [TestMethod]
    public async Task GetProducerData_NonReportablePackaging_DoesNotReachErrorDetectionOrAlignment()
    {
        // sp_GetPaycalPomData filtered out non-reportable packaging types before any consumer saw
        // them; a POM for a subsidiary with no registration must not raise "Missing Registration Data"
        // when it isn't a reportable type.
        var submitterId = Guid.NewGuid().ToString();

        var org = new PayCalOrganisation
        {
            OrganisationId = 1, OrganisationName = "Org Co", ObligationStatus = "O",
            SubmitterId = submitterId, RegulatorStatus = "Granted", SubmissionPeriodYear = 2024, HasH1 = true, HasH2 = true
        };

        var reportablePom = new PayCalPom
        {
            OrganisationId = 1, SubmitterId = submitterId, PackagingType = "HH", PackagingMaterial = "PL",
            SubmissionPeriod = "2024-P1", PackagingMaterialWeight = 1000
        };
        var nonReportablePom = new PayCalPom
        {
            OrganisationId = 1, SubsidiaryId = "99", SubmitterId = submitterId, PackagingType = "NH",
            PackagingMaterial = "AL", SubmissionPeriod = "2024-P1", PackagingMaterialWeight = 500
        };

        var service = CreateService(orgs: [org], poms: [reportablePom, nonReportablePom]);

        var result = await service.GetProducerData(2024, null, ["PL"]);

        result.Count.ShouldBe(1);
        result[0].Errors.ShouldBeEmpty();
        result[0].Warnings.ShouldBeEmpty();
        result[0].ReportedMaterials.ShouldHaveSingleItem().MaterialCode.ShouldBe("PL");
    }

    [TestMethod]
    public async Task GetProducerData_WithUnrecognisedRamRagRating_TreatsWeightAsRed()
    {
        // A rating value that isn't one of the six known codes (unlike a blank/missing rating, which
        // is dropped from every bucket) is treated as Red rather than rejecting the row outright.
        var submitterId = Guid.NewGuid().ToString();

        var org = new PayCalOrganisation
        {
            OrganisationId = 1, OrganisationName = "Org Co", ObligationStatus = "O",
            SubmitterId = submitterId, SubmissionPeriodYear = 2024, RegulatorStatus = "Accepted", HasH1 = true, HasH2 = true
        };
        var pom = new PayCalPom
        {
            OrganisationId = 1, SubmitterId = submitterId, PackagingType = "HH", PackagingMaterial = "PL",
            SubmissionPeriod = "2024-P1", PackagingMaterialWeight = 1000, RamRagRating = "Not-A-Real-Rating"
        };

        var service = CreateService(orgs: [org], poms: [pom]);

        var result = await service.GetProducerData(2024, null, ["PL"]);

        var material = result.Single().ReportedMaterials.ShouldHaveSingleItem();
        material.TotalWeight.ShouldBe(1000d);
        material.RedWeight.ShouldBe(1000d);
        material.AmberWeight.ShouldBe(0d);
        material.GreenWeight.ShouldBe(0d);
    }

    [TestMethod]
    [DataRow("HH", "PL", true)]
    [DataRow("CW", "PL", true)]
    [DataRow("PB", "PL", true)]
    [DataRow("HDC", "GL", true)]
    [DataRow("HDC", "PL", false)]
    [DataRow("NH", "PL", false)]
    public async Task GetProducerData_ReportsPackagingOnlyWhenReportable(string packagingType, string material, bool expectedReported)
    {
        var submitterId = Guid.NewGuid().ToString();

        var org = new PayCalOrganisation
        {
            OrganisationId = 1, OrganisationName = "Org Co", ObligationStatus = "O",
            SubmitterId = submitterId, RegulatorStatus = "Granted", SubmissionPeriodYear = 2024, HasH1 = true, HasH2 = true
        };
        var pom = new PayCalPom
        {
            OrganisationId = 1, SubmitterId = submitterId, PackagingType = packagingType, PackagingMaterial = material,
            SubmissionPeriod = "2024-P1", PackagingMaterialWeight = 1000
        };

        var service = CreateService(orgs: [org], poms: [pom]);

        var result = await service.GetProducerData(2024, null, [material]);

        result.Count.ShouldBe(1);
        (result[0].ReportedMaterials.Count > 0).ShouldBe(expectedReported);
    }

    [TestMethod]
    public async Task GetProducerData_PomWithNoRegistrationAtAll_StillReturnsMissingRegistrationError()
    {
        // No PayCalOrganisation at all for this org - the registration is missing entirely, not just
        // mismatched to a different subsidiary/submitter. There's no organisation row to attach the
        // error to, so it gets a best-effort empty identity rather than being silently dropped.
        var pom = new PayCalPom
        {
            OrganisationId = 99,
            SubsidiaryId = "SUB",
            SubmitterId = Guid.NewGuid().ToString(),
            PackagingType = "HH",
            PackagingMaterial = "PL",
            SubmissionPeriod = "2024-P1",
            PackagingMaterialWeight = 1000
        };

        var service = CreateService(orgs: [], poms: [pom]);

        var result = await service.GetProducerData(2024, null, ["PL"]);

        result.Count.ShouldBe(1);
        result[0].OrganisationId.ShouldBe(99);
        result[0].SubsidiaryId.ShouldBe("SUB");
        result[0].ProducerName.ShouldBe(string.Empty);
        result[0].ReportedMaterials.ShouldBeEmpty();
        result[0].Errors.Count.ShouldBe(1);
        result[0].Errors[0].ErrorCode.ShouldBe("Missing Registration Data");
        result[0].IsError.ShouldBeTrue();
    }

    [TestMethod]
    public async Task GetProducerData_PomEligibility_ExcludesCancelledRegistrationsFromTheGate()
    {
        var submitterId = Guid.NewGuid().ToString();
        var granted = new PayCalOrganisation { OrganisationId = 1, OrganisationName = "A", ObligationStatus = "O", SubmitterId = submitterId, RegulatorStatus = "Granted", SubmissionPeriodYear = 2024 };
        var accepted = new PayCalOrganisation { OrganisationId = 2, OrganisationName = "B", ObligationStatus = "O", SubmitterId = submitterId, RegulatorStatus = "Accepted", SubmissionPeriodYear = 2024 };
        var cancelled = new PayCalOrganisation { OrganisationId = 3, OrganisationName = "C", ObligationStatus = "E", SubmitterId = submitterId, RegulatorStatus = "Cancelled", SubmissionPeriodYear = 2024 };

        IReadOnlyCollection<int>? capturedIds = null;
        var mockEligibilityFilter = new Mock<IPomEligibilityFilter>();
        mockEligibilityFilter
            .Setup(f => f.Filter(It.IsAny<IReadOnlyList<PayCalPom>>(), It.IsAny<IReadOnlyCollection<int>>()))
            .Callback((IReadOnlyList<PayCalPom> _, IReadOnlyCollection<int> ids) => capturedIds = ids)
            .Returns((IReadOnlyList<PayCalPom> p, IReadOnlyCollection<int> _) => p);

        var service = CreateService(orgs: [granted, accepted, cancelled], poms: [], eligibilityFilter: mockEligibilityFilter.Object);

        await service.GetProducerData(2024, null, []);

        capturedIds.ShouldNotBeNull();
        capturedIds.OrderBy(x => x).ShouldBe([1, 2]);
    }

    [TestMethod]
    public async Task GetProducerData_WhenBothStreamsFail_Throws()
    {
        var service = CreateService(
            orgsStream: ThrowingAsyncEnumerable<PayCalOrganisation>(new InvalidOperationException("org stream failed")),
            pomsStream: ThrowingAsyncEnumerable<PayCalPom>(new InvalidOperationException("pom stream failed")));

        await Should.ThrowAsync<InvalidOperationException>(async () => await service.GetProducerData(2024, null, []));
    }

    [TestMethod]
    public async Task GetProducerData_WhenAlreadyCancelled_Throws()
    {
        var service = CreateService(orgs: [], poms: []);

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Should.ThrowAsync<OperationCanceledException>(async () => await service.GetProducerData(2024, null, [], cts.Token));
    }

    [TestMethod]
    public async Task GetProducerData_WhenLoadTableEnabled_RefreshesLoadTablesFirst()
    {
        var refresher = new Mock<ILoadTableRefresher>();
        var service = CreateService(orgs: [], poms: [], loadTableEnabled: true, loadTableRefresher: refresher.Object);

        await service.GetProducerData(2024, cutOffDate: null, materialCodes: []);

        refresher.Verify(r => r.RefreshAsync(2024, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task GetProducerData_WhenLoadTableDisabled_DoesNotRefreshLoadTables()
    {
        var refresher = new Mock<ILoadTableRefresher>();
        var service = CreateService(orgs: [], poms: [], loadTableEnabled: false, loadTableRefresher: refresher.Object);

        await service.GetProducerData(2024, cutOffDate: null, materialCodes: []);

        refresher.Verify(r => r.RefreshAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static ProducerDataService CreateService(
        IReadOnlyList<PayCalOrganisation>? orgs = null,
        IReadOnlyList<PayCalPom>? poms = null,
        IAsyncEnumerable<PayCalOrganisation>? orgsStream = null,
        IAsyncEnumerable<PayCalPom>? pomsStream = null,
        IProducerObligationDeterminer? determiner = null,
        IPomEligibilityFilter? eligibilityFilter = null,
        bool loadTableEnabled = false,
        ILoadTableRefresher? loadTableRefresher = null)
    {
        var mockOrgHandler = new Mock<IStreamOrganisationsRequestHandler>();
        mockOrgHandler
            .Setup(h => h.Handle(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(() => orgsStream ?? ToAsyncEnumerable(orgs ?? []));

        // Handle() is invoked twice by StreamPoms (once to pick winning files, once to buffer their
        // rows), so hand out a fresh enumerable each time.
        var mockPomHandler = new Mock<IStreamPomsRequestHandler>();
        mockPomHandler
            .Setup(h => h.Handle(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(() => pomsStream ?? ToAsyncEnumerable(poms ?? []));

        // The real selector is a cheap pure component - the fixtures leave file names null, so every
        // group's sole candidate wins and nothing is filtered out, matching the previous pass-through.
        var selector = new AcceptedFileSelector();

        IPomEligibilityFilter eligibilityFilterToUse;
        if (eligibilityFilter is not null)
        {
            eligibilityFilterToUse = eligibilityFilter;
        }
        else
        {
            var mockEligibilityFilter = new Mock<IPomEligibilityFilter>();
            mockEligibilityFilter
                .Setup(f => f.Filter(It.IsAny<IReadOnlyList<PayCalPom>>(), It.IsAny<IReadOnlyCollection<int>>()))
                .Returns((IReadOnlyList<PayCalPom> p, IReadOnlyCollection<int> _) => p);
            eligibilityFilterToUse = mockEligibilityFilter.Object;
        }

        var mockFlagsCalculator = new Mock<IOrganisationPeriodFlagsCalculator>();
        mockFlagsCalculator
            .Setup(c => c.ApplyPeriodFlags(It.IsAny<IReadOnlyList<PayCalOrganisation>>(), It.IsAny<IReadOnlyList<PayCalPom>>()))
            .Returns((IReadOnlyList<PayCalOrganisation> o, IReadOnlyList<PayCalPom> _) => o);

        var mockDeterminer = determiner is null ? new Mock<IProducerObligationDeterminer>() : null;
        mockDeterminer?
            .Setup(d => d.Determine(It.IsAny<IReadOnlyList<PayCalOrganisation>>()))
            .Returns((IReadOnlyList<PayCalOrganisation> o) => o);

        // The load-table stage's DB behaviour is exercised in the integration tests; here the source
        // reads straight from the mocked handlers and the refresher is a mock so these tests can
        // assert the routing (RefreshAsync runs only when the option is on).
        var dataSource = new CommonDataApiSource(mockOrgHandler.Object, mockPomHandler.Object);
        var loadOptions = Options.Create(new DataApiLoadOptions { Enabled = loadTableEnabled });

        var pipeline = new ProducerDataPipelineServices(
            selector,
            determiner ?? mockDeterminer!.Object,
            eligibilityFilterToUse,
            mockFlagsCalculator.Object,
            new ProducerErrorDetector(),
            new ProducerPomAligner());

        return new ProducerDataService(
            dataSource,
            pipeline,
            loadTableRefresher ?? Mock.Of<ILoadTableRefresher>(),
            loadOptions);
    }

    private static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(
        IReadOnlyList<T> items,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        foreach (var item in items)
        {
            yield return item;
            await Task.Yield();
        }
    }

    private static async IAsyncEnumerable<T> ThrowingAsyncEnumerable<T>(
        Exception exception,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();
        throw exception;
#pragma warning disable CS0162
        yield break;
#pragma warning restore CS0162
    }
}
