using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
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

            var resultSummary = new List<CalcResultSummary>();

            foreach (var producer in producerDetailList)
            {
                var costSummary = new List<CalcResultSummaryMaterialCost>();

                foreach (var material in materials)
                {
                    costSummary.Add(new CalcResultSummaryMaterialCost
                    {
                        HouseholdPackagingWasteTonnage = GetHouseholdPackagingWasteTonnage(producer, material),
                        ManagedConsumerWasteTonnage = GetManagedConsumerWasteTonnage(producer, material),
                        NetReportedTonnage = GetNetReportedTonnage(producer, material),
                        PricePerTonnage = GetPricePerTonnage(producer, material),
                        ProducerDisposalFee = GetProducerDisposalFee(producer, material),
                        BadDebtProvision = GetBadDebtProvision(producer, material),
                        ProducerDisposalFeeWithBadDebtProvision = GetProducerDisposalFeeWithBadDebtProvision(producer, material),
                        EnglandWithBadDebtProvision = GetEnglandWithBadDebtProvision(producer, material),
                        WalesWithBadDebtProvision = GetWalesWithBadDebtProvision(producer, material),
                        ScotlandWithBadDebtProvision = GetScotlandWithBadDebtProvision(producer, material),
                        NorthernIrelandWithBadDebtProvision = GetNorthernIrelandWithBadDebtProvision(producer, material)
                    });

                    materialCostSummary.Add(material, costSummary);
                }

                resultSummary.Add(new CalcResultSummary
                {
                    ProducerId = producer.Id,
                    ProducerName = producer.ProducerName,
                    SubsidiaryId = producer.SubsidiaryId,
                    MaterialCostSummary = materialCostSummary
                });

                materialCostSummary.Clear();
            }

            return result;
        }

        private decimal GetHouseholdPackagingWasteTonnage(ProducerDetail producer, MaterialDetail material)
        {
            return 10.00m;
        }

        private decimal GetManagedConsumerWasteTonnage(ProducerDetail producer, MaterialDetail material)
        {
            return 10.00m;
        }

        private decimal GetNetReportedTonnage(ProducerDetail producer, MaterialDetail material)
        {
            return 10.00m;
        }

        private decimal GetPricePerTonnage(ProducerDetail producer, MaterialDetail material)
        {
            return 10.00m;
        }

        private decimal GetProducerDisposalFee(ProducerDetail producer, MaterialDetail material)
        {
            return 10.00m;
        }

        private decimal GetBadDebtProvision(ProducerDetail producer, MaterialDetail material)
        {
            return 10.00m;
        }

        private decimal GetProducerDisposalFeeWithBadDebtProvision(ProducerDetail producer, MaterialDetail material)
        {
            return 10.00m;
        }

        private decimal GetEnglandWithBadDebtProvision(ProducerDetail producer, MaterialDetail material)
        {
            return 10.00m;
        }

        private decimal GetWalesWithBadDebtProvision(ProducerDetail producer, MaterialDetail material)
        {
            return 10.00m;
        }

        private decimal GetScotlandWithBadDebtProvision(ProducerDetail producer, MaterialDetail material)
        {
            return 10.00m;
        }

        private decimal GetNorthernIrelandWithBadDebtProvision(ProducerDetail producer, MaterialDetail material)
        {
            return 10.00m;
        }
    }
}
