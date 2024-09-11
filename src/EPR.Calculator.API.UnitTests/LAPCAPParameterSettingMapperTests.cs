using api.Tests.Controllers;
using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Data.DataModels;
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
            var details = new List<LapcapDataDetail>
                {
                    new LapcapDataDetail
                    {Id=1, LapcapDataMasterId = 1, UniqueReference = "ENG-AL", TotalCost = 30.99m }
                };
            var detail = new LapcapDataDetail
            {
                Id = 1,
                UniqueReference = "ENG-AL",
                TotalCost = 30.99m
            };
            var defaultParameterSettingMaster = new LapcapDataMaster
            {
                Year = "2024-25",
                CreatedBy = "Testuser",
                CreatedAt = DateTime.Now,
            };

            details.ForEach(detail => defaultParameterSettingMaster.Details.Add(detail));

            var template = new LapcapDataTemplateMaster
            {
                UniqueReference = "ENG-AL",
                Country = "England",
                Material = "Aluminium"
            };

            // Act
            var result = LAPCAPParameterSettingMapper.Map(defaultParameterSettingMaster, dbContext?.LapcapDataTemplateMaster);

            // Assert
            var mappedItem = result.First();
            Assert.AreEqual(detail.Id, mappedItem.Id);
            Assert.AreEqual(defaultParameterSettingMaster.Year, mappedItem.Year);
            Assert.AreEqual(defaultParameterSettingMaster.CreatedBy, mappedItem.CreatedBy);
            Assert.AreEqual(defaultParameterSettingMaster.CreatedAt, mappedItem.CreatedAt);
            Assert.AreEqual(detail.Id, mappedItem.LapcapDataMasterId);
            Assert.AreEqual(detail.UniqueReference, mappedItem.LapcapTempUniqueRef);
            Assert.AreEqual(template.Country, mappedItem.Country);
            Assert.AreEqual(template.Material, mappedItem.Material);
            Assert.AreEqual(detail.TotalCost, mappedItem.TotalCost);
        }
    }
}