using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;
using System.Globalization;

namespace EPR.Calculator.API.Builder.CommsCost
{
    public class CalcResultCommsCostBuilder(ApplicationDBContext context) : ICalcResultCommsCostBuilder
    {
        public const string Header = "Parameters - Comms Costs";
        public const string CommunicationCostByMaterial = "Communication costs by material";
        public const string LateReportingTonnage = "Late reporting tonnage";
        public const string CommunicationCostByCountry = "Communication costs by country";
        public const string TwoCCommsCostByCountry = "2c Comms Costs - by Country";
        public const string Uk = "United Kingdom";
        public const string TwoBCommsCostUkWide = "2b Comms Costs - UK wide";
        public const string OnePlusFourApportionment = "1 + 4 Apportionment %s";
        public const string CurrencyFormat = "C";
        public const string EnGb = "en-GB";
        public const string PoundSign = "£";

        public CalcResultCommsCost Construct(CalcResultsRequestDto resultsRequestDto,
            CalcResultOnePlusFourApportionment apportionment)
        {
            var runId = resultsRequestDto.RunId;
            var culture = CultureInfo.CreateSpecificCulture(EnGb);
            culture.NumberFormat.CurrencySymbol = PoundSign;
            culture.NumberFormat.CurrencyPositivePattern = 0;

            var apportionmentDetails = apportionment.CalcResultOnePlusFourApportionmentDetails;
            var apportionmentDetail = apportionmentDetails.Last();

            var result = new CalcResultCommsCost();
            CalculateApportionment(apportionmentDetail, result);
            result.Name = Header;

            var materials = context.Material.ToList();
            var materialNames = materials.Select(x => x.Name).ToList();

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
                x.ParameterType == CommunicationCostByMaterial && materialNames.Contains(x.ParameterCategory));

            var producerReportedMaterials = (from run in context.CalculatorRuns
                join pd in context.ProducerDetail on run.Id equals pd.CalculatorRunId
                join mat in context.ProducerReportedMaterial on pd.Id equals mat.ProducerDetailId
                where run.Id == runId && mat.PackagingType == CommonConstants.Household
                select new
                {
                    mat.Id,
                    mat.MaterialId,
                    mat.PackagingTonnage
                }).Distinct().ToList();


            var list = new List<CalcResultCommsCostCommsCostByMaterial>();

            var header = new CalcResultCommsCostCommsCostByMaterial
            {
                Name = CommsCostByMaterialHeaderConstant.Name,
                England = CommsCostByMaterialHeaderConstant.England,
                Wales = CommsCostByMaterialHeaderConstant.Wales,
                Scotland = CommsCostByMaterialHeaderConstant.Scotland,
                NorthernIreland = CommsCostByMaterialHeaderConstant.NorthernIreland,
                Total = CommsCostByMaterialHeaderConstant.Total,
                ProducerReportedHouseholdPackagingWasteTonnage =
                    CommsCostByMaterialHeaderConstant.ProducerReportedHouseholdPackagingWasteTonnage,
                LateReportingTonnage = CommsCostByMaterialHeaderConstant.LateReportingTonnage,
                ProducerReportedHouseholdPlusLateReportingTonnage = CommsCostByMaterialHeaderConstant
                    .ProducerReportedHouseholdPlusLateReportingTonnage,
                CommsCostByMaterialPricePerTonne = CommsCostByMaterialHeaderConstant.CommsCostByMaterialPricePerTonne
            };
            list.Add(header);

            foreach (var materialName in materialNames)
            {
                var commsCost = GetCommsCost(materialDefaults, materialName, apportionmentDetail, culture);
                var currentMaterial = materials.Single(x => x.Name == materialName);
                var producerReportedTon = producerReportedMaterials.Where(x => x.MaterialId == currentMaterial.Id)
                    .Sum(x => x.PackagingTonnage);
                var lateReportingTonnage = allDefaultResults.Single(x =>
                    x.ParameterType == LateReportingTonnage && x.ParameterCategory == materialName);

                commsCost.ProducerReportedHouseholdPackagingWasteTonnageValue = producerReportedTon;
                commsCost.LateReportingTonnageValue = lateReportingTonnage.ParameterValue;
                commsCost.ProducerReportedHouseholdPlusLateReportingTonnageValue =
                    commsCost.ProducerReportedHouseholdPackagingWasteTonnageValue +
                    commsCost.LateReportingTonnageValue;
                commsCost.CommsCostByMaterialPricePerTonneValue =
                    commsCost.TotalValue / commsCost.ProducerReportedHouseholdPlusLateReportingTonnageValue;

                commsCost.ProducerReportedHouseholdPackagingWasteTonnage =
                    $"{commsCost.ProducerReportedHouseholdPackagingWasteTonnageValue:0.000}";
                commsCost.LateReportingTonnage = $"{commsCost.LateReportingTonnageValue:0.000}";
                commsCost.ProducerReportedHouseholdPlusLateReportingTonnage =
                    $"{commsCost.ProducerReportedHouseholdPlusLateReportingTonnageValue:0.000}";
                commsCost.CommsCostByMaterialPricePerTonne =
                    $"{commsCost.CommsCostByMaterialPricePerTonneValue:0.0000}";

                list.Add(commsCost);
            }

