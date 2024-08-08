using EPR.Calculator.API.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EPR.Calculator.API.UnitTests.Moq;
using EPR.Calculator.API.Validators;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Http;
using EPR.Calculator.API.Constants;

namespace api.Tests.Controllers
{
    [TestClass]
    public class DefaultParameterSettingControllerTest : BaseControllerTest
    {
        [TestMethod]
        public void CreateTest_With_Records()
        {
            var actionResult = DataPostCall();
            Assert.AreEqual(actionResult.StatusCode, 201);

            Assert.AreEqual(_dbContext.DefaultParameterSettingDetail.Count(), CommonConstants.TemplateCount);
            Assert.AreEqual(_dbContext.DefaultParameterSettings.Count(), 1);
            Assert.AreEqual(_dbContext.DefaultParameterTemplateMasterList.Count(), CommonConstants.TemplateCount);
        }

        [TestMethod]
        public void CreateTest_With_Records_When_Existing_Updates()
        {
            var actionResult1 = DataPostCall();
            Assert.AreEqual(actionResult1.StatusCode, 201);

            var actionResult2 = DataPostCall();
            Assert.AreEqual(actionResult2.StatusCode, 201);

            Assert.AreEqual(_dbContext.DefaultParameterSettingDetail.Count(), CommonConstants.TemplateCount*2);
            Assert.AreEqual(_dbContext.DefaultParameterSettings.Count(), 2);
            Assert.AreEqual(_dbContext.DefaultParameterTemplateMasterList.Count(), CommonConstants.TemplateCount);

            Assert.AreEqual(_dbContext.DefaultParameterSettingDetail.Count(x => x.DefaultParameterSettingMasterId == 2), CommonConstants.TemplateCount);
            Assert.AreEqual(_dbContext.DefaultParameterSettings.Count(a => a.EffectiveTo == null), 1);
        }
        //GET API
        [TestMethod]
        public void Get_RequestOkResult_WithDefaultSchemeParametersDto_WhenDataExist()
        {
            DataPostCall();

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

            //Act
            var actionResult1 = _controller.Get("2024-25") as ObjectResult;

            //Assert
            var okResult = actionResult1 as ObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(okResult.StatusCode, 200);

            var actionResul2 = okResult.Value as List<DefaultSchemeParametersDto>;
            Assert.AreEqual(actionResul2.Count, CommonConstants.TemplateCount);

            Assert.AreEqual(tempdateData.Id, actionResul2[0].Id);
            Assert.AreEqual(tempdateData.ParameterValue, actionResul2[0].ParameterValue);
            Assert.AreEqual(tempdateData.ParameterUniqueRef, actionResul2[0].ParameterUniqueRef);
        }

        [TestMethod]
        public void GetSchemeParameter_ReturnNotFound_WithDefaultSchemeParametersDoesNotExist()
        {
            DataPostCall();

            // Return 404 error if the year does not exist
            //Act
            var result = _controller.Get("2028-25");
            //Assert
            var okResult = result as ObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(okResult.StatusCode, 404);

        }

        [TestMethod]
        public void GetSchemeParameter_Return_400_Error_WithN_No_YearSupplied()
        {
            ParameterYearValueValidationValidator _validator = new ParameterYearValueValidationValidator();
            string _parameter = string.Empty;
            var result = _validator.Validate(_parameter);

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Errors.First().ErrorMessage, "Parameter Year is required");
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