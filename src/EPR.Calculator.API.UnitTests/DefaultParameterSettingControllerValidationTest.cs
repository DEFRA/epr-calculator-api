using api.Tests.Controllers;
using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.Calculator.API.UnitTests
{
    [TestClass]
    public class DefaultParameterSettingControllerValidationTest : BaseControllerTest
    {
        private static string[] _uniqueReferences = {"BADEBT-P", "COMC-AL", "COMC-FC", "COMC-GL",
                                                     "COMC-OT", "COMC-PC", "COMC-PL", "COMC-ST",
                                                     "COMC-WD", "LAPC-ENG","LAPC-NIR", "LAPC-SCT",
                                                    "LAPC-WLS", "LEVY-ENG", "LEVY-NIR", "LEVY-SCT", "LEVY-WLS",
                                                    "LRET-AL", "LRET-FC", "LRET-GL", "LRET-OT",
                                                    "LRET-PC", "LRET-PL", "LRET-ST", "LRET-WD", "MATT-AD",
                                                    "MATT-AI", "MATT-PD", "MATT-PI", "SAOC-ENG", "SAOC-NIR",
                                                    "SAOC-SCT", "SAOC-WLS", "SCSC-ENG","SCSC-NIR", "SCSC-SCT",
                                                    "SCSC-WLS", "TONT-AI", "TONT-AD", "TONT-PD","TONT-PI" };

        [TestMethod]
        public void InvalidTest_With_NoRecords()
        {
            var schemeParameterTemplateValues = new List<SchemeParameterTemplateValueDto>();
            var createDefaultParameterDto = new CreateDefaultParameterSettingDto
            {
                ParameterYear = "",
                SchemeParameterTemplateValues = schemeParameterTemplateValues
            };

            _controller.ModelState.AddModelError("ParameterYear", ErrorMessages.YearRequired);
            _controller.ModelState.AddModelError("SchemeParameterTemplateValues", ErrorMessages.SchemeParameterTemplateValuesMissing);
            var actionResult = _controller.Create(createDefaultParameterDto) as ObjectResult;
            Assert.AreEqual(actionResult.StatusCode, 400);

            var modelErrors = actionResult.Value as IEnumerable<ModelError>;
            Assert.IsNotNull(modelErrors);
            Assert.IsTrue(modelErrors.Count(x => x.ErrorMessage == ErrorMessages.YearRequired) == 1);
            Assert.IsTrue(modelErrors.Count(x => x.ErrorMessage == ErrorMessages.SchemeParameterTemplateValuesMissing) == 1);
        }

        [TestMethod]
        public void InvalidTest_With_Invalid_Data()
        {
            var schemeParameterTemplateValues = new List<SchemeParameterTemplateValueDto>();
            var createDefaultParameterDto = new CreateDefaultParameterSettingDto
            {
                ParameterYear = "",
                SchemeParameterTemplateValues = schemeParameterTemplateValues
            };

            var actionResult = _controller.Create(createDefaultParameterDto) as ObjectResult;
            Assert.AreEqual(actionResult.StatusCode, 400);

            var errors = actionResult.Value as IEnumerable<CreateDefaultParameterSettingErrorDto>;
            Assert.IsNotNull(errors);
            Assert.IsTrue(errors.Count() == CommonConstants.TemplateCount);
        }

        [TestMethod]
        public void InvalidTest_With_Missing_Data()
        {
            var schemeParameterTemplateValues = new List<SchemeParameterTemplateValueDto>();
            foreach (var uniqueRef in _uniqueReferences)
            {
                schemeParameterTemplateValues.Add(new SchemeParameterTemplateValueDto
                {
                    ParameterValue = 0,
                    ParameterUniqueReferenceId = uniqueRef
                });
            }
            schemeParameterTemplateValues.Single(x => x.ParameterUniqueReferenceId == "BADEBT-P").ParameterUniqueReferenceId = "Somehthing else";
            var createDefaultParameterDto = new CreateDefaultParameterSettingDto
            {
                ParameterYear = "",
                SchemeParameterTemplateValues = schemeParameterTemplateValues
            };

            var actionResult = _controller.Create(createDefaultParameterDto) as ObjectResult;
            Assert.AreEqual(actionResult.StatusCode, 400);

            var errors = actionResult.Value as IEnumerable<CreateDefaultParameterSettingErrorDto>;
            Assert.IsNotNull(errors);
            Assert.IsTrue(errors.Count() == 1);
            Assert.AreEqual(errors.First().ParameterUniqueRef, "BADEBT-P");
            Assert.AreEqual(errors.First().ParameterCategory, "Communication costs");
            Assert.AreEqual(errors.First().ParameterType, "Aluminium");
        }
    }
}
