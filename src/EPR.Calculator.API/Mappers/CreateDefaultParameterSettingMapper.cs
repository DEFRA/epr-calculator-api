using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using Microsoft.EntityFrameworkCore;

namespace api.Mappers
{
    public static class CreateDefaultParameterSettingMapper
    {
        public static List<DefaultSchemeParametersDto> Map(DefaultParameterSettingMaster defaultParameterSettingMaster, DbSet<DefaultParameterTemplateMaster> defaultParameterTemplate)
        {
            var result = new List<DefaultSchemeParametersDto>();

            foreach (var item in defaultParameterSettingMaster.Details)
            {
                var data = new DefaultSchemeParametersDto
                {
                    Id = item.Id,
                    ParameterYear = defaultParameterSettingMaster.ParameterYear,
                    EffectiveFrom = defaultParameterSettingMaster.EffectiveFrom,
                    EffectiveTo = defaultParameterSettingMaster.EffectiveTo,
                    CreatedBy = defaultParameterSettingMaster.CreatedBy,
                    CreatedAt = defaultParameterSettingMaster.CreatedAt,
                    DefaultParameterSettingMasterId = item.Id,
                    ParameterUniqueRef = item.ParameterUniqueReferenceId,
                    ParameterType = defaultParameterTemplate.Single(x => x.ParameterUniqueReferenceId == item.ParameterUniqueReferenceId)?.ParameterType,
                    ParameterCategory = defaultParameterTemplate.Single(x => x.ParameterUniqueReferenceId == item.ParameterUniqueReferenceId)?.ParameterCategory,
                    ParameterValue = item.ParameterValue
                };

                result.Add(data);
            }
            return result;
        }
    }
}
