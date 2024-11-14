using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.Builder.CommsCost
{
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Models;

    public class CalcResultCommsCostBuilder(ApplicationDBContext context) : ICalcResultCommsCostBuilder
    {
        /// <summary>
        /// The key used to identify household records in the producer_reported_material table.
        /// </summary>
        private const string HouseHoldIndicator = "HH";

        public CalcResultCommsCost Construct(int runId, CalcResultOnePlusFourApportionment apportionment)
        {
            var apportionmentDetails = apportionment.CalcResultOnePlusFourApportionmentDetails;
            var apportionmentDetail = apportionmentDetails.Last();

            var result = new CalcResultCommsCost();
            CalculateApportionment(apportionmentDetail, result);

            var materialNames = context.Material.Select(x => x.Name).ToList();

            var allDefaultResults =
                (from run in context.CalculatorRuns
                    join defaultMaster in context.DefaultParameterSettings on run.DefaultParameterSettingMasterId equals
                        defaultMaster.Id
                    join defaultDetail in context.DefaultParameterSettingDetail on defaultMaster.Id equals defaultDetail
                        .DefaultParameterSettingMasterId
                    join defaultTemplate in context.DefaultParameterTemplateMasterList on defaultDetail
                        .ParameterUniqueReferenceId equals defaultTemplate.ParameterUniqueReferenceId
                    where run.Id == runId
                    select new
                    {
                        defaultDetail.ParameterValue,
                        defaultTemplate.ParameterType,
                        defaultTemplate.ParameterCategory
                    });
            var materialDefaults = allDefaultResults.Where(x =>
                x.ParameterType == "Communication costs by material" && materialNames.Contains(x.ParameterCategory));

            var list = new List<CalcResultCommsCostCommsCostByMaterial>();

            foreach (var materialDefault in materialDefaults)
            {
                var commsCost = new CalcResultCommsCostCommsCostByMaterial
                {
                    EnglandValue = apportionmentDetail.EnglandTotal * materialDefault.ParameterValue,
                    WalesValue = apportionmentDetail.WalesTotal * materialDefault.ParameterValue,
                    NorthernIrelandValue = apportionmentDetail.NorthernIrelandTotal * materialDefault.ParameterValue,
                    ScotlandValue = apportionmentDetail.ScotlandTotal * materialDefault.ParameterValue,
                    Name = materialDefault.ParameterCategory,
                    TotalValue = apportionmentDetail.AllTotal * materialDefault.ParameterValue
                };
                list.Add(commsCost);
            }

            new CalcResultCommsCostCommsCostByMaterial
            {
                EnglandValue = list.Sum(x => x.EnglandValue),
                WalesValue = list.Sum(x => x.WalesValue),
                NorthernIrelandValue = list.Sum(x => x.NorthernIrelandValue),
                ScotlandValue = list.Sum(x => x.ScotlandValue),
                TotalValue = list.Sum(x => x.TotalValue),
            };
            result.CalcResultCommsCostCommsCostByMaterial = list;

            return result;
        }

        private static void CalculateApportionment(CalcResultOnePlusFourApportionmentDetail apportionmentDetail,
            CalcResultCommsCost result)
        {
            var commsApportionmentHeader = new CalcResultCommsCostOnePlusFourApportionment
            {
                England = "England",
                Wales = "Wales",
                Scotland = "Scotland",
                NorthernIreland = "Northern Ireland",
            };

            var commsApportionments = new List<CalcResultCommsCostOnePlusFourApportionment>();
            commsApportionments.Add(commsApportionmentHeader);

            var commsApportionment = new CalcResultCommsCostOnePlusFourApportionment
            {
                England = apportionmentDetail.EnglandDisposalTotal,
                Wales = apportionmentDetail.WalesDisposalTotal,
                Scotland = apportionmentDetail.ScotlandDisposalTotal,
                NorthernIreland = apportionmentDetail.NorthernIrelandDisposalTotal
            };
            commsApportionments.Add(commsApportionment);

            result.CalcResultCommsCostOnePlusFourApportionment = commsApportionments;
        }

    }
}