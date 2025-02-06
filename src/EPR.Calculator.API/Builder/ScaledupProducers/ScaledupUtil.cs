using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.Builder.ScaledupProducers
{
    public static class ScaledupUtil
    {
        public static IEnumerable<ProducerDetail> GetOrderedListOfProducersAssociatedRunId(
           int runId,
           IEnumerable<ProducerDetail> producerDetails)
        {
            return producerDetails.Where(pd => pd.CalculatorRunId == runId).OrderBy(pd => pd.ProducerId).ToList();
        }

        public static decimal GetReportedHouseholdPackagingWasteTonnage()
        {
            return 0;
        }

        public static decimal GetReportedPublicBinTonnage()
        {
            return 0;
        }

        public static decimal GetReportedSelfManagedConsumerWasteTonnage()
        {
            return 0;
        }

        public static decimal GetNetReportedTonnage()
        {
            return 0;
        }

        public static decimal GetScaledupReportedHouseholdPackagingWasteTonnage()
        {
            return 0;
        }

        public static decimal GetScaledupReportedPublicBinTonnage()
        {
            return 0;
        }

        public static decimal GetScaledupTotalReportedTonnage()
        {
            return 0;
        }

        public static decimal GetScaledupReportedSelfManagedConsumerWasteTonnage()
        {
            return 0;
        }

        public static decimal GetScaledupNetReportedTonnage()
        {
            return 0;
        }
    }
}
