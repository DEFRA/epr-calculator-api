using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Features.Common;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Services;

public interface IParameterService
{
    public Task<IReadOnlyDictionary<string, decimal>> GetDefaultParameters(RunContext runContext);
}

public class ParameterService : IParameterService
{
    private readonly ApplicationDBContext dbContext;

    public ParameterService(ApplicationDBContext context) => dbContext = context;

    public async Task<IReadOnlyDictionary<string, decimal>> GetDefaultParameters(RunContext runContext)
    {
        return await (
            from run in dbContext.CalculatorRuns.AsNoTracking()
            join defaultMaster in dbContext.DefaultParameterSettings.AsNoTracking() on run.DefaultParameterSettingMasterId equals
                defaultMaster.Id
            join defaultDetail in dbContext.DefaultParameterSettingDetail.AsNoTracking() on defaultMaster.Id equals defaultDetail
                .DefaultParameterSettingMasterId
            join defaultTemplate in dbContext.DefaultParameterTemplateMasterList.AsNoTracking() on defaultDetail
                .ParameterUniqueReferenceId equals defaultTemplate.ParameterUniqueReferenceId
            where run.Id == runContext.RunId
            select new { Key = defaultDetail.ParameterUniqueReferenceId, Value = defaultDetail.ParameterValue }
        ).ToDictionaryAsync(pair => pair.Key, pair => pair.Value);
    }
}
