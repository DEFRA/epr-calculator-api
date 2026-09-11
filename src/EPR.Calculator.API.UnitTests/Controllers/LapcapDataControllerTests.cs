using System.Security.Claims;
using System.Security.Principal;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.API.UnitTests.Controllers
{
    [TestClass]
    public class LapcapDataControllerTests : BaseControllerTest
    {
        public TestContext TestContext { get; set; }

        // GET API
        [TestMethod]
        public async Task Get_RequestOkResult_WithLapCapParametersDto_WhenDataExist()
        {
            var createDefaultParameterDto = CreateDto();
            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);

            var context = new DefaultHttpContext()
            {
                User = principal,
            };

            this.LapcapDataController.ControllerContext = new ControllerContext
            {
                HttpContext = context,
            };
            await this.LapcapDataController.Set(createDefaultParameterDto);

            var tempdateData = new LapCapParameterDto()
            {
                Id = 1,
                RelativeYear = new RelativeYear(2024),
                LapcapDataMasterId = 1,

                LapcapTempUniqueRef = "ENG-AL",
                CreatedBy = "Testuser",
                CreatedAt = DateTime.UtcNow,

                Country = "England",
                Material = "Aluminium",
                TotalCost = 20m,
                EffectiveFrom = DateTime.UtcNow,
            };

            // Act
            var actionResult1 = await this.LapcapDataController.Get(2024) as ObjectResult;

            // Assert
            var okResult = actionResult1 as ObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var actionResul2 = okResult.Value as List<LapCapParameterDto>;
            Assert.AreEqual(tempdateData.Id, actionResul2?[0].Id);
            Assert.AreEqual(tempdateData.TotalCost, actionResul2?[0].TotalCost);
            Assert.AreEqual(tempdateData.LapcapTempUniqueRef, actionResul2?[0].LapcapTempUniqueRef);
        }

        [TestMethod]
        public async Task Get_InvalidModelState_ReturnsBadRequest()
        {
            // Act
            var result = await this.LapcapDataController.Get(2024) as ObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(StatusCodes.Status404NotFound, result.StatusCode);
        }

        [TestMethod]
        public async Task Get_NoDataForYear_ReturnsNotFound()
        {
            // Act
            var result = await this.LapcapDataController.Get(2000) as ObjectResult;

            // Assert
            var okResult = result as ObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status404NotFound, okResult.StatusCode);
        }

        [TestMethod]
        public async Task CreateTest_With_Records()
        {
            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);

            var context = new DefaultHttpContext()
            {
                User = principal,
            };

            this.LapcapDataController.ControllerContext = new ControllerContext
            {
                HttpContext = context,
            };
            var createDefaultParameterDto = CreateDto();
            var result = await this.LapcapDataController.Set(createDefaultParameterDto);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, this.DbContext.LapcapDataMaster.Count());
        }

        private static SetLapcapDataRequest CreateDto(IReadOnlyCollection<string>? uniqueRefsToAvoid = null)
        {
            uniqueRefsToAvoid ??= new List<string>();

            return new SetLapcapDataRequest
            {
                RelativeYear = new RelativeYear(2024),
                Values = [
                    ..GetLapcapTemplateMasterData()
                        .Where(m => !uniqueRefsToAvoid.Contains(m.UniqueReference))
                        .Select(m => new SetLapcapDataRequest.LapcapValue
                        {
                            Country = m.Country,
                            Material = m.Material,
                            TotalCost = 20
                        })
                ],
                Filename = "SomeTestFileName",
            };
        }
    }
}