            var totalRow = GetTotalRow(list, culture);

            list.Add(totalRow);
            result.CalcResultCommsCostCommsCostByMaterial = list;


            var commsCostByUk =
                allDefaultResults.Single(x =>
                    x.ParameterType == CommunicationCostByCountry && x.ParameterCategory == Uk);

            var ukCost = new CalcResultCommsCostOnePlusFourApportionment
            {
                EnglandValue = commsCostByUk.ParameterValue * apportionmentDetail.EnglandTotal,
                WalesValue = commsCostByUk.ParameterValue * apportionmentDetail.WalesTotal,
                ScotlandValue = commsCostByUk.ParameterValue * apportionmentDetail.ScotlandTotal,
                NorthernIrelandValue = commsCostByUk.ParameterValue * apportionmentDetail.NorthernIrelandTotal,
                TotalValue = commsCostByUk.ParameterValue * apportionmentDetail.AllTotal,
                Name = TwoBCommsCostUkWide,
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
                ProducerReportedHouseholdPackagingWasteTonnageValue = list.Sum(x => x.ProducerReportedHouseholdPackagingWasteTonnageValue),
                ProducerReportedHouseholdPlusLateReportingTonnageValue = list.Sum(x => x.ProducerReportedHouseholdPlusLateReportingTonnageValue),
                LateReportingTonnageValue = list.Sum(x => x.LateReportingTonnageValue),
            };
            totalRow.Name = CommsCostByMaterialHeaderConstant.Total;
            totalRow.England = $"{totalRow.EnglandValue.ToString(CurrencyFormat, culture)}";
            totalRow.Wales = $"{totalRow.WalesValue.ToString(CurrencyFormat, culture)}";
            totalRow.NorthernIreland = $"{totalRow.NorthernIrelandValue.ToString(CurrencyFormat, culture)}";
            totalRow.Scotland = $"{totalRow.ScotlandValue.ToString(CurrencyFormat, culture)}";

            totalRow.ProducerReportedHouseholdPackagingWasteTonnage = $"{totalRow.ProducerReportedHouseholdPackagingWasteTonnageValue:0.000}";
            totalRow.LateReportingTonnage = $"{totalRow.LateReportingTonnageValue:0.000}";
            totalRow.ProducerReportedHouseholdPlusLateReportingTonnage = $"{totalRow.ProducerReportedHouseholdPlusLateReportingTonnageValue:0.000}";

            totalRow.Total = $"{totalRow.TotalValue.ToString(CurrencyFormat, culture)}";
            return totalRow;
        }

        private static CalcResultCommsCostCommsCostByMaterial GetCommsCost(
            IEnumerable<CalcCommsBuilderResult> materialDefaults, string materialName,
            CalcResultOnePlusFourApportionmentDetail apportionmentDetail, CultureInfo culture)
        {
            var materialDefault = materialDefaults.Single(m => m.ParameterCategory == materialName);
            var commsCost = new CalcResultCommsCostCommsCostByMaterial();
            commsCost.TotalValue = Math.Round(materialDefault.ParameterValue, 2);
            commsCost.EnglandValue = apportionmentDetail.EnglandTotal * commsCost.TotalValue / 100;
            commsCost.WalesValue = apportionmentDetail.WalesTotal * commsCost.TotalValue / 100;
            commsCost.NorthernIrelandValue =
                apportionmentDetail.NorthernIrelandTotal * commsCost.TotalValue / 100;
            commsCost.ScotlandValue = apportionmentDetail.ScotlandTotal * commsCost.TotalValue / 100;
            commsCost.Name = materialDefault.ParameterCategory;
            commsCost.England = $"{Math.Round(commsCost.EnglandValue, 2).ToString(CurrencyFormat, culture)}";
            commsCost.Wales = $"{Math.Round(commsCost.WalesValue, 2).ToString(CurrencyFormat, culture)}";
            commsCost.NorthernIreland =
                $"{Math.Round(commsCost.NorthernIrelandValue, 2).ToString(CurrencyFormat, culture)}";
            commsCost.Scotland = $"{Math.Round(commsCost.ScotlandValue, 2).ToString(CurrencyFormat, culture)}";


            commsCost.Total = $"{commsCost.TotalValue.ToString(CurrencyFormat, culture)}";
            return commsCost;
        }

