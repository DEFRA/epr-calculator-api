using EPR.Calculator.API.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace api.Tests.Controllers
{
    [TestClass]
    public class DefaultParameterSettingControllerTest : BaseControllerTest
    {
        private static string[] _uniqueReferences = {"BADEBT-P", "COMC-AL", "COMC-FC", "COMC-GL",
                                                     "COMC-OT", "COMC-PC", "COMC-PL", "COMC-ST",
                                                     "COMC-WD", "LAPC-ENG","LAPC-NIR", "LAPC-SCT",
                                                    "LAPC-WLS", "LEVY-ENG", "LEVY-NIR", "LEVY-SCT", "LEVY-WLS",
                                                    "LRET-AL", "LRET-FC", "LRET-GL", "LRET-OT",
                                                    "LRET-PC", "LRET-PL", "LRET-ST", "LRET-WD", "MATT-AD",
                                                    "MATT-AI", "MATT-PD", "MATT-PI", "SAOC-ENG", "SAOC-NIR", 
                                                    "SAOC-SCT", "SAOC-WLS", "SCSC-ENG","SCSC-NIR", "SCSC-SCT",
                                                    "SCSC-WLS", "TONT-AI", "TONT-DI", "TONT-PD","TONT-PI" };

        [TestMethod]
        public void CreateTest_With41_Records()
        {
            var schemeParameterTemplateValues = new List<SchemeParameterTemplateValueDto>();
            foreach (var item in _uniqueReferences)
            {
                schemeParameterTemplateValues.Add(new SchemeParameterTemplateValueDto
                {
                    ParameterValue = 90,
                    ParameterUniqueReferenceId = item
                });
            }
            var createDefaultParameterDto = new CreateDefaultParameterSettingDto
            {
                ParameterYear = "2024-25",
                SchemeParameterTemplateValues = schemeParameterTemplateValues
            };
            var actionResult = _controller.Create(createDefaultParameterDto) as ObjectResult;
            Assert.AreEqual(actionResult.StatusCode, 201);

            Assert.AreEqual(_dbContext.DefaultParameterSettingDetail.Count(), 41);
            Assert.AreEqual(_dbContext.DefaultParameterSettings.Count(), 1);
            Assert.AreEqual(_dbContext.DefaultParameterTemplateMasterList.Count(), 41);
        }

        [TestMethod]
        public void CreateTest_With41_Records_When_Existing_Updates()
        {
            var schemeParameterTemplateValues = new List<SchemeParameterTemplateValueDto>();
            foreach (var item in _uniqueReferences)
            {
                schemeParameterTemplateValues.Add(new SchemeParameterTemplateValueDto
                {
                    ParameterValue = 90,
                    ParameterUniqueReferenceId = item
                });
            }
            var createDefaultParameterDto = new CreateDefaultParameterSettingDto
            {
                ParameterYear = "2024-25",
                SchemeParameterTemplateValues = schemeParameterTemplateValues
            };
            var actionResult1 = _controller.Create(createDefaultParameterDto) as ObjectResult;
            Assert.AreEqual(actionResult1.StatusCode, 201);

            var actionResult2 = _controller.Create(createDefaultParameterDto) as ObjectResult;
            Assert.AreEqual(actionResult2.StatusCode, 201);

            Assert.AreEqual(_dbContext.DefaultParameterSettingDetail.Count(), 82);
            Assert.AreEqual(_dbContext.DefaultParameterSettings.Count(), 2);
            Assert.AreEqual(_dbContext.DefaultParameterTemplateMasterList.Count(), 41);

            Assert.AreEqual(_dbContext.DefaultParameterSettingDetail.Count(x => x.DefaultParameterSettingMasterId == 2), 41);
            Assert.AreEqual(_dbContext.DefaultParameterSettings.Count(a => a.EffectiveTo == null), 1);
        }
    }
}
