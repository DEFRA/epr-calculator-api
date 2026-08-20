using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.Builder.OnePlusFourApportionment
{
    public interface ICalcResultOnePlusFourApportionmentBuilder
    {
        CalcResultOnePlusFourApportionment Construct(CalcResult calcResult);
    }

    public class CalcResultOnePlusFourApportionmentBuilder : ICalcResultOnePlusFourApportionmentBuilder
    {
        [ActivityTrace]
        public CalcResultOnePlusFourApportionment Construct(CalcResult calcResult)
        {
            return new CalcResultOnePlusFourApportionment {
                LaDisposalCost   = calcResult.CalcResultLapcapData.Total,
                LADataPrepCharge = calcResult.CalcResultParameterOtherCost.LaDataPrepCharge with { }
            };
        }
    }
}
