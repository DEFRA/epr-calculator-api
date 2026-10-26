using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using EPR.Calculator.API.BackgroundService.Constants;
using EPR.Calculator.API.BackgroundService.Converter;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.Exporter.JsonExporter.Model;

public record ProducerDisposalFeesWithBadDebtProvision1
{
    [JsonPropertyName("materialBreakdown")]
    public required IEnumerable<ProducerDisposalFeesWithBadDebtProvision1MaterialBreakdown> MaterialBreakdown { get; set; }

    public static ProducerDisposalFeesWithBadDebtProvision1 From(
        IReadOnlyDictionary<string, DisposalFee> producerDisposalFeesByMaterial,
        IImmutableList<MaterialDetail> materials,
        string level,
        bool applyModulation
    ) => new ()
        {
            MaterialBreakdown = producerDisposalFeesByMaterial
                .Select(x => ProducerDisposalFeesWithBadDebtProvision1MaterialBreakdown.From(x.Value, materials.Single(m => m.Code == x.Key), level, applyModulation))
                .ToList()
        };
}

public abstract record ValueOrModulated<TValue, TModulated>
{
    public TValue? Value { get; init; }

    public TModulated? Modulated { get; init; }

    protected static TWrapper FromValue<TWrapper>(TValue value)
        where TWrapper : ValueOrModulated<TValue, TModulated>, new()
        => new() { Value = value };

    protected static TWrapper FromModulated<TWrapper>(TModulated value)
        where TWrapper : ValueOrModulated<TValue, TModulated>, new()
        => new() { Modulated = value };
}

[SuppressMessage(
    "Major Code Smell",
    "S2436:Types and methods should not have too many generic parameters",
    Justification = "Strongly typed JsonConverter requires wrapper, value, and modulated types for reusable serialization.")]
public abstract class ValueOrModulatedConverter<TWrapper, TValue, TModulated>: JsonConverter<TWrapper>
    where TWrapper : ValueOrModulated<TValue, TModulated>
{
    public override TWrapper Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => throw new NotImplementedException();

    public override void Write(Utf8JsonWriter writer, TWrapper value, JsonSerializerOptions options)
    {
        if (value.Modulated is not null)
        {
            JsonSerializer.Serialize(writer, value.Modulated, options);
            return;
        }

        JsonSerializer.Serialize(writer, value.Value, options);
    }
}

public class DecimalOrModulatedTonnageBreakdownConverter
    : ValueOrModulatedConverter<DecimalOrModulatedTonnageBreakdown, decimal, ModulatedTonnageBreakdown>{}

[JsonConverter(typeof(DecimalOrModulatedTonnageBreakdownConverter))]
public record DecimalOrModulatedTonnageBreakdown
    : ValueOrModulated<decimal, ModulatedTonnageBreakdown>
{
    public static implicit operator DecimalOrModulatedTonnageBreakdown(decimal value)
        => FromValue<DecimalOrModulatedTonnageBreakdown>(value);

    public static implicit operator DecimalOrModulatedTonnageBreakdown(ModulatedTonnageBreakdown value)
        => FromModulated<DecimalOrModulatedTonnageBreakdown>(value);
}

public record ModulatedTonnageBreakdown
{
    [JsonPropertyName("total")]
    [JsonConverter(typeof(DecimalPrecision3Converter))]
    public decimal Total { get; init; }

    [JsonPropertyName("red")]
    [JsonConverter(typeof(DecimalPrecision3Converter))]
    public decimal Red { get; init; }

    [JsonPropertyName("amber")]
    [JsonConverter(typeof(DecimalPrecision3Converter))]
    public decimal Amber { get; init; }

    [JsonPropertyName("green")]
    [JsonConverter(typeof(DecimalPrecision3Converter))]
    public decimal Green { get; init; }

    [JsonPropertyName("redMedical")]
    [JsonConverter(typeof(DecimalPrecision3Converter))]
    public decimal RedMedical { get; init; }

