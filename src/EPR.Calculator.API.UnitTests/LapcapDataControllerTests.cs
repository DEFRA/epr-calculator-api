using api.Tests.Controllers;
using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.Calculator.API.UnitTests
{
    [TestClass]
    public class LapcapDataControllerTests : BaseControllerTest
    {
        [TestMethod]
        public void CreateTest_With_Records()
        {
            var actionResult = DataPostCall();
            Assert.AreEqual(201, actionResult?.StatusCode);

            Assert.AreEqual(LapcapDataUniqueReferences.UniqueReferences.Length, dbContext?.LapcapDataDetail.Count());
            Assert.AreEqual(1, dbContext?.LapcapDataMaster.Count());
        }

        //GET API
        [TestMethod]
        public void Get_RequestOkResult_WithLapCapParametersDto_WhenDataExist()
        {
            DataPostCall();

            var tempdateData = new LapCapParameterDto()
            {
                Id = 1,
                Year = "2024-25",
                LapcapDataMasterId = 1,

                LapcapTempUniqueRef = "ENG-AL",
                CreatedBy = "Testuser",
                CreatedAt = DateTime.Now,

                Country = "England",
                Material = "Aluminium",
                TotalCost = 20m,
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
            DataPostCall();

            //Act
            var result = lapcapDataController?.Get("2028-25") as ObjectResult;
            //Assert
            var okResult = result as ObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(StatusCodes.Status404NotFound, okResult.StatusCode);
        }

        public ObjectResult? DataPostCall()
        {
            var lapcapDataTemplateValues = new List<LapcapDataTemplateValueDto>();
            foreach (var uniqueRef in LapcapDataUniqueReferences.UniqueReferences)
            {
                lapcapDataTemplateValues.Add(new LapcapDataTemplateValueDto { TotalCost = "20", UniqueReference = uniqueRef });
            }
            var createDefaultParameterDto = new CreateLapcapDataDto
            {
                ParameterYear = "2024-25",
                LapcapDataTemplateValues = lapcapDataTemplateValues
            };
            var actionResult = lapcapDataController?.Create(createDefaultParameterDto) as ObjectResult;
            return actionResult;
        }
    }
}