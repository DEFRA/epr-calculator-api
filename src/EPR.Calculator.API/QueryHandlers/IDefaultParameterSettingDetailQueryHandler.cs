using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Queries;

namespace EPR.Calculator.API.QueryHandlers
{
    public interface IDefaultParameterSettingDetailQueryHandler
    {
        public IEnumerable<DefaultSchemeParametersDto> Query(DefaultParameterSettingDetailQuery query);
    }
}
