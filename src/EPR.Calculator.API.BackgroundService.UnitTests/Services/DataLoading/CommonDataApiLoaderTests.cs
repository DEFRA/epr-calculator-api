using System.Runtime.CompilerServices;
using EPR.Calculator.API.BackgroundService.Options;
using EPR.Calculator.API.BackgroundService.Services.DataLoading;
using EPR.Calculator.API.BackgroundService.Telemetry.Internals;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers.TestData;
using EPR.Calculator.API.Data;
using EPR.CommonDataService.DataApi.CommonDataApi;
using EPR.CommonDataService.DataApi.CommonDataApi.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.API.BackgroundService.UnitTests.Services.DataLoading;

/// <summary>
///     Unit tests for <see cref="CommonDataApiLoader" />.
///     <para>
///         The database context factory is mocked (no SQLite/InMemory) because the bulk-insert
///         and transaction behaviour in <c>UpdateDatabase</c>/<c>BulkInsert</c> is incompatible
///         with those providers.
///     </para>
///     <para>
///         Tests focus on the observable behaviour of the non-excluded code paths:
///         the disabled guard, logging, the time-provider call, and stream
///         initialisation / error handling inside <c>GetStreams</c> and <c>Run</c>.
///     </para>
/// </summary>
[TestClass]
public class CommonDataApiLoaderTests
{
    private static readonly DateTimeOffset FixedTime = new(2024, 6, 15, 12, 0, 0, TimeSpan.Zero);

    private Mock<IDbContextFactory<ApplicationDBContext>> mockDbFactory = null!;
    private Mock<ILogger<CommonDataApiLoader>> mockLogger = null!;
    private Mock<TimeProvider> mockTimeProvider = null!;

    [TestInitialize]
    public void SetUp()
    {
        mockDbFactory = new Mock<IDbContextFactory<ApplicationDBContext>>();
        mockLogger = new Mock<ILogger<CommonDataApiLoader>>();
        mockTimeProvider = new Mock<TimeProvider>();
        mockTimeProvider.Setup(t => t.GetUtcNow()).Returns(FixedTime);
    }

    // ─────────────────────────── LoadData – disabled path ───────────────────────────

    /// <summary>
    ///     When the loader is disabled the only thing it should do is log that information.
    /// </summary>
    [TestMethod]
    public async Task LoadData_WhenDisabled_DoesNotRun()
    {
        // Arrange
        var mockOrgHandler = new Mock<IStreamOrganisationsRequestHandler>();
        var mockPomHandler = new Mock<IStreamPomsRequestHandler>();
        var loader = CreateLoader(false, mockOrgHandler, mockPomHandler);

        // Act
        await loader.LoadData(TestDataHelper.CalculatorRun2024);

        // Assert
        VerifyLogContains(LogLevel.Information, "Disabled", Times.Once(), "Logger should record it is disabled.");
        mockOrgHandler.Verify(h => h.Handle(It.IsAny<int>(), It.IsAny<DateTimeOffset?>()), Times.Never, "Organisation stream should not be requested when disabled.");
        mockPomHandler.Verify(h => h.Handle(It.IsAny<int>(), It.IsAny<DateTimeOffset?>()), Times.Never, "POM stream should not be requested when disabled.");
        mockDbFactory.Verify(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()), Times.Never, "DB Context should not be created when disabled.");
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
    ///     When the POM stream fails (while the organisation stream is empty), the exception must
    ///     propagate and the organisation enumerator must be disposed.
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
    ///     When the organisation stream fails (while the POM stream is empty), the exception must
    ///     propagate and the POM enumerator must be disposed.
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

    // ─────────────────────────── Run – try-catch-finally ───────────────────────────

    /// <summary>
    ///     When stream initialisation succeeds but the DB context factory throws, the
    ///     exception must propagate through the <c>catch when</c> / <c>finally</c> block
    ///     inside <c>Run</c>, exercising the linked-cancellation-token cancellation and
    ///     stream-enumerator disposal paths.
    /// </summary>
    [TestMethod]
    public async Task LoadData_WhenDbContextCreationFails_ExceptionPropagates()
    {
        // Arrange – both streams are empty so GetStreams succeeds.
        // The DB factory then throws, causing UpdateDatabase to fail.
        mockDbFactory
            .Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("DB unavailable"));

        var loader = CreateLoader(
            true,
            organisations: EmptyAsyncEnumerable<PayCalOrganisation>(),
            poms: EmptyAsyncEnumerable<PayCalPom>());

        // Act & Assert
        await Should.ThrowAsync<InvalidOperationException>(async () => await loader.LoadData(TestDataHelper.CalculatorRun2024));
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
            Enabled = enabled,
            PomBatchSize = 100,
            OrganisationBatchSize = 100
        });

        organisationsHandler ??= new Mock<IStreamOrganisationsRequestHandler>();
        organisationsHandler
            .Setup(h => h.Handle(It.IsAny<int>(), It.IsAny<DateTimeOffset?>()))
            .Returns(organisations ?? EmptyAsyncEnumerable<PayCalOrganisation>());

        pomsHandler ??= new Mock<IStreamPomsRequestHandler>();
        pomsHandler
            .Setup(h => h.Handle(It.IsAny<int>(), It.IsAny<DateTimeOffset?>()))
            .Returns(poms ?? EmptyAsyncEnumerable<PayCalPom>());

        return new CommonDataApiLoader(
            loaderOptions,
            mockDbFactory.Object,
            organisationsHandler.Object,
            pomsHandler.Object,
            mockTimeProvider.Object,
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
