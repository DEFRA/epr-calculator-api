namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultLapcapData
    {
        public required Dictionary<string, ByCountryCost> ByMaterial { get; set; }

        private ByCountryCost? total;
        public ByCountryCost Total =>
            total ??=
                new ByCountryCost
                {
                    England         = ByMaterial.Values.Sum(x => x.England),
                    NorthernIreland = ByMaterial.Values.Sum(x => x.NorthernIreland),
                    Scotland        = ByMaterial.Values.Sum(x => x.Scotland),
                    Wales           = ByMaterial.Values.Sum(x => x.Wales)
                };

        private ByCountryApportionment? countryApportionment;
        public ByCountryApportionment CountryApportionment =>
            countryApportionment ??=
                Total.Total == 0
                ? ByCountryApportionment.Empty
                : new ByCountryApportionment
                {
                    England         = Total.England         / Total.Total * 100,
                    Wales           = Total.Wales           / Total.Total * 100,
                    Scotland        = Total.Scotland        / Total.Total * 100,
                    NorthernIreland = Total.NorthernIreland / Total.Total * 100
                };
    }
}
