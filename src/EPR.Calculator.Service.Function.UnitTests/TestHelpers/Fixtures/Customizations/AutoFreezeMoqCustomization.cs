using System.Collections.Concurrent;
using AutoFixture.AutoMoq;
using AutoFixture.Kernel;

namespace EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures.Customizations;

/// <summary>
///     Basically <see cref="AutoMoqCustomization" />, but freezes any Moq created objects automatically.
/// </summary>
public class AutoFreezeMoqCustomization : ICustomization
{
    public void Customize(IFixture fixture)
    {
        fixture.Customizations.Add(
            new CachedSpecimensBuilder(
                new MockPostprocessor(
                    new MethodInvoker(new MockConstructorQuery()))));

        fixture.ResidueCollectors.Add(new MockRelay());
    }

    private class CachedSpecimensBuilder(ISpecimenBuilder builder) : ISpecimenBuilder
    {
        private readonly ConcurrentDictionary<object, object> _instances = new();

        public object Create(object request, ISpecimenContext context) => _instances.GetOrAdd(request, r => builder.Create(r, context));
    }
}
