using System.Security.Claims;
using System.Security.Principal;
using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.UnitTests.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.Calculator.API.UnitTests.Validator
{
    [TestClass]
    public class DefaultParameterSettingControllerValidationTest : BaseControllerTest
    {
        [TestMethod]
        public async Task InvalidTest_With_NoRecordsAsync()
        {
            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);

            var context = new DefaultHttpContext()
            {
                User = principal,
            };

            this.DefaultParameterSettingController.ControllerContext = new ControllerContext
            {
                HttpContext = context,
            };
            var schemeParameterTemplateValues = new List<SchemeParameterTemplateValueDto>();
            var createDefaultParameterDto = new CreateDefaultParameterSettingDto
            {
                ParameterYear = string.Empty,
                SchemeParameterTemplateValues = schemeParameterTemplateValues,
                ParameterFileName = "TestFileName",
            };

            this.DefaultParameterSettingController.ModelState.AddModelError("ParameterYear", ErrorMessages.YearRequired);
            this.DefaultParameterSettingController.ModelState.AddModelError("SchemeParameterTemplateValues", ErrorMessages.SchemeParameterTemplateValuesMissing);
            var actionResult = await this.DefaultParameterSettingController.Create(createDefaultParameterDto) as ObjectResult;
            Assert.AreEqual(400, actionResult?.StatusCode);

            var modelErrors = actionResult?.Value as IEnumerable<ModelError>;
            Assert.IsNotNull(modelErrors);
            Assert.IsTrue(modelErrors.Count(x => x.ErrorMessage == ErrorMessages.YearRequired) == 1);
            Assert.IsTrue(modelErrors.Count(x => x.ErrorMessage == ErrorMessages.SchemeParameterTemplateValuesMissing) == 1);
        }

        [TestMethod]
        public async Task InvalidTest_With_Invalid_DataAsync()
        {
            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);

            var context = new DefaultHttpContext()
            {
                User = principal,
            };

            this.DefaultParameterSettingController.ControllerContext = new ControllerContext
            {
                HttpContext = context,
            };

            var schemeParameterTemplateValues = new List<SchemeParameterTemplateValueDto>();
            var createDefaultParameterDto = new CreateDefaultParameterSettingDto
            {
                ParameterYear = string.Empty,
                SchemeParameterTemplateValues = schemeParameterTemplateValues,
                ParameterFileName = "TestFileName",
            };

            var actionResult = await this.DefaultParameterSettingController
                .Create(createDefaultParameterDto) as ObjectResult;
            Assert.AreEqual(400, actionResult?.StatusCode);

            var errors = actionResult?.Value as IEnumerable<CreateDefaultParameterSettingErrorDto>;
            Assert.IsNotNull(errors);
            Assert.IsTrue(errors.Count() == DefaultParameterUniqueReferences.UniqueReferences.Length);
        }

        [TestMethod]
        public async Task InvalidTest_With_Missing_DataAsync()
        {
            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);

            var context = new DefaultHttpContext()
            {
                User = principal,
            };

            this.DefaultParameterSettingController.ControllerContext = new ControllerContext
            {
                HttpContext = context,
            };
            var schemeParameterTemplateValues = new List<SchemeParameterTemplateValueDto>();
            foreach (var uniqueRef in DefaultParameterUniqueReferences.UniqueReferences)
            {
                schemeParameterTemplateValues.Add(new SchemeParameterTemplateValueDto
                {
                    ParameterValue = "0",
                    ParameterUniqueReferenceId = uniqueRef,
                });
            }

            schemeParameterTemplateValues.Single(x => x.ParameterUniqueReferenceId == "BADEBT-P").ParameterUniqueReferenceId = "Somehthing else";
            var createDefaultParameterDto = new CreateDefaultParameterSettingDto
            {
                ParameterYear = string.Empty,
                SchemeParameterTemplateValues = schemeParameterTemplateValues,
                ParameterFileName = "TestFileName",
            };

            var actionResult = await this.DefaultParameterSettingController
                .Create(createDefaultParameterDto) as ObjectResult;
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