    [JsonPropertyName("amberMedical")]
    [JsonConverter(typeof(DecimalPrecision3Converter))]
    public decimal AmberMedical { get; init; }

    [JsonPropertyName("greenMedical")]
    [JsonConverter(typeof(DecimalPrecision3Converter))]
    public decimal GreenMedical { get; init; }

    public static ModulatedTonnageBreakdown From(RamTonnage ramTonnage)
    {
        return new ModulatedTonnageBreakdown
        {
            Total        = ramTonnage.TotalRamTonnage(),
            Red          = ramTonnage.Red,
            Amber        = ramTonnage.Amber,
            Green        = ramTonnage.Green,
            RedMedical   = ramTonnage.RedMedical,
            AmberMedical = ramTonnage.AmberMedical,
            GreenMedical = ramTonnage.GreenMedical
        };
    }
}

public class DecimalOrCombinedModulatedTonnageBreakdownConverter
    : ValueOrModulatedConverter<DecimalOrCombinedModulatedTonnageBreakdown, decimal, CombinedModulatedTonnageBreakdown> {}

[JsonConverter(typeof(DecimalOrCombinedModulatedTonnageBreakdownConverter))]
public record DecimalOrCombinedModulatedTonnageBreakdown
    : ValueOrModulated<decimal, CombinedModulatedTonnageBreakdown>
{
    public static implicit operator DecimalOrCombinedModulatedTonnageBreakdown(decimal value)
        => FromValue<DecimalOrCombinedModulatedTonnageBreakdown>(value);

    public static implicit operator DecimalOrCombinedModulatedTonnageBreakdown(CombinedModulatedTonnageBreakdown value)
        => FromModulated<DecimalOrCombinedModulatedTonnageBreakdown>(value);
}

public record CombinedModulatedTonnageBreakdown
{
    [JsonPropertyName("total")]
    [JsonConverter(typeof(DecimalPrecision3Converter))]
    public decimal Total { get; init; }

    [JsonPropertyName("redAndRedMedical")]
    [JsonConverter(typeof(DecimalPrecision3Converter))]
    public decimal RedAndRedMedical { get; init; }

    [JsonPropertyName("amberAndAmberMedical")]
    [JsonConverter(typeof(DecimalPrecision3Converter))]
    public decimal AmberAndAmberMedical { get; init; }

    [JsonPropertyName("greenAndGreenMedical")]
    [JsonConverter(typeof(DecimalPrecision3Converter))]
    public decimal GreenAndGreenMedical { get; init; }
}

public class StringOrCombinedModulatedCostBreakdownConverter
    : ValueOrModulatedConverter<StringOrCombinedModulatedCostBreakdown, string , CombinedModulatedCostBreakdown> {}

[JsonConverter(typeof(StringOrCombinedModulatedCostBreakdownConverter))]
public record StringOrCombinedModulatedCostBreakdown
    : ValueOrModulated<string, CombinedModulatedCostBreakdown>
{
    public static implicit operator StringOrCombinedModulatedCostBreakdown(string value)
        => FromValue<StringOrCombinedModulatedCostBreakdown>(value);

    public static implicit operator StringOrCombinedModulatedCostBreakdown(CombinedModulatedCostBreakdown value)
        => FromModulated<StringOrCombinedModulatedCostBreakdown>(value);
}

public record CombinedModulatedCostBreakdown
{
    [JsonPropertyName("total")]
    public required string Total { get; init; }

    [JsonPropertyName("redAndRedMedical")]
    public string RedAndRedMedical { get; init; } = default!;

    [JsonPropertyName("amberAndAmberMedical")]
    public string AmberAndAmberMedical { get; init; } = default!;

    [JsonPropertyName("greenAndGreenMedical")]
    public string GreenAndGreenMedical { get; init; } = default!;
}

