using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Features.Common;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Services;

public record L1Producer(
    int OrganisationId,
    List<ProducerDetail> Producers
);

public interface IReportedProducerService
{
    Task<List<L1Producer>> GetProducers(RunContext runContext);
}

public class ReportedProducerService(ApplicationDBContext dbContext)
    : IReportedProducerService
{
    public async Task<List<L1Producer>> GetProducers(RunContext runContext)
    {
        return
            await dbContext.ProducerDetail.AsNoTracking()
                .Include(pd => pd.ProducerReportedMaterials).ThenInclude(prm => prm.Material)
                .Where(pd => pd.CalculatorRunId == runContext.RunId)
                .GroupBy(pd => pd.ProducerId)
                .Select(pds => new L1Producer(pds.Key, pds.ToList()))
                .ToListAsync();
    }
}
