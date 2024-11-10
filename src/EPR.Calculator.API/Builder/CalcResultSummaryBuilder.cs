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

            result.ResultSummaryHeader = new CalcResultSummaryHeader
            {
                Name = "Calculation Result",
                ColumnIndex = 0
            };

            result.ProducerDisposalFeesHeader = new CalcResultSummaryHeader
            {
                Name = "1 Producer Disposal Fees with Bad Debt Provision",
                ColumnIndex = 4
            };

            var materialsFromDb = this.context.Material.ToList();

            var materials = Mappers.MaterialMapper.Map(materialsFromDb);

            var materialsBreakdownHeader = new List<CalcResultSummaryHeader>();
            var columnIndex = 4;

            foreach (var material in materials)
            {
                materialsBreakdownHeader.Add(new CalcResultSummaryHeader
                {
                    Name = $"{material.Name} Breakdown",
                    ColumnIndex = columnIndex
                });
                columnIndex = columnIndex + 11;
            }

            result.MaterialBreakdownHeaders = materialsBreakdownHeader;

            var columnHeaders = new List<string>();

            columnHeaders.AddRange([
                CalcResultSummaryHeaders.ProducerId,
                CalcResultSummaryHeaders.SubsidiaryId,
                CalcResultSummaryHeaders.ProducerOrSubsidiaryName,
                CalcResultSummaryHeaders.Level
            ]);

            foreach (var material in materials)
            {
                columnHeaders.AddRange([
                    CalcResultSummaryHeaders.ReportedHouseholdPackagingWasteTonnage,
                    CalcResultSummaryHeaders.ReportedSelfManagedConsumerWasteTonnage,
                    CalcResultSummaryHeaders.NetReportedTonnage,
                    CalcResultSummaryHeaders.PricePerTonne,
                    CalcResultSummaryHeaders.ProducerDisposalFee,
                    CalcResultSummaryHeaders.BadDebtProvision,
                    CalcResultSummaryHeaders.ProducerDisposalFeeWithBadDebtProvision,
                    CalcResultSummaryHeaders.EnglandWithBadDebtProvision,
                    CalcResultSummaryHeaders.WalesWithBadDebtProvision,
                    CalcResultSummaryHeaders.ScotlandWithBadDebtProvision,
                    CalcResultSummaryHeaders.NorthernIrelandWithBadDebtProvision,
                    // 2a comms headers
                    CalcResultSummaryHeaders.ReportedHouseholdPackagingWasteTonnage,
                    CalcResultSummaryHeaders.PricePerTonne,
                    CalcResultSummaryHeaders.ProducerTotalCostWithoutBadDebtProvision,
                    CalcResultSummaryHeaders.BadDebtProvision,
                    CalcResultSummaryHeaders.ProducerTotalCostwithBadDebtProvision,
                    CalcResultSummaryHeaders.EnglandWithBadDebtProvision,
                    CalcResultSummaryHeaders.WalesWithBadDebtProvision,
                    CalcResultSummaryHeaders.ScotlandWithBadDebtProvision,
                    CalcResultSummaryHeaders.NorthernIrelandWithBadDebtProvision,
                ]);
            }

            result.ColumnHeaders = columnHeaders;

            var producerDetailList = this.context.ProducerDetail.ToList();

            var resultSummary = new List<CalcResultSummaryProducerDisposalFees>();

            foreach (var producer in producerDetailList)
            {
                var costSummary = new List<CalcResultSummaryProducerDisposalFeesByMaterial>();
                var feesCommsCostSummary = new List<CalcResultFeesCommsCostSummary>();
                var materialCostSummary = new Dictionary<MaterialDetail, IEnumerable<CalcResultSummaryProducerDisposalFeesByMaterial>>();

                foreach (var material in materials)
                {
                    var hhPackagingWasteTonnage = GetHouseholdPackagingWasteTonnage(producer, material);
                    decimal BadDebtProvision = 0.00M;
                    decimal PriceperTonne = GetPriceperTonne_FromParamOthers(producer, material); // by Tim
                    decimal ProducerTotalCostWithoutBadDebtProvision = GetProducerTotalCostWithoutBadDebtProvision(hhPackagingWasteTonnage, PriceperTonne);
                    decimal BadDebtProvisionCost = GetBadDebtProvision1(ProducerTotalCostWithoutBadDebtProvision, BadDebtProvision);
                    decimal ProducerTotalCostwithBadDebtProvision = GetProducerTotalCostwithBadDebtProvision(ProducerTotalCostWithoutBadDebtProvision, BadDebtProvision);

                    costSummary.Add(new CalcResultSummaryProducerDisposalFeesByMaterial
                    {
                        HouseholdPackagingWasteTonnage = hhPackagingWasteTonnage,
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

                    feesCommsCostSummary.Add(new CalcResultFeesCommsCostSummary
                    {
                        HouseholdPackagingWasteTonnage = hhPackagingWasteTonnage,
                        PriceperTonne = PriceperTonne,
                        ProducerTotalCostWithoutBadDebtProvision = ProducerTotalCostWithoutBadDebtProvision,
                        BadDebtProvision = BadDebtProvisionCost,
                        ProducerTotalCostwithBadDebtProvision = ProducerTotalCostwithBadDebtProvision,
                        EnglandWithBadDebtProvision = GetEnglandWithBadDebtProvisionFor2aComms(ProducerTotalCostwithBadDebtProvision, calcResult),
                        WalesWithBadDebtProvision = GetWalesWithBadDebtProvisionFor2aComms(ProducerTotalCostwithBadDebtProvision, calcResult),
                        ScotlandWithBadDebtProvision = GetScotlandWithBadDebtProvisionFor2aComms(ProducerTotalCostwithBadDebtProvision, calcResult),
                        NorthernIrelandWithBadDebtProvision = GetNorthernIrelandWithBadDebtProvisionFor2aComms(ProducerTotalCostwithBadDebtProvision, calcResult)
                    });

                    materialCostSummary.Add(material, costSummary);
                }

                resultSummary.Add(new CalcResultSummaryProducerDisposalFees
                {
                    ProducerId = producer.Id.ToString(),
                    ProducerName = producer.ProducerName,
                    SubsidiaryId = producer.SubsidiaryId,
                    Level = 1,
                    Order = 2,
                    ProducerDisposalFeesByMaterial = materialCostSummary
                });
            }

            return result;
        }

        private decimal GetHouseholdPackagingWasteTonnage(ProducerDetail producer, MaterialDetail material)
        {
            // return producer.ProducerReportedMaterials.FirstOrDefault(p => p.Material.Code == material.Code && p.PackagingType == "HH").PackagingTonnage;

            return 0.0m;
        }

        private decimal GetManagedConsumerWasteTonnage(ProducerDetail producer, MaterialDetail material)
        {
            // return producer.ProducerReportedMaterials.FirstOrDefault(p => p.Material.Code == material.Code && p.PackagingType == "CW").PackagingTonnage;

            return 0.0m;
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

        private decimal GetPriceperTonne_FromParamOthers(ProducerDetail producer, MaterialDetail material)
        {
            throw new NotImplementedException(); // Tim PR
        }

        private static decimal GetProducerTotalCostWithoutBadDebtProvision(decimal HHPackagingWasteTonnage, decimal PriceperTonne)
        {
            return HHPackagingWasteTonnage * PriceperTonne;
        }

        private static decimal GetBadDebtProvision1(decimal ProducerTotalCostWithoutBadDebtProvision, decimal BadDebtProvision)
        {
            //Formula:  F5*'Params - Other'!$B$10
            return ProducerTotalCostWithoutBadDebtProvision * BadDebtProvision;

        }

        private static decimal GetProducerTotalCostwithBadDebtProvision(decimal ProducerTotalCostWithoutBadDebtProvision, decimal BadDebtProvision)
        {
            // Formula: F5*(1+'Params - Other'!$B$10) --uday (Build the calculator - Params - Other - 3)
            return ProducerTotalCostWithoutBadDebtProvision * (1 + BadDebtProvision);
        }

        private static decimal GetEnglandWithBadDebtProvisionFor2aComms(decimal ProducerTotalCostwithBadDebtProvision, CalcResult calcResult)
        {
            // Formula: H5*'1 + 4 Apportionment %s'!$C$6
            return ProducerTotalCostwithBadDebtProvision * (1 + Convert.ToDecimal(calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.First().EnglandDisposalTotal));
        }

        private static decimal GetWalesWithBadDebtProvisionFor2aComms(decimal ProducerTotalCostwithBadDebtProvision, CalcResult calcResult)
        {
            // Formula: H5*'1 + 4 Apportionment %s'!$D$6
            return ProducerTotalCostwithBadDebtProvision * (1 + Convert.ToDecimal(calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.First().WalesDisposalTotal));
        }

        private static decimal GetScotlandWithBadDebtProvisionFor2aComms(decimal ProducerTotalCostwithBadDebtProvision, CalcResult calcResult)
        {
            // Formula: H5*'1 + 4 Apportionment %s'!$E$6
            return ProducerTotalCostwithBadDebtProvision * (1 + Convert.ToDecimal(calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.First().ScotlandDisposalTotal));

        }

        private static decimal GetNorthernIrelandWithBadDebtProvisionFor2aComms(decimal ProducerTotalCostwithBadDebtProvision, CalcResult calcResult)
        {
            // Formula: H5*'1 + 4 Apportionment %s'!$F$6
            return ProducerTotalCostwithBadDebtProvision * (1 + Convert.ToDecimal(calcResult.CalcResultOnePlusFourApportionment.CalcResultOnePlusFourApportionmentDetails.First().NorthernIrelandDisposalTotal));

        }
    }
}
