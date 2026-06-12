using EntityFrameworkCore.AutoFixture.InMemory;
using EPR.Calculator.API.UnitTests.TestHelpers.Fixtures.Customizations;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EPR.Calculator.API.UnitTests.TestHelpers.Fixtures;

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
            .Customize(new InMemoryCustomization
            {
                Configure = opts => opts.ConfigureWarnings(
                    warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)
                )
            });

        return fixture;
    }
}
