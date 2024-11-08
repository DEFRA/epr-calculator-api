using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder
{
    public class CalcResultSummaryBuilder : ICalcResultSummaryBuilder
    {
        private readonly ApplicationDBContext context;
        public CalcResultSummaryBuilder(ApplicationDBContext context)
        {
            this.context = context;
        }

        public CalcResultSummary Construct(CalcResultsRequestDto resultsRequestDto)
        {
            var result = new CalcResultSummary();

            var materialsFromDb = this.context.Material.ToList();

            var materials = Mappers.MaterialMapper.Map(materialsFromDb);

            var producerDetailList = this.context.ProducerDetail.ToList();

            var materialCostSummary = new Dictionary<MaterialDetail, IEnumerable<CalcResultSummaryMaterialCost>>();

            foreach ( var producer in producerDetailList)
            {
                materialCostSummary.Add(materials[0], new List<CalcResultSummaryMaterialCost>());
            }

            return result;
        }
    }
}
