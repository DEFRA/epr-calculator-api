using System.Security.Claims;
using System.Security.Principal;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.Calculator.API.UnitTests.Controllers
{
    [TestClass]
    public class DefaultParameterSettingControllerTests : BaseControllerTest
    {
        public DefaultParameterSettingControllerTests()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "PayCal")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            this.DbContext = new ApplicationDBContext(dbContextOptions);
            this.DbContext.Database.EnsureCreated();

            var validator = new CreateDefaultParameterDataValidator(this.DbContext);
            this.DefaultParameterSettingController = new DefaultParameterSettingController(this.DbContext, validator, TelemetryClient);
        }

        [TestMethod]
        public async Task CreateTest_With_Records()
        {
            var actionResult = await this.DataPostCallAsync();
            Assert.AreEqual(201, actionResult?.StatusCode);

            Assert.AreEqual(
                CommonResources.DefaultParameterUniqueReferences.Split(',').Length,
                this.DbContext.DefaultParameterSettingDetail.Count());
            Assert.AreEqual(1, this.DbContext.DefaultParameterSettings.Count());
            Assert.AreEqual(
                CommonResources.DefaultParameterUniqueReferences.Split(',').Length,
                this.DbContext.DefaultParameterTemplateMasterList.Count());
        }

        [TestMethod]
        public async Task CreateTest_With_Records_When_Existing_Updates()
        {
            var actionResult1 = await this.DataPostCallAsync();
            Assert.AreEqual(201, actionResult1?.StatusCode);

            var actionResult2 = await this.DataPostCallAsync();
            Assert.AreEqual(201, actionResult2?.StatusCode);

            var expectedLength = CommonResources.DefaultParameterUniqueReferences.Split(',').Length * 2;
            Assert.AreEqual(expectedLength, this.DbContext.DefaultParameterSettingDetail.Count());
            Assert.AreEqual(2, this.DbContext.DefaultParameterSettings.Count());
            Assert.AreEqual(
                CommonResources.DefaultParameterUniqueReferences.Split(',').Length,
                this.DbContext.DefaultParameterTemplateMasterList.Count());

            Assert.AreEqual(
                CommonResources.DefaultParameterUniqueReferences.Split(',').Length,
                this.DbContext.DefaultParameterSettingDetail.Count(x => x.DefaultParameterSettingMasterId == 2));
            Assert.AreEqual(1, this.DbContext.DefaultParameterSettings.Count(a => a.EffectiveTo == null));
        }

        // GET API
        [TestMethod]
        public async Task Get_RequestOkResult_WithDefaultSchemeParametersDto_WhenDataExist()
        {
            await this.DataPostCallAsync();

            var tempdateData = new DefaultSchemeParametersDto()
            {
                Id = 1,
                ParameterYear = "2024-25",
                EffectiveFrom = DateTime.Now,

                EffectiveTo = null,
                CreatedBy = "Testuser",
                CreatedAt = DateTime.Now,

                DefaultParameterSettingMasterId = 1,
                ParameterUniqueRef = "BADEBT-P",
                ParameterType = "Aluminium",
                ParameterCategory = "Communication costs",
                ParameterValue = 90m,
            };

            // Act
            var actionResult1 = await this.DefaultParameterSettingController
                .Get(this.FinancialYear24_25.Name) as ObjectResult;

            // Assert
            var okResult = actionResult1 as ObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var actionResul2 = okResult.Value as List<DefaultSchemeParametersDto>;
            Assert.AreEqual(actionResul2?.Count, CommonResources.DefaultParameterUniqueReferences.Split(',').Length);

            Assert.AreEqual(tempdateData.Id, actionResul2?[0].Id);
            Assert.AreEqual(tempdateData.ParameterValue, actionResul2?[0].ParameterValue);
            Assert.AreEqual(tempdateData.ParameterUniqueRef, actionResul2?[0].ParameterUniqueRef);
        }

        [TestMethod]
        public async Task GetSchemeParameter_ReturnBadRequest_WithDefaultSchemeParametersDoesNotExist()
        {
            await this.DataPostCallAsync();

            // Return 400 error if the year does not exist
            // Act
            var result = await this.DefaultParameterSettingController.Get("2028-25") as ObjectResult;

            // Assert
            var okResult = result as ObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(400, okResult.StatusCode);
        }

        [TestMethod]
        public void GetSchemeParameter_Return_400_Error_With_No_YearSupplied()
        {
            ParameterYearValueValidationValidator validator = new ParameterYearValueValidationValidator();
            string parameter = string.Empty;
            var result = validator.Validate(parameter);

            Assert.IsNotNull(result);
            Assert.AreEqual("Parameter Year is required", result.Errors.First().ErrorMessage);
        }

        [TestMethod]
        public void Create_Default_Parameter_Setting_With_No_FileName()
        {
            var schemeParameterTemplateValues = new List<SchemeParameterTemplateValueDto>();
            foreach (var item in CommonResources.DefaultParameterUniqueReferences.Split(','))
            {
                if (item == "MATT-AD" || item == "MATT-PD" || item == "TONT-AD" || item == "TONT-PD")
                {
                    schemeParameterTemplateValues.Add(new SchemeParameterTemplateValueDto
                    {
                        ParameterValue = "-90",
                        ParameterUniqueReferenceId = item,
                    });
                }
                else
                {
                    schemeParameterTemplateValues.Add(new SchemeParameterTemplateValueDto
                    {
                        ParameterValue = "90",
                        ParameterUniqueReferenceId = item,
                    });
                }
            }

            CreateDefaultParameterSettingValidator validator = new CreateDefaultParameterSettingValidator();
            CreateDefaultParameterSettingDto parameter = new CreateDefaultParameterSettingDto()
            {
                ParameterFileName = string.Empty,
                ParameterYear = "2024-25",
                SchemeParameterTemplateValues = schemeParameterTemplateValues,
            };
            var result = validator.Validate(parameter);

            Assert.IsNotNull(result);
            Assert.AreEqual("FileName is required", result.Errors.First().ErrorMessage);
        }

        [TestMethod]
        public void Create_Default_Parameter_Setting_With_Max_FileName_Length()
        {
            var schemeParameterTemplateValues = new List<SchemeParameterTemplateValueDto>();
            foreach (var item in CommonResources.DefaultParameterUniqueReferences.Split(','))
            {
                if (item == "MATT-AD" || item == "MATT-PD" || item == "TONT-AD" || item == "TONT-PD")
                {
                    schemeParameterTemplateValues.Add(new SchemeParameterTemplateValueDto
                    {
                        ParameterValue = "-90",
                        ParameterUniqueReferenceId = item,
                    });
                }
                else
                {
                    schemeParameterTemplateValues.Add(new SchemeParameterTemplateValueDto
                    {
                        ParameterValue = "90",
                        ParameterUniqueReferenceId = item,
                    });
                }
            }

            CreateDefaultParameterSettingValidator validator = new CreateDefaultParameterSettingValidator();
            CreateDefaultParameterSettingDto parameter = new CreateDefaultParameterSettingDto()
            {
                ParameterFileName = new string('A', 257),
                ParameterYear = "2024-25",
                SchemeParameterTemplateValues = schemeParameterTemplateValues,
            };
            var result = validator.Validate(parameter);

            Assert.IsNotNull(result);
            Assert.AreEqual(CommonResources.MaxFileNameLength, result.Errors.First().ErrorMessage);
        }

        // Private Methods
        public async Task<ObjectResult?> DataPostCallAsync()
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
            foreach (var item in CommonResources.DefaultParameterUniqueReferences.Split(','))
            {
                if (item == "MATT-AD" || item == "MATT-PD" || item == "TONT-AD" || item == "TONT-PD")
                {
                    schemeParameterTemplateValues.Add(new SchemeParameterTemplateValueDto
                    {
                        ParameterValue = "-90",
                        ParameterUniqueReferenceId = item,
                    });
                }
                else
                {
                    schemeParameterTemplateValues.Add(new SchemeParameterTemplateValueDto
                    {
                        ParameterValue = "90",
                        ParameterUniqueReferenceId = item,
                    });
                }
            }

            var createDefaultParameterDto = new CreateDefaultParameterSettingDto
            {
                ParameterYear = "2024-25",
                SchemeParameterTemplateValues = schemeParameterTemplateValues,
                ParameterFileName = "TestFileName",
            };
            var actionResult = await this.DefaultParameterSettingController.Create(createDefaultParameterDto);
            return actionResult as ObjectResult;
        }
    }
}