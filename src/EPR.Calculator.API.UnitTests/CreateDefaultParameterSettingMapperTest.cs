using api.Mappers;
using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.UnitTests.Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace api.Tests.Controllers
{
    [TestClass]
    public class CreateDefaultParameterSettingMapperTest : BaseControllerTest
    {
        [TestMethod]
        public void Check_TheResult_IsNotNullOf_ResultSet_WithDefaultSchemeParametersDto_WithCorrectYear()
        {

            DataPostCall();
            var currentDefaultSetting = _dbContext.DefaultParameterSettings.SingleOrDefault(x => x.EffectiveTo == null && x.ParameterYear == "2024-25");
            var _templateDetails = _dbContext.DefaultParameterTemplateMasterList;

            var schemeParameters = CreateDefaultParameterSettingMapper.Map(currentDefaultSetting, _templateDetails);

            //var schemeParametersDto = schemeParameters.Select(
            Assert.IsNotNull(currentDefaultSetting);
            Assert.IsNotNull(currentDefaultSetting.Details);
            Assert.IsNotNull(_templateDetails);
            Assert.IsNotNull(schemeParameters);
            Assert.AreEqual(CommonConstants.TemplateCount, schemeParameters.Count);
        }

        // Private Methods
        public ObjectResult DataPostCall()
        {
            var schemeParameterTemplateValues = new List<SchemeParameterTemplateValueDto>();
            foreach (var item in CreateDefaultParameterMoqData._uniqueReferences)
            {
                if (item == "MATT-PD" || item == "TONT-PD")
                {
                    schemeParameterTemplateValues.Add(new SchemeParameterTemplateValueDto
                    {
                        ParameterValue = 0,
                        ParameterUniqueReferenceId = item
                    });
                }
                else
                {

                    schemeParameterTemplateValues.Add(new SchemeParameterTemplateValueDto
                    {
                        ParameterValue = 90,
                        ParameterUniqueReferenceId = item
                    });

                }
            }
            var createDefaultParameterDto = new CreateDefaultParameterSettingDto
            {
                ParameterYear = "2024-25",
                SchemeParameterTemplateValues = schemeParameterTemplateValues
            };
            var actionResult = _controller.Create(createDefaultParameterDto) as ObjectResult;
            return actionResult;
        }
    }
}