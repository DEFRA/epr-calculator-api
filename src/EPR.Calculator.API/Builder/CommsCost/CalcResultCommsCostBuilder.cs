using EPR.Calculator.API.Data;
using EPR.Calculator.API.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace EPR.Calculator.API.Builder.CommsCost
{
    public class CalcResultCommsCostBuilder(ApplicationDBContext context) : ICalcResultCommsCostBuilder
    {
        /// <summary>
        /// The key used to identify household records in the producer_reported_material table.
        /// </summary>
        private const string HouseHoldIndicator = "HH";

        public CalcResultCommsCost Construct(int runId, CalcResultOnePlusFourApportionment apportionment)
        {
            var culture = CultureInfo.CreateSpecificCulture("en-GB");
            culture.NumberFormat.CurrencySymbol = "£";
            culture.NumberFormat.CurrencyPositivePattern = 0;

            var apportionmentDetails = apportionment.CalcResultOnePlusFourApportionmentDetails;
            var apportionmentDetail = apportionmentDetails.Last();

            var result = new CalcResultCommsCost();
            CalculateApportionment(apportionmentDetail, result);
            result.Name = "Parameters - Comms Costs";

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
                    select new CalcCommsBuilderResult
                    {
                        ParameterValue = defaultDetail.ParameterValue,
                        ParameterType = defaultTemplate.ParameterType,
                        ParameterCategory = defaultTemplate.ParameterCategory
                    }).ToList();
            var materialDefaults = allDefaultResults.Where(x =>
                x.ParameterType == "Communication costs by material" && materialNames.Contains(x.ParameterCategory));


            var list = new List<CalcResultCommsCostCommsCostByMaterial>();

            var header = new CalcResultCommsCostCommsCostByMaterial()
            {
                Name = "2a Comms Costs - by Material",
                England = "England",
                Wales = "Wales",
                Scotland = "Scotland",
                NorthernIreland = "Northern Ireland",
                Total = "Total"
            };
            list.Add(header);

            foreach (var materialName in materialNames)
            {
                var commsCost = GetCommsCost(materialDefaults, materialName, apportionmentDetail, culture);
                list.Add(commsCost);
            }

            var totalRow = GetTotalRow(list, culture);

            list.Add(totalRow);
            result.CalcResultCommsCostCommsCostByMaterial = list;


            var commsCostByUk =
                allDefaultResults.Single(x =>
                    x.ParameterType == "Communication costs by country" && x.ParameterCategory == "United Kingdom");

            var ukCost = new CalcResultCommsCostOnePlusFourApportionment
            {
                EnglandValue = commsCostByUk.ParameterValue * apportionmentDetail.EnglandTotal,
                WalesValue = commsCostByUk.ParameterValue * apportionmentDetail.WalesTotal,
                ScotlandValue = commsCostByUk.ParameterValue * apportionmentDetail.ScotlandTotal,
                NorthernIrelandValue = commsCostByUk.ParameterValue * apportionmentDetail.NorthernIrelandTotal,
                TotalValue = commsCostByUk.ParameterValue * apportionmentDetail.AllTotal,
                Name = "2b Comms Costs - UK wide",
                OrderId = 2
            };


            var commsCostByCountryList = GetCommsCostByCountryList(ukCost, allDefaultResults, culture);

            result.CommsCostByCountry = commsCostByCountryList;

            return result;
        }

        private static CalcResultCommsCostCommsCostByMaterial GetTotalRow(IEnumerable<CalcResultCommsCostCommsCostByMaterial> list,
            CultureInfo culture)
        {
            var totalRow = new CalcResultCommsCostCommsCostByMaterial
            {
                EnglandValue = list.Sum(x => x.EnglandValue),
                WalesValue = list.Sum(x => x.WalesValue),
                NorthernIrelandValue = list.Sum(x => x.NorthernIrelandValue),
                ScotlandValue = list.Sum(x => x.ScotlandValue),
                TotalValue = list.Sum(x => x.TotalValue),
            };
            totalRow.Name = "Total";
            totalRow.England = $"{totalRow.EnglandValue.ToString("C", culture)}";
            totalRow.Wales = $"{totalRow.WalesValue.ToString("C", culture)}";
            totalRow.NorthernIreland = $"{totalRow.NorthernIrelandValue.ToString("C", culture)}";
            totalRow.Scotland = $"{totalRow.ScotlandValue.ToString("C", culture)}";

            totalRow.Total = $"{totalRow.TotalValue.ToString("C", culture)}";
            return totalRow;
        }

        private static CalcResultCommsCostCommsCostByMaterial GetCommsCost(IEnumerable<CalcCommsBuilderResult> materialDefaults, string materialName,
            CalcResultOnePlusFourApportionmentDetail apportionmentDetail, CultureInfo culture)
        {
            var materialDefault = materialDefaults.Single(m => m.ParameterCategory == materialName);
            var commsCost = new CalcResultCommsCostCommsCostByMaterial
            {
                EnglandValue = apportionmentDetail.EnglandTotal * materialDefault.ParameterValue,
                WalesValue = apportionmentDetail.WalesTotal * materialDefault.ParameterValue,
                NorthernIrelandValue = apportionmentDetail.NorthernIrelandTotal * materialDefault.ParameterValue,
                ScotlandValue = apportionmentDetail.ScotlandTotal * materialDefault.ParameterValue,
                Name = materialDefault.ParameterCategory,
            };
            commsCost.England = $"{commsCost.EnglandValue.ToString("C", culture)}";
            commsCost.Wales = $"{commsCost.WalesValue.ToString("C", culture)}";
            commsCost.NorthernIreland = $"{commsCost.NorthernIrelandValue.ToString("C", culture)}";
            commsCost.Scotland = $"{commsCost.ScotlandValue.ToString("C", culture)}";

            commsCost.TotalValue = commsCost.EnglandValue + commsCost.WalesValue + commsCost.NorthernIrelandValue +
                                   commsCost.ScotlandValue;
            commsCost.Total = $"{commsCost.TotalValue.ToString("C", culture)}";
            return commsCost;
        }

        private static List<CalcResultCommsCostOnePlusFourApportionment> GetCommsCostByCountryList(CalcResultCommsCostOnePlusFourApportionment ukCost,
            IEnumerable<CalcCommsBuilderResult> allDefaultResults, CultureInfo culture)
        {
            var commsCostByCountryList = new List<CalcResultCommsCostOnePlusFourApportionment>();
            commsCostByCountryList.Add(new CalcResultCommsCostCommsCostByMaterial()
            {
                England = "England",
                Wales = "Wales",
                Scotland = "Scotland",
                NorthernIreland = "Northern Ireland",
                Total = "Total",
                OrderId = 1
            });
            commsCostByCountryList.Add(ukCost);

            var englandValue =
                allDefaultResults.Single(x =>
                    x.ParameterType == "Communication costs by country" && x.ParameterCategory == "England").ParameterValue;
            var walesValue =
                allDefaultResults.Single(x =>
                    x.ParameterType == "Communication costs by country" && x.ParameterCategory == "Wales").ParameterValue;
            var niValue =
                allDefaultResults.Single(x =>
                    x.ParameterType == "Communication costs by country" && x.ParameterCategory == "Northern Ireland").ParameterValue;
            var scotlandValue =
                allDefaultResults.Single(x =>
                    x.ParameterType == "Communication costs by country" && x.ParameterCategory == "Scotland").ParameterValue;

            var countryCost = new CalcResultCommsCostOnePlusFourApportionment
            {
                EnglandValue = englandValue,
                WalesValue = walesValue,
                ScotlandValue = scotlandValue,
                NorthernIrelandValue = niValue,
                TotalValue = englandValue + walesValue + scotlandValue + niValue,
                Name = "2c Comms Costs - by Country",
                OrderId = 3
            };

            commsCostByCountryList.Add(countryCost);

            foreach (var calcResultCountry in commsCostByCountryList.Where(x => x.OrderId != 1))
            {
                calcResultCountry.England = $"{calcResultCountry.EnglandValue.ToString("C", culture)}";
                calcResultCountry.Wales = $"{calcResultCountry.WalesValue.ToString("C", culture)}";
                calcResultCountry.NorthernIreland = $"{calcResultCountry.NorthernIrelandValue.ToString("C", culture)}";
                calcResultCountry.Scotland = $"{calcResultCountry.ScotlandValue.ToString("C", culture)}";

                calcResultCountry.Total = $"{calcResultCountry.TotalValue.ToString("C", culture)}";
            }

            return commsCostByCountryList;
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
                Total = "Total"
            };

            var commsApportionments = new List<CalcResultCommsCostOnePlusFourApportionment>();
            commsApportionments.Add(commsApportionmentHeader);

            var commsApportionment = new CalcResultCommsCostOnePlusFourApportionment
            {
                Name = "1 + 4 Apportionment %s",
                England = apportionmentDetail.EnglandDisposalTotal,
                Wales = apportionmentDetail.WalesDisposalTotal,
                Scotland = apportionmentDetail.ScotlandDisposalTotal,
                NorthernIreland = apportionmentDetail.NorthernIrelandDisposalTotal,
                Total = apportionmentDetail.Total,
            };

            commsApportionments.Add(commsApportionment);

            result.CalcResultCommsCostOnePlusFourApportionment = commsApportionments;
        }
    }
}