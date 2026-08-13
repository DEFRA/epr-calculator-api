using AutoFixture.Kernel;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers.Fixtures.Customizations;

/// <summary>
/// Wires up IMaterialService as a Mock returning test data from <see cref="TestDataHelper"/>.
/// </summary>
public class MaterialServiceCustomization : ICustomization
{
    public void Customize(IFixture fixture) =>
        fixture.Customizations.Add(new MaterialServiceBuilder());

    private class MaterialServiceBuilder : ISpecimenBuilder
    {
        private readonly Mock<IMaterialService> materialService = new();

        public MaterialServiceBuilder()
        {
            materialService
                .Setup(x => x.GetMaterials())
                .ReturnsAsync(TestDataHelper.GetMaterialDetails);
        }

        public object Create(object request, ISpecimenContext context)
        {
            if (!typeof(IMaterialService).Equals(request) && !typeof(Mock<IMaterialService>).Equals(request))
                return new NoSpecimen();

            return typeof(IMaterialService).Equals(request)
                ? materialService.Object
                : materialService;
        }
    }
}
