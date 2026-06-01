using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.Builder;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    /// <summary>
    /// Unit tests for the <see cref="MaterialService"/> class.
    /// </summary>
    [TestClass]
    public class MaterialServiceTests
    {
        private ApplicationDBContext dbContext;
        private MaterialService materialService;

        public MaterialServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            dbContext = new ApplicationDBContext(options);
            materialService = new MaterialService(dbContext);
        }

        [TestCleanup]
        public void Cleanup()
        {
            dbContext.Dispose();
        }

        [TestMethod]
        public async Task ShouldReturnMaterials()
        {
            // Arrange
            var materials = new List<Material>();
            var materialDetails = TestDataHelper.GetMaterials();

            foreach (var material in materialDetails)
            {
                materials.Add(new Material
                {
                    Name = material.Name,
                    Code = material.Code,
                    Description = material.Description
                });
            }

            dbContext.Material.AddRange(materials);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await materialService.GetMaterials();

            // Assert
            Assert.AreEqual(8, result.Count);
        }
    }
}