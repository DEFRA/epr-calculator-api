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
                var feesCommsCostSummary = new List<CalcResultFeesCommsCostSummary>();

                foreach (var material in materials)
                {
                    var hhPackagingWasteTonnage = GetHouseholdPackagingWasteTonnage(producer, material);

                    decimal BadDebtProvision = 0.00M;
                    decimal Apportionment = 0.00M;
                    decimal PriceperTonne = GetPriceperTonne(producer, material); // by Tim
                    decimal ProducerTotalCostWithoutBadDebtProvision = GetProducerTotalCostWithoutBadDebtProvision(hhPackagingWasteTonnage, PriceperTonne);
                    decimal BadDebtProvisionCost = GetBadDebtProvision1(ProducerTotalCostWithoutBadDebtProvision, BadDebtProvision);
                    decimal ProducerTotalCostwithBadDebtProvision = GetProducerTotalCostwithBadDebtProvision(ProducerTotalCostWithoutBadDebtProvision, BadDebtProvision);

                    costSummary.Add(new CalcResultSummaryMaterialCost
                    {
                        HouseholdPackagingWasteTonnage = hhPackagingWasteTonnage,
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
                    feesCommsCostSummary.Add(new CalcResultFeesCommsCostSummary
                    {
                        HouseholdPackagingWasteTonnage = hhPackagingWasteTonnage, // parametr D4 John doing
                        PriceperTonne = PriceperTonne,
                        ProducerTotalCostWithoutBadDebtProvision = ProducerTotalCostWithoutBadDebtProvision, //Done
                        BadDebtProvision = BadDebtProvisionCost,  //Done
                        ProducerTotalCostwithBadDebtProvision = ProducerTotalCostwithBadDebtProvision,//Done

                        EnglandWithBadDebtProvision = GetEnglandWithBadDebtProvision1(ProducerTotalCostwithBadDebtProvision, Apportionment), //Done
                        WalesWithBadDebtProvision = GetWalesWithBadDebtProvision1(ProducerTotalCostwithBadDebtProvision, Apportionment), //Done
                        ScotlandWithBadDebtProvision = GetScotlandWithBadDebtProvision1(ProducerTotalCostwithBadDebtProvision, Apportionment), //Done
                        NorthernIrelandWithBadDebtProvision = GetNorthernIrelandWithBadDebtProvision1(ProducerTotalCostwithBadDebtProvision, Apportionment) //Done
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

        private decimal GetPriceperTonne(ProducerDetail producer, MaterialDetail material)
        {
            throw new NotImplementedException(); // Tim PR
        }

        private decimal GetProducerTotalCostWithoutBadDebtProvision(decimal HHPackagingWasteTonnage, decimal PriceperTonne)
        {
            return HHPackagingWasteTonnage * PriceperTonne;
        }

        private decimal GetBadDebtProvision1(decimal ProducerTotalCostWithoutBadDebtProvision, decimal BadDebtProvision)
        {
            //Formula:  F5*'Params - Other'!$B$10
            return ProducerTotalCostWithoutBadDebtProvision * BadDebtProvision;

        }
        private decimal GetProducerTotalCostwithBadDebtProvision(decimal ProducerTotalCostWithoutBadDebtProvision, decimal BadDebtProvision)
        {
            // Formula: F5*(1+'Params - Other'!$B$10) --uday (Build the calculator - Params - Other - 3)
            return ProducerTotalCostWithoutBadDebtProvision * ( 1 + BadDebtProvision );
        }
        private decimal GetEnglandWithBadDebtProvision1(decimal ProducerTotalCostwithBadDebtProvision, decimal Apportionment)
        {
            // Formula: H5*'1 + 4 Apportionment %s'!$C$6
            // Rekha ( Build the calculator - 1 + 4 Apportionment %s - 4 )
            return ProducerTotalCostwithBadDebtProvision * (1 + Apportionment);
        }

        private decimal GetWalesWithBadDebtProvision1(decimal ProducerTotalCostwithBadDebtProvision, decimal Apportionment)
        {
            // Formula: H5*'1 + 4 Apportionment %s'!$D$6
            // Rekha ( Build the calculator - 1 + 4 Apportionment %s - 4 )
            return ProducerTotalCostwithBadDebtProvision * (1 + Apportionment);

        }
        private decimal GetScotlandWithBadDebtProvision1(decimal ProducerTotalCostwithBadDebtProvision, decimal Apportionment)
        {
            // Formula: H5*'1 + 4 Apportionment %s'!$E$6
            // Rekha ( Build the calculator - 1 + 4 Apportionment %s - 4 )
            return ProducerTotalCostwithBadDebtProvision * (1 + Apportionment);

        }

        private decimal GetNorthernIrelandWithBadDebtProvision1(decimal ProducerTotalCostwithBadDebtProvision, decimal Apportionment)
        {
            // Formula: H5*'1 + 4 Apportionment %s'!$F$6
            // Rekha ( Build the calculator - 1 + 4 Apportionment %s - 4 )
            return ProducerTotalCostwithBadDebtProvision * (1 + Apportionment);

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
