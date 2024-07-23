using api.Dtos;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace api.Tests.Controllers
{
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

        [Test]
        public void CreateTest_With41_Records()
        {
            var schemeParameterTemplateValues = new List<SchemeParameterTemplateValue>();
            foreach (var item in _uniqueReferences)
            {
                schemeParameterTemplateValues.Add(new SchemeParameterTemplateValue
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
            Assert.That(actionResult.StatusCode, Is.EqualTo(201));

            Assert.That(_dbContext.DefaultParameterSettingDetail.Count(), Is.EqualTo(41));
            Assert.That(_dbContext.DefaultParameterSettings.Count(), Is.EqualTo(1));
            Assert.That(_dbContext.DefaultParameterTemplateMasterList.Count(), Is.EqualTo(41));
        }

        [Test]
        public void CreateTest_With41_Records_When_Existing_Updates()
        {
            var schemeParameterTemplateValues = new List<SchemeParameterTemplateValue>();
            foreach (var item in _uniqueReferences)
            {
                schemeParameterTemplateValues.Add(new SchemeParameterTemplateValue
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
            Assert.That(actionResult1.StatusCode, Is.EqualTo(201));

            var actionResult2 = _controller.Create(createDefaultParameterDto) as ObjectResult;
            Assert.That(actionResult2.StatusCode, Is.EqualTo(201));

            Assert.That(_dbContext.DefaultParameterSettingDetail.Count(), Is.EqualTo(82));
            Assert.That(_dbContext.DefaultParameterSettings.Count(), Is.EqualTo(2));
            Assert.That(_dbContext.DefaultParameterTemplateMasterList.Count(), Is.EqualTo(41));

            Assert.That(_dbContext.DefaultParameterSettingDetail.Count(x => x.DefaultParameterSettingMasterId == 2), Is.EqualTo(41));
            Assert.That(_dbContext.DefaultParameterSettings.Count(a => a.EffectiveTo == null), Is.EqualTo(1));
        }
    }
}
