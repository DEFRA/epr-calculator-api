using EPR.Calculator.API.Builder.Summary;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Builder.ScaledupProducers
{
    public class CalcResultScaledupProducersBuilder : ICalcResultScaledupProducersBuilder
    {
        private readonly ApplicationDBContext context;

        public CalcResultScaledupProducersBuilder(ApplicationDBContext context)
        {
            this.context = context;
        }

        public async Task<CalcResultScaledupProducers> Construct(CalcResultsRequestDto resultsRequestDto, CalcResult calcResult)
        {
            var runId = resultsRequestDto.RunId;
            var materialsFromDb = await context.Material.ToListAsync();
            var materials = Mappers.MaterialMapper.Map(materialsFromDb);

            var runProducerMaterialDetails = await (from pd in context.ProducerDetail
                                                    join prm in context.ProducerReportedMaterial on pd.Id equals prm.ProducerDetailId
                                                    where pd.CalculatorRunId == runId
                                                    select new CalcResultsProducerAndReportMaterialDetail
                                                    {
                                                        ProducerDetail = pd,
                                                        ProducerReportedMaterial = prm
                                                    }).ToListAsync();

            return new CalcResultScaledupProducers();
        }
    }
}
