using System.Security.Claims;
using System.Security.Principal;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.UnitTests.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Azure.Amqp.Framing;

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

            DefaultParameterSettingController.ControllerContext = new ControllerContext
            {
                HttpContext = context,
            };
            var schemeParameterTemplateValues = new List<SchemeParameterTemplateValueDto>();
            var createDefaultParameterDto = new CreateDefaultParameterSettingDto
            {
                RelativeYear = new RelativeYear(2000),
                SchemeParameterTemplateValues = schemeParameterTemplateValues,
                ParameterFileName = "TestFileName",
            };

            DefaultParameterSettingController.ModelState.AddModelError("RelativeYear", CommonResources.RelativeYearRequired);
            DefaultParameterSettingController.ModelState.AddModelError("SchemeParameterTemplateValues", string.Format(CommonResources.LapcapDataTemplateValuesMissing, CommonResources.DefaultParameterUniqueReferences.Split(',').Length));
            var actionResult = await DefaultParameterSettingController.Create(createDefaultParameterDto) as ObjectResult;
            Assert.AreEqual(400, actionResult?.StatusCode);

            var modelErrors = actionResult?.Value as IEnumerable<ModelError>;
            Assert.IsNotNull(modelErrors);
            Assert.AreEqual(1, modelErrors.Count(x => x.ErrorMessage == CommonResources.RelativeYearRequired));
            Assert.AreEqual(1, modelErrors.Count(x => x.ErrorMessage == string.Format(CommonResources.LapcapDataTemplateValuesMissing, CommonResources.DefaultParameterUniqueReferences.Split(',').Length)));
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

            DefaultParameterSettingController.ControllerContext = new ControllerContext
            {
                HttpContext = context,
            };

            var schemeParameterTemplateValues = new List<SchemeParameterTemplateValueDto>();
            var createDefaultParameterDto = new CreateDefaultParameterSettingDto
            {
                RelativeYear = new RelativeYear(2000),
                SchemeParameterTemplateValues = schemeParameterTemplateValues,
                ParameterFileName = "TestFileName",
            };

            var actionResult = await DefaultParameterSettingController
                .Create(createDefaultParameterDto) as ObjectResult;
            Assert.AreEqual(400, actionResult?.StatusCode);

            var errors = actionResult?.Value as IEnumerable<CreateDefaultParameterSettingErrorDto>;
            Assert.IsNotNull(errors);
            Assert.AreEqual(CommonResources.DefaultParameterUniqueReferences.Split(',').Length, errors.Count());
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

            DefaultParameterSettingController.ControllerContext = new ControllerContext
            {
                HttpContext = context,
            };
            var schemeParameterTemplateValues = new List<SchemeParameterTemplateValueDto>();
            foreach (var uniqueRef in CommonResources.DefaultParameterUniqueReferences.Split(','))
            {
                schemeParameterTemplateValues.Add(new SchemeParameterTemplateValueDto
                {
                    ParameterValue = "0",
                    ParameterUniqueReferenceId = uniqueRef,
                });
            }

            schemeParameterTemplateValues.Single(x => x.ParameterUniqueReferenceId == "REDM-RF").ParameterValue = "1.200";
            schemeParameterTemplateValues.Single(x => x.ParameterUniqueReferenceId == "BADEBT-P").ParameterUniqueReferenceId = "Somehthing else";
            var createDefaultParameterDto = new CreateDefaultParameterSettingDto
            {
                RelativeYear = new RelativeYear(2000),
                SchemeParameterTemplateValues = schemeParameterTemplateValues,
                ParameterFileName = "TestFileName",
            };

            var actionResult = await DefaultParameterSettingController
                .Create(createDefaultParameterDto) as ObjectResult;
            Assert.AreEqual(400, actionResult?.StatusCode);

            var errors = actionResult?.Value as IEnumerable<CreateDefaultParameterSettingErrorDto>;
            Assert.IsNotNull(errors);

            foreach (var error in errors)
            {
                Console.WriteLine(error.Message);
            }

            CollectionAssert.AreEquivalent(
                new[]
                {
                    "The parameter BADEBT-P is missing. Add the parameter to the file.",
                    "The parameter Somehthing else is an unexpected parameter. Remove it from the file."
                },
                errors.Select(x => x.Message).ToList());
        }
    }
}
