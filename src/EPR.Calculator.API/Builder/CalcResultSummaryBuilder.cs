using EPR.Calculator.API.Constants;
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

        public CalcResultSummary Construct(CalcResultsRequestDto resultsRequestDto, CalcResult calcResult)
        {
            var result = new CalcResultSummary();

            var materialsFromDb = this.context.Material.ToList();

            var materials = Mappers.MaterialMapper.Map(materialsFromDb);

            var producerDetailList = this.context.ProducerDetail.ToList();

            var resultSummary = new List<CalcResultSummary>();

            // var headerRecords = GetHeaderRecords(materials);

            // resultSummary.AddRange(headerRecords);

            foreach (var producer in producerDetailList)
            {
                var costSummary = new List<CalcResultSummaryMaterialCost>();

                var materialCostSummary = new Dictionary<MaterialDetail, IEnumerable<CalcResultSummaryMaterialCost>>();

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
                    ProducerId = producer.Id.ToString(),
                    ProducerName = producer.ProducerName,
                    SubsidiaryId = producer.SubsidiaryId,
                    Level = "1",
                    Order = 2,
                    MaterialCostSummary = materialCostSummary
                });
            }

            return result;
        }

        private string GetHouseholdPackagingWasteTonnage(ProducerDetail producer, MaterialDetail material)
        {
            return string.Empty;
        }

        private string GetManagedConsumerWasteTonnage(ProducerDetail producer, MaterialDetail material)
        {
            return string.Empty;
        }

        private string GetNetReportedTonnage(ProducerDetail producer, MaterialDetail material)
        {
            return string.Empty;
        }

        private string GetPricePerTonnage(ProducerDetail producer, MaterialDetail material)
        {
            return string.Empty;
        }

        private string GetProducerDisposalFee(ProducerDetail producer, MaterialDetail material)
        {
            return string.Empty;
        }

        private string GetBadDebtProvision(ProducerDetail producer, MaterialDetail material)
        {
            return string.Empty;
        }

        private string GetProducerDisposalFeeWithBadDebtProvision(ProducerDetail producer, MaterialDetail material)
        {
            return string.Empty;
        }

        private string GetEnglandWithBadDebtProvision(ProducerDetail producer, MaterialDetail material)
        {
            return string.Empty;
        }

        private string GetWalesWithBadDebtProvision(ProducerDetail producer, MaterialDetail material)
        {
            return string.Empty;
        }

        private string GetScotlandWithBadDebtProvision(ProducerDetail producer, MaterialDetail material)
        {
            return string.Empty;
        }

        private string GetNorthernIrelandWithBadDebtProvision(ProducerDetail producer, MaterialDetail material)
        {
            return string.Empty;
        }

        private IEnumerable<CalcResultSummary> GetHeaderRecords(List<MaterialDetail> materials)
        {
            var resultSummaryHeaders = new List<CalcResultSummary>();

            var materialCostSummaryHeaders = new Dictionary<MaterialDetail, IEnumerable<CalcResultSummaryMaterialCost>>();

            var costSummaryHeaders = new List<CalcResultSummaryMaterialCost>();

            // First header record
            foreach (var material in materials)
            {
                costSummaryHeaders.Add(new CalcResultSummaryMaterialCost
                {
                    HouseholdPackagingWasteTonnage = $"{material.Name} Breakdown",
                    ManagedConsumerWasteTonnage = string.Empty,
                    NetReportedTonnage = string.Empty,
                    PricePerTonnage = string.Empty,
                    ProducerDisposalFee = string.Empty,
                    BadDebtProvision = string.Empty,
                    ProducerDisposalFeeWithBadDebtProvision = string.Empty,
                    EnglandWithBadDebtProvision = string.Empty,
                    WalesWithBadDebtProvision = string.Empty,
                    ScotlandWithBadDebtProvision = string.Empty,
                    NorthernIrelandWithBadDebtProvision = string.Empty
                });

                materialCostSummaryHeaders.Add(material, costSummaryHeaders);
            }

            resultSummaryHeaders.Add(new CalcResultSummary
            {
                ProducerId = string.Empty,
                SubsidiaryId = string.Empty,
                ProducerName = string.Empty,
                Level = string.Empty,
                Order = 0,
                MaterialCostSummary = materialCostSummaryHeaders
            });

            // Second header record
            foreach (var material in materials)
            {
                costSummaryHeaders.Add(new CalcResultSummaryMaterialCost
                {
                    HouseholdPackagingWasteTonnage = CalcResultSummaryHeaders.ReportedHouseholdPackagingWasteTonnage,
                    ManagedConsumerWasteTonnage = CalcResultSummaryHeaders.ReportedSelfManagedConsumerWasteTonnage,
                    NetReportedTonnage = CalcResultSummaryHeaders.NetReportedTonnage,
                    PricePerTonnage = CalcResultSummaryHeaders.PricePerTonne,
                    ProducerDisposalFee = CalcResultSummaryHeaders.ProducerDisposalFee,
                    BadDebtProvision = CalcResultSummaryHeaders.BadDebtProvision,
                    ProducerDisposalFeeWithBadDebtProvision = CalcResultSummaryHeaders.ProducerDisposalFeeWithBadDebtProvision,
                    EnglandWithBadDebtProvision = CalcResultSummaryHeaders.EnglandWithBadDebtProvision,
                    WalesWithBadDebtProvision = CalcResultSummaryHeaders.WalesWithBadDebtProvision,
                    ScotlandWithBadDebtProvision = CalcResultSummaryHeaders.ScotlandWithBadDebtProvision,
                    NorthernIrelandWithBadDebtProvision = CalcResultSummaryHeaders.NorthernIrelandWithBadDebtProvision
                });

                materialCostSummaryHeaders.Add(material, costSummaryHeaders);
            }

            resultSummaryHeaders.Add(new CalcResultSummary
            {
                ProducerId = CalcResultSummaryHeaders.ProducerId,
                SubsidiaryId = CalcResultSummaryHeaders.SubsidiaryId,
                ProducerName = CalcResultSummaryHeaders.ProducerOrSubsidiaryName,
                Level = CalcResultSummaryHeaders.Level,
                Order = 2,
                MaterialCostSummary = materialCostSummaryHeaders
            });

            return resultSummaryHeaders;
        }
    }
}
