using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Tests.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.Calculator.API.UnitTests
{
    [TestClass]
    public class LapcapDataControllerTests : BaseControllerTest
    {
        //GET API
        [TestMethod]
        public void Get_RequestOkResult_WithLapCapParametersDto_WhenDataExist()
        {
            var createDefaultParameterDto = CreateDto();
            lapcapDataController?.Create(createDefaultParameterDto);

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
            var actionResult1 = lapcapDataController?.Get("2024-25") as ObjectResult;

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
        public void Get_InvalidModelState_ReturnsBadRequest()
        {
            lapcapDataController?.ModelState.AddModelError("parameterYear", "Invalid year");
            //Act
            var result = lapcapDataController?.Get("2024") as ObjectResult;
            //Assert
            var okResult = result as ObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status400BadRequest, okResult.StatusCode);
        }

        [TestMethod]
        public void Get_NoDataForYear_ReturnsNotFound()
        {
            //Act
            var result = lapcapDataController?.Get("2028-25") as ObjectResult;
            //Assert
            var okResult = result as ObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status404NotFound, okResult.StatusCode);
        }
        [TestMethod]
        public void CreateTest_With_Records()
        {
            var createDefaultParameterDto = CreateDto();
            var actionResult = lapcapDataController?.Create(createDefaultParameterDto) as ObjectResult;
            Assert.AreEqual(201, actionResult?.StatusCode);

            Assert.AreEqual(LapcapDataUniqueReferences.UniqueReferences.Length, dbContext?.LapcapDataDetail.Count());
            Assert.AreEqual(1, dbContext?.LapcapDataMaster.Count());
        }

        [TestMethod]
        public void CreateTest_With_Missing_Year()
        {
            var createDefaultParameterDto = CreateDto();
            createDefaultParameterDto.ParameterYear = string.Empty;
            lapcapDataController?.ModelState.AddModelError("ParameterYear", ErrorMessages.YearRequired);
            var actionResult = lapcapDataController?.Create(createDefaultParameterDto) as ObjectResult;
            Assert.AreEqual(400, actionResult?.StatusCode);
        }

        [TestMethod]
        public void CreateTest_With_Missing_Records()
        {
            var uniqueRef = "ENG-WD";
            var createDefaultParameterDto = CreateDto([uniqueRef]);
            createDefaultParameterDto.LapcapDataTemplateValues = createDefaultParameterDto.LapcapDataTemplateValues;
            var actionResult = lapcapDataController?.Create(createDefaultParameterDto) as ObjectResult;
            Assert.AreEqual(400, actionResult?.StatusCode);
            var errors = actionResult?.Value as IEnumerable<CreateLapcapDataErrorDto>;
            Assert.IsNotNull(errors);
            Assert.AreEqual(uniqueRef, errors.First().UniqueReference);
            Assert.AreEqual("Enter the total costs for Wood in England", errors.First().Message);
        }

        [TestMethod]
        public void CreateTest_With_More_Records()
        {
            var createDefaultParameterDto = CreateDto();
            var list = new List<LapcapDataTemplateValueDto>(createDefaultParameterDto.LapcapDataTemplateValues);
            if (list != null)
            {
                list.Add(new LapcapDataTemplateValueDto { CountryName = "England", Material = "Wood", TotalCost = "9" });
                createDefaultParameterDto.LapcapDataTemplateValues = list.AsEnumerable();
            }

            var actionResult = lapcapDataController?.Create(createDefaultParameterDto) as ObjectResult;
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
                LapcapFileName  = "Some Filename"

            };
            return createDefaultParameterDto;
        }
    }
}