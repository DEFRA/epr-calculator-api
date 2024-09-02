using api.Mappers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Queries;

namespace EPR.Calculator.API.QueryHandlers
{
    public class DefaultParameterSettingDetailQueryHandler : IDefaultParameterSettingDetailQueryHandler
    {
        private readonly ApplicationDBContext context;

        public DefaultParameterSettingDetailQueryHandler(ApplicationDBContext context)
        {
            this.context = context;
        }
        public IEnumerable<DefaultSchemeParametersDto> Query(DefaultParameterSettingDetailQuery query)
        {
            var result = new List<DefaultSchemeParametersDto>();
            var parameterYear = query.ParameterYear;
            var currentDefaultSetting = this.context.DefaultParameterSettings.SingleOrDefault(x => x.EffectiveTo == null && x.ParameterYear == parameterYear);

            if (currentDefaultSetting == null)
            {
                return result;
            }

            var _pramSettingDetails = this.context.DefaultParameterSettingDetail.ToList();
            var _templateDetails = this.context.DefaultParameterTemplateMasterList;
            result.AddRange(CreateDefaultParameterSettingMapper.Map(currentDefaultSetting, _templateDetails));
            return result;
        }
    }
}
