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
    }
}
