namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultOnePlusFourApportionment
    {
        public required ByCountryCost LaDisposalCost { get; init; }
        public required ByCountryCost LADataPrepCharge { get; init; }

        private ByCountryCost? totalOnePlusFour;
        public ByCountryCost TotalOnePlusFour =>
            totalOnePlusFour ??=
                new ByCountryCost
                {
                    England         = LaDisposalCost.England         + LADataPrepCharge.England,
                    Wales           = LaDisposalCost.Wales           + LADataPrepCharge.Wales,
                    Scotland        = LaDisposalCost.Scotland        + LADataPrepCharge.Scotland,
                    NorthernIreland = LaDisposalCost.NorthernIreland + LADataPrepCharge.NorthernIreland
                };

        private ByCountryApportionment? onePlusFourApportionment;
        public ByCountryApportionment OnePlusFourApportionment =>
            onePlusFourApportionment ??=
                TotalOnePlusFour.Total == 0
                ? ByCountryApportionment.Empty
                : new ByCountryApportionment
                  {
                      England         = 100 * TotalOnePlusFour.England         / TotalOnePlusFour.Total,
                      Wales           = 100 * TotalOnePlusFour.Wales           / TotalOnePlusFour.Total,
                      Scotland        = 100 * TotalOnePlusFour.Scotland        / TotalOnePlusFour.Total,
                      NorthernIreland = 100 * TotalOnePlusFour.NorthernIreland / TotalOnePlusFour.Total,
                  };

    }
}
