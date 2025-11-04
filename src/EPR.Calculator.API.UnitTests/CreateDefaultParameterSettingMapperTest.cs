using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Mappers;
using EPR.Calculator.API.UnitTests.Controllers;

namespace EPR.Calculator.API.UnitTests
{
    [TestClass]
    public class CreateDefaultParameterSettingMapperTest : BaseControllerTest
    {
        [TestMethod]
        public void Check_TheResult_Parmeter_Are_Equal_IsNotNullOf_ResultSet_WithDefaultSchemeParametersDto_WithCorrectYear()
        {
            var defaultParameterSettingMaster = new DefaultParameterSettingMaster
            {
                Id = 200,
                ParameterYear = FinancialYear24_25,
                CreatedBy = "Testuser",
                CreatedAt = DateTime.UtcNow,
            };

            var details = new List<DefaultParameterSettingDetail>
            {
                new()
                {
                    Id = 150,
                    DefaultParameterSettingMasterId = 200,
                    ParameterUniqueReferenceId = "BADEBT-P",
                    ParameterValue = 30.99m,
                    DefaultParameterSettingMaster = defaultParameterSettingMaster,
                },
            };

            var detail = new DefaultParameterSettingDetail
            {
                Id = 150,
                DefaultParameterSettingMasterId = 200,
                ParameterUniqueReferenceId = "BADEBT-P",
                ParameterValue = 30.99m,
                DefaultParameterSettingMaster = defaultParameterSettingMaster,
            };

            details.ForEach(detail => defaultParameterSettingMaster.Details.Add(detail));

            var template = new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "BADEBT-P",
                ParameterType = "Percentage",
                ParameterCategory = "Bad debt provision",
            };

            // Check if dbContext is not null
            if (DbContext != null)
            {
                // Act
                var result = CreateDefaultParameterSettingMapper.Map(
                    defaultParameterSettingMaster,
                    DbContext.DefaultParameterTemplateMasterList);
                Assert.HasCount(1, result);
                Assert.IsNotNull(result);
                //// Assert
                var mappedItem = result[0];
                Assert.AreEqual(detail.Id, mappedItem.Id);
                Assert.AreEqual(defaultParameterSettingMaster.ParameterYear.Name, mappedItem.ParameterYear);
                Assert.AreEqual(defaultParameterSettingMaster.CreatedBy, mappedItem.CreatedBy);
                Assert.AreEqual(defaultParameterSettingMaster.CreatedAt, mappedItem.CreatedAt);
                Assert.AreEqual(detail.DefaultParameterSettingMasterId, mappedItem.DefaultParameterSettingMasterId);
                Assert.AreEqual(detail.ParameterValue, mappedItem.ParameterValue);
                Assert.AreEqual(template.ParameterType, mappedItem.ParameterType);
                Assert.AreEqual(template.ParameterCategory, mappedItem.ParameterCategory);
            }
            else
            {
                throw new Exception(typeof(DefaultParameterTemplateMaster).FullName);
            }
        }
    }
}