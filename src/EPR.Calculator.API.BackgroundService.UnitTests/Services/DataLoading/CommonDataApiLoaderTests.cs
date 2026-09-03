using System.Runtime.CompilerServices;
using EPR.Calculator.API.BackgroundService.Options;
using EPR.Calculator.API.BackgroundService.Services.DataLoading;
using EPR.Calculator.API.BackgroundService.Telemetry.Internals;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers.TestData;
using EPR.CommonDataService.DataApi.CommonDataApi;
using EPR.CommonDataService.DataApi.CommonDataApi.Entities;
using EPR.CommonDataService.DataApi.CommonDataApi.ObligationDetermination;
using EPR.CommonDataService.DataApi.CommonDataApi.PomEligibility;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.API.BackgroundService.UnitTests.Services.DataLoading;

/// <summary>
///     Unit tests for <see cref="CommonDataApiLoader" />.
///     <para>
///         The loader performs no database access at all - it streams organisations and POMs from
///         DataApi into memory and returns them. Tests focus on: the disabled guard, stream failure
///         propagation, cancellation, and that streamed items are correctly mapped and returned.
///     </para>
/// </summary>
[TestClass]
public class CommonDataApiLoaderTests
{
    private Mock<ILogger<CommonDataApiLoader>> mockLogger = null!;

    [TestInitialize]
    public void SetUp()
    {
        mockLogger = new Mock<ILogger<CommonDataApiLoader>>();
    }

    // ─────────────────────────── LoadData – disabled path ───────────────────────────

    /// <summary>
    ///     When the loader is disabled the only thing it should do is log that information and
    ///     return empty results.
    /// </summary>
    [TestMethod]
    public async Task LoadData_WhenDisabled_DoesNotRun()
    {
        // Arrange
        var mockOrgHandler = new Mock<IStreamOrganisationsRequestHandler>();
        var mockPomHandler = new Mock<IStreamPomsRequestHandler>();
        var loader = CreateLoader(false, mockOrgHandler, mockPomHandler);

        // Act
        var (organisations, poms) = await loader.LoadData(TestDataHelper.CalculatorRun2024);

        // Assert
        VerifyLogContains(LogLevel.Information, "Disabled", Times.Once(), "Logger should record it is disabled.");
        organisations.ShouldBeEmpty();
        poms.ShouldBeEmpty();
        mockOrgHandler.Verify(h => h.Handle(It.IsAny<int>(), It.IsAny<DateTimeOffset?>()), Times.Never, "Organisation stream should not be requested when disabled.");
        mockPomHandler.Verify(h => h.Handle(It.IsAny<int>(), It.IsAny<DateTimeOffset?>()), Times.Never, "POM stream should not be requested when disabled.");
    }

    // ─────────────────────────── LoadData – enabled path, happy path ───────────────────────────

    /// <summary>
    ///     When enabled, streamed organisations/POMs are mapped and returned.
    /// </summary>
    [TestMethod]
    public async Task LoadData_WhenEnabled_ReturnsMappedOrganisationsAndPoms()
    {
        // Arrange
        var submitterId = Guid.NewGuid().ToString();
        var loader = CreateLoader(
            true,
            organisations: ToAsyncEnumerable(new PayCalOrganisation
            {
                OrganisationId = 1,
                OrganisationName = "Org Co",
                ObligationStatus = "O",
                SubmitterId = submitterId
            }),
            poms: ToAsyncEnumerable(new PayCalPom
            {
                OrganisationId = 1,
                SubmitterId = submitterId,
                PackagingMaterial = "PL"
            }));

        // Act
        var (organisations, poms) = await loader.LoadData(TestDataHelper.CalculatorRun2024);

        // Assert
        organisations.Count.ShouldBe(1);
        organisations[0].OrganisationId.ShouldBe(1);
        organisations[0].OrganisationName.ShouldBe("Org Co");
        poms.Count.ShouldBe(1);
        poms[0].PackagingMaterial.ShouldBe("PL");
    }

    /// <summary>
    ///     Obligation determination runs over the whole streamed organisation list before mapping - the
    ///     mapped result must reflect what the determiner returns, not the raw streamed input.
    /// </summary>
    [TestMethod]
    public async Task LoadData_WhenEnabled_DeterminesObligationBeforeMapping()
    {
        // Arrange
        var submitterId = Guid.NewGuid().ToString();
        var rawOrganisation = new PayCalOrganisation
        {
            OrganisationId = 1,
            OrganisationName = "Org Co",
            SubmitterId = submitterId
        };

        var mockOrganisationsHandler = new Mock<IStreamOrganisationsRequestHandler>();
        var mockDeterminer = new Mock<IProducerObligationDeterminer>();
        mockDeterminer
            .Setup(d => d.Determine(It.Is<IReadOnlyList<PayCalOrganisation>>(l => l.Count == 1 && l[0] == rawOrganisation)))
            .Returns([rawOrganisation with { ObligationStatus = "O", NumDaysObligated = 42 }]);

        var loaderOptions = new OptionsWrapper<CommonDataApiLoaderOptions>(new CommonDataApiLoaderOptions { Enabled = true });
        mockOrganisationsHandler
            .Setup(h => h.Handle(It.IsAny<int>(), It.IsAny<DateTimeOffset?>()))
            .Returns(ToAsyncEnumerable(rawOrganisation));
        var mockPomsHandler = new Mock<IStreamPomsRequestHandler>();
        mockPomsHandler
            .Setup(h => h.Handle(It.IsAny<int>(), It.IsAny<DateTimeOffset?>()))
            .Returns(EmptyAsyncEnumerable<PayCalPom>());

        var mockPomEligibilityFilter = new Mock<IPomEligibilityFilter>();
        mockPomEligibilityFilter
            .Setup(f => f.Filter(It.IsAny<IReadOnlyList<PayCalPom>>(), It.IsAny<IReadOnlyCollection<int>>()))
            .Returns((IReadOnlyList<PayCalPom> poms, IReadOnlyCollection<int> _) => poms);

        var loader = new CommonDataApiLoader(
            loaderOptions,
            mockOrganisationsHandler.Object,
            mockPomsHandler.Object,
            mockDeterminer.Object,
            mockPomEligibilityFilter.Object,
            mockLogger.Object,
            new Telemetry<CommonDataApiLoader>());

        // Act
        var (organisations, _) = await loader.LoadData(TestDataHelper.CalculatorRun2024);

        // Assert
        organisations.Count.ShouldBe(1);
        organisations[0].ObligationStatus.ShouldBe("O");
        organisations[0].DaysObligated.ShouldBe((short)42);
        mockDeterminer.VerifyAll();
    }

    // ─────────────────────────── LoadData – enabled path, stream failures ───────────────────────────

    /// <summary>
    ///     When both streams fail, the exception must propagate out of <see cref="CommonDataApiLoader.LoadData" />.
    /// </summary>
    [TestMethod]
    public async Task LoadData_WhenBothStreamsFail_ThrowsException()
    {
        // Arrange
        var loader = CreateLoader(
            true,
            organisations: ThrowingAsyncEnumerable<PayCalOrganisation>(new InvalidOperationException("org stream failed")),
            poms: ThrowingAsyncEnumerable<PayCalPom>(new InvalidOperationException("pom stream failed")));

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(async () => await loader.LoadData(TestDataHelper.CalculatorRun2024));
    }

    /// <summary>
    ///     When the POM stream fails (while the organisation stream is empty), the exception must propagate.
    /// </summary>
    [TestMethod]
    public async Task LoadData_WhenPomStreamFails_ThrowsException()
    {
        // Arrange
        var loader = CreateLoader(
            true,
            organisations: EmptyAsyncEnumerable<PayCalOrganisation>(),
            poms: ThrowingAsyncEnumerable<PayCalPom>(new InvalidOperationException("pom stream failed")));

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(async () => await loader.LoadData(TestDataHelper.CalculatorRun2024));
    }

    /// <summary>
    ///     When the organisation stream fails (while the POM stream is empty), the exception must propagate.
    /// </summary>
    [TestMethod]
    public async Task LoadData_WhenOrgStreamFails_ThrowsException()
    {
        // Arrange
        var loader = CreateLoader(
            true,
            organisations: ThrowingAsyncEnumerable<PayCalOrganisation>(new InvalidOperationException("org stream failed")),
            poms: EmptyAsyncEnumerable<PayCalPom>());

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(async () => await loader.LoadData(TestDataHelper.CalculatorRun2024));
    }

    // ─────────────────────────── LoadData – cancellation ───────────────────────────

    /// <summary>
    ///     When the supplied cancellation token is already cancelled before the streams are
    ///     initialised, an <see cref="OperationCanceledException" /> must propagate.
    /// </summary>
    [TestMethod]
    public async Task LoadData_WhenAlreadyCancelled_Throws()
    {
        // Arrange
        var loader = CreateLoader(
            true,
            organisations: EmptyAsyncEnumerable<PayCalOrganisation>(),
            poms: EmptyAsyncEnumerable<PayCalPom>());

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () => await loader.LoadData(TestDataHelper.CalculatorRun2024, cts.Token));
    }

    // ─────────────────────────── Helpers ───────────────────────────

    private CommonDataApiLoader CreateLoader(
        bool enabled,
        Mock<IStreamOrganisationsRequestHandler>? organisationsHandler = null,
        Mock<IStreamPomsRequestHandler>? pomsHandler = null,
        IAsyncEnumerable<PayCalOrganisation>? organisations = null,
        IAsyncEnumerable<PayCalPom>? poms = null)
    {
        var loaderOptions = new OptionsWrapper<CommonDataApiLoaderOptions>(new CommonDataApiLoaderOptions
        {
            Enabled = enabled
        });

        organisationsHandler ??= new Mock<IStreamOrganisationsRequestHandler>();
        organisationsHandler
            .Setup(h => h.Handle(It.IsAny<int>(), It.IsAny<DateTimeOffset?>()))
            .Returns(organisations ?? EmptyAsyncEnumerable<PayCalOrganisation>());

        pomsHandler ??= new Mock<IStreamPomsRequestHandler>();
        pomsHandler
            .Setup(h => h.Handle(It.IsAny<int>(), It.IsAny<DateTimeOffset?>()))
            .Returns(poms ?? EmptyAsyncEnumerable<PayCalPom>());

        var mockObligationDeterminer = new Mock<IProducerObligationDeterminer>();
        mockObligationDeterminer
            .Setup(d => d.Determine(It.IsAny<IReadOnlyList<PayCalOrganisation>>()))
            .Returns((IReadOnlyList<PayCalOrganisation> organisations) => organisations);

        var mockPomEligibilityFilter = new Mock<IPomEligibilityFilter>();
        mockPomEligibilityFilter
            .Setup(f => f.Filter(It.IsAny<IReadOnlyList<PayCalPom>>(), It.IsAny<IReadOnlyCollection<int>>()))
            .Returns((IReadOnlyList<PayCalPom> poms, IReadOnlyCollection<int> _) => poms);

        return new CommonDataApiLoader(
            loaderOptions,
            organisationsHandler.Object,
            pomsHandler.Object,
            mockObligationDeterminer.Object,
            mockPomEligibilityFilter.Object,
            mockLogger.Object,
            new Telemetry<CommonDataApiLoader>());
    }

    /// <summary>
    ///     Verifies that the mock logger received a log entry at the given level whose message contains
    ///     <paramref name="text" />.
    /// </summary>
    private void VerifyLogContains(LogLevel level, string text, Times times, string? failMessage = null)
    {
        mockLogger.Verify(
            l => l.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, _) => o.ToString()!.Contains(text)),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times,
            failMessage ?? $"Log message should contain '{text}'.");
    }

    // ─────────────────────────── Fake async streams ───────────────────────────

    private static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(params T[] items)
    {
        foreach (var item in items)
        {
            yield return item;
            await Task.Yield();
        }
    }

    private static async IAsyncEnumerable<T> EmptyAsyncEnumerable<T>(
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        yield break;
    }

    private static async IAsyncEnumerable<T> ThrowingAsyncEnumerable<T>(
        Exception exception,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        cancellationToken.ThrowIfCancellationRequested();

        if (exception != null) throw exception;

        yield break;
    }
}
