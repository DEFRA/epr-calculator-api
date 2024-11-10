using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder.ParametersOther
{
    public class CalcResultParameterOtherCostBuilder : ICalcResultParameterOtherCostBuilder
    {
        private const string SchemeAdminOperatingCost = "Scheme administrator operating costs";
        private const string LaPrepCharge = "Local authority data preparation costs";
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

            saDetails.Add(GetLaPrepCharge("3 SA Operating Costs", 2, schemeAdminCosts));
            other.SaOperatingCost = saDetails;

            var lapPrepCharges = results.Where(x => x.ParameterType == LaPrepCharge);
            var laDataPrepCharges = new List<CalcResultParameterOtherCostDetail>();
            laDataPrepCharges.Add(GetLaPrepCharge("4 LA Data Prep Charge", 1, lapPrepCharges));
            other.Details = laDataPrepCharges;

            return other;
        }

        private static CalcResultParameterOtherCostDetail GetLaPrepCharge(string name, int orderId, IEnumerable<DefaultParamResultsClass> lapPrepCharges)
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
