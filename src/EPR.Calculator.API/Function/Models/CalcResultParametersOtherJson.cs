using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Utils;

namespace EPR.Calculator.Service.Function.Models
{
    public class CalcResultParametersOtherJson
    {
        [JsonPropertyName("threeSAOperatingCost")]
        public CountryAmountJson ThreeSAOperatingCost { get; set; } = new CountryAmountJson();

        [JsonPropertyName("fourDataPreparationCharge")]
        public CountryAmountJson FourDataPreparationCharge { get; set; } = new CountryAmountJson();

        [JsonPropertyName("fourCountryApportionmentPercentages")]
        public CountryAmountJson FourCountryApportionmentPercentages { get; set; } = new CountryAmountJson();

        [JsonPropertyName("fiveSchemeSetupCost")]
        public CountryAmountJson FiveSchemeSetupCost { get; set; } = new CountryAmountJson();

        [JsonPropertyName("sixBadDebtProvision")]
        public PercentageJson SixBadDebtProvision { get; set; } = new PercentageJson();

        [JsonPropertyName("sevenMateriality")]
        public ChangeJson SevenMateriality { get; set; } = new ChangeJson();

        [JsonPropertyName("eightTonnageChange")]
        public ChangeJson EightTonnageChange { get; set; } = new ChangeJson();

        private const string FourCountryApportionmentPercentage = "4 Country Apportionment %s";
        private const string EightTonnageChangeHeader = "8 Tonnage Change";

        public static CalcResultParametersOtherJson From(CalcResultParameterOtherCost otherCost)
        {
            return new CalcResultParametersOtherJson
            {
                ThreeSAOperatingCost = CountryAmountJson.From(otherCost.SaOperatingCost),
                FourDataPreparationCharge = CountryAmountJson.From(otherCost.LaDataPrepCharge),
                FourCountryApportionmentPercentages = CountryAmountJson.From(otherCost.CountryApportionment),
                FiveSchemeSetupCost = CountryAmountJson.From(otherCost.SchemeSetupCost),
                SixBadDebtProvision = new PercentageJson
                {
                    Percentage = $"{otherCost.BadDebtValue:0.00}%"
                },
                SevenMateriality = ChangeJson.From(otherCost.MaterialityIncrease, otherCost.MaterialityDecrease),
                EightTonnageChange = ChangeJson.From(otherCost.TonnageChangeIncrease, otherCost.TonnageChangeDecrease),
            };
        }
    }

    public class CountryAmountJson
    {
        [JsonPropertyName("england")]
        public string England { get; set; } = string.Empty;

        [JsonPropertyName("wales")]
        public string Wales { get; set; } = string.Empty;

        [JsonPropertyName("scotland")]
        public string Scotland { get; set; } = string.Empty;

        [JsonPropertyName("northernIreland")]
        public string NorthernIreland { get; set; } = string.Empty;

        [JsonPropertyName("total")]
        public string Total { get; set; } = string.Empty;

        public static CountryAmountJson From(ByCountryCost? costDetail)
        {
            if (costDetail == null) return new CountryAmountJson();

            return new CountryAmountJson
            {
                England         = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(costDetail.England        , 2, ","),
                Wales           = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(costDetail.Wales          , 2, ","),
                Scotland        = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(costDetail.Scotland       , 2, ","),
                NorthernIreland = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(costDetail.NorthernIreland, 2, ","),
                Total           = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(costDetail.Total          , 2, ",")
            };
        }

        public static CountryAmountJson From(ByCountryApportionment? apportionment)
        {
            if (apportionment == null) return new CountryAmountJson();

            return new CountryAmountJson
            {
                England         = $"{Math.Round(apportionment.England        , (int)DecimalPlaces.Eight):0.00000000}%",
                Wales           = $"{Math.Round(apportionment.Wales          , (int)DecimalPlaces.Eight):0.00000000}%",
                Scotland        = $"{Math.Round(apportionment.Scotland       , (int)DecimalPlaces.Eight):0.00000000}%",
                NorthernIreland = $"{Math.Round(apportionment.NorthernIreland, (int)DecimalPlaces.Eight):0.00000000}%",
                Total           = $"{100                                                                :0.00000000}%"
            };
        }
    }

    public class PercentageJson
    {
        [JsonPropertyName("percentage")]
        public string Percentage { get; set; } = string.Empty;
    }

    public class ChangeJson
    {
        [JsonPropertyName("increase")]
        public ChangeDetailJson Increase { get; set; } = new ChangeDetailJson();

        [JsonPropertyName("decrease")]
        public ChangeDetailJson Decrease { get; set; } = new ChangeDetailJson();

        public static ChangeJson From(Materiality increase, Materiality decrease)
        {
            return new ChangeJson
            {
                Increase = ChangeDetailJson.From(increase),
                Decrease = ChangeDetailJson.From(decrease),
            };
        }
    }

    public class ChangeDetailJson
    {
        [JsonPropertyName("amount")]
        public string Amount { get; set; } = string.Empty;

        [JsonPropertyName("percentage")]
        public string Percentage { get; set; } = string.Empty;

        public static ChangeDetailJson From(Materiality materiality)
        {
            return new ChangeDetailJson
            {
                Amount     = CurrencyConverterUtils.FormatCurrencyWithGbpSymbol(materiality.Amount, 2, ","),
                Percentage = $"{materiality.Percentage:0.00}%"
            };
        }
    }
}
