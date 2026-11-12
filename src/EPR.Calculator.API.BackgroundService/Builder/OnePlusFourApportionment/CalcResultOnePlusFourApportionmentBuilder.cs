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
            return Create(
                laDisposalCost:   calcResult.CalcResultLapcapData.Total,
                laDataPrepCharge: calcResult.CalcResultParameterOtherCost.LaDataPrepCharge with { });
        }

        public static CalcResultOnePlusFourApportionment Create(ByCountryCost laDisposalCost, ByCountryCost laDataPrepCharge)
        {
            var total = new ByCountryCost
            {
                England         = laDisposalCost.England         + laDataPrepCharge.England,
                Wales           = laDisposalCost.Wales           + laDataPrepCharge.Wales,
                Scotland        = laDisposalCost.Scotland        + laDataPrepCharge.Scotland,
                NorthernIreland = laDisposalCost.NorthernIreland + laDataPrepCharge.NorthernIreland
            };

            return new CalcResultOnePlusFourApportionment
            {
                LaDisposalCost           = laDisposalCost,
                LADataPrepCharge         = laDataPrepCharge,
                TotalOnePlusFour         = total,
                OnePlusFourApportionment = total.Total == 0
                    ? ByCountryApportionment.Empty
                    : new ByCountryApportionment
                    {
                        England         = 100 * total.England         / total.Total,
                        Wales           = 100 * total.Wales           / total.Total,
                        Scotland        = 100 * total.Scotland        / total.Total,
                        NorthernIreland = 100 * total.NorthernIreland / total.Total
                    }
            };
        }
    }
}
