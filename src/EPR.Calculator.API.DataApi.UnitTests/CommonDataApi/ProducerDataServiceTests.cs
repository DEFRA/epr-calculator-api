using System.Runtime.CompilerServices;
using EPR.CommonDataService.DataApi.AcceptedFileSelection;
using EPR.CommonDataService.DataApi.Alignment;
using EPR.CommonDataService.DataApi.CommonDataApi;
using EPR.CommonDataService.DataApi.CommonDataApi.Entities;
using EPR.CommonDataService.DataApi.ObligationDetermination;
using EPR.CommonDataService.DataApi.PomEligibility;

namespace EPR.Calculator.API.DataApi.UnitTests.CommonDataApi;

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
    public async Task GetProducerData_HappyPath_ReturnsOrganisationsAndAlignedProducers()
    {
        var submitterId = Guid.NewGuid().ToString();

        var org = new PayCalOrganisation
        {
            OrganisationId = 1,
            OrganisationName = "Org Co",
            ObligationStatus = "O",
            SubmitterId = submitterId,
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

        var result = await service.GetProducerData(2024, cutOffDate: null, materialCodes: ["PL"], invoicedOrganisationIds: []);

        result.Organisations.Count.ShouldBe(1);
        result.Organisations[0].OrganisationId.ShouldBe(1);

        result.Producers.Count.ShouldBe(1);
        result.Producers[0].OrganisationId.ShouldBe(1);
        result.Producers[0].ReportedMaterials.Count.ShouldBe(1);
        result.Producers[0].ReportedMaterials[0].MaterialCode.ShouldBe("PL");

        result.Errors.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task GetProducerData_ObligationDeterminationRunsBeforeMapping()
    {
        var submitterId = Guid.NewGuid().ToString();
        var rawOrganisation = new PayCalOrganisation { OrganisationId = 1, OrganisationName = "Org Co", SubmitterId = submitterId };

        var mockDeterminer = new Mock<IProducerObligationDeterminer>();
        mockDeterminer
            .Setup(d => d.Determine(It.Is<IReadOnlyList<PayCalOrganisation>>(l => l.Count == 1 && l[0] == rawOrganisation)))
            .Returns([rawOrganisation with { ObligationStatus = "O", NumDaysObligated = 42 }]);

        var service = CreateService(orgs: [rawOrganisation], poms: [], determiner: mockDeterminer.Object);

        var result = await service.GetProducerData(2024, null, [], []);

        result.Organisations.Count.ShouldBe(1);
        result.Organisations[0].ObligationStatus.ShouldBe("O");
        result.Organisations[0].DaysObligated.ShouldBe(42);
        mockDeterminer.VerifyAll();
    }

    [TestMethod]
    public async Task GetProducerData_HardErroredOrganisation_IsExcludedFromProducers_ButIncludedInErrors()
    {
        var submitterId = Guid.NewGuid().ToString();

        var org = new PayCalOrganisation
        {
            OrganisationId = 1,
            OrganisationName = "Org Co",
            ObligationStatus = "E",
            ErrorCode = "some synapse error",
            SubmitterId = submitterId
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

        var result = await service.GetProducerData(2024, null, ["PL"], []);

        // The org is obligation-status "E" and has a matching POM, so it's a hard error - it should
        // never reach Align, even though a matching POM exists.
        result.Producers.ShouldBeEmpty();
        result.Errors.Count.ShouldBe(1);
        result.Errors[0].OrganisationId.ShouldBe(1);
        result.Errors[0].ErrorCode.ShouldBe("some synapse error");
        result.Errors[0].IsWarning.ShouldBeFalse();
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

        var result = await service.GetProducerData(2024, null, ["PL"], []);

        // A warning is kept in calculation - the org/sub should get both its POM data (via Producers)
        // and the warning (via Errors).
        result.Producers.Count.ShouldBe(1);
        result.Producers[0].OrganisationId.ShouldBe(1);

        result.Errors.Count.ShouldBe(1);
        result.Errors[0].IsWarning.ShouldBeTrue();
        result.Errors[0].ErrorCode.ShouldBe("some warning");
    }

    [TestMethod]
    public async Task GetProducerData_WhenBothStreamsFail_Throws()
    {
        var service = CreateService(
            orgsStream: ThrowingAsyncEnumerable<PayCalOrganisation>(new InvalidOperationException("org stream failed")),
            pomsStream: ThrowingAsyncEnumerable<PayCalPom>(new InvalidOperationException("pom stream failed")));

        await Should.ThrowAsync<InvalidOperationException>(async () => await service.GetProducerData(2024, null, [], []));
    }

    [TestMethod]
    public async Task GetProducerData_WhenAlreadyCancelled_Throws()
    {
        var service = CreateService(orgs: [], poms: []);

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Should.ThrowAsync<OperationCanceledException>(async () => await service.GetProducerData(2024, null, [], [], cts.Token));
    }

    private static ProducerDataService CreateService(
        IReadOnlyList<PayCalOrganisation>? orgs = null,
        IReadOnlyList<PayCalPom>? poms = null,
        IAsyncEnumerable<PayCalOrganisation>? orgsStream = null,
        IAsyncEnumerable<PayCalPom>? pomsStream = null,
        IProducerObligationDeterminer? determiner = null)
    {
        var mockOrgHandler = new Mock<IStreamOrganisationsRequestHandler>();
        mockOrgHandler
            .Setup(h => h.Handle(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(orgsStream ?? ToAsyncEnumerable(orgs ?? []));

        var mockPomHandler = new Mock<IStreamPomsRequestHandler>();
        mockPomHandler
            .Setup(h => h.Handle(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(pomsStream ?? ToAsyncEnumerable(poms ?? []));

        var mockSelector = new Mock<IAcceptedFileSelector>();
        mockSelector
            .Setup(s => s.SelectLatestOrganisationFiles(It.IsAny<IReadOnlyList<PayCalOrganisation>>(), It.IsAny<DateTimeOffset?>()))
            .Returns((IReadOnlyList<PayCalOrganisation> o, DateTimeOffset? _) => o);
        mockSelector
            .Setup(s => s.SelectLatestPomFiles(It.IsAny<IReadOnlyList<PayCalPom>>(), It.IsAny<DateTimeOffset?>()))
            .Returns((IReadOnlyList<PayCalPom> p, DateTimeOffset? _) => p);

        var mockEligibilityFilter = new Mock<IPomEligibilityFilter>();
        mockEligibilityFilter
            .Setup(f => f.Filter(It.IsAny<IReadOnlyList<PayCalPom>>(), It.IsAny<IReadOnlyCollection<int>>()))
            .Returns((IReadOnlyList<PayCalPom> p, IReadOnlyCollection<int> _) => p);

        var mockFlagsCalculator = new Mock<IOrganisationPeriodFlagsCalculator>();
        mockFlagsCalculator
            .Setup(c => c.ApplyPeriodFlags(It.IsAny<IReadOnlyList<PayCalOrganisation>>(), It.IsAny<IReadOnlyList<PayCalPom>>()))
            .Returns((IReadOnlyList<PayCalOrganisation> o, IReadOnlyList<PayCalPom> _) => o);

        var mockDeterminer = determiner is null ? new Mock<IProducerObligationDeterminer>() : null;
        mockDeterminer?
            .Setup(d => d.Determine(It.IsAny<IReadOnlyList<PayCalOrganisation>>()))
            .Returns((IReadOnlyList<PayCalOrganisation> o) => o);

        return new ProducerDataService(
            mockOrgHandler.Object,
            mockPomHandler.Object,
            mockSelector.Object,
            determiner ?? mockDeterminer!.Object,
            mockEligibilityFilter.Object,
            mockFlagsCalculator.Object,
            new ProducerErrorDetector(),
            new ProducerPomAligner());
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