public record ProducerDisposalFeesWithBadDebtProvision1MaterialBreakdown
{
    [JsonPropertyName("material")]
    public required string MaterialName { get; init; }

    [JsonPropertyName("previousInvoicedTonnage")]
    public required string PreviousInvoicedTonnage { get; init; }

    [JsonPropertyName("householdPackagingWasteTonnage")]
    public required DecimalOrModulatedTonnageBreakdown HouseholdPackagingWasteTonnage { get; init; }

    [JsonPropertyName("publicBinTonnage")]
    public required DecimalOrModulatedTonnageBreakdown PublicBinTonnage { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("householdDrinksContainersTonnageGlass")]
    public DecimalOrModulatedTonnageBreakdown? HouseholdDrinksContainersTonnageGlass { get; set; }

    [JsonPropertyName("totalTonnage")]
    public required DecimalOrModulatedTonnageBreakdown TotalTonnage { get; init; }

    [JsonPropertyName("selfManagedConsumerWasteTonnage")]
    [JsonConverter(typeof(DecimalPrecision3Converter))]
    public required decimal SelfManagedConsumerWasteTonnage { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("actionedSelfManagedConsumerWasteTonnage")]
    public CombinedModulatedTonnageBreakdown? ActionedSelfManagedConsumerWasteTonnage { get; init; }

