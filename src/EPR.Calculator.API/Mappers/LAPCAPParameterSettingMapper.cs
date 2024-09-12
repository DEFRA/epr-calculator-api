
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Mappers
{
    public static class LAPCAPParameterSettingMapper
    {
        public static List<LapCapParameterDto> Map(LapcapDataMaster defaultParameterSettingMaster, IEnumerable<LapcapDataTemplateMaster> defaultParameterTemplate)
        {
            var result = new List<LapCapParameterDto>();

            foreach (var item in defaultParameterSettingMaster.Details)
            {
                var selectedTemplate = defaultParameterTemplate.Single(x => x.UniqueReference == item.UniqueReference);
                var data = new LapCapParameterDto
                {
                    Id = item.Id,
                    Year = defaultParameterSettingMaster.Year,
                    CreatedBy = defaultParameterSettingMaster.CreatedBy,
                    CreatedAt = defaultParameterSettingMaster.CreatedAt,
                    LapcapDataMasterId = defaultParameterSettingMaster.Id,
                    LapcapTempUniqueRef = item.UniqueReference,
                    Country = selectedTemplate.Country,
                    Material = selectedTemplate.Material,
                    TotalCost = item.TotalCost
                };

                result.Add(data);
            }
            return result;
        }
    }
}