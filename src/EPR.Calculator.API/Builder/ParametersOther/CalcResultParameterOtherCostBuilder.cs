using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder.ParametersOther
{
    public class CalcResultParameterOtherCostBuilder : ICalcResultParameterOtherCostBuilder
    {
        private const string SchemeAdminOperatingCost = "Scheme administrator operating costs";
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

            other.SaOperatingCost = GetOtherSaOperatingCost(schemeAdminCosts);

            return other;
        }

        private static IEnumerable<CalcResultParameterOtherCostDetail> GetOtherSaOperatingCost(IEnumerable<DefaultParamResultsClass> schemeAdminCosts)
        {
            var details = new List<CalcResultParameterOtherCostDetail>();

            var header = new CalcResultParameterOtherCostDetail
            {
                England = "England",
                Wales = "Wales",
                Scotland = "Scotland",
                NorthernIreland = "Northern Ireland"
            };
            details.Add(header);

            var saOperatingCost = new CalcResultParameterOtherCostDetail
            {
                Name = "3 SA Operating Costs",
                EnglandValue = schemeAdminCosts.Single(cost => cost.ParameterCategory == "England").ParameterValue,
                NorthernIrelandValue = schemeAdminCosts.Single(cost => cost.ParameterCategory == "Northern Ireland").ParameterValue,
                ScotlandValue = schemeAdminCosts.Single(cost => cost.ParameterCategory == "Scotland").ParameterValue,
                WalesValue = schemeAdminCosts.Single(cost => cost.ParameterCategory == "Wales").ParameterValue,
                OrderId = 2
            };
            saOperatingCost.TotalValue = saOperatingCost.EnglandValue + saOperatingCost.ScotlandValue + saOperatingCost.WalesValue + saOperatingCost.NorthernIrelandValue;
            details.Add(saOperatingCost);
            return details;
        }
    }
}
