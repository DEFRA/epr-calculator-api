using EPR.Calculator.API.Tests.Controllers;
using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Security.Principal;

namespace EPR.Calculator.API.UnitTests
{
    [TestClass]
    public class DefaultParameterSettingControllerValidationTest : BaseControllerTest
    {
        [TestMethod]
        public void InvalidTest_With_NoRecords()
        {
            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);

            var context = new DefaultHttpContext()
            {
                User = principal
            };

            defaultParameterSettingController.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };
            var schemeParameterTemplateValues = new List<SchemeParameterTemplateValueDto>();
            var createDefaultParameterDto = new CreateDefaultParameterSettingDto
            {
                ParameterYear = "",
                SchemeParameterTemplateValues = schemeParameterTemplateValues,
                ParameterFileName = "TestFileName"
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
            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);

            var context = new DefaultHttpContext()
            {
                User = principal
            };

            defaultParameterSettingController.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };

            var schemeParameterTemplateValues = new List<SchemeParameterTemplateValueDto>();
            var createDefaultParameterDto = new CreateDefaultParameterSettingDto
            {
                ParameterYear = "",
                SchemeParameterTemplateValues = schemeParameterTemplateValues,
                ParameterFileName = "TestFileName"
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
            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);

            var context = new DefaultHttpContext()
            {
                User = principal
            };

            defaultParameterSettingController.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };
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
                SchemeParameterTemplateValues = schemeParameterTemplateValues,
                ParameterFileName = "TestFileName"
            };

            var actionResult = defaultParameterSettingController?.Create(createDefaultParameterDto) as ObjectResult;
            Assert.AreEqual(400, actionResult?.StatusCode);

            var errors = actionResult?.Value as IEnumerable<CreateDefaultParameterSettingErrorDto>;
            Assert.IsNotNull(errors);
            Assert.IsTrue(errors.Count() == 1);
            var firstError = errors.First();
            Assert.AreEqual("BADEBT-P", firstError.ParameterUniqueRef);
            Assert.AreEqual("Bad debt provision", firstError.ParameterCategory);
            Assert.AreEqual("Percentage", firstError.ParameterType);
        }
    }
}
