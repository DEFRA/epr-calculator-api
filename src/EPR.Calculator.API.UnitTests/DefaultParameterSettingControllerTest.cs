using api.Dtos;
using api.Mappers;
using EPR.Calculator.API.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace api.Tests.Controllers
{
    [TestClass]
    public class DefaultParameterSettingControllerTest : BaseControllerTest
    {
        private static string[] _uniqueReferences = {"BADEBT-P", "COMC-AL", "COMC-FC", "COMC-GL",
                                                     "COMC-OT", "COMC-PC", "COMC-PL", "COMC-ST",
                                                     "COMC-WD", "LAPC-ENG","LAPC-NIR", "LAPC-SCT",
                                                    "LAPC-WLS", "LEVY-ENG", "LEVY-NIR", "LEVY-SCT", "LEVY-WLS",
                                                    "LRET-AL", "LRET-FC", "LRET-GL", "LRET-OT",
                                                    "LRET-PC", "LRET-PL", "LRET-ST", "LRET-WD", "MATT-AD",
                                                    "MATT-AI", "MATT-PD", "MATT-PI", "SAOC-ENG", "SAOC-NIR",
                                                    "SAOC-SCT", "SAOC-WLS", "SCSC-ENG","SCSC-NIR", "SCSC-SCT",
                                                    "SCSC-WLS", "TONT-AI", "TONT-DI", "TONT-PD","TONT-PI" };


        [TestMethod]
        public void CreateTest_With41_Records()
        {
            var schemeParameterTemplateValues = new List<SchemeParameterTemplateValue>();
            foreach (var item in _uniqueReferences)
            {
                schemeParameterTemplateValues.Add(new SchemeParameterTemplateValue
                {
                    ParameterValue = 90,
                    ParameterUniqueReferenceId = item
                });
            }
            var createDefaultParameterDto = new CreateDefaultParameterSettingDto
            {
                ParameterYear = "2024-25",
                SchemeParameterTemplateValues = schemeParameterTemplateValues
            };
            var actionResult = _controller.Create(createDefaultParameterDto) as ObjectResult;
            Assert.AreEqual(actionResult.StatusCode, 201);

            Assert.AreEqual(_dbContext.DefaultParameterSettingDetail.Count(), 41);
            Assert.AreEqual(_dbContext.DefaultParameterSettings.Count(), 1);
            Assert.AreEqual(_dbContext.DefaultParameterTemplateMasterList.Count(), 41);
        }

        [TestMethod]
        public void CreateTest_With41_Records_When_Existing_Updates()
        {
            var schemeParameterTemplateValues = new List<SchemeParameterTemplateValue>();
            foreach (var item in _uniqueReferences)
            {
                schemeParameterTemplateValues.Add(new SchemeParameterTemplateValue
                {
                    ParameterValue = 90,
                    ParameterUniqueReferenceId = item
                });
            }
            var createDefaultParameterDto = new CreateDefaultParameterSettingDto
            {
                ParameterYear = "2024-25",
                SchemeParameterTemplateValues = schemeParameterTemplateValues
            };
            var actionResult1 = _controller.Create(createDefaultParameterDto) as ObjectResult;
            Assert.AreEqual(actionResult1.StatusCode, 201);

            var actionResult2 = _controller.Create(createDefaultParameterDto) as ObjectResult;
            Assert.AreEqual(actionResult2.StatusCode, 201);

            Assert.AreEqual(_dbContext.DefaultParameterSettingDetail.Count(), 82);
            Assert.AreEqual(_dbContext.DefaultParameterSettings.Count(), 2);
            Assert.AreEqual(_dbContext.DefaultParameterTemplateMasterList.Count(), 41);

            Assert.AreEqual(_dbContext.DefaultParameterSettingDetail.Count(x => x.DefaultParameterSettingMasterId == 2), 41);
            Assert.AreEqual(_dbContext.DefaultParameterSettings.Count(a => a.EffectiveTo == null), 1);
        }

        //GET API
        [TestMethod]
        public void Get_RequestOkResult_WithDefaultSchemeParametersDto()
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
            Assert.AreEqual(actionResul2.Count, 41);

            Assert.AreEqual(tempdateData.Id, actionResul2[0].Id);
            Assert.AreEqual(tempdateData.ParameterValue, actionResul2[0].ParameterValue);
            Assert.AreEqual(tempdateData.ParameterUniqueRef, actionResul2[0].ParameterUniqueRef);
        }

        [TestMethod]
        public void GetSchemeParameter_ReturnNotFound_WithDefaultSchemeParametersDoesNotExist()
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

            // Return 400 error if the year is null or empty
            //Act
            var result = _controller.Get("");
            //Assert
            var okResult = result as ObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(okResult.StatusCode, 400);

        }

        // Private Methods
        public void DataPostCall()
        {
            var schemeParameterTemplateValues = new List<SchemeParameterTemplateValue>();
            foreach (var item in _uniqueReferences)
            {
                schemeParameterTemplateValues.Add(new SchemeParameterTemplateValue
                {
                    ParameterValue = 90,
                    ParameterUniqueReferenceId = item
                });
            }
            var createDefaultParameterDto = new CreateDefaultParameterSettingDto
            {
                ParameterYear = "2024-25",
                SchemeParameterTemplateValues = schemeParameterTemplateValues
            };
            var actionResult = _controller.Create(createDefaultParameterDto) as ObjectResult;
        }
    }
}
