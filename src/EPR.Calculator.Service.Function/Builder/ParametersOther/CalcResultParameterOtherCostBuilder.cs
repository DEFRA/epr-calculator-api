using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Services;
using Microsoft.EntityFrameworkCore;


namespace EPR.Calculator.Service.Function.Builder.ParametersOther
{
    public interface ICalcResultParameterOtherCostBuilder
    {
        Task<CalcResultParameterOtherCost> ConstructAsync(RunContext runContext);
    }

    public class CalcResultParameterOtherCostBuilder(
        ApplicationDBContext dbContext,
        ICalcCountryApportionmentService calcCountryApportionmentService)
        : ICalcResultParameterOtherCostBuilder
    {
        public async Task<CalcResultParameterOtherCost> ConstructAsync(RunContext runContext)
        {
            var dp        = runContext.DefaultParameters;
            var countries = await dbContext.Country.ToListAsync();
            var costType  = await dbContext.CostType.SingleAsync(x => x.Name == "LA Data Prep Charge");

            if (runContext.RunType == RunType.Calculator)
            {
                await calcCountryApportionmentService.SaveChangesAsync(new CalcCountryApportionmentServiceDto
                {
                    RunId               = runContext.RunId,
                    Countries           = countries,
                    CostTypeId          = costType.Id,
                    EnglandCost         = dp.LocalAuthorityDataPreparationCostsByCountry.England,
                    NorthernIrelandCost = dp.LocalAuthorityDataPreparationCostsByCountry.NorthernIreland,
                    ScotlandCost        = dp.LocalAuthorityDataPreparationCostsByCountry.Scotland,
                    WalesCost           = dp.LocalAuthorityDataPreparationCostsByCountry.Wales
                });
            }

            return new CalcResultParameterOtherCost
            {
                LaDataPrepCharge      = dp.LocalAuthorityDataPreparationCostsByCountry,
                CountryApportionment  = GetCountryApportionment(dp.LocalAuthorityDataPreparationCostsByCountry),
                SaOperatingCost       = dp.SchemeAdministratorOperatingCostsByCountry,
                SchemeSetupCost       = dp.SchemeSetupCostsByCountry,
                BadDebtValue          = dp.BadDebtProvision,
                MaterialityIncrease   = new() { Amount = dp.MaterialityThreshold.AmountIncrease  , Percentage = dp.MaterialityThreshold.PercentIncrease   },
                MaterialityDecrease   = new() { Amount = dp.MaterialityThreshold.AmountDecrease  , Percentage = dp.MaterialityThreshold.PercentDecrease   },
                TonnageChangeIncrease = new() { Amount = dp.TonnageChangeThreshold.AmountIncrease, Percentage = dp.TonnageChangeThreshold.PercentIncrease },
                TonnageChangeDecrease = new() { Amount = dp.TonnageChangeThreshold.AmountDecrease, Percentage = dp.TonnageChangeThreshold.PercentDecrease },
                CutOffDate            = dp.CutOffDate
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
    }
}
