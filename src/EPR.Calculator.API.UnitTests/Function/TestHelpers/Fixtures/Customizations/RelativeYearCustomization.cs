using AutoFixture.Kernel;
using EPR.Calculator.API.Data.Models;

namespace EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures.Customizations;

/// <summary>
///     Customization for AutoFixture to generate relative years.
/// </summary>
public class RelativeYearCustomization : ICustomization
{
    public void Customize(IFixture fixture) =>
        fixture.Customizations.Add(new RelativeYears());
}

public class RelativeYears : ISpecimenBuilder
{
    public object? Create(object request, ISpecimenContext context)
    {
        if (!typeof(RelativeYear).Equals(request))
            return new NoSpecimen();

        var year = new Random().Next(2000, 2099);
        return new RelativeYear(year);
    }
}
