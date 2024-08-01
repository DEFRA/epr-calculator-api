﻿using api.Dtos;
using api.Mappers;
using EPR.Calculator.API.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace api.Tests.Controllers
{
    [TestClass]
    public class CreateDefaultParameterSettingMapper : BaseControllerTest
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
        public void Get_Required_ResultSet_WithDefaultSchemeParametersDto()
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
            Assert.AreEqual(tempdateData.ParameterYear, actionResul2[0].ParameterYear);
            Assert.AreEqual(tempdateData.CreatedBy, actionResul2[0].CreatedBy);
            Assert.AreEqual(tempdateData.DefaultParameterSettingMasterId, actionResul2[0].DefaultParameterSettingMasterId);
            Assert.AreEqual(tempdateData.ParameterUniqueRef, actionResul2[0].ParameterUniqueRef);
            Assert.AreEqual(tempdateData.ParameterType, actionResul2[0].ParameterType);
            Assert.AreEqual(tempdateData.ParameterCategory, actionResul2[0].ParameterCategory);
            Assert.AreEqual(tempdateData.ParameterValue, actionResul2[0].ParameterValue);
        }

        // Private Methods
        private void DataPostCall()
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
