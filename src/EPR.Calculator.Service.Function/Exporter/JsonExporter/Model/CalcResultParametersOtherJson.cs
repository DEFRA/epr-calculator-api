using System.Text.Json.Serialization;
using EPR.Calculator.API.Data.DataModels;
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

        [JsonPropertyName("cutOffDate")]
        public DateOnly? CutOffDate { get; set; }

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
                    Percentage = FormatUtils.FormatPercentage(otherCost.BadDebtValue, 2)
                },
                SevenMateriality = ChangeJson.From(otherCost.MaterialityIncrease, otherCost.MaterialityDecrease),
                EightTonnageChange = ChangeJson.From(otherCost.TonnageChangeIncrease, otherCost.TonnageChangeDecrease),
                CutOffDate = otherCost.CutOffDate is null
                                ? null
                                : DateOnly.FromDateTime(otherCost.CutOffDate.Value)
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

        public static CountryAmountJson From(ByCountryCost costDetail) =>
            new ()
            {
                England         = FormatUtils.FormatCurrency(costDetail.England        , 2, useGrouping: true),
                Wales           = FormatUtils.FormatCurrency(costDetail.Wales          , 2, useGrouping: true),
                Scotland        = FormatUtils.FormatCurrency(costDetail.Scotland       , 2, useGrouping: true),
                NorthernIreland = FormatUtils.FormatCurrency(costDetail.NorthernIreland, 2, useGrouping: true),
                Total           = FormatUtils.FormatCurrency(costDetail.Total          , 2, useGrouping: true)
            };

        public static CountryAmountJson From(ByCountryApportionment apportionment) =>
            new()
            {
                England         = FormatUtils.FormatPercentage(apportionment.England        ),
                Wales           = FormatUtils.FormatPercentage(apportionment.Wales          ),
                Scotland        = FormatUtils.FormatPercentage(apportionment.Scotland       ),
                NorthernIreland = FormatUtils.FormatPercentage(apportionment.NorthernIreland),
                Total           = FormatUtils.FormatPercentage(100                          )
            };
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
                Amount     = FormatUtils.FormatCurrency(materiality.Amount      , 2, useGrouping: true),
                Percentage = FormatUtils.FormatPercentage(materiality.Percentage, 2)
            };
        }
    }
}
