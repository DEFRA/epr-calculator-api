using System.Security.Claims;
using System.Security.Principal;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

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

            this.DefaultParameterSettingController = new DefaultParameterSettingController(this.DbContext);
        }

        [TestMethod]
        public async Task CreateTest_With_Records()
        {
            var actionResult = await this.DataPostCallAsync() as NoContentResult;
            Assert.AreEqual(204, actionResult?.StatusCode);

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
            var actionResult1 = await this.DataPostCallAsync() as NoContentResult;
            Assert.AreEqual(204, actionResult1?.StatusCode);

            var actionResult2 = await this.DataPostCallAsync() as NoContentResult;
            Assert.AreEqual(204, actionResult2?.StatusCode);

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
                RelativeYear = new RelativeYear(2024),
                EffectiveFrom = DateTime.UtcNow,

                EffectiveTo = null,
                CreatedBy = "Testuser",
                CreatedAt = DateTime.UtcNow,

                DefaultParameterSettingMasterId = 1,
                ParameterUniqueRef = "BADEBT-P",
                ParameterType = "Aluminium",
                ParameterCategory = "Communication costs",
                ParameterValue = "90",
            };

            // Act
            var actionResult1 = await this.DefaultParameterSettingController
                .Get(this.RelativeYear24_25.Value) as ObjectResult;

            // Assert
            var okResult = actionResult1 as ObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var actionResul2 = okResult.Value as List<DefaultSchemeParametersDto>;
            Assert.AreEqual(actionResul2?.Count, CommonResources.DefaultParameterUniqueReferences.Split(',').Length);

            Assert.AreEqual(1         , actionResul2?[0].Id);
            Assert.AreEqual("90.000"  , actionResul2?[0].ParameterValue);
            Assert.AreEqual("BADEBT-P", actionResul2?[0].ParameterUniqueRef);
        }

        [TestMethod]
        public async Task GetSchemeParameter_ReturnBadRequest_WithDefaultSchemeParametersDoesNotExist()
        {
            await this.DataPostCallAsync();

            // Return 400 error if the year does not exist
            // Act
            var result = await this.DefaultParameterSettingController.Get(2028) as ObjectResult;

            // Assert
            var okResult = result as ObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(400, okResult.StatusCode);
        }

        // Private Methods
        public async Task<IActionResult?> DataPostCallAsync()
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

            var parameters = new List<SetDefaultParametersRequest.ParameterValue>();
            foreach (var item in CommonResources.DefaultParameterUniqueReferences.Split(','))
            {
                if (item == "MATT-AD" || item == "MATT-PD" || item == "TONT-AD" || item == "TONT-PD")
                {
                    parameters.Add(new ()
                    {
                        Value = "-90",
                        Id = item,
                    });
                }
                else if (item == "REDM-RF")
                {
                    parameters.Add(new ()
                    {
                        Value = "1.200",
                        Id = item,
                    });
                }
                else if (item == "COFF-DT")
                {
                    parameters.Add(new ()
                    {
                        Value = "30/12/2026",
                        Id = item,
                    });
                }
                else
                {
                    parameters.Add(new ()
                    {
                        Value = "90",
                        Id = item,
                    });
                }
            }

            var createDefaultParameterDto = new SetDefaultParametersRequest
            {
                RelativeYear = new RelativeYear(2024),
                Parameters = [..parameters],
                Filename = "TestFileName",
            };

            return await this.DefaultParameterSettingController.Set(createDefaultParameterDto);
        }
    }
}
