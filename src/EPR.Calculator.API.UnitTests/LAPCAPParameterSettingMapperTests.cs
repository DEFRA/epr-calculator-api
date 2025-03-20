using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Mappers;
using EPR.Calculator.API.UnitTests.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.Calculator.API.UnitTests
{
    [TestClass]
    public class LapcapParameterSettingMapperTests : BaseControllerTest
    {
        [TestMethod]
        public void Check_TheResult_IsNotNullOf_ResultSet_WithDefaultLAPCAPParametersDto_WithCorrectYear()
        {
            var defaultParameterSettingMaster = new LapcapDataMaster
            {
                Id = 2,
                ProjectionYear = "2024-25",
                CreatedBy = "Testuser",
                CreatedAt = DateTime.Now,
                EffectiveFrom = DateTime.Now,
            };
            var details = new List<LapcapDataDetail>
            {
                new LapcapDataDetail
                {
                    Id = 1,
                    LapcapDataMasterId = 1,
                    LapcapDataMaster = defaultParameterSettingMaster,
                    UniqueReference = "ENG-AL",
                    TotalCost = 30.99m,
                },
            };
            var detail = new LapcapDataDetail
            {
                Id = 1,
                LapcapDataMasterId = 2,
                LapcapDataMaster = defaultParameterSettingMaster,
                UniqueReference = "ENG-AL",
                TotalCost = 30.99m,
            };

            details.ForEach(defaultParameterSettingMaster.Details.Add);

            var template = new LapcapDataTemplateMaster
            {
                UniqueReference = "ENG-AL",
                Country = "England",
                Material = "Aluminium",
            };

            // Check if dbContext is not null
            if (this.DbContext != null)
            {
                // Act
                var result = LapcapDataParameterSettingMapper.Map(
                    defaultParameterSettingMaster,
                    this.DbContext.LapcapDataTemplateMaster);

                // Assert
                var mappedItem = result.First();
                Assert.AreEqual(detail.Id, mappedItem.Id);
                Assert.AreEqual(defaultParameterSettingMaster.ProjectionYear, mappedItem.ProjectionYear);
                Assert.AreEqual(defaultParameterSettingMaster.CreatedBy, mappedItem.CreatedBy);
                Assert.AreEqual(defaultParameterSettingMaster.CreatedAt, mappedItem.CreatedAt);
                Assert.AreEqual(detail.LapcapDataMasterId, mappedItem.LapcapDataMasterId);
                Assert.AreEqual(detail.UniqueReference, mappedItem.LapcapTempUniqueRef);
                Assert.AreEqual(template.Country, mappedItem.Country);
                Assert.AreEqual(template.Material, mappedItem.Material);
                Assert.AreEqual(detail.TotalCost, mappedItem.TotalCost);
                Assert.AreEqual(defaultParameterSettingMaster.EffectiveFrom, mappedItem.EffectiveFrom);
            }
            else
            {
                throw new Exception(typeof(LapcapDataTemplateMaster).FullName);
            }
        }
    }
}