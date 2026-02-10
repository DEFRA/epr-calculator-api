using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;

namespace EPR.Calculator.API.Mappers
{
    public static class CreateDefaultParameterSettingMapper
    {
        public static List<DefaultSchemeParametersDto> Map(
            DefaultParameterSettingMaster defaultParameterSettingMaster,
            IEnumerable<DefaultParameterTemplateMaster> defaultParameterTemplate)
        {
            return [.. defaultParameterSettingMaster
                .Details
                .Select(item =>
                {
                    var selectedTemplate = defaultParameterTemplate
                        .Single(x => x.ParameterUniqueReferenceId == item.ParameterUniqueReferenceId);
                    return new DefaultSchemeParametersDto
                    {
                        Id = item.Id,
                        RelativeYear = defaultParameterSettingMaster.RelativeYear,
                        EffectiveFrom = defaultParameterSettingMaster.EffectiveFrom,
                        EffectiveTo = defaultParameterSettingMaster.EffectiveTo,
                        CreatedBy = defaultParameterSettingMaster.CreatedBy,
                        CreatedAt = defaultParameterSettingMaster.CreatedAt,
                        DefaultParameterSettingMasterId = defaultParameterSettingMaster.Id,
                        ParameterUniqueRef = item.ParameterUniqueReferenceId,
                        ParameterType = selectedTemplate.ParameterType,
                        ParameterCategory = selectedTemplate.ParameterCategory,
                        ParameterValue = item.ParameterValue,
                    };
                })];
        }
    }
}
