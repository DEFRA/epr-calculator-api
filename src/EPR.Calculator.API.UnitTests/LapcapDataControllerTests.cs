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
            Assert.AreEqual("Enter the lapcap data for England and Wood", errors.First().Message);
        }

        [TestMethod]
        public void CreateTest_With_More_Records()
        {
            var createDefaultParameterDto = CreateDto();
            var list = new List<LapcapDataTemplateValueDto>(createDefaultParameterDto.LapcapDataTemplateValues);
            list?.Add(new LapcapDataTemplateValueDto { CountryName = "England", Material = "Wood", TotalCost = "9" });
            createDefaultParameterDto.LapcapDataTemplateValues = list.AsEnumerable();
            var actionResult = lapcapDataController?.Create(createDefaultParameterDto) as ObjectResult;
            Assert.AreEqual(400, actionResult?.StatusCode);
            var errors = actionResult?.Value as IEnumerable<CreateLapcapDataErrorDto>;
            Assert.IsNotNull(errors);
            Assert.AreEqual(1, errors.Count(x => x.Message == "Expecting only One with England and Wood"));
        }

        public CreateLapcapDataDto CreateDto(IEnumerable<string>? uniqueRefsToAvoid = null)
        {
            var lapcapDataTemplateValues = new List<LapcapDataTemplateValueDto>();
            var masterData = GetTemplateMasterData();
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
                LapcapDataTemplateValues = lapcapDataTemplateValues
            };
            return createDefaultParameterDto;
        }
    }
}
