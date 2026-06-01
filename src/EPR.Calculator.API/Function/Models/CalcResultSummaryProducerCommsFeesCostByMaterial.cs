
namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultSummaryProducerCommsFeesCostByMaterial
        : CalcResultSummaryProducerMaterialBase
    {
        public decimal PriceperTonne { get; set; }

        public decimal ProducerTotalCostWithoutBadDebtProvision { get; set; }

        public decimal ProducerTotalCostwithBadDebtProvision { get; set; }
    }
}