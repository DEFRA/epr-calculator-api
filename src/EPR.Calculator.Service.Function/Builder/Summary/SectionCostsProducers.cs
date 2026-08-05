using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.Builder.Summary;

// Sections 3, 4, 5 are structurally identical: a fixed cost total is apportioned to producers
// by their ProducerOverallPercentage and country apportionment. The only differences between
// sections are the source of the total, which summary header fields to write, which fee property
// to assign, and which country-apportionment table to use.

public static class ThreeSaCostsProducer
{
    public static void SetValues(CalcResult calcResult, ProducerFees producerFees) =>
        SectionCosts.Apply(
            producerFees,
            badDebt:       calcResult.CalcResultParameterOtherCost.BadDebtValue,
            total:         calcResult.CalcResultParameterOtherCost.SaOperatingCost.Total,
            apportionment: calcResult.CalcResultOnePlusFourApportionment.OnePlusFourApportionment,
            setFee:    (f, p) => f.SaOperatingCostsSection3 = p
        );
}

public static class LaDataPrepCostsProducer
{
    public static void SetValues(CalcResult calcResult, ProducerFees producerFees) =>
        SectionCosts.Apply(
            producerFees,
            badDebt:       calcResult.CalcResultParameterOtherCost.BadDebtValue,
            total:         calcResult.CalcResultParameterOtherCost.LaDataPrepCharge.Total,
            apportionment: calcResult.CalcResultParameterOtherCost.CountryApportionment,
            setFee:    (f, p) => f.LaDataPrepSection4 = p
        );
}

public static class SaSetupCostsProducer
{
    public static void SetValues(CalcResult calcResult, ProducerFees producerFees) =>
        SectionCosts.Apply(
            producerFees,
            badDebt:       calcResult.CalcResultParameterOtherCost.BadDebtValue,
            total:         calcResult.CalcResultParameterOtherCost.SchemeSetupCost.Total,
            apportionment: calcResult.CalcResultOnePlusFourApportionment.OnePlusFourApportionment,
            setFee:    (f, p) => f.SaSetupCostsSection5 = p
        );
}

internal static class SectionCosts
{
    internal static void Apply(
        ProducerFees producerFees,
        decimal badDebt,
        decimal total,
        ByCountryApportionment apportionment,
        Action<FeeDetail, FeeWithBadDebt> setFee
    )
    {
        foreach (var fee in producerFees.Details.Select(fee => fee.FeeDetail))
            setFee(fee, BadDebt(badDebt, total, apportionment, fee.TotalOnePlus2A2B2CWithBadDebtPercentage));
        setFee(producerFees.Total, BadDebt(badDebt, total, apportionment, producerFees.Total.TotalOnePlus2A2B2CWithBadDebtPercentage));
    }

    internal static FeeWithBadDebt BadDebt(
        decimal badDebtRate,
        decimal sectionTotal,
        ByCountryApportionment apportionment,
        decimal producerPct
    )
    {
        var without = producerPct * sectionTotal / 100;
        return new FeeWithBadDebt
        {
            FeeWithoutBadDebt = without,
            BadDebt           = without * badDebtRate / 100,
            ByCountry    = ApplyApportionment(badDebtRate, sectionTotal, producerPct, apportionment)
        };
    }

    // A producer's country-apportioned share of a section cost total, with bad debt applied.
    // Formula: sectionTotal × (1 + badDebt%) × producerPct% × countryApportionment%
    // Used by sections 3, 4, and 5 which share this calculation structure.
    public static ByCountryCost ApplyApportionment(
        decimal badDebt,
        decimal sectionTotal,
        decimal producerPct,
        ByCountryApportionment apportionment
    )
    {
        var factor =
            sectionTotal
                * (1 + (badDebt / 100))
                * (producerPct / 100)
                / 100;
        return new ByCountryCost
        {
            England          = factor * apportionment.England,
            Wales            = factor * apportionment.Wales,
            Scotland         = factor * apportionment.Scotland,
            NorthernIreland  = factor * apportionment.NorthernIreland
        };
    }
}
