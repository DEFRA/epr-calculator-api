using api.Tests.Controllers;
using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Dtos;
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
            Assert.AreEqual(actionResult?.StatusCode, 201);

            Assert.AreEqual(LapcapDataUniqueReferences.UniqueReferences.Length, dbContext?.LapcapDataDetail.Count());
            Assert.AreEqual(1, dbContext?.LapcapDataMaster.Count());
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
