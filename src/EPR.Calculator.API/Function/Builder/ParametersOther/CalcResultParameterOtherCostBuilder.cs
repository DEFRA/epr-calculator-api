using EPR.Calculator.API.Data;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Builder.ParametersOther
{
    public interface ICalcResultParameterOtherCostBuilder
    {
        Task<CalcResultParameterOtherCost> ConstructAsync(CalcResultsRequestDto resultsRequestDto);
    }

    public class CalcResultParameterOtherCostBuilder : ICalcResultParameterOtherCostBuilder
    {
        public const string SchemeAdminOperatingCost = "Scheme administrator operating costs";
        public const string LaPrepCharge = "Local authority data preparation costs";
        public const string SchemeSetupCost = "Scheme setup costs";
        public const string BadDebtProvision = "Bad debt provision";

        private readonly ApplicationDBContext dbContext;
        private readonly ICalcCountryApportionmentService calcCountryApportionmentService;

        public CalcResultParameterOtherCostBuilder(
            ApplicationDBContext dbContext,
            ICalcCountryApportionmentService calcCountryApportionmentService)
        {
            this.dbContext = dbContext;
            this.calcCountryApportionmentService = calcCountryApportionmentService;
        }

        public async Task<CalcResultParameterOtherCost> ConstructAsync(CalcResultsRequestDto resultsRequestDto)
        {
            //TODO inject params
            var results = await (
                from run in dbContext.CalculatorRuns
                join defaultMaster in dbContext.DefaultParameterSettings on run.DefaultParameterSettingMasterId equals defaultMaster.Id
                join defaultDetail in dbContext.DefaultParameterSettingDetail on defaultMaster.Id equals defaultDetail.DefaultParameterSettingMasterId
                join defaultTemplate in dbContext.DefaultParameterTemplateMasterList on defaultDetail.ParameterUniqueReferenceId equals defaultTemplate.ParameterUniqueReferenceId
                where run.Id == resultsRequestDto.RunId
                select new DefaultParamResultsClass
                {
                    ParameterValue = defaultDetail.ParameterValue,
                    ParameterCategory = defaultTemplate.ParameterCategory,
                    ParameterType = defaultTemplate.ParameterType,
                    ParameterUniqueReference = defaultDetail.ParameterUniqueReferenceId
                }
            ).ToListAsync();

            var schemeAdminCosts = results.Where(x => x.ParameterType == SchemeAdminOperatingCost);

            var saOperatingCost = GetPrepCharge(schemeAdminCosts);

            var lapPrepCharges = results.Where(x => x.ParameterType == LaPrepCharge);
            var laDataPrep = GetPrepCharge(lapPrepCharges);
            var countryApportionment = GetCountryApportionment(laDataPrep);

            var schemeSetUpCharges = results.Where(x => x.ParameterType == SchemeSetupCost);
            var schemeSetupCharge = GetPrepCharge(schemeSetUpCharges);

            var badDebtValue = results.Single(x => x.ParameterType == BadDebtProvision).ParameterValue;

            var materialityResults = results.Where(x => x.ParameterType == "Materiality threshold");

            var amountIncrease = materialityResults.Single(x => x.ParameterCategory == "Amount Increase");
            var amountDecrease = materialityResults.Single(x => x.ParameterCategory == "Amount Decrease");
            var percentageDecrease = materialityResults.Single(x => x.ParameterCategory == "Percent Decrease");
            var percentageIncrease = materialityResults.Single(x => x.ParameterCategory == "Percent Increase");

            var tonnageResults = results.Where(x => x.ParameterType == "Tonnage change threshold");
            var tonIncrease = tonnageResults.Single(x => x.ParameterCategory == "Amount Increase");
            var tonDecrease = tonnageResults.Single(x => x.ParameterCategory == "Amount Decrease");
            var tonPercentageDecrease = tonnageResults.Single(x => x.ParameterCategory == "Percent Decrease");
            var tonPercentageIncrease = tonnageResults.Single(x => x.ParameterCategory == "Percent Increase");

            var countries = await dbContext.Country.ToListAsync();

            var costType = await dbContext.CostType.SingleAsync(x => x.Name == "LA Data Prep Charge");
            var costTypeId = costType.Id;

            if (!resultsRequestDto.IsBillingFile)
            {
                await calcCountryApportionmentService.SaveChangesAsync(new CalcCountryApportionmentServiceDto
                {
                    RunId               = resultsRequestDto.RunId,
                    Countries           = countries,
                    CostTypeId          = costTypeId,
                    EnglandCost         = laDataPrep.England,
                    NorthernIrelandCost = laDataPrep.NorthernIreland,
                    ScotlandCost        = laDataPrep.Scotland,
                    WalesCost           = laDataPrep.Wales
                });
            }

            return new CalcResultParameterOtherCost
            {
                LaDataPrepCharge      = laDataPrep,
                CountryApportionment  = countryApportionment,
                SaOperatingCost       = saOperatingCost,
                SchemeSetupCost       = schemeSetupCharge,
                BadDebtValue          = badDebtValue,
                MaterialityIncrease   = new Materiality { Amount = amountIncrease.ParameterValue, Percentage = percentageIncrease.ParameterValue },
                MaterialityDecrease   = new Materiality { Amount = amountDecrease.ParameterValue, Percentage = percentageDecrease.ParameterValue },
                TonnageChangeIncrease = new Materiality { Amount = tonIncrease.ParameterValue   , Percentage = tonPercentageIncrease.ParameterValue },
                TonnageChangeDecrease = new Materiality { Amount = tonDecrease.ParameterValue   , Percentage = tonPercentageDecrease.ParameterValue }
            };
        }

        private static ByCountryApportionment GetCountryApportionment(ByCountryCost laDataPrep)
        {
            var total = laDataPrep.England + laDataPrep.NorthernIreland + laDataPrep.Wales + laDataPrep.Scotland;
            return new ByCountryApportionment
            {
                England         = total != 0 ? (laDataPrep.England         / total) * 100 : 0M,
                NorthernIreland = total != 0 ? (laDataPrep.NorthernIreland / total) * 100 : 0M,
                Scotland        = total != 0 ? (laDataPrep.Scotland        / total) * 100 : 0M,
                Wales           = total != 0 ? (laDataPrep.Wales           / total) * 100 : 0M
            };
        }

        private static ByCountryCost GetPrepCharge(IEnumerable<DefaultParamResultsClass> lapPrepCharges)
        {
            var e  = lapPrepCharges.Single(cost => cost.ParameterCategory == "England"         ).ParameterValue;
            var w  = lapPrepCharges.Single(cost => cost.ParameterCategory == "Wales"           ).ParameterValue;
            var s  = lapPrepCharges.Single(cost => cost.ParameterCategory == "Scotland"        ).ParameterValue;
            var ni = lapPrepCharges.Single(cost => cost.ParameterCategory == "Northern Ireland").ParameterValue;
            return new ByCountryCost
            {
                England         = e,
                Wales           = w,
                Scotland        = s,
                NorthernIreland = ni
            };
        }
    }
}
