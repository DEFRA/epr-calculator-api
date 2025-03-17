using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Mappers;
using EPR.Calculator.API.UnitTests.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace EPR.Calculator.API.Tests.Controllers
{
    [TestClass]
    public class CreateDefaultParameterSettingMapperTest : BaseControllerTest
    {
        private CalculatorRunFinancialYear FinancialYear24_25 { get; } = new CalculatorRunFinancialYear { Name = "2024-25" };

        [TestMethod]
        public void Check_TheResult_Parmeter_Are_Equal_IsNotNullOf_ResultSet_WithDefaultSchemeParametersDto_WithCorrectYear()
        {
            var defaultParameterSettingMaster = new DefaultParameterSettingMaster
            {
                Id = 200,
                ParameterYear = FinancialYear24_25,
                CreatedBy = "Testuser",
                CreatedAt = DateTime.Now,
            };

            var details = new List<DefaultParameterSettingDetail>
            {
                new DefaultParameterSettingDetail
                {
                    Id = 150, 
                    DefaultParameterSettingMasterId = 200, 
                    ParameterUniqueReferenceId = "BADEBT-P",
                    ParameterValue = 30.99m,
                    DefaultParameterSettingMaster = defaultParameterSettingMaster,
                }
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
                ParameterCategory = "Bad debt provision"
            };

            //Check if dbContext is not null
            if (dbContext != null)
            {
                // Act           
                var result = CreateDefaultParameterSettingMapper.Map(defaultParameterSettingMaster, dbContext.DefaultParameterTemplateMasterList);
                Assert.AreEqual(1, result.Count);
                Assert.IsNotNull(result);
                //// Assert
                var mappedItem = result.First();
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