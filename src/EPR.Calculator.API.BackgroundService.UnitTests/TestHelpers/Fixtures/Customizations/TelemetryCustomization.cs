using AutoFixture.Kernel;
using EPR.Calculator.API.BackgroundService.Telemetry;

namespace EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers.Fixtures.Customizations;

/// <summary>
///     Resolves <c>ITelemetry&lt;T&gt;</c> to a real <see cref="Telemetry{TCategory}" /> instead of an
///     auto-mock.
/// </summary>
/// <remarks>
///     <see cref="Telemetry{TCategory}" /> is a transparent wrapper that must actually invoke the delegate
///     passed to Trace/TraceAsync. An auto-mocked <c>ITelemetry&lt;T&gt;</c> (via <c>MockRelay</c>) would
///     short-circuit on any unstubbed call and return <c>default(TResult)</c> without ever invoking the wrapped
///     code, silently breaking any test whose test subject depends on that code running.
/// </remarks>
public class TelemetryCustomization : ICustomization
{
    public void Customize(IFixture fixture) => fixture.Customizations.Add(new TelemetryRelay());

    private sealed class TelemetryRelay : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            if (request is not Type { IsGenericType: true } type || type.GetGenericTypeDefinition() != typeof(ITelemetry<>))
                return new NoSpecimen();

            var category = type.GetGenericArguments()[0];
            return Activator.CreateInstance(typeof(Telemetry<>).MakeGenericType(category))!;
        }
    }
}