    [JsonPropertyName("netTonnage")]
    public required DecimalOrCombinedModulatedTonnageBreakdown NetTonnage { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [JsonPropertyName("residualSelfManagedConsumerWasteTonnage")]
    [JsonConverter(typeof(DecimalPrecision3Converter))]
    public decimal? ResidualSelfManagedConsumerWasteTonnage { get; init; }

    [JsonPropertyName("feeWithoutBadDebt")]
    public required StringOrCombinedModulatedCostBreakdown FeeWithoutBadDebt { get; init; }

    [JsonPropertyName("tonnageChange")]
    public required string TonnageChange { get; init; }

    [JsonPropertyName("badDebtProvision")]
    public required string BadDebtProvision { get; init; }

    [JsonPropertyName("fee")]
    public required string Fee { get; init; }

    [JsonPropertyName("england")]
    public required string England { get; init; }

    [JsonPropertyName("wales")]
    public required string Wales { get; init; }

    [JsonPropertyName("scotland")]
    public required string Scotland { get; init; }

    [JsonPropertyName("northernIreland")]
    public required string NorthernIreland { get; init; }

    private static string F2(decimal value) => value.ToString("F2", CultureInfo.InvariantCulture);

    public static ProducerDisposalFeesWithBadDebtProvision1MaterialBreakdown From(
        DisposalFee producerTonnage,
        MaterialDetail material,
        string level,
        bool applyModulation)
    {
        if (!applyModulation)
        {
            return new ProducerDisposalFeesWithBadDebtProvision1MaterialBreakdown
            {
                MaterialName                          = material.Name,
                PreviousInvoicedTonnage               = level == "1" ? producerTonnage.PreviousInvoicedTonnage?.ToString() ?? CommonConstants.Hyphen : CommonConstants.Hyphen,
                TonnageChange                         = level == "1" ? producerTonnage.TonnageChange?.ToString()           ?? CommonConstants.Hyphen : CommonConstants.Hyphen,
                BadDebtProvision                      = F2(producerTonnage.BadDebt),
                Fee                                   = F2(producerTonnage.FeeWithBadDebtByCountry.Total),
                England                               = F2(producerTonnage.FeeWithBadDebtByCountry.England),
                Wales                                 = F2(producerTonnage.FeeWithBadDebtByCountry.Wales),
                Scotland                              = F2(producerTonnage.FeeWithBadDebtByCountry.Scotland),
                NorthernIreland                       = F2(producerTonnage.FeeWithBadDebtByCountry.NorthernIreland),
                HouseholdPackagingWasteTonnage        = producerTonnage.HhTonnage.TotalRamTonnage(),
                PublicBinTonnage                      = producerTonnage.PbTonnage.TotalRamTonnage(),
                HouseholdDrinksContainersTonnageGlass = material.Code == MaterialCodes.Glass ? producerTonnage.HdcTonnage.TotalRamTonnage() : null,
                TotalTonnage                          = producerTonnage.TotalTonnage.TotalRamTonnage(),
                SelfManagedConsumerWasteTonnage       = producerTonnage.SmcwTonnage,
                NetTonnage                            = producerTonnage.NetTonnage.Total ?? 0,
                FeeWithoutBadDebt                     = F2(producerTonnage.Fee.Total ?? 0)
            };
        }

        return new ProducerDisposalFeesWithBadDebtProvision1MaterialBreakdown
        {
            MaterialName                            = material.Name,
            PreviousInvoicedTonnage                 = level == "1" ? producerTonnage.PreviousInvoicedTonnage?.ToString() ?? CommonConstants.Hyphen : CommonConstants.Hyphen,
            TonnageChange                           = level == "1" ? producerTonnage.TonnageChange?.ToString()           ?? CommonConstants.Hyphen : CommonConstants.Hyphen,
            BadDebtProvision                        = F2(producerTonnage.BadDebt),
            Fee                                     = F2(producerTonnage.FeeWithBadDebtByCountry.Total),
            England                                 = F2(producerTonnage.FeeWithBadDebtByCountry.England),
            Wales                                   = F2(producerTonnage.FeeWithBadDebtByCountry.Wales),
            Scotland                                = F2(producerTonnage.FeeWithBadDebtByCountry.Scotland),
            NorthernIreland                         = F2(producerTonnage.FeeWithBadDebtByCountry.NorthernIreland),
            HouseholdPackagingWasteTonnage          = ModulatedTonnageBreakdown.From(producerTonnage.HhTonnage),
            PublicBinTonnage                        = ModulatedTonnageBreakdown.From(producerTonnage.PbTonnage),
            HouseholdDrinksContainersTonnageGlass   = material.Code == MaterialCodes.Glass
                                                          ? (DecimalOrModulatedTonnageBreakdown)ModulatedTonnageBreakdown.From(producerTonnage.HdcTonnage)
                                                          : null,
            TotalTonnage                            = ModulatedTonnageBreakdown.From(producerTonnage.TotalTonnage),
            SelfManagedConsumerWasteTonnage         = producerTonnage.SmcwTonnage,
            ActionedSelfManagedConsumerWasteTonnage = new CombinedModulatedTonnageBreakdown
            {
                Total                = producerTonnage.ActionedSmcwTonnage.Total ?? 0,
                RedAndRedMedical     = producerTonnage.ActionedSmcwTonnage.Red   ?? 0,
                AmberAndAmberMedical = producerTonnage.ActionedSmcwTonnage.Amber ?? 0,
                GreenAndGreenMedical = producerTonnage.ActionedSmcwTonnage.Green ?? 0
            },
            NetTonnage = new CombinedModulatedTonnageBreakdown
            {
                Total                = producerTonnage.NetTonnage.Total ?? 0,
                RedAndRedMedical     = producerTonnage.NetTonnage.Red   ?? 0,
                AmberAndAmberMedical = producerTonnage.NetTonnage.Amber ?? 0,
                GreenAndGreenMedical = producerTonnage.NetTonnage.Green ?? 0
            },
            ResidualSelfManagedConsumerWasteTonnage = producerTonnage.ResidualSmcwTonnage,
            FeeWithoutBadDebt = new CombinedModulatedCostBreakdown
            {
                Total                = F2(producerTonnage.Fee.Total ?? 0),
                RedAndRedMedical     = F2(producerTonnage.Fee.Red   ?? 0),
                AmberAndAmberMedical = F2(producerTonnage.Fee.Amber ?? 0),
                GreenAndGreenMedical = F2(producerTonnage.Fee.Green ?? 0)
            }
        };
    }
}
