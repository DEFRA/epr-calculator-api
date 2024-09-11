using api.Tests.Controllers;
using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.Calculator.API.UnitTests
{
    [TestClass]
    public class LAPCAPParameterSettingMapperTests : BaseControllerTest
    {

        [TestMethod]
        public void Check_TheResult_IsNotNullOf_ResultSet_WithDefaultLAPCAPParametersDto_WithCorrectYear()
        {
            DataPostCall();
            var currentDefaultSetting = dbContext?.LapcapDataMaster.SingleOrDefault(x => x.EffectiveTo == null && x.Year == "2024-25");
            var _templateDetails = dbContext?.LapcapDataTemplateMaster;

            // Check if currentDefaultSetting is null before proceeding
            if (currentDefaultSetting == null || _templateDetails == null)
            {
                throw new InvalidOperationException("No default setting found for the specified year.");
            }

            var lapcapParameters = LAPCAPParameterSettingMapper.Map(currentDefaultSetting, _templateDetails);

            // Perform assertions
            Assert.IsNotNull(currentDefaultSetting);
            Assert.IsNotNull(currentDefaultSetting.Details);
            Assert.IsNotNull(_templateDetails);
            Assert.IsNotNull(lapcapParameters);
            Assert.AreEqual(LapcapDataUniqueReferences.UniqueReferences.Length, lapcapParameters.Count);
        }

        // Private Methods
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