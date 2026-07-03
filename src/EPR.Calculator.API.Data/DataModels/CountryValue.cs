namespace EPR.Calculator.API.Data.DataModels
{
    public record ByCountryCost
    {
        public required decimal England { get; init; }

        public required decimal Wales { get; init; }

        public required decimal Scotland { get; init; }

        public required decimal NorthernIreland { get; init; }

        public decimal Total => England + Wales + Scotland + NorthernIreland;

        public static readonly ByCountryCost Empty =
            new(){ England = 0, Wales = 0, Scotland = 0, NorthernIreland = 0 };

        public static ByCountryCost operator *(ByCountryCost a, decimal scalar) =>
            new()
            {
                England         = a.England         * scalar,
                Wales           = a.Wales           * scalar,
                Scotland        = a.Scotland        * scalar,
                NorthernIreland = a.NorthernIreland * scalar,
            };

        public static ByCountryCost operator *(decimal scalar, ByCountryCost a) => a * scalar;

        public static ByCountryCost operator *(ByCountryCost c, ByCountryApportionment a) =>
            new()
            {
                England         = c.England         * a.England,
                Wales           = c.Wales           * a.Wales,
                Scotland        = c.Scotland        * a.Scotland,
                NorthernIreland = c.NorthernIreland * a.NorthernIreland,
            };

        public static ByCountryCost operator +(ByCountryCost a, ByCountryCost b) =>
            new()
            {
                England         = a.England         + b.England,
                Wales           = a.Wales           + b.Wales,
                Scotland        = a.Scotland        + b.Scotland,
                NorthernIreland = a.NorthernIreland + b.NorthernIreland,
            };

        public static ByCountryCost Sum(IReadOnlyCollection<ByCountryCost> costs)
        {
            return new ByCountryCost
            {
                England          = costs.Sum(x => x.England),
                Wales            = costs.Sum(x => x.Wales),
                NorthernIreland  = costs.Sum(x => x.NorthernIreland),
                Scotland         = costs.Sum(x => x.Scotland),
            };
        }
    }

    public record ByCountryApportionment
    {
        public required decimal England { get; init; }

        public required decimal Wales { get; init; }

        public required decimal Scotland { get; init; }

        public required decimal NorthernIreland { get; init; }

        public decimal Total => England + Wales + Scotland + NorthernIreland;

        public static readonly ByCountryApportionment Empty =
            new(){ England = 0, Wales = 0, Scotland = 0, NorthernIreland = 0 };

        public static ByCountryCost operator *(decimal value, ByCountryApportionment a) =>
            new()
            {
                England         = value * a.England         / 100,
                Wales           = value * a.Wales           / 100,
                Scotland        = value * a.Scotland        / 100,
                NorthernIreland = value * a.NorthernIreland / 100,
            };

        public static ByCountryCost operator *(ByCountryApportionment a, decimal value) => value * a;
    }
}
