using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Services;

[TestClass]
public class MaterialServiceTests : TestsFor<MaterialService>
{
    [TestMethod]
    public async Task ShouldReturnMaterials()
    {
        // Arrange
        var materials = new List<Material>();
        var materialDetails = TestDataHelper.GetMaterialDetails();

        foreach (var material in materialDetails)
        {
            materials.Add(new Material
            {
                Name = material.Name,
                Code = material.Code,
                Description = "ignored"
            });
        }

        dbContext.Material.AddRange(materials);
        await dbContext.SaveChangesAsync();

        // Act
        var result = await testSubject.GetMaterials();

        // Assert
        Assert.AreEqual(8, result.Count);
    }
}