        private static List<CalcResultCommsCostOnePlusFourApportionment> GetCommsCostByCountryList(
            CalcResultCommsCostOnePlusFourApportionment ukCost,
            IEnumerable<CalcCommsBuilderResult> allDefaultResults, CultureInfo culture)
        {
            var commsCostByCountryList = new List<CalcResultCommsCostOnePlusFourApportionment>();
            commsCostByCountryList.Add(new CalcResultCommsCostCommsCostByMaterial()
            {
                England = CommsCostByMaterialHeaderConstant.England,
                Wales = CommsCostByMaterialHeaderConstant.Wales,
                Scotland = CommsCostByMaterialHeaderConstant.Scotland,
                NorthernIreland = CommsCostByMaterialHeaderConstant.NorthernIreland,
                Total = CommsCostByMaterialHeaderConstant.Total,
                OrderId = 1
            });
            commsCostByCountryList.Add(ukCost);

            var englandValue =
                allDefaultResults.Single(x =>
                    x.ParameterType == CommunicationCostByCountry &&
                    x.ParameterCategory == CommsCostByMaterialHeaderConstant.England).ParameterValue;
            var walesValue =
                allDefaultResults.Single(x =>
                    x.ParameterType == CommunicationCostByCountry &&
                    x.ParameterCategory == CommsCostByMaterialHeaderConstant.Wales).ParameterValue;
            var niValue =
                allDefaultResults.Single(x =>
                    x.ParameterType == CommunicationCostByCountry &&
                    x.ParameterCategory == CommsCostByMaterialHeaderConstant.NorthernIreland).ParameterValue;
            var scotlandValue =
                allDefaultResults.Single(x =>
                    x.ParameterType == CommunicationCostByCountry &&
                    x.ParameterCategory == CommsCostByMaterialHeaderConstant.Scotland).ParameterValue;

            var countryCost = new CalcResultCommsCostOnePlusFourApportionment
            {
                EnglandValue = englandValue,
                WalesValue = walesValue,
                ScotlandValue = scotlandValue,
                NorthernIrelandValue = niValue,
                TotalValue = englandValue + walesValue + scotlandValue + niValue,
                Name = TwoCCommsCostByCountry,
                OrderId = 3
            };

            commsCostByCountryList.Add(countryCost);

            foreach (var calcResultCountry in commsCostByCountryList.Where(x => x.OrderId != 1))
            {
                calcResultCountry.England = $"{calcResultCountry.EnglandValue.ToString(CurrencyFormat, culture)}";
                calcResultCountry.Wales = $"{calcResultCountry.WalesValue.ToString(CurrencyFormat, culture)}";
                calcResultCountry.NorthernIreland = $"{calcResultCountry.NorthernIrelandValue.ToString(CurrencyFormat, culture)}";
                calcResultCountry.Scotland = $"{calcResultCountry.ScotlandValue.ToString(CurrencyFormat, culture)}";

                calcResultCountry.Total = $"{calcResultCountry.TotalValue.ToString(CurrencyFormat, culture)}";
            }

            return commsCostByCountryList;
        }

        private static void CalculateApportionment(CalcResultOnePlusFourApportionmentDetail apportionmentDetail,
            CalcResultCommsCost result)
        {
            var commsApportionmentHeader = new CalcResultCommsCostOnePlusFourApportionment
            {
                England = CommsCostByMaterialHeaderConstant.England,
                Wales = CommsCostByMaterialHeaderConstant.Wales,
                Scotland = CommsCostByMaterialHeaderConstant.Scotland,
                NorthernIreland = CommsCostByMaterialHeaderConstant.NorthernIreland,
                Total = CommsCostByMaterialHeaderConstant.Total
            };

            var commsApportionments = new List<CalcResultCommsCostOnePlusFourApportionment> { commsApportionmentHeader };

            var commsApportionment = new CalcResultCommsCostOnePlusFourApportionment
            {
                Name = OnePlusFourApportionment,
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