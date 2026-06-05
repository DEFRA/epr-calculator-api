using EntityFrameworkCore.AutoFixture.InMemory;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.Telemetry;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures.Customizations;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Services;
using Microsoft.EntityFrameworkCore.Diagnostics;
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
            .Customize(new MaterialServiceCustomization())
            .Customize(new InMemoryCustomization
            {
                Configure = opts => opts.ConfigureWarnings(
                    warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)
                )
            });

        fixture.Register<TimeProvider>(() => new FakeTimeProvider());
        fixture.Register<ITelemetryClient>(() => new TestTelemetryClient());
        fixture.Register<IBulkOperations>(() => new TestBulkOps());

        return fixture;
    }
}
