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
                        PricePerTonnage = GetPricePerTonne(producer, material, calcResult),
                        ProducerDisposalFee = GetProducerDisposalFee(producer, material, calcResult),
                        BadDebtProvision = GetBadDebtProvision(producer, material, calcResult),
                        ProducerDisposalFeeWithBadDebtProvision = GetProducerDisposalFeeWithBadDebtProvision(producer, material, calcResult),
                        EnglandWithBadDebtProvision = GetEnglandWithBadDebtProvision(producer, material, calcResult),
                        WalesWithBadDebtProvision = GetWalesWithBadDebtProvision(producer, material, calcResult),
                        ScotlandWithBadDebtProvision = GetScotlandWithBadDebtProvision(producer, material, calcResult),
                        NorthernIrelandWithBadDebtProvision = GetNorthernIrelandWithBadDebtProvision(producer, material, calcResult)
                    });

                    materialCostSummary.Add(material, costSummary);
                }

                resultSummary.Add(new CalcResultSummary
                {
                    ProducerId = producer.Id.ToString(),
                    ProducerName = producer.ProducerName,
                    SubsidiaryId = producer.SubsidiaryId,
                    Level = 1,
                    Order = 2,
                    MaterialCostSummary = materialCostSummary
                });
            }

            return result;
        }

        private decimal GetHouseholdPackagingWasteTonnage(ProducerDetail producer, MaterialDetail material)
        {
            return producer.ProducerReportedMaterials.FirstOrDefault(p => p.Material.Code == material.Code && p.PackagingType == "HH").PackagingTonnage;
        }

        private decimal GetManagedConsumerWasteTonnage(ProducerDetail producer, MaterialDetail material)
        {
            return producer.ProducerReportedMaterials.FirstOrDefault(p => p.Material.Code == material.Code && p.PackagingType == "CW").PackagingTonnage;
        }

        private decimal GetNetReportedTonnage(ProducerDetail producer, MaterialDetail material)
        {
            var householdPackagingWasteTonnage = GetHouseholdPackagingWasteTonnage(producer, material);
            var managedConsumerWasteTonnage = GetManagedConsumerWasteTonnage(producer, material);
            return householdPackagingWasteTonnage - managedConsumerWasteTonnage;
        }

        private decimal GetPricePerTonne(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
        {
            return 10m;
        }

        private decimal GetProducerDisposalFee(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
        {
            var netReportedTonnage = GetNetReportedTonnage(producer, material);
            var pricePerTonne = GetPricePerTonne(producer, material, calcResult);

            return netReportedTonnage * pricePerTonne;
        }

        private decimal GetBadDebtProvision(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
        {
            var producerDisposalFee = GetProducerDisposalFee(producer, material, calcResult);


            return producerDisposalFee * 6;
        }

        private decimal GetProducerDisposalFeeWithBadDebtProvision(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
        {
            var producerDisposalFee = GetProducerDisposalFee(producer, material, calcResult);

            return producerDisposalFee * (1 + 6);
        }

        private decimal GetEnglandWithBadDebtProvision(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
        {
            var producerDisposalFeeWithBadDebtProvision = GetProducerDisposalFeeWithBadDebtProvision(producer, material, calcResult);

            // producerDisposalFeeWithBadDebtProvision * LAPCAP Data B12

            return 10m;
        }

        private decimal GetWalesWithBadDebtProvision(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
        {
            var producerDisposalFeeWithBadDebtProvision = GetProducerDisposalFeeWithBadDebtProvision(producer, material, calcResult);

            // producerDisposalFeeWithBadDebtProvision * LAPCAP Data C12

            return 10m;
        }

        private decimal GetScotlandWithBadDebtProvision(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
        {
            var producerDisposalFeeWithBadDebtProvision = GetProducerDisposalFeeWithBadDebtProvision(producer, material, calcResult);

            // producerDisposalFeeWithBadDebtProvision * LAPCAP Data D12

            return 10m;
        }

        private decimal GetNorthernIrelandWithBadDebtProvision(ProducerDetail producer, MaterialDetail material, CalcResult calcResult)
        {
            var producerDisposalFeeWithBadDebtProvision = GetProducerDisposalFeeWithBadDebtProvision(producer, material, calcResult);

            // producerDisposalFeeWithBadDebtProvision * LAPCAP Data E12

            return 10m;
        }

        /* private IEnumerable<CalcResultSummary> GetHeaderRecords(List<MaterialDetail> materials)
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
        } */
    }
}
