using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder.ParametersOther
{
    public class CalcResultParameterOtherCostBuilder : ICalcResultParameterOtherCostBuilder
    {
        private const string SchemeAdminOperatingCost = "Scheme administrator operating costs";
        private const string LaPrepCharge = "Local authority data preparation costs";
        private const string SaOperatingCostHeader = "3 SA Operating Costs";
        private const string LaDataPrepChargeHeader = "4 LA Data Prep Charge";
        private const string SchemeSetupCost = "Scheme setup costs";
        private const string SchemeSetupYearlyCostHeader = "5 Scheme set up cost Yearly Cost";
        private const string BadDebtProvision = "Bad debt provision";
        private const string BadDebtProvisionHeader = "6 Bad debt provision";
        private readonly ApplicationDBContext context;
        public CalcResultParameterOtherCostBuilder(ApplicationDBContext context) 
        {
            this.context = context;
        }

        public CalcResultParameterOtherCost Construct(CalcResultsRequestDto resultsRequestDto)
        {
            var results = (from run in context.CalculatorRuns
                           join defaultMaster in context.DefaultParameterSettings on run.DefaultParameterSettingMasterId equals defaultMaster.Id
                           join defaultDetail in context.DefaultParameterSettingDetail on defaultMaster.Id equals defaultDetail.DefaultParameterSettingMasterId
                           join defaultTemplate in context.DefaultParameterTemplateMasterList on defaultDetail.ParameterUniqueReferenceId equals defaultTemplate.ParameterUniqueReferenceId
                           where run.Id == resultsRequestDto.RunId
                           select new DefaultParamResultsClass
                           {
                               ParameterValue = defaultDetail.ParameterValue,
                               ParameterCategory = defaultTemplate.ParameterCategory,
                               ParameterType = defaultTemplate.ParameterType,
                           }).ToList();

            var schemeAdminCosts = results.Where(x => x.ParameterCategory == SchemeAdminOperatingCost);

            var other = new CalcResultParameterOtherCost();

            var saDetails = new List<CalcResultParameterOtherCostDetail>();
            var header = new CalcResultParameterOtherCostDetail
            {
                England = "England",
                Wales = "Wales",
                Scotland = "Scotland",
                NorthernIreland = "Northern Ireland"
            };
            saDetails.Add(header);

            saDetails.Add(GetPrepCharge(SaOperatingCostHeader, 2, schemeAdminCosts));
            other.SaOperatingCost = saDetails;

            var lapPrepCharges = results.Where(x => x.ParameterType == LaPrepCharge);
            var laDataPrepCharges = new List<CalcResultParameterOtherCostDetail>();
            var laDataPrep = GetPrepCharge(LaDataPrepChargeHeader, 1, lapPrepCharges);
            laDataPrepCharges.Add(laDataPrep);
            laDataPrepCharges.Add(GetCountryApportionment(laDataPrep));
            other.Details = laDataPrepCharges;

            var schemeSetUpCharges = results.Where(x => x.ParameterType == SchemeSetupCost);
            var schemeSetupCharge = GetPrepCharge(SchemeSetupYearlyCostHeader, 1, lapPrepCharges);
            other.SchemeSetupCost = schemeSetupCharge;

            var badDebtValue = results.Single(x => x.ParameterType == BadDebtProvision).ParameterValue;
            other.BadDebtProvision = new KeyValuePair<string, string> (BadDebtProvisionHeader, $"{badDebtValue}%");

            return other;
        }

        private static CalcResultParameterOtherCostDetail GetCountryApportionment(CalcResultParameterOtherCostDetail laDataPrep)
        {
            var otherCostDetail = new CalcResultParameterOtherCostDetail
            {
                Name = "4 Country Apportionment",
                EnglandValue = (laDataPrep.EnglandValue / laDataPrep.TotalValue) * 100,
                NorthernIrelandValue = (laDataPrep.NorthernIrelandValue / laDataPrep.TotalValue) * 100,
                ScotlandValue = (laDataPrep.ScotlandValue / laDataPrep.TotalValue) * 100,
                WalesValue = (laDataPrep.WalesValue / laDataPrep.TotalValue) * 100,
                OrderId = 2,
                TotalValue = 100M
            };
            
            return otherCostDetail;
        }

        private static CalcResultParameterOtherCostDetail GetPrepCharge(string name, int orderId, IEnumerable<DefaultParamResultsClass> lapPrepCharges)
        {
            var otherCostDetail = new CalcResultParameterOtherCostDetail
            {
                Name = name,
                EnglandValue = lapPrepCharges.Single(cost => cost.ParameterCategory == "England").ParameterValue,
                NorthernIrelandValue = lapPrepCharges.Single(cost => cost.ParameterCategory == "Northern Ireland").ParameterValue,
                ScotlandValue = lapPrepCharges.Single(cost => cost.ParameterCategory == "Scotland").ParameterValue,
                WalesValue = lapPrepCharges.Single(cost => cost.ParameterCategory == "Wales").ParameterValue,
                OrderId = orderId
            };
            otherCostDetail.TotalValue = otherCostDetail.EnglandValue + otherCostDetail.ScotlandValue + otherCostDetail.WalesValue + otherCostDetail.NorthernIrelandValue;
            return otherCostDetail;
        }
    }
}
