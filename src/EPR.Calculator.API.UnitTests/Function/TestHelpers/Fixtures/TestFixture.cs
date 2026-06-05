using EntityFrameworkCore.AutoFixture.InMemory;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.Telemetry;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures.Customizations;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Utils;
using Microsoft.Extensions.Time.Testing;

namespace EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

public static class TestFixtures
{
    /// <summary>
    ///     Creates a new AutoFixture instance with many supporting services pre-registered and configured.
    /// </summary>
    public static IFixture New()
    {
        var fixture = new Fixture()
            .Customize(new AutoFreezeMoqCustomization())
            .Customize(new ImmutableCollectionsCustomization())
            .Customize(new IgnoreVirtualMembersCustomization())
            .Customize(new RelativeYearCustomization())
            .Customize(new InMemoryCustomization());

        fixture.Register<TimeProvider>(() => new FakeTimeProvider());
        fixture.Register<ITelemetryClient>(() => new TestTelemetryClient());
        fixture.Register<IBulkOperations>(() => new TestBulkOps());

        return fixture;
    }
}
