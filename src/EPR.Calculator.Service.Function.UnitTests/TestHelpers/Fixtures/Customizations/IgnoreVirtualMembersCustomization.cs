using System.Reflection;
using AutoFixture.Kernel;

namespace EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures.Customizations;

/// <summary>
///     Ignore virtual members customization for AutoFixture. Prevents AutoFixture from throwing recursion exceptions due
///     to recursive EF navigation properties (which should be marked as virtual).
/// </summary>
public class IgnoreVirtualMembersCustomization : ICustomization
{
    public void Customize(IFixture fixture) =>
        fixture.Customizations.Add(new IgnoreVirtualMembers());

    private class IgnoreVirtualMembers : ISpecimenBuilder
    {
        public object? Create(object request, ISpecimenContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            var propertyInfo = request as PropertyInfo;
            if (propertyInfo == null) return new NoSpecimen();

            if (propertyInfo.GetMethod != null && propertyInfo.GetMethod.IsVirtual) return null;

            return new NoSpecimen();
        }
    }
}
