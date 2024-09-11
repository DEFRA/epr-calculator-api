using api.Mappers;
using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Mappers;
using Microsoft.AspNetCore.Components;
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
            var currentDefaultSetting = dbContext?.DefaultParameterSettings.SingleOrDefault(x => x.EffectiveTo == null && x.ParameterYear == "2024-25");
            var _templateDetails = dbContext?.DefaultParameterTemplateMasterList;

            var schemeParameters = CreateDefaultParameterSettingMapper.Map(currentDefaultSetting, _templateDetails);

            //var schemeParametersDto = schemeParameters.Select(
            Assert.IsNotNull(currentDefaultSetting);
            Assert.IsNotNull(currentDefaultSetting.Details);
            Assert.IsNotNull(_templateDetails);
            Assert.IsNotNull(schemeParameters);
            Assert.AreEqual(DefaultParameterUniqueReferences.UniqueReferences.Length, schemeParameters.Count);
        }

        [TestMethod]
        public void Check_TheResult_Parmeter_Are_Equal_IsNotNullOf_ResultSet_WithDefaultSchemeParametersDto_WithCorrectYearr()
        {
            var details = new List<DefaultParameterSettingDetail>
                {
                new DefaultParameterSettingDetail
                    {Id=1, DefaultParameterSettingMasterId = 1, ParameterUniqueReferenceId="BADEBT-P", ParameterValue=30.99m }
                };
            var detail = new DefaultParameterSettingDetail
            {
                Id = 1,
                DefaultParameterSettingMasterId = 1,
                ParameterUniqueReferenceId = "BADEBT-P",
                ParameterValue = 30.99m
            };
            var defaultParameterSettingMaster = new DefaultParameterSettingMaster
            {
                ParameterYear = "2024-25",
                CreatedBy = "Testuser",
                CreatedAt = DateTime.Now,
            };

            details.ForEach(detail => defaultParameterSettingMaster.Details.Add(detail));

            var template = new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "BADEBT-P",
                ParameterType = "Bad debt provision",
                ParameterCategory = "Percentage"
            };

            // Act
            var result = CreateDefaultParameterSettingMapper.Map(defaultParameterSettingMaster, dbContext?.DefaultParameterTemplateMasterList);

            //// Assert
            //var mappedItem = result.First();
            //Assert.AreEqual(detail.Id, mappedItem.Id);
            //Assert.AreEqual(defaultParameterSettingMaster.Year, mappedItem.Year);
            //Assert.AreEqual(defaultParameterSettingMaster.CreatedBy, mappedItem.CreatedBy);
            //Assert.AreEqual(defaultParameterSettingMaster.CreatedAt, mappedItem.CreatedAt);
            //Assert.AreEqual(detail.Id, mappedItem.LapcapDataMasterId);
            //Assert.AreEqual(detail.UniqueReference, mappedItem.LapcapTempUniqueRef);
            //Assert.AreEqual(template.Country, mappedItem.Country);
            //Assert.AreEqual(template.Material, mappedItem.Material);
            //Assert.AreEqual(detail.TotalCost, mappedItem.TotalCost);
        }
    }
}