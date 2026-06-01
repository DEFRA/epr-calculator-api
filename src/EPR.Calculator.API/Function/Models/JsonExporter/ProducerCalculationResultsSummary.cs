using System.Text.Json.Serialization;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class ProducerCalculationResultsSummary
    {
        [JsonPropertyName("feeForLaDisposalCostsWithoutBadDebtprovision1")]
        public required string FeeForLaDisposalCostsWithoutBadDebtprovision1 { get; init; }
        
        [JsonPropertyName("badDebtProvision1")]
        public required string BadDebtProvision1 { get; init; }

        [JsonPropertyName("feeForLaDisposalCostsWithBadDebtprovision1")]
        public required string FeeForLaDisposalCostsWithBadDebtprovision1 { get; init; }

        [JsonPropertyName("feeForCommsCostsByMaterialWithoutBadDebtprovision2a")]
        public required string FeeForCommsCostsByMaterialWithoutBadDebtprovision2a { get; init; }

        [JsonPropertyName("badDebtProvision2a")]
        public required string BadDebtProvision2a { get; init; }

        [JsonPropertyName("feeForCommsCostsByMaterialWitBadDebtprovision2a")]
        public required string FeeForCommsCostsByMaterialWitBadDebtprovision2a { get; init; }

        [JsonPropertyName("feeForCommsCostsUkWideWithoutBadDebtprovision2b")]
        public required string FeeForCommsCostsUkWideWithoutBadDebtprovision2b { get; init; }

        [JsonPropertyName("badDebtProvision2b")]
        public required string BadDebtProvision2b { get; init; }

        [JsonPropertyName("feeForCommsCostsUkWideWithBadDebtprovision2b")]
        public required string FeeForCommsCostsUkWideWithBadDebtprovision2b { get; init; }

        [JsonPropertyName("feeForCommsCostsByCountryWithoutBadDebtprovision2c")]
        public required string FeeForCommsCostsByCountryWithoutBadDebtprovision2c { get; init; }

        [JsonPropertyName("badDebtProvision2c")]
        public required string BadDebtProvision2c { get; init; }

        [JsonPropertyName("feeForCommsCostsByCountryWideWithBadDebtprovision2c")]
        public required string FeeForCommsCostsByCountryWideWithBadDebtprovision2c { get; init; }

        [JsonPropertyName("total12a2b2cWithBadDebt")]
        public required string Total12a2b2cWithBadDebt { get; init; }

        [JsonPropertyName("saOperatingCostsWithoutBadDebtProvision3")]
        public required string SaOperatingCostsWithoutBadDebtProvision3 { get; init; }

        [JsonPropertyName("badDebtProvision3")]
        public required string BadDebtProvision3 { get; init; }

        [JsonPropertyName("saOperatingCostsWithBadDebtProvision3")]
        public required string SaOperatingCostsWithBadDebtProvision3 { get; init; }

        [JsonPropertyName("laDataPrepCostsWithoutBadDebtProvision4")]
        public required string LaDataPrepCostsWithoutBadDebtProvision4 { get; init; }

        [JsonPropertyName("badDebtProvision4")]
        public required string BadDebtProvision4 { get; init; }

        [JsonPropertyName("laDataPrepCostsWithbadDebtProvision4")]
        public required string LaDataPrepCostsWithbadDebtProvision4 { get; init; }

        [JsonPropertyName("oneOffFeeSaSetuCostsWithbadDebtProvision5")]
        public required string OneOffFeeSaSetuCostsWithbadDebtProvision5 { get; init; }

        [JsonPropertyName("badDebtProvision5")]
        public required string BadDebtProvision5 { get; init; }

        [JsonPropertyName("oneOffFeeSaSetuCostsWithoutbadDebtProvision5")]
        public required string OneOffFeeSaSetuCostsWithoutbadDebtProvision5 { get; init; }
    }
}
