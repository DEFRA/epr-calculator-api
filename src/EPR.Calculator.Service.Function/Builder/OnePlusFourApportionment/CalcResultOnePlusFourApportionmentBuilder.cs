using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Builder.OnePlusFourApportionment
{
    public interface ICalcResultOnePlusFourApportionmentBuilder
    {
        CalcResultOnePlusFourApportionment Construct(CalcResult calcResult);
    }

    public class CalcResultOnePlusFourApportionmentBuilder : ICalcResultOnePlusFourApportionmentBuilder
    {
        public CalcResultOnePlusFourApportionment Construct(CalcResult calcResult)
        {
            return new CalcResultOnePlusFourApportionment {
                LaDisposalCost   = calcResult.CalcResultLapcapData.Total,
                LADataPrepCharge = calcResult.CalcResultParameterOtherCost.LaDataPrepCharge
            };
        }
    }
}
