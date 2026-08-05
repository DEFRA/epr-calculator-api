using System.Reflection.Metadata.Ecma335;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.Service.Function.Features.BillingRuns.Contexts;
using EPR.Calculator.Service.Function.Features.CalculatorRuns.Contexts;
using EPR.Calculator.Service.Function.Services;

namespace EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

public static partial class TestDataHelper
{
    public static CalculatorRunContext CalculatorRun2024 => new()
    {
        RunId = 2024001,
        RunName = "2024 Calculator Run",
        ProcessingStartedAt = DateTimeOffset.Parse("2024-01-01T00:00:00Z"),
        RelativeYear = new RelativeYear(2024),
        User = "TestUser",
        DefaultParameters = DefaultParameters
    };

    public static CalculatorRunContext CalculatorRun2025 => new()
    {
        RunId = 2025001,
        RunName = "2025 Calculator Run",
        ProcessingStartedAt = DateTimeOffset.Parse("2025-01-01T00:00:00Z"),
        RelativeYear = new RelativeYear(2025),
        User = "TestUser",
        DefaultParameters = DefaultParameters
    };

    public static CalculatorRunContext CalculatorRun2026 => new()
    {
        RunId = 2026001,
        RunName = "2026 Calculator Run",
        ProcessingStartedAt = DateTimeOffset.Parse("2026-01-01T00:00:00Z"),
        RelativeYear = new RelativeYear(2026),
        User = "TestUser",
        DefaultParameters = DefaultParameters
    };

    public static BillingRunContext BillingRun2024 => new()
    {
        RunId = 2024011,
        RunName = "2024 Billing Run",
        ProcessingStartedAt = DateTimeOffset.Parse("2024-02-01T00:00:00Z"),
        RelativeYear = new RelativeYear(2024),
        User = "TestUser",
        DefaultParameters = DefaultParameters,
        AcceptedProducerIds = [1, 2, 3]
    };

    public static BillingRunContext BillingRun2025 => new()
    {
        RunId = 2025011,
        RunName = "2025 Billing Run",
        ProcessingStartedAt = DateTimeOffset.Parse("2025-02-01T00:00:00Z"),
        RelativeYear = new RelativeYear(2025),
        User = "TestUser",
        DefaultParameters = DefaultParameters,
        AcceptedProducerIds = [1, 2, 3]
    };

    public static BillingRunContext BillingRun2026 => new()
    {
        RunId = 2026011,
        RunName = "2026 Billing Run",
        ProcessingStartedAt = DateTimeOffset.Parse("2026-02-01T00:00:00Z"),
        RelativeYear = new RelativeYear(2026),
        User = "TestUser",
        DefaultParameters = DefaultParameters,
        AcceptedProducerIds = [1, 2, 3]
    };

    private static DefaultParameters DefaultParameters => new DefaultParameters
    {
        CommunicationCosts = new CommunicationCosts
        {
            ByMaterialCode = new Dictionary<string, decimal>
            {
                ["AL"] = 0,
                ["FC"] = 0,
                ["GL"] = 0,
                ["OT"] = 0,
                ["PC"] = 0,
                ["PL"] = 0,
                ["ST"] = 0,
                ["WD"] = 0
            },
            ByCountry = new ByCountryCostWithUk
            {
                UnitedKingdom = 0,
                England = 0,
                Wales = 0,
                Scotland = 0,
                NorthernIreland = 0
            }
        },

        SchemeAdministratorOperatingCostsByCountry = new ByCountryCost
        {
            England = 0,
            Wales = 0,
            Scotland = 0,
            NorthernIreland = 0
        },

        SchemeSetupCostsByCountry = new ByCountryCost
        {
            England = 0,
            Wales = 0,
            Scotland = 0,
            NorthernIreland = 0
        },

        LocalAuthorityDataPreparationCostsByCountry = new ByCountryCost
        {
            England = 0,
            Wales = 0,
            Scotland = 0,
            NorthernIreland = 0
        },

        LateReportingTonnageByMaterialCode = new Dictionary<string, RamTonnageGroup>
        {
            ["AL"] = RamTonnageGroup.Zero,
            ["FC"] = RamTonnageGroup.Zero,
            ["GL"] = RamTonnageGroup.Zero,
            ["OT"] = RamTonnageGroup.Zero,
            ["PC"] = RamTonnageGroup.Zero,
            ["PL"] = RamTonnageGroup.Zero,
            ["ST"] = RamTonnageGroup.Zero,
            ["WD"] = RamTonnageGroup.Zero
        },

        MaterialityThreshold = new Threshold
        {
            AmountIncrease = 0,
            AmountDecrease = 0,
            PercentIncrease = 0,
            PercentDecrease = 0
        },

        TonnageChangeThreshold = new Threshold
        {
            AmountIncrease = 0,
            AmountDecrease = 0,
            PercentIncrease = 0,
            PercentDecrease = 0
        },

        BadDebtProvision = 0,
        RedModulationFactor = 0,
        CutOffDate = null
    };
}
