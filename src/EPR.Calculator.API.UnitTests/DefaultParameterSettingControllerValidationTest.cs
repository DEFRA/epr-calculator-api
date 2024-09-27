using EPR.Calculator.API.Tests.Controllers;
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
        [TestMethod]
        public void InvalidTest_With_NoRecords()
        {
            var schemeParameterTemplateValues = new List<SchemeParameterTemplateValueDto>();
            var createDefaultParameterDto = new CreateDefaultParameterSettingDto
            {
                ParameterYear = "",
                SchemeParameterTemplateValues = schemeParameterTemplateValues
            };

            defaultParameterSettingController?.ModelState.AddModelError("ParameterYear", ErrorMessages.YearRequired);
            defaultParameterSettingController?.ModelState.AddModelError("SchemeParameterTemplateValues", ErrorMessages.SchemeParameterTemplateValuesMissing);
            var actionResult = defaultParameterSettingController?.Create(createDefaultParameterDto) as ObjectResult;
            Assert.AreEqual(400, actionResult?.StatusCode);

            var modelErrors = actionResult?.Value as IEnumerable<ModelError>;
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

            var actionResult = defaultParameterSettingController?.Create(createDefaultParameterDto) as ObjectResult;
            Assert.AreEqual(400, actionResult?.StatusCode);

            var errors = actionResult?.Value as IEnumerable<CreateDefaultParameterSettingErrorDto>;
            Assert.IsNotNull(errors);
            Assert.IsTrue(errors.Count() == DefaultParameterUniqueReferences.UniqueReferences.Length);
        }

        [TestMethod]
        public void InvalidTest_With_Missing_Data()
        {
            var schemeParameterTemplateValues = new List<SchemeParameterTemplateValueDto>();
            foreach (var uniqueRef in DefaultParameterUniqueReferences.UniqueReferences)
            {
                schemeParameterTemplateValues.Add(new SchemeParameterTemplateValueDto
                {
                    ParameterValue = "0",
                    ParameterUniqueReferenceId = uniqueRef
                });
            }
            schemeParameterTemplateValues.Single(x => x.ParameterUniqueReferenceId == "BADEBT-P").ParameterUniqueReferenceId = "Somehthing else";
            var createDefaultParameterDto = new CreateDefaultParameterSettingDto
            {
                ParameterYear = "",
                SchemeParameterTemplateValues = schemeParameterTemplateValues
            };

            var actionResult = defaultParameterSettingController?.Create(createDefaultParameterDto) as ObjectResult;
            Assert.AreEqual(400, actionResult?.StatusCode);

            var errors = actionResult?.Value as IEnumerable<CreateDefaultParameterSettingErrorDto>;
            Assert.IsNotNull(errors);
            Assert.IsTrue(errors.Count() == 1);
            var firstError = errors.First();
            Assert.AreEqual(firstError.ParameterUniqueRef, "BADEBT-P");
            Assert.AreEqual(firstError.ParameterCategory, "Bad debt provision");
            Assert.AreEqual(firstError.ParameterType, "Percentage");
        }
    }
}
