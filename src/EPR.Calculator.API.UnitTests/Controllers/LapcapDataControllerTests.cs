using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Claims;
using System.Security.Principal;

namespace EPR.Calculator.API.UnitTests.Controllers
{
    [TestClass]
    public class LapcapDataControllerTests : BaseControllerTest
    {
        //GET API
        [TestMethod]
        public async Task Get_RequestOkResult_WithLapCapParametersDto_WhenDataExist()
        {
            var createDefaultParameterDto = CreateDto();
            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);

            var context = new DefaultHttpContext()
            {
                User = principal
            };

            LapcapDataController.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };
            await LapcapDataController.Create(createDefaultParameterDto);

            var tempdateData = new LapCapParameterDto()
            {
                Id = 1,
                ProjectionYear = "2024-25",
                LapcapDataMasterId = 1,

                LapcapTempUniqueRef = "ENG-AL",
                CreatedBy = "Testuser",
                CreatedAt = DateTime.Now,

                Country = "England",
                Material = "Aluminium",
                TotalCost = 20m,
                EffectiveFrom = DateTime.Now,
            };

            //Act
            var actionResult1 = await LapcapDataController.Get("2024-25") as ObjectResult;

            //Assert
            var okResult = actionResult1 as ObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var actionResul2 = okResult.Value as List<LapCapParameterDto>;
            Assert.AreEqual(actionResul2?.Count, LapcapDataUniqueReferences.UniqueReferences.Length);

            Assert.AreEqual(tempdateData.Id, actionResul2?[0].Id);
            Assert.AreEqual(tempdateData.TotalCost, actionResul2?[0].TotalCost);
            Assert.AreEqual(tempdateData.LapcapTempUniqueRef, actionResul2?[0].LapcapTempUniqueRef);
        }

        [TestMethod]
        public async Task Get_InvalidModelState_ReturnsBadRequest()
        {
            LapcapDataController.ModelState.AddModelError("parameterYear", "Invalid year");
            //Act
            var result = await LapcapDataController.Get("2024") as ObjectResult;
            //Assert
            var okResult = result as ObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status400BadRequest, okResult.StatusCode);
        }

        [TestMethod]
        public async Task Get_NoDataForYear_ReturnsNotFound()
        {
            //Act
            var result = await LapcapDataController.Get("2028-25") as ObjectResult;
            //Assert
            var okResult = result as ObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status404NotFound, okResult.StatusCode);
        }

        [TestMethod]
        public void CreateTest_With_Records()
        {
            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);

            var context = new DefaultHttpContext()
            {
                User = principal
            };

            LapcapDataController.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };
            var createDefaultParameterDto = CreateDto();
            var task = LapcapDataController.Create(createDefaultParameterDto);
            task.Wait();
            var actionResult = task.Result as ObjectResult;
            Assert.AreEqual(201, actionResult?.StatusCode);

            Assert.AreEqual(LapcapDataUniqueReferences.UniqueReferences.Length, DbContext?.LapcapDataDetail.Count());
            Assert.AreEqual(1, DbContext?.LapcapDataMaster.Count());
        }

        [TestMethod]
        public void CreateTest_With_Missing_Year()
        {
            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);

            var context = new DefaultHttpContext()
            {
                User = principal
            };

            LapcapDataController.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };
            var createDefaultParameterDto = CreateDto();
            createDefaultParameterDto.ParameterYear = string.Empty;
            LapcapDataController.ModelState.AddModelError("ParameterYear", ErrorMessages.YearRequired);
            var task = LapcapDataController.Create(createDefaultParameterDto);
            task.Wait();
            var actionResult = task.Result as ObjectResult;
            Assert.AreEqual(400, actionResult?.StatusCode);
        }

        [TestMethod]
        public void CreateTest_With_Missing_Records()
        {
            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);

            var context = new DefaultHttpContext()
            {
                User = principal
            };

            LapcapDataController.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };
            var uniqueRef = "ENG-WD";
            var createDefaultParameterDto = CreateDto([uniqueRef]);
            var task = LapcapDataController.Create(createDefaultParameterDto);
            task.Wait();
            var actionResult = task.Result as ObjectResult;
            Assert.AreEqual(400, actionResult?.StatusCode);
            var errors = actionResult?.Value as IEnumerable<CreateLapcapDataErrorDto>;
            Assert.IsNotNull(errors);
            Assert.AreEqual(uniqueRef, errors.First().UniqueReference);
            Assert.AreEqual("Enter the total costs for Wood in England", errors.First().Message);
        }

        [TestMethod]
        public void CreateTest_With_More_Records()
        {
            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);

            var context = new DefaultHttpContext()
            {
                User = principal
            };

            LapcapDataController.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };
            var createDefaultParameterDto = CreateDto();
            var list = new List<LapcapDataTemplateValueDto>(createDefaultParameterDto.LapcapDataTemplateValues);
            if (list != null)
            {
                list.Add(new LapcapDataTemplateValueDto { CountryName = "England", Material = "Wood", TotalCost = "9" });
                createDefaultParameterDto.LapcapDataTemplateValues = list.AsEnumerable();
            }
            var task = LapcapDataController.Create(createDefaultParameterDto);
            task.Wait();
            var actionResult = task.Result as ObjectResult;
            Assert.AreEqual(400, actionResult?.StatusCode);
            var errors = actionResult?.Value as IEnumerable<CreateLapcapDataErrorDto>;
            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Count(x => x.Message == "You have entered the total costs for Wood in England more than once"));
        }

        public static CreateLapcapDataDto CreateDto(IEnumerable<string>? uniqueRefsToAvoid = null)
        {
            var lapcapDataTemplateValues = new List<LapcapDataTemplateValueDto>();
            var masterData = GetLapcapTemplateMasterData();
            foreach (var templateMaster in masterData)
            {
                if (uniqueRefsToAvoid == null || !uniqueRefsToAvoid.Contains(templateMaster.UniqueReference))
                {
                    lapcapDataTemplateValues.Add(new LapcapDataTemplateValueDto
                    {
                        TotalCost = "20",
                        CountryName = templateMaster.Country,
                        Material = templateMaster.Material,
                    });
                }
            }
            var createDefaultParameterDto = new CreateLapcapDataDto
            {
                ParameterYear = "2024-25",
                LapcapDataTemplateValues = lapcapDataTemplateValues,
                LapcapFileName = "SomeTestFileName"

            };
            return createDefaultParameterDto;
        }
    }
}