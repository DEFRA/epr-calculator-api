
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;

namespace EPR.Calculator.API.Mappers
{
    public static class LapcapDataParameterSettingMapper
    {
        public static List<LapCapParameterDto> Map(LapcapDataMaster lapcapSettingMaster,
                                                   IEnumerable<LapcapDataTemplateMaster> lapcapDataTemplate)
        {
            var result = new List<LapCapParameterDto>();

            foreach (var item in lapcapSettingMaster.Details)
            {
                var selectedTemplate = lapcapDataTemplate.Single(x => x.UniqueReference == item.UniqueReference);
                var data = new LapCapParameterDto
                {
                    Id = item.Id,
                    ProjectionYear = lapcapSettingMaster.ProjectionYear,
                    CreatedBy = lapcapSettingMaster.CreatedBy,
                    CreatedAt = lapcapSettingMaster.CreatedAt,
                    LapcapDataMasterId = lapcapSettingMaster.Id,
                    LapcapTempUniqueRef = item.UniqueReference,
                    Country = selectedTemplate.Country,
                    Material = selectedTemplate.Material,
                    TotalCost = item.TotalCost,
                    EffectiveFrom = lapcapSettingMaster.EffectiveFrom
                };

                result.Add(data);
            }
            return result;
        }
    }
}