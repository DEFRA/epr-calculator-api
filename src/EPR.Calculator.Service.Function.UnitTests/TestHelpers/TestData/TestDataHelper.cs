using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Helpers;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

public static partial class TestDataHelper
{
    public static CalcResult GetCalcResult(bool applyModulation = false)
    {
        return new CalcResult
        {
            CalcResultScaledupProducers        = GetScaledupProducers(),
            CalcResultPartialObligations       = GetPartialObligations(),
            CalcResultParameterOtherCost       = GetCalcResultParameterOtherCost(),
            CalcResultDetail                   = GetCalcResultDetail(),
            CalcResultLapcapData               = GetCalcResultLapcapData(),
            CalcResultLateReportingTonnageData = GetCalcResultLateReportingTonnage(),
            CalcResultOnePlusFourApportionment = GetCalcResultOnePlusFourApportionment(),
            CalcResultLaDisposalCostData       = GetCalcResultLaDisposalCostData(),
            CalcResultCommsCostReportDetail    = GetCalcResultCommsCostReportDetail(),
            ProducerFees                       = GetProducerFees(applyModulation),
            CalcResultProjectedProducers       = new CalcResultProjectedProducers(){
                H1ProjectedProducers = ImmutableList<CalcResultH1ProjectedProducer>.Empty,
                H2ProjectedProducers = ImmutableList<CalcResultH2ProjectedProducer>.Empty,
            },
            CalcResultModulation               = applyModulation
                ? new ModulationResult
                {
                    CalculatorRunId      = 1,
                    GreenFactor          = 1,
                    RedFactor            = 2,
                    ModulationByMaterial = new Dictionary<MaterialDetail, ModulationDetail>()
                }
                : null
        };
    }

    public static CalcResultParameterOtherCost GetCalcResultParameterOtherCost()
    {
        return new CalcResultParameterOtherCost
        {
            SaOperatingCost = new ByCountryCost
            {
                England         = 25000,
                Wales           = 14000,
                Scotland        = 17000,
                NorthernIreland = 9000
            },
            LaDataPrepCharge = new ByCountryCost
            {
                England         = 16000,
                Wales           = 7000,
                Scotland        = 9000,
                NorthernIreland = 4500
            },
            CountryApportionment = new ByCountryApportionment
            {
                England         = 43.83561643835616m,
                Wales           = 19.17808219178082m,
                Scotland        = 24.65753424657534m,
                NorthernIreland = 12.32876712328767m
            },
            SchemeSetupCost = new ByCountryCost
            {
                England         = 17500,
                Wales           = 23400,
                Scotland        = 12400,
                NorthernIreland = 9450
            },
            BadDebtValue = 6,
            MaterialityIncrease   = new Materiality { Amount =  5000, Percentage =  2.0m },
            MaterialityDecrease   = new Materiality { Amount = -1000, Percentage = -1.0m },
            TonnageChangeIncrease = new Materiality { Amount =    50, Percentage =  2.0m },
            TonnageChangeDecrease = new Materiality { Amount =   -10, Percentage = -0.5m }
        };
    }

    public static CalcResultDetail GetCalcResultDetail() => new()
    {
        RunId          = 1,
        RunDate        = DateTime.UtcNow,
        RunName        = "CalculatorRunName",
        RunBy          = "Test user",
        RelativeYear   = new RelativeYear(2024),
        RpdFileORG     = "21/07/2017 17:32",
        RpdFilePOM     = "21/07/2017 17:32",
        LapcapFile     = "lapcap-data.csv,24/06/2025 10:00, test",
        ParametersFile = "parameter-data.csv,24/06/2025 10:00, test"
    };

    public static CalcResultLaDisposalCostData GetCalcResultLaDisposalCostData()
    {
        return new CalcResultLaDisposalCostData
        {
            ByMaterial = new Dictionary<string, CalcResultLaDisposalCostDataDetail>
            {
                ["AL"] =
                    new()
                    {
                        Cost = new ByCountryCost { England = 5000, Wales = 1750, Scotland = 2000, NorthernIreland = 1250 },
                        HouseholdPackagingWasteTonnage = 6980,
                        PublicBinTonnage = 2000,
                        HouseholdDrinkContainersTonnage = 0,
                        LateReportingTonnage = 8000
                    },
                ["FC"] =
                    new()
                    {
                        Cost = new ByCountryCost { England = 7500, Wales = 2100, Scotland = 3400, NorthernIreland = 1750 },
                        HouseholdPackagingWasteTonnage = 11850,
                        PublicBinTonnage = 2000,
                        HouseholdDrinkContainersTonnage = 0,
                        LateReportingTonnage = 7000
                    },
                ["GL"] =
                    new()
                    {
                        Cost = new ByCountryCost { England = 45000, Wales = 0, Scotland = 20700, NorthernIreland = 4500 },
                        HouseholdPackagingWasteTonnage = 4900,
                        PublicBinTonnage = 2000,
                        HouseholdDrinkContainersTonnage = 100,
                        LateReportingTonnage = 6000
                    },
                ["PC"] =
                    new()
                    {
                        Cost = new ByCountryCost { England = 12500, Wales = 2300, Scotland = 4500, NorthernIreland = 3400 },
                        HouseholdPackagingWasteTonnage = 4270,
                        PublicBinTonnage = 2000,
                        HouseholdDrinkContainersTonnage = 0,
                        LateReportingTonnage = 5000
                    },
                ["PL"] =
                    new()
                    {
                        Cost = new ByCountryCost { England = 23000, Wales = 4500, Scotland = 6700, NorthernIreland = 2100 },
                        HouseholdPackagingWasteTonnage = 12805,
                        PublicBinTonnage = 2000,
                        HouseholdDrinkContainersTonnage = 0,
                        LateReportingTonnage = 4000
                    },
                ["ST"] =
                    new()
                    {
                        Cost = new ByCountryCost { England = 13400, Wales = 0, Scotland = 7800, NorthernIreland = 0 },
                        HouseholdPackagingWasteTonnage = 7700,
                        PublicBinTonnage = 2000,
                        HouseholdDrinkContainersTonnage = 0,
                        LateReportingTonnage = 3000
                    },
                ["WD"] =
                    new()
                    {
                        Cost = new ByCountryCost { England = 0, Wales = 12000, Scotland = 0, NorthernIreland = 5600 },
                        HouseholdPackagingWasteTonnage = 6800,
                        PublicBinTonnage = 2000,
                        HouseholdDrinkContainersTonnage = 0,
                        LateReportingTonnage = 2000
                    },
                ["OT"] =
                    new()
                    {
                        Cost = new ByCountryCost { England = 3400, Wales = 2100, Scotland = 4200, NorthernIreland = 700 },
                        HouseholdPackagingWasteTonnage = 7700,
                        PublicBinTonnage = 2000,
                        HouseholdDrinkContainersTonnage = 0,
                        LateReportingTonnage = 1000
                    }
            }
        };
    }

    public static CalcResultLapcapData GetCalcResultLapcapData()
    {
        return new CalcResultLapcapData
        {
            ByMaterial = new Dictionary<string, ByCountryCost>
            {
                ["AL"] = new() { England =  5000, Wales =  1750, Scotland =  2000, NorthernIreland = 1250 },
                ["FC"] = new() { England =  7500, Wales =  2100, Scotland =  3400, NorthernIreland = 1750 },
                ["GL"] = new() { England = 45000, Wales =     0, Scotland = 20700, NorthernIreland = 4500 },
                ["PC"] = new() { England = 12500, Wales =  2300, Scotland =  4500, NorthernIreland = 3400 },
                ["PL"] = new() { England = 23000, Wales =  4500, Scotland =  6700, NorthernIreland = 2100 },
                ["ST"] = new() { England = 13400, Wales =     0, Scotland =  7800, NorthernIreland =    0 },
                ["WD"] = new() { England =     0, Wales = 12000, Scotland =     0, NorthernIreland = 5600 },
                ["OT"] = new() { England =  3400, Wales =  2100, Scotland =  4200, NorthernIreland =  700 }
            }
        };
    }

    public static CalcResultOnePlusFourApportionment GetCalcResultOnePlusFourApportionment()
    {
        return new CalcResultOnePlusFourApportionment
        {
            LaDisposalCost   = new ByCountryCost { England = 30, Wales = 5, Scotland = 15, NorthernIreland = 35 },
            LADataPrepCharge = new ByCountryCost { England = 10, Wales = 5, Scotland =  0, NorthernIreland =  0 }
        };
    }

    public static CalcResultCommsCost GetCalcResultCommsCostReportDetail()
    {
        return new CalcResultCommsCost
        {
            OnePlusFourApportionment = new ByCountryApportionment
            {
                England         = 50.23m,
                Wales           = 30.34m,
                Scotland        = 10.45m,
                NorthernIreland = 8.98m
            },
            ByMaterial = new Dictionary<string, CalcResultCommsCostCommsCostByMaterial>
            {
                ["AL"] = new()
                {
                    Cost = ByCountryCost.Empty with { England = 4.347m },
                    TotalCost = 4.347m,
                    HouseholdPackagingWasteTonnage = 2.34m,
                    PublicBinTonnage = 4.56m,
                    HouseholdDrinksContainersTonnage = 0m,
                    LateReportingTonnage = 3.45m
                },
                ["GL"] = new()
                {
                    Cost = ByCountryCost.Empty,
                    TotalCost = 0m,
                    HouseholdPackagingWasteTonnage = 3.45m,
                    PublicBinTonnage = 5.67m,
                    HouseholdDrinksContainersTonnage = 1.23m,
                    LateReportingTonnage = 4.56m
                }
            },
            CommsCostUkWide    = new ByCountryCost { England = 1500, Wales = 200, Scotland = 500, NorthernIreland = 331 },
            CommsCostByCountry = new ByCountryCost { England = 1400, Wales = 250, Scotland = 600, NorthernIreland = 280 }
        };
    }

    public static CalcResultLateReportingTonnage GetCalcResultLateReportingTonnage()
    {
        return new CalcResultLateReportingTonnage
        {
            ByMaterial = new Dictionary<string, CalcResultLateReportingTonnageDetail>
            {
                ["AL"] = new() { Red = 1000.00m, Amber = 2000.00m, Green = 5000.00m, Total = 8000.00m },
                ["FC"] = new() { Red =    5.00m, Amber =    0.00m, Green =    5.00m, Total =   10.00m },
                ["GL"] = new() { Red =   10.00m, Amber =    0.00m, Green =    0.00m, Total =   10.00m },
                ["PC"] = new() { Red =    0.00m, Amber =    0.00m, Green =    0.00m, Total =    0.00m },
                ["PL"] = new() { Red = 1000.00m, Amber =  500.00m, Green =  500.00m, Total = 2000.00m },
                ["ST"] = new() { Red =    0.00m, Amber =    0.00m, Green =    0.00m, Total =    0.00m },
                ["WD"] = new() { Red =    0.00m, Amber =    0.00m, Green =    0.00m, Total =    0.00m },
                ["OT"] = new() { Red =    0.00m, Amber =    0.00m, Green =    0.00m, Total =    0.00m }
            }
        };
    }

    public static ProducerFees GetProducerFees(bool applyModulation = false)
    {
        return new ProducerFees
        {
            CalculatorRunId = 0,
            Details = GetProducerFeesDetail(applyModulation),
            Total = GetOverallTotalRow(applyModulation)
        };
    }

    public static List<ProducerFeeDetail> GetProducerFeesDetail(bool applyModulation = false)
    {
        return new List<ProducerFeeDetail>
        {
            new()
            {
                FeeDetail = new FeeDetail
                {
                    ProducerId = 1,
                    SubsidiaryId = string.Empty,
                    Level = "1",
                    ProducerName = "Allied Packaging",
                    LADisposalCostsSection1 = new FeeWithBadDebt
                    {
                        FeeWithoutBadDebt = 4423.39438m,
                        BadDebt           = 265.4036628m,
                        ByCountry    = new ByCountryCost { England = 2534.2359097426884m, Wales = 571.2417008090152m, Scotland = 1137.8673076088023m, NorthernIreland = 445.4531246394942m }
                    },
                    CommsCostsSection2a = new FeeWithBadDebt
                    {
                        FeeWithoutBadDebt = 1290.778m,
                        BadDebt           = 77.44668m,
                        ByCountry    = new ByCountryCost { England = 718.2251815154783m, Wales = 181.2690740598454m, Scotland = 332.8499847265775m, NorthernIreland = 135.88043969809883m }
                    },
                    CommsCostsSection2c = new FeeWithBadDebt
                    {
                        FeeWithoutBadDebt = 1339.100071422903m,
                        BadDebt           = 80.34600428537418m,
                        ByCountry    = new ByCountryCost { England = 607.4748035870169m, Wales = 300.7301007856519m, Scotland = 360.87612094278234m, NorthernIreland = 150.36505039282596m }
                    },
                    ReportedTonnagePercentage = 5.6741528450123m,
                    TotalOnePlus2A2B2CWithBadDebtPercentage = 4.7341913352015945m,
                    SaOperatingCostsSection3 = new FeeWithBadDebt
                    {
                        FeeWithoutBadDebt = 3077.2243678810364m,
                        BadDebt           = 184.6334620728622m,
                        ByCountry    = new ByCountryCost { England = 1712.2541832180282m, Wales = 432.1468228710047m, Scotland = 793.5168432560496m, NorthernIreland = 323.93998060881614m }
                    },
                    LaDataPrepSection4 = new FeeWithBadDebt
                    {
                        FeeWithoutBadDebt = 1727.9798373485821m,
                        BadDebt           = 103.67879024091492m,
                        ByCountry    = new ByCountryCost { England = 802.9188504501905m, Wales = 351.2769970719583m, Scotland = 451.6418533782321m, NorthernIreland = 225.82092668911605m }
                    },
                    SaSetupCostsSection5 = new FeeWithBadDebt
                    {
                        FeeWithoutBadDebt = 2970.7050628390007m,
                        BadDebt           = 178.24230377034004m,
                        ByCountry    = new ByCountryCost { England = 1652.983846106635m, Wales = 417.1878943870084m, Scotland = 766.0489525279556m, NorthernIreland = 312.72667358774174m }
                    },
                    TotalBillBreakdown = new FeeWithBadDebt
                    {
                        FeeWithoutBadDebt = 9897.32808192842m,
                        BadDebt           = 593.8396849157051m,
                        ByCountry    = new ByCountryCost { England = 5442.448434925617m, Wales = 1452.6428880194774m, Scotland = 2564.98356493499m, NorthernIreland = 1031.0928789640386m }
                    },
                    BillingInstruction = new BillingInstruction
                    {
                        CurrentYearInvoiceTotalToDate = 1250.89m,
                        TonnageChangeSinceLastInvoice = "Tonnage Changed",
                        LiabilityDifference = 580.73m,
                        MaterialityLiabilityDirection = null,
                        TonnageAmountLiabilityDirection = null,
                        PercentageLiabilityDifference = 123.45m,
                        MaterialityPercentageLiabilityDirection = null,
                        TonnageAmountPercentageLiabilityDirection = null,
                        SuggestedBillingInstruction = string.Empty,
                        SuggestedInvoiceAmount = 4039m
                    },
                    CommsCostsSection2b = new FeeWithBadDebt
                    {
                        FeeWithoutBadDebt = 2844.0556305055156m,
                        BadDebt           = 170.64333783033092m,
                        ByCountry    = new ByCountryCost { England = 1582.5125400804336m, Wales = 399.4020123649648m, Scotland = 733.3901516568284m, NorthernIreland = 299.39426423361965m }
                    },
                    FeesByMaterial = GetProducerFeesByMaterial(applyModulation),
                    TonnageChangeCount = "0",
                    TonnageChangeAdvice = ""
                }
            }
        };
    }

    public static FeeDetail GetOverallTotalRow(bool applyModulation = false)
    {
        return new FeeDetail
        {
                ProducerId = 0,
                SubsidiaryId = string.Empty,
                ProducerName = string.Empty,
                LADisposalCostsSection1 = new FeeWithBadDebt
                {
                    FeeWithoutBadDebt = 4423.39438m,
                    BadDebt           = 265.4036628m,
                    ByCountry    = new ByCountryCost { England = 2534.2359097426884m, Wales = 571.2417008090152m, Scotland = 1137.8673076088023m, NorthernIreland = 445.4531246394942m }
                },
                CommsCostsSection2a = new FeeWithBadDebt
                {
                    FeeWithoutBadDebt = 1290.778m,
                    BadDebt           = 77.44668m,
                    ByCountry    = new ByCountryCost { England = 718.2251815154783m, Wales = 181.2690740598454m, Scotland = 332.8499847265775m, NorthernIreland = 135.88043969809883m }
                },
                CommsCostsSection2c = new FeeWithBadDebt
                {
                    FeeWithoutBadDebt = 1339.100071422903m,
                    BadDebt           = 80.34600428537418m,
                    ByCountry    = new ByCountryCost { England = 607.4748035870169m, Wales = 300.7301007856519m, Scotland = 360.87612094278234m, NorthernIreland = 150.36505039282596m }
                },
                ReportedTonnagePercentage = 5.6741528450123m,
                TotalOnePlus2A2B2CWithBadDebtPercentage = 4.7341913352015945m,
                SaOperatingCostsSection3 = new FeeWithBadDebt
                {
                    FeeWithoutBadDebt = 3077.2243678810364m,
                    BadDebt           = 184.6334620728622m,
                    ByCountry    = new ByCountryCost { England = 1712.2541832180282m, Wales = 432.1468228710047m, Scotland = 793.5168432560496m, NorthernIreland = 323.93998060881614m }
                },
                LaDataPrepSection4 = new FeeWithBadDebt
                {
                    FeeWithoutBadDebt = 1727.9798373485821m,
                    BadDebt           = 103.67879024091492m,
                    ByCountry    = new ByCountryCost { England = 802.9188504501905m, Wales = 351.2769970719583m, Scotland = 451.6418533782321m, NorthernIreland = 225.82092668911605m }
                },
                SaSetupCostsSection5 = new FeeWithBadDebt
                {
                    FeeWithoutBadDebt = 2970.7050628390007m,
                    BadDebt           = 178.24230377034004m,
                    ByCountry    = new ByCountryCost { England = 1652.983846106635m, Wales = 417.1878943870084m, Scotland = 766.0489525279556m, NorthernIreland = 312.72667358774174m }
                },
                TotalBillBreakdown = new FeeWithBadDebt
                {
                    FeeWithoutBadDebt = 9897.32808192842m,
                    BadDebt           = 593.8396849157051m,
                    ByCountry    = new ByCountryCost { England = 5442.448434925617m, Wales = 1452.6428880194774m, Scotland = 2564.98356493499m, NorthernIreland = 1031.0928789640386m }
                },
                BillingInstruction = new BillingInstruction
                {
                    CurrentYearInvoiceTotalToDate = null,
                    TonnageChangeSinceLastInvoice = null,
                    LiabilityDifference = 580.73m,
                    MaterialityLiabilityDirection = null,
                    TonnageAmountLiabilityDirection = null,
                    PercentageLiabilityDifference = null,
                    MaterialityPercentageLiabilityDirection = null,
                    TonnageAmountPercentageLiabilityDirection = null,
                    SuggestedBillingInstruction = string.Empty,
                    SuggestedInvoiceAmount = 4039m
                },
                CommsCostsSection2b = new FeeWithBadDebt
                {
                    FeeWithoutBadDebt = 2844.0556305055156m,
                    BadDebt           = 170.64333783033092m,
                    ByCountry    = new ByCountryCost { England = 1582.5125400804336m, Wales = 399.4020123649648m, Scotland = 733.3901516568284m, NorthernIreland = 299.39426423361965m }
                },
                FeesByMaterial = GetProducerFeesByMaterial(applyModulation),
                TonnageChangeCount = "0",
                TonnageChangeAdvice = ""
        };
    }

    public static List<ProducerFeeDetail> GetProducerDisposalFeesTonnageValueNull(bool applyModulation = false)
    {
        return new List<ProducerFeeDetail>
        {
            new()
            {
                FeeDetail = new FeeDetail
                {
                    ProducerId = 1,
                    SubsidiaryId = string.Empty,
                    Level = "2",
                    ProducerName = "Allied Packaging",
                    LADisposalCostsSection1 = new FeeWithBadDebt
                    {
                        FeeWithoutBadDebt = 4423.39438m,
                        BadDebt           = 265.4036628m,
                        ByCountry    = new ByCountryCost { England = 2534.2359097426884m, Wales = 571.2417008090152m, Scotland = 1137.8673076088023m, NorthernIreland = 445.4531246394942m }
                    },
                    CommsCostsSection2a = new FeeWithBadDebt
                    {
                        FeeWithoutBadDebt = 1290.778m,
                        BadDebt           = 77.44668m,
                        ByCountry    = new ByCountryCost { England = 718.2251815154783m, Wales = 181.2690740598454m, Scotland = 332.8499847265775m, NorthernIreland = 135.88043969809883m }
                    },
                    CommsCostsSection2c = new FeeWithBadDebt
                    {
                        FeeWithoutBadDebt = 1339.100071422903m,
                        BadDebt           = 80.34600428537418m,
                        ByCountry    = new ByCountryCost { England = 607.4748035870169m, Wales = 300.7301007856519m, Scotland = 360.87612094278234m, NorthernIreland = 150.36505039282596m }
                    },
                    ReportedTonnagePercentage = 5.6741528450123m,
                    TotalOnePlus2A2B2CWithBadDebtPercentage = 4.7341913352015945m,
                    SaOperatingCostsSection3 = new FeeWithBadDebt
                    {
                        FeeWithoutBadDebt = 3077.2243678810364m,
                        BadDebt           = 184.6334620728622m,
                        ByCountry    = new ByCountryCost { England = 1712.2541832180282m, Wales = 432.1468228710047m, Scotland = 793.5168432560496m, NorthernIreland = 323.93998060881614m }
                    },
                    LaDataPrepSection4 = new FeeWithBadDebt
                    {
                        FeeWithoutBadDebt = 1727.9798373485821m,
                        BadDebt           = 103.67879024091492m,
                        ByCountry    = new ByCountryCost { England = 802.9188504501905m, Wales = 351.2769970719583m, Scotland = 451.6418533782321m, NorthernIreland = 225.82092668911605m }
                    },
                    SaSetupCostsSection5 = new FeeWithBadDebt
                    {
                        FeeWithoutBadDebt = 2970.7050628390007m,
                        BadDebt           = 178.24230377034004m,
                        ByCountry    = new ByCountryCost { England = 1652.983846106635m, Wales = 417.1878943870084m, Scotland = 766.0489525279556m, NorthernIreland = 312.72667358774174m }
                    },
                    TotalBillBreakdown = new FeeWithBadDebt
                    {
                        FeeWithoutBadDebt = 9897.32808192842m,
                        BadDebt           = 593.8396849157051m,
                        ByCountry    = new ByCountryCost { England = 5442.448434925617m, Wales = 1452.6428880194774m, Scotland = 2564.98356493499m, NorthernIreland = 1031.0928789640386m }
                    },
                    BillingInstruction = new BillingInstruction
                    {
                        CurrentYearInvoiceTotalToDate = 1250.89m,
                        TonnageChangeSinceLastInvoice = string.Empty,
                        LiabilityDifference = 580.73m,
                        MaterialityLiabilityDirection = null,
                        TonnageAmountLiabilityDirection = null,
                        PercentageLiabilityDifference = null,
                        MaterialityPercentageLiabilityDirection = null,
                        TonnageAmountPercentageLiabilityDirection = null,
                        SuggestedBillingInstruction = string.Empty,
                        SuggestedInvoiceAmount = 4039m
                    },
                    CommsCostsSection2b = new FeeWithBadDebt
                    {
                        FeeWithoutBadDebt = 2844.0556305055156m,
                        BadDebt           = 170.64333783033092m,
                        ByCountry    = new ByCountryCost { England = 1582.5125400804336m, Wales = 399.4020123649648m, Scotland = 733.3901516568284m, NorthernIreland = 299.39426423361965m }
                    },
                    FeesByMaterial = GetProducerFeesByMaterial(applyModulation),
                    TonnageChangeCount = null,
                    TonnageChangeAdvice = null
                }
            }
        };
    }

    public static Dictionary<string, Fees> GetProducerFeesByMaterial(bool applyModulation = false)
    {
        return GetProducerDisposalFeesByMaterial(applyModulation)
                    .Join(GetProducerCommsFeesByMaterial(),
                    d1 => d1.Key,
                    d2 => d2.Key,
                    (d1, d2) => (MaterialCode: d1.Key, Fees: new Fees
                    {
                        DisposalFee = d1.Value,
                        CommFee = d2.Value,
                    })).ToDictionary(k => k.MaterialCode, k => k.Fees);
    }

    public static Dictionary<string, DisposalFee> GetProducerDisposalFeesByMaterial(bool applyModulation = false)
    {
        return new Dictionary<string, DisposalFee>
        {
            {
                "AL",
                new DisposalFee
                {
                    HhTonnage = applyModulation
                        ? new RamTonnage
                            {
                                Red          = 100,
                                Amber        = 200,
                                Green        = 300,
                                RedMedical   = 150,
                                AmberMedical = 150,
                                GreenMedical = 100
                            }
                        : new RamTonnage { Amber = 1000 },
                    PbTonnage = applyModulation
                        ? new RamTonnage
                            {
                                Red          = 21,
                                Amber        = 22,
                                Green        = 23,
                                RedMedical   = 24,
                                AmberMedical = 25,
                                GreenMedical = 26
                            }
                        : RamTonnage.Empty,
                    HdcTonnage = RamTonnage.Empty,
                    SmcwTonnage = 90,
                    TotalTonnage = applyModulation
                        ? new RamTonnage
                        {
                            Red = 1,
                            Amber = 2,
                            Green = 3,
                            RedMedical = 4,
                            AmberMedical = 5,
                            GreenMedical = 6
                        }
                        : RamTonnage.Empty,
                    NetTonnage = applyModulation
                        ? new RamTonnageGroup { Total = 910, Red = 300, Amber = 200, Green = 410 }
                        : new RamTonnageGroup { Total = 910, Red = null, Amber = null, Green = null },
                    PricePerTonne = applyModulation
                        ? new RamTonnageGroup { Total = 0.6676m, Red = 1, Amber = 2, Green = 3 }
                        : new RamTonnageGroup { Total = 0.6676m, Red = null, Amber = null, Green = null },
                    Fee = applyModulation
                        ? new RamTonnageGroup { Total = 607.525000m, Red = 4.525001m, Amber = 5, Green = 6 }
                        : new RamTonnageGroup { Total = 607.525000m, Red = null, Amber = null, Green = null },
                    BadDebt = 36.45m,
                    FeeWithBadDebtByCountry = new ByCountryCost { England = 348.06m, Wales = 78.46m, Scotland = 156.28m, NorthernIreland = 61.18m },
                    PreviousInvoicedTonnage = null,
                    TonnageChange = 0,
                    ActionedSmcwTonnage = new RamTonnageGroup { Total = 90, Red = 0, Amber = 90, Green = 0 }
                }
            },
            {
                "FC",
                new DisposalFee
                {
                    HhTonnage = new RamTonnage { Amber = 2000 },
                    PbTonnage = RamTonnage.Empty,
                    HdcTonnage = RamTonnage.Empty,
                    SmcwTonnage = 140,
                    TotalTonnage = applyModulation
                        ? new RamTonnage
                        {
                            Red          = 1,
                            Amber        = 2,
                            Green        = 3,
                            RedMedical   = 4,
                            AmberMedical = 5,
                            GreenMedical = 6
                        }
                        : RamTonnage.Empty,
                    NetTonnage = applyModulation
                        ? new RamTonnageGroup { Total = 1860, Red = 860, Amber = 0, Green = 1000 }
                        : new RamTonnageGroup { Total = 1860, Red = null, Amber = null, Green = null },
                    PricePerTonne = applyModulation
                        ? new RamTonnageGroup { Total = 0.7825m, Red = 1, Amber = 2, Green = 3 }
                        : new RamTonnageGroup { Total = 0.7825m, Red = null, Amber = null, Green = null },
                    Fee = applyModulation
                        ? new RamTonnageGroup { Total = 1455.45m, Red = 4, Amber = 5, Green = 6 }
                        : new RamTonnageGroup { Total = 1455.45m, Red = null, Amber = null, Green = null },
                    BadDebt = 87.33m,
                    FeeWithBadDebtByCountry = new ByCountryCost { England = 833.85m, Wales = 187.96m, Scotland = 374.40m, NorthernIreland = 146.57m },
                    PreviousInvoicedTonnage = 0,
                    TonnageChange = 0,
                    ActionedSmcwTonnage = new RamTonnageGroup { Total = 140, Red = 0, Amber = 90, Green = 140 }
                }
            },
            {
                "GL",
                new DisposalFee
                {
                    HhTonnage = new RamTonnage { Amber = 500 },
                    PbTonnage = RamTonnage.Empty,
                    HdcTonnage = new RamTonnage { Amber = 220 },
                    SmcwTonnage = 150,
                    TotalTonnage = applyModulation
                        ? new RamTonnage
                        {
                            Red          = 1,
                            Amber        = 2,
                            Green        = 3,
                            RedMedical   = 4,
                            AmberMedical = 5,
                            GreenMedical = 6
                        }
                        : RamTonnage.Empty,
                    NetTonnage = applyModulation
                        ? new RamTonnageGroup { Total = 350, Red = 300, Amber = 50, Green = 0 }
                        : new RamTonnageGroup { Total = 350, Red = null, Amber = null, Green = null },
                    PricePerTonne = applyModulation
                        ? new RamTonnageGroup { Total = 6.4404m, Red = 1, Amber = 2, Green = 3 }
                        : new RamTonnageGroup { Total = 6.4404m, Red = null, Amber = null, Green = null },
                    Fee = applyModulation
                        ? new RamTonnageGroup { Total = 2254.14m, Red = 4, Amber = 5, Green = 6 }
                        : new RamTonnageGroup { Total = 2254.14m, Red = null, Amber = null, Green = null },
                    BadDebt = 135.25m,
                    FeeWithBadDebtByCountry = new ByCountryCost { England = 1291.43m, Wales = 291.10m, Scotland = 579.85m, NorthernIreland = 227 },
                    PreviousInvoicedTonnage = 0,
                    TonnageChange = 0,
                    ActionedSmcwTonnage = new RamTonnageGroup { Total = 150, Red = 50, Amber = 100, Green = 0 }
                }
            },
            {
                "PC",
                new DisposalFee
                {
                    HhTonnage = new RamTonnage { Amber = 20 },
                    PbTonnage = RamTonnage.Empty,
                    HdcTonnage = RamTonnage.Empty,
                    SmcwTonnage = 2.200m,
                    TotalTonnage = applyModulation
                        ? new RamTonnage
                        {
                            Red          = 1,
                            Amber        = 2,
                            Green        = 3,
                            RedMedical   = 4,
                            AmberMedical = 5,
                            GreenMedical = 6
                        }
                        : RamTonnage.Empty,
                    NetTonnage = applyModulation
                        ? new RamTonnageGroup { Total = 17.800m, Red = 0, Amber = 0, Green = 0 }
                        : new RamTonnageGroup { Total = 17.800m, Red = null, Amber = null, Green = null },
                    PricePerTonne = applyModulation
                        ? new RamTonnageGroup { Total = 2.4488m, Red = 1, Amber = 2, Green = 3 }
                        : new RamTonnageGroup { Total = 2.4488m, Red = null, Amber = null, Green = null },
                    Fee = applyModulation
                        ? new RamTonnageGroup { Total = 43.59m, Red = 4, Amber = 5, Green = 6 }
                        : new RamTonnageGroup { Total = 43.59m, Red = null, Amber = null, Green = null },
                    BadDebt = 2.62m,
                    FeeWithBadDebtByCountry = new ByCountryCost { England = 24.97m, Wales = 5.63m, Scotland = 11.21m, NorthernIreland = 4.39m },
                    PreviousInvoicedTonnage = 0,
                    TonnageChange = 0,
                    ActionedSmcwTonnage = new RamTonnageGroup { Total = 2.200m, Red = 0, Amber = 2.200m, Green = 0 }
                }
            },
            {
                "PL",
                new DisposalFee
                {
                    HhTonnage = new RamTonnage { Amber = 5.000m },
                    PbTonnage = RamTonnage.Empty,
                    HdcTonnage = RamTonnage.Empty,
                    SmcwTonnage = 0.600m,
                    TotalTonnage = applyModulation
                        ? new RamTonnage
                        {
                            Red          = 1,
                            Amber        = 2,
                            Green        = 3,
                            RedMedical   = 4,
                            AmberMedical = 5,
                            GreenMedical = 6
                        }
                        : RamTonnage.Empty,
                    NetTonnage = applyModulation
                        ? new RamTonnageGroup { Total = 4.400m, Red = 4.400m, Amber = 0, Green = 0 }
                        : new RamTonnageGroup { Total = 4.400m, Red = null, Amber = null, Green = null },
                    PricePerTonne = applyModulation
                        ? new RamTonnageGroup { Total = 2.1601m, Red = 1, Amber = 2, Green = 3 }
                        : new RamTonnageGroup { Total = 2.1601m, Red = null, Amber = null, Green = null },
                    Fee = applyModulation
                        ? new RamTonnageGroup { Total = 9.50m, Red = 4, Amber = 5, Green = 6 }
                        : new RamTonnageGroup { Total = 9.50m, Red = null, Amber = null, Green = null },
                    BadDebt = 0.57m,
                    FeeWithBadDebtByCountry = new ByCountryCost { England = 5.45m, Wales = 1.23m, Scotland = 2.44m, NorthernIreland = 0.96m },
                    PreviousInvoicedTonnage = 0,
                    TonnageChange = 0,
                    ActionedSmcwTonnage = new RamTonnageGroup { Total = 0.600m, Red = 0, Amber = 0.600m, Green = 0 }
                }
            },
            {
                "ST",
                new DisposalFee
                {
                    HhTonnage = new RamTonnage { Amber = 0.000m },
                    PbTonnage = RamTonnage.Empty,
                    HdcTonnage = RamTonnage.Empty,
                    SmcwTonnage = 0.000m,
                    TotalTonnage = applyModulation
                        ? new RamTonnage
                        {
                            Red          = 1,
                            Amber        = 2,
                            Green        = 3,
                            RedMedical   = 4,
                            AmberMedical = 5,
                            GreenMedical = 6
                        }
                        : RamTonnage.Empty,
                    NetTonnage = applyModulation
                        ? new RamTonnageGroup { Total = 0, Red = 0, Amber = 0, Green = 0 }
                        : new RamTonnageGroup { Total = 0, Red = null, Amber = null, Green = null },
                    PricePerTonne = applyModulation
                        ? new RamTonnageGroup { Total = 1.9813m, Red = 1, Amber = 2, Green = 3 }
                        : new RamTonnageGroup { Total = 1.9813m, Red = null, Amber = null, Green = null },
                    Fee = applyModulation
                        ? new RamTonnageGroup { Total = 0.00m, Red = 4, Amber = 5, Green = 6 }
                        : new RamTonnageGroup { Total = 0.00m, Red = null, Amber = null, Green = null },
                    BadDebt = 0.00m,
                    FeeWithBadDebtByCountry = new ByCountryCost { England = 0.00m, Wales = 0.00m, Scotland = 0.00m, NorthernIreland = 0.00m },
                    PreviousInvoicedTonnage = 0,
                    TonnageChange = 0,
                    ActionedSmcwTonnage = new RamTonnageGroup { Total = 0, Red = 0, Amber = 0, Green = 0 }
                }
            },
            {
                "WD",
                new DisposalFee
                {
                    HhTonnage = new RamTonnage { Amber = 500.000m },
                    PbTonnage = RamTonnage.Empty,
                    HdcTonnage = RamTonnage.Empty,
                    SmcwTonnage = 95.000m,
                    TotalTonnage = applyModulation
                        ? new RamTonnage
                        {
                            Red          = 1,
                            Amber        = 2,
                            Green        = 3,
                            RedMedical   = 4,
                            AmberMedical = 5,
                            GreenMedical = 6
                        }
                        : RamTonnage.Empty,
                    NetTonnage = applyModulation
                        ? new RamTonnageGroup { Total = 405.000m, Red = 300, Amber = 100, Green = 5 }
                        : new RamTonnageGroup { Total = 405.000m, Red = null, Amber = null, Green = null },
                    PricePerTonne = applyModulation
                        ? new RamTonnageGroup { Total = 2.0000m, Red = 1, Amber = 2, Green = 3 }
                        : new RamTonnageGroup { Total = 2.0000m, Red = null, Amber = null, Green = null },
                    Fee = applyModulation
                        ? new RamTonnageGroup { Total = 810.00m, Red = 4, Amber = 5, Green = 6 }
                        : new RamTonnageGroup { Total = 810.00m, Red = null, Amber = null, Green = null },
                    BadDebt = 48.60m,
                    FeeWithBadDebtByCountry = new ByCountryCost { England = 464.06m, Wales = 104.60m, Scotland = 208.36m, NorthernIreland = 81.57m },
                    PreviousInvoicedTonnage = 0,
                    TonnageChange = 0,
                    ActionedSmcwTonnage = new RamTonnageGroup { Total = 95, Red = 0, Amber = 95, Green = 0 }
                }
            },
            {
                "OT",
                new DisposalFee
                {
                    HhTonnage = new RamTonnage { Amber = 50.000m },
                    PbTonnage = RamTonnage.Empty,
                    HdcTonnage = RamTonnage.Empty,
                    SmcwTonnage = 5.500m,
                    TotalTonnage = applyModulation
                        ? new RamTonnage
                        {
                            Red          = 1,
                            Amber        = 2,
                            Green        = 3,
                            RedMedical   = 4,
                            AmberMedical = 5,
                            GreenMedical = 6
                        }
                        : RamTonnage.Empty,
                    NetTonnage = applyModulation
                        ? new RamTonnageGroup { Total = 44.500m, Red = 0, Amber = 44.500m, Green = 0 }
                        : new RamTonnageGroup { Total = 44.500m, Red = null, Amber = null, Green = null },
                    PricePerTonne = applyModulation
                        ? new RamTonnageGroup { Total = 1.1954m, Red = 1, Amber = 2, Green = 3 }
                        : new RamTonnageGroup { Total = 1.1954m, Red = null, Amber = null, Green = null },
                    Fee = applyModulation
                        ? new RamTonnageGroup { Total = 53.20m, Red = 4, Amber = 5, Green = 6 }
                        : new RamTonnageGroup { Total = 53.20m, Red = null, Amber = null, Green = null },
                    BadDebt = 3.19m,
                    FeeWithBadDebtByCountry = new ByCountryCost { England = 30.48m, Wales = 6.87m, Scotland = 13.68m, NorthernIreland = 5.36m },
                    PreviousInvoicedTonnage = 0,
                    TonnageChange = 0,
                    ActionedSmcwTonnage = new RamTonnageGroup { Total = 5.500m, Red = 0, Amber = 5.500m, Green = 0 }
                }
            }
        };
    }


    public static Dictionary<string, CommsFee> GetProducerCommsFeesByMaterial()
    {
        return new Dictionary<string, CommsFee>
        {
            {
                "AL",
                new CommsFee
                {
                    HhTonnage = 1000,
                    PricePerTonne = 0.1916m,
                    Costs = new FeeWithBadDebt
                    {
                        FeeWithoutBadDebt = 191.60m,
                        BadDebt           = 11.50m,
                        ByCountry    = new ByCountryCost { England = 106.61m, Wales = 26.91m, Scotland = 49.41m, NorthernIreland = 20.17m },
                    },
                }
            },
            {
                "FC",
                new CommsFee
                {
                    HhTonnage = 2000.000m,
                    PricePerTonne = 0.4032m,
                    Costs = new FeeWithBadDebt
                    {
                        FeeWithoutBadDebt = 806.40m,
                        BadDebt           = 48.38m,
                        ByCountry    = new ByCountryCost { England = 448.70m, Wales = 113.25m, Scotland = 207.94m, NorthernIreland = 84.89m },
                    },
                }
            },
            {
                "GL",
                new CommsFee
                {
                    HhTonnage = 500.000m,
                    PricePerTonne = 0.4404m,
                    Costs = new FeeWithBadDebt
                    {
                        FeeWithoutBadDebt = 220.20m,
                        BadDebt           = 13.21m,
                        ByCountry    = new ByCountryCost { England = 122.53m, Wales = 30.92m, Scotland = 56.78m, NorthernIreland = 23.18m },
                    },
                }
            },
            {
                "PC",
                new CommsFee
                {
                    HhTonnage = 20.000m,
                    PricePerTonne = 1.1100m,
                    Costs = new FeeWithBadDebt
                    {
                        FeeWithoutBadDebt = 22.20m,
                        BadDebt           = 1.33m,
                        ByCountry    = new ByCountryCost { England = 12.35m, Wales = 3.12m, Scotland = 5.72m, NorthernIreland = 2.34m },
                    },
                }
            },
            {
                "PL",
                new CommsFee
                {
                    HhTonnage = 5.000m,
                    PricePerTonne = 0.5356m,
                    Costs = new FeeWithBadDebt
                    {
                        FeeWithoutBadDebt = 2.68m,
                        BadDebt           = 0.16m,
                        ByCountry    = new ByCountryCost { England = 1.49m, Wales = 0.38m, Scotland = 0.69m, NorthernIreland = 0.28m },
                    },
                }
            },
            {
                "ST",
                new CommsFee
                {
                    HhTonnage = 0.000m,
                    PricePerTonne = 0.8879m,
                    Costs = new FeeWithBadDebt
                    {
                        FeeWithoutBadDebt = 0.00m,
                        BadDebt           = 0.00m,
                        ByCountry    = new ByCountryCost { England = 0.00m, Wales = 0.00m, Scotland = 0.00m, NorthernIreland = 0.00m },
                    },
                }
            },
            {
                "WD",
                new CommsFee
                {
                    HhTonnage = 500.000m,
                    PricePerTonne = 0.1364m,
                    Costs = new FeeWithBadDebt
                    {
                        FeeWithoutBadDebt = 68.20m,
                        BadDebt           = 4.09m,
                        ByCountry    = new ByCountryCost { England = 37.95m, Wales = 9.58m, Scotland = 17.59m, NorthernIreland = 7.18m },
                    },
                }
            },
            {
                "OT",
                new CommsFee
                {
                    HhTonnage = 50.000m,
                    PricePerTonne = 0.9540m,
                    Costs = new FeeWithBadDebt
                    {
                        FeeWithoutBadDebt = 47.70m,
                        BadDebt           = 2.86m,
                        ByCountry    = new ByCountryCost { England = 26.54m, Wales = 6.70m, Scotland = 12.30m, NorthernIreland = 5.02m },
                    },
                }
            }
        };
    }

    public static CalcResultScaledupProducers GetScaledupProducers()
    {
        return new CalcResultScaledupProducers
        {
            ScaledupProducers =
            [
                new CalcResultScaledupProducer
                {
                    ProducerId = 1,
                    ProducerName = "Producer Name",
                    DaysInSubmissionPeriod = 91,
                    DaysInWholePeriod = 91,
                    IsSubtotalRow = false,
                    Level = "1",
                    ScaleupFactor = 1,
                    SubmissionPeriodCode = "2024-P2",
                    SubsidiaryId = string.Empty,
                    ScaledupProducerTonnageByMaterial = new Dictionary<string, CalcResultScaledupProducerTonnage>
                    {
                        {
                            "AL",
                            new CalcResultScaledupProducerTonnage
                            {
                                ReportedHouseholdPackagingWasteTonnage = 100,
                                ReportedPublicBinTonnage = 20,
                                TotalReportedTonnage = 120,
                                ReportedSelfManagedConsumerWasteTonnage = 60,
                                NetReportedTonnage = 180,
                                ScaledupReportedHouseholdPackagingWasteTonnage = 200,
                                ScaledupReportedPublicBinTonnage = 40,
                                ScaledupTotalReportedTonnage = 240,
                                ScaledupReportedSelfManagedConsumerWasteTonnage = 120,
                                ScaledupNetReportedTonnage = 360
                            }
                        },
                        {
                            "GL",
                            new CalcResultScaledupProducerTonnage
                            {
                                ReportedHouseholdPackagingWasteTonnage = 100,
                                ReportedPublicBinTonnage = 20,
                                TotalReportedTonnage = 120,
                                ReportedSelfManagedConsumerWasteTonnage = 60,
                                HouseholdDrinksContainersTonnageGlass = 70,
                                NetReportedTonnage = 180,
                                ScaledupReportedHouseholdPackagingWasteTonnage = 200,
                                ScaledupReportedPublicBinTonnage = 40,
                                ScaledupTotalReportedTonnage = 240,
                                ScaledupReportedSelfManagedConsumerWasteTonnage = 120,
                                ScaledupHouseholdDrinksContainersTonnageGlass = 140,
                                ScaledupNetReportedTonnage = 360
                            }
                        }
                    }
                }
            ]
        };
    }

    public static CalcResultPartialObligations GetPartialObligations()
    {
        return new CalcResultPartialObligations
        {
            PartialObligations =
            [
                new CalcResultPartialObligation
                {
                    ProducerId = 1,
                    ProducerName = "Producer Name",
                    DaysObligated = 183,
                    DaysInSubmissionYear = 366,
                    Level = "1",
                    JoiningDate = "15/07/2024",
                    ObligatedFactor = 0.5m,
                    SubmissionYear = 2024,
                    SubsidiaryId = null,
                    PartialObligationTonnageByMaterial = new Dictionary<string, CalcResultPartialObligationTonnage>
                    {
                        {
                            "AL",
                            new CalcResultPartialObligationTonnage
                            {
                                ObligatedFactor = 0.5m,
                                HouseholdTonnage = 100,
                                HouseholdRAMTonnage = new RamTonnage(),
                                PublicBinTonnage = 20,
                                PublicBinRAMTonnage = new RamTonnage(),
                                SelfManagedConsumerWasteTonnage = 60
                            }
                        },
                        {
                            "GL",
                            new CalcResultPartialObligationTonnage
                            {
                                ObligatedFactor = 0.5m,
                                HouseholdTonnage = 100,
                                HouseholdRAMTonnage = new RamTonnage(),
                                PublicBinTonnage = 20,
                                PublicBinRAMTonnage = new RamTonnage(),
                                HouseholdDrinksContainersTonnage = 70,
                                HouseholdDrinksContainersRAMTonnage = new RamTonnage(),
                                SelfManagedConsumerWasteTonnage = 60
                            }
                        }
                    }
                }
            ]
        };
    }

    public static IImmutableList<MaterialDetail> GetMaterialDetails() =>
        MaterialHelper.GetMaterials().ToDetails();

    public static List<ProducerDetail> GetProducers()
    {
        var producers = new List<ProducerDetail>
        {
            new()
            {
                Id = 1,
                ProducerId = 1,
                ProducerName = "Allied Packaging",
                CalculatorRunId = 1,
                CalculatorRun = new CalculatorRun { RelativeYear = new RelativeYear(204), Name = "Test Run 1" }
            },
            new()
            {
                Id = 2,
                ProducerId = 2,
                ProducerName = "Beeline Materials",
                CalculatorRunId = 1,
                CalculatorRun = new CalculatorRun { RelativeYear = new RelativeYear(204), Name = "Test Run 1" }
            },
            new()
            {
                Id = 3,
                ProducerId = 3,
                ProducerName = "Cloud Boxes",
                CalculatorRunId = 1,
                CalculatorRun = new CalculatorRun { RelativeYear = new RelativeYear(204), Name = "Test Run 1" }
            }
        };

        var producerReportedMaterials = GetProducerReportedMaterials();

        producers.ForEach(producer => { producerReportedMaterials.ForEach(producerReportedMaterial => { producer.ProducerReportedMaterials.Add(producerReportedMaterial); }); });

        return producers;
    }

    public static List<ProducerReportedMaterial> GetProducerReportedMaterials()
    {
        var prodMats = new List<ProducerReportedMaterial>();
        foreach (var subPeriod in new[] { "2025-H1", "2025-H2" })
        {
            prodMats.AddRange(new[]
            {
                new ProducerReportedMaterial
                {
                    Material = new Material { Id = 1, Code = "AL", Name = "Aluminium", Description = "Aluminium" },
                    PackagingTonnage = 500.00m,
                    PackagingType = "HH",
                    SubmissionPeriod = subPeriod,
                    MaterialId = 1
                },
                new ProducerReportedMaterial
                {
                    Material = new Material { Id = 1, Code = "AL", Name = "Aluminium", Description = "Aluminium" },
                    PackagingTonnage = 10.00m,
                    PackagingType = "CW",
                    MaterialId = 1,
                    SubmissionPeriod = subPeriod
                },
                new ProducerReportedMaterial
                {
                    Material = new Material { Id = 5, Code = "PL", Name = "Plastic", Description = "Plastic" },
                    PackagingTonnage = 10.00m,
                    PackagingType = "PB",
                    MaterialId = 5,
                    SubmissionPeriod = subPeriod
                },
                new ProducerReportedMaterial
                {
                    Material = new Material { Id = 3, Code = "GL", Name = "Glass", Description = "Glass" },
                    PackagingTonnage = 20.00m,
                    PackagingType = "HH",
                    MaterialId = 3,
                    SubmissionPeriod = subPeriod
                },
                new ProducerReportedMaterial
                {
                    Material = new Material { Id = 3, Code = "GL", Name = "Glass", Description = "Glass" },
                    PackagingTonnage = 10.00m,
                    PackagingType = "HDC",
                    MaterialId = 3,
                    SubmissionPeriod = subPeriod
                },
                new ProducerReportedMaterial
                {
                    Material = new Material { Id = 3, Code = "GL", Name = "Glass", Description = "Glass" },
                    PackagingTonnage = 50.00m,
                    PackagingType = "CW",
                    MaterialId = 3,
                    SubmissionPeriod = subPeriod
                }
            });
        }

        return prodMats;
    }

    public static IEnumerable<DefaultParameterTemplateMaster> GetDefaultParameterTemplateMasterData()
    {
        var list = new List<DefaultParameterTemplateMaster>();
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "COMC-ENG",
            ParameterCategory = "England",
            ParameterType = "Communication costs by country",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "COMC-NIR",
            ParameterCategory = "Northern Ireland",
            ParameterType = "Communication costs by country",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "COMC-SCT",
            ParameterCategory = "Scotland",
            ParameterType = "Communication costs by country",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "COMC-UK",
            ParameterCategory = "United Kingdom",
            ParameterType = "Communication costs by country",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "COMC-WLS",
            ParameterCategory = "Wales",
            ParameterType = "Communication costs by country",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "COMC-AL",
            ParameterCategory = "Aluminium",
            ParameterType = "Communication costs by material",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "COMC-FC",
            ParameterCategory = "Fibre composite",
            ParameterType = "Communication costs by material",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "COMC-GL",
            ParameterCategory = "Glass",
            ParameterType = "Communication costs by material",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "COMC-PC",
            ParameterCategory = "Paper or card",
            ParameterType = "Communication costs by material",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "COMC-PL",
            ParameterCategory = "Plastic",
            ParameterType = "Communication costs by material",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "COMC-ST",
            ParameterCategory = "Steel",
            ParameterType = "Communication costs by material",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "COMC-WD",
            ParameterCategory = "Wood",
            ParameterType = "Communication costs by material",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "LRET-FC-R",
            ParameterCategory = "Fibre composite-R",
            ParameterType = "Late reporting tonnage",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "LRET-FC",
            ParameterCategory = "Fibre composite-A",
            ParameterType = "Late reporting tonnage",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "LRET-FC-G",
            ParameterCategory = "Fibre composite-G",
            ParameterType = "Late reporting tonnage",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "LRET-GL-R",
            ParameterCategory = "Glass-R",
            ParameterType = "Late reporting tonnage",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "LRET-GL",
            ParameterCategory = "Glass-A",
            ParameterType = "Late reporting tonnage",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "LRET-GL-G",
            ParameterCategory = "Glass-G",
            ParameterType = "Late reporting tonnage",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "LRET-AL-R",
            ParameterCategory = "Aluminium-R",
            ParameterType = "Late reporting tonnage",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "LRET-AL",
            ParameterCategory = "Aluminium-A",
            ParameterType = "Late reporting tonnage",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "LRET-AL-G",
            ParameterCategory = "Aluminium-G",
            ParameterType = "Late reporting tonnage",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "LRET-WD-R",
            ParameterCategory = "Wood-R",
            ParameterType = "Late reporting tonnage",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "LRET-WD",
            ParameterCategory = "Wood-A",
            ParameterType = "Late reporting tonnage",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "LRET-WD-G",
            ParameterCategory = "Wood-G",
            ParameterType = "Late reporting tonnage",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "LRET-ST-R",
            ParameterCategory = "Steel-R",
            ParameterType = "Late reporting tonnage",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "LRET-ST",
            ParameterCategory = "Steel-A",
            ParameterType = "Late reporting tonnage",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "LRET-ST-G",
            ParameterCategory = "Steel-G",
            ParameterType = "Late reporting tonnage",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "LRET-PC-R",
            ParameterCategory = "Paper or card-R",
            ParameterType = "Late reporting tonnage",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "LRET-PC",
            ParameterCategory = "Paper or card-A",
            ParameterType = "Late reporting tonnage",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "LRET-PL-G",
            ParameterCategory = "Plastic-G",
            ParameterType = "Late reporting tonnage",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "LRET-OT-R",
            ParameterCategory = "Other materials-R",
            ParameterType = "Late reporting tonnage",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "LRET-OT",
            ParameterCategory = "Other materials-A",
            ParameterType = "Late reporting tonnage",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "LRET-OT-G",
            ParameterCategory = "Other materials-G",
            ParameterType = "Late reporting tonnage",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "LAPC-ENG",
            ParameterCategory = "England",
            ParameterType = "Local authority data preparation costs",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "LAPC-NIR",
            ParameterCategory = "Northern Ireland",
            ParameterType = "Local authority data preparation costs",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "LAPC-SCT",
            ParameterCategory = "Scotland",
            ParameterType = "Local authority data preparation costs",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "LAPC-WLS",
            ParameterCategory = "Wales",
            ParameterType = "Local authority data preparation costs",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "MATT-AD",
            ParameterCategory = "Amount Decrease",
            ParameterType = "Materiality threshold",
            ValidRangeFrom = -999999999.990m,
            ValidRangeTo = 0.00m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "MATT-AI",
            ParameterCategory = "Amount Increase",
            ParameterType = "Materiality threshold",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "MATT-PD",
            ParameterCategory = "Percent Decrease",
            ParameterType = "Materiality threshold",
            ValidRangeFrom = -999.990m,
            ValidRangeTo = 0.00m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "MATT-PI",
            ParameterCategory = "Percent Increase",
            ParameterType = "Materiality threshold",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999.990m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "COMC-OT",
            ParameterCategory = "Other",
            ParameterType = "Other materials",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "BADEBT-P",
            ParameterCategory = "Bad debt provision",
            ParameterType = "Percentage",
            ValidRangeFrom = 0m,
            ValidRangeTo = 1000.000m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "SAOC-ENG",
            ParameterCategory = "England",
            ParameterType = "Scheme administrator operating costs",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "SAOC-NIR",
            ParameterCategory = "Northern Ireland",
            ParameterType = "Scheme administrator operating costs",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "SAOC-SCT",
            ParameterCategory = "Scotland",
            ParameterType = "Scheme administrator operating costs",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "SAOC-WLS",
            ParameterCategory = "Wales",
            ParameterType = "Scheme administrator operating costs",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "SCSC-ENG",
            ParameterCategory = "England",
            ParameterType = "Scheme setup costs",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "SCSC-NIR",
            ParameterCategory = "Northern Ireland",
            ParameterType = "Scheme setup costs",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "SCSC-SCT",
            ParameterCategory = "Scotland",
            ParameterType = "Scheme setup costs",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "SCSC-WLS",
            ParameterCategory = "Wales",
            ParameterType = "Scheme setup costs",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "TONT-AD",
            ParameterCategory = "Amount Decrease",
            ParameterType = "Tonnage change threshold",
            ValidRangeFrom = -999999999.990m,
            ValidRangeTo = 0.00m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "TONT-AI",
            ParameterCategory = "Amount Increase",
            ParameterType = "Tonnage change threshold",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999999999.99m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "TONT-PD",
            ParameterCategory = "Percent Decrease",
            ParameterType = "Tonnage change threshold",
            ValidRangeFrom = -999.990m,
            ValidRangeTo = 0.00m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "TONT-PI",
            ParameterCategory = "Percent Increase",
            ParameterType = "Tonnage change threshold",
            ValidRangeFrom = 0m,
            ValidRangeTo = 999.990m
        });
        list.Add(new DefaultParameterTemplateMaster
        {
            ParameterUniqueReferenceId = "REDM-RF",
            ParameterCategory = "Modulation Factor",
            ParameterType = "Red modulation factor",
            ValidRangeFrom = 1.000m,
            ValidRangeTo = 2.000m
        });
        return list;
    }

    public static ImmutableList<CalculatorRun> GetCaculatorRuns()
    {
        return
        [
            new CalculatorRun
            {
                Id = 1,
                CalculatorRunClassificationId = 3,
                Name = "Test Run 1",
                RelativeYear = new RelativeYear(204),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Test user",
                CalculatorRunOrganisationDataMasterId = 1,
                CalculatorRunPomDataMasterId = 1,
                DefaultParameterSettingMasterId = 1,
                LapcapDataMasterId = 1
            },
            new CalculatorRun
            {
                Id = 2,
                CalculatorRunClassificationId = 2,
                Name = "Test Run 2",
                RelativeYear = new RelativeYear(204),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Test user",
                CalculatorRunOrganisationDataMasterId = 2,
                CalculatorRunPomDataMasterId = 2,
                DefaultParameterSettingMasterId = 1,
                LapcapDataMasterId = 1
            }
        ];
    }

    public static ImmutableList<CalculatorRunOrganisationDataDetail> GetCalculatorRunOrganisationDataDetails(CalculatorRunOrganisationDataMaster? master = null)
    {
        master ??= GetCalculatorRunOrganisationDataMaster()[0];
        var submitterId1 = Guid.NewGuid();

        return
        [
            new CalculatorRunOrganisationDataDetail
            {
                Id = 1,
                OrganisationId = 1,
                SubsidiaryId = null,
                OrganisationName = "Allied Packaging",
                TradingName = "Allied Trading",
                LoadTimeStamp = DateTime.UtcNow,
                ObligationStatus = ObligationStates.Obligated,
                SubmitterId = submitterId1,
                CalculatorRunOrganisationDataMasterId = master.Id
            },
            new CalculatorRunOrganisationDataDetail
            {
                Id = 2,
                OrganisationId = 1,
                SubsidiaryId = "901",
                OrganisationName = "Allied Subsidiary",
                LoadTimeStamp = DateTime.UtcNow,
                CalculatorRunOrganisationDataMasterId = master.Id
            },
            new CalculatorRunOrganisationDataDetail
            {
                Id = 3,
                OrganisationId = 2,
                SubsidiaryId = null,
                OrganisationName = "",
                TradingName = "",
                LoadTimeStamp = DateTime.UtcNow,
                CalculatorRunOrganisationDataMasterId = master.Id
            },
            new CalculatorRunOrganisationDataDetail
            {
                Id = 4,
                OrganisationId = 2,
                SubsidiaryId = "Sub 2",
                OrganisationName = "",
                LoadTimeStamp = DateTime.UtcNow,
                CalculatorRunOrganisationDataMasterId = master.Id
            },
            new CalculatorRunOrganisationDataDetail
            {
                Id = 5,
                OrganisationId = 1,
                SubsidiaryId = "Sub 1",
                OrganisationName = "Allied Packaging sub 1",
                TradingName = "Allied Trading sub 1",
                LoadTimeStamp = DateTime.UtcNow,
                ObligationStatus = ObligationStates.Obligated,
                SubmitterId = submitterId1,
                CalculatorRunOrganisationDataMasterId = master.Id
            },
            new CalculatorRunOrganisationDataDetail
            {
                Id = 6,
                OrganisationId = 1,
                SubsidiaryId = "Sub 2",
                OrganisationName = "Allied Packaging sub 2",
                TradingName = "Allied Trading sub 2",
                LoadTimeStamp = DateTime.UtcNow,
                ObligationStatus = ObligationStates.Obligated,
                SubmitterId = submitterId1,
                CalculatorRunOrganisationDataMasterId = master.Id
            },
            new CalculatorRunOrganisationDataDetail
            {
                Id = 7,
                OrganisationId = 1,
                SubsidiaryId = null,
                OrganisationName = "Allied Packaging",
                TradingName = "Allied Trading - Old Compliance Scheme",
                LoadTimeStamp = DateTime.UtcNow,
                ObligationStatus = ObligationStates.NotObligated,
                CalculatorRunOrganisationDataMasterId = master.Id,
                SubmitterId = submitterId1
            },
            new CalculatorRunOrganisationDataDetail
            {
                Id = 8,
                OrganisationId = 1,
                SubsidiaryId = "Sub 1",
                OrganisationName = "Allied Packaging sub 1 - Old Compliance Scheme",
                TradingName = "Allied Trading",
                ObligationStatus = ObligationStates.NotObligated,
                LoadTimeStamp = DateTime.UtcNow,
                CalculatorRunOrganisationDataMasterId = master.Id,
                SubmitterId = submitterId1
            },
            new CalculatorRunOrganisationDataDetail
            {
                Id = 9,
                OrganisationId = 1,
                SubsidiaryId = "Sub 2",
                OrganisationName = "Allied Packaging sub 2 - Old Compliance Scheme",
                TradingName = "Allied Trading",
                ObligationStatus = ObligationStates.NotObligated,
                LoadTimeStamp = DateTime.UtcNow,
                CalculatorRunOrganisationDataMasterId = master.Id
            }
        ];
    }

    public static ImmutableList<CalculatorRunOrganisationDataMaster> GetCalculatorRunOrganisationDataMaster(RelativeYear relativeYear = default)
    {
        relativeYear = relativeYear == default ? new RelativeYear(2024) : relativeYear;

        return
        [
            new CalculatorRunOrganisationDataMaster
            {
                Id = 1,
                RelativeYear = relativeYear,
                EffectiveFrom = DateTime.UtcNow,
                EffectiveTo = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Test user"
            },
            new CalculatorRunOrganisationDataMaster
            {
                Id = 2,
                RelativeYear = relativeYear,
                EffectiveFrom = DateTime.UtcNow,
                EffectiveTo = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Test user"
            }
        ];
    }

    public static void SeedDatabaseForInitialRun(ApplicationDBContext dbContext, RunContext? runContext = null)
    {
        runContext ??= CalculatorRun2025;

        dbContext.CalculatorRunRelativeYears.AddRange(
                new CalculatorRunRelativeYear { Value = new RelativeYear(2024) },
                new CalculatorRunRelativeYear { Value = new RelativeYear(2025) });

        var dataMasters = GetCalculatorRunOrganisationDataMaster();
        dbContext.CalculatorRunOrganisationDataMaster.AddRange(dataMasters);

        var dataDetails = GetCalculatorRunOrganisationDataDetails(dataMasters[0]);
        dbContext.CalculatorRunOrganisationDataDetails.AddRange(dataDetails);

        List<CalculatorRun> runs =
        [
            new() { Id = runContext.RunId,     RelativeYear = runContext.RelativeYear, CalculatorRunClassificationId = 7, Name = "CalculatorRunTest1", CalculatorRunOrganisationDataMaster = dataMasters[0] },
            new() { Id = runContext.RunId + 1, RelativeYear = runContext.RelativeYear, CalculatorRunClassificationId = 2, Name = "CalculatorRunTest2", CalculatorRunOrganisationDataMaster = dataMasters[0] }
        ];
        dbContext.CalculatorRuns.AddRange(runs);

        List<ProducerDetail> producerDetails =
        [
            new() { Id = 1, CalculatorRun = runs[0], ProducerName = "Test1", ProducerId = 1, TradingName = "TN1" },
            new() { Id = 2, CalculatorRun = runs[0], ProducerName = "Test2", ProducerId = 2, TradingName = "TN2" },
            new() { Id = 3, CalculatorRun = runs[1], ProducerName = "Test1", ProducerId = 1, TradingName = "TN3" },
            new() { Id = 4, CalculatorRun = runs[0], ProducerName = "Test3", ProducerId = 3, TradingName = "TN4" }
        ];
        dbContext.ProducerDetail.AddRange(producerDetails);

        var materials = MaterialHelper.GetMaterials();
        dbContext.Material.AddRange(materials);

        List<ProducerReportedMaterial> producerReportedMaterials =
        [
            new() { ProducerDetail = producerDetails[0], Material = materials[0], SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.Household                , PackagingTonnage =  50, PackagingTonnageRed          = 50 },
            new() { ProducerDetail = producerDetails[0], Material = materials[0], SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.Household                , PackagingTonnage =  50, PackagingTonnageAmber        = 50 },
            new() { ProducerDetail = producerDetails[0], Material = materials[1], SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.PublicBin                , PackagingTonnage = 100, PackagingTonnageGreen        = 100 },
            new() { ProducerDetail = producerDetails[0], Material = materials[1], SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.PublicBin                , PackagingTonnage = 100, PackagingTonnageRedMedical   = 100 },
            new() { ProducerDetail = producerDetails[0], Material = materials[3], SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 150, PackagingTonnageAmberMedical = 150 },
            new() { ProducerDetail = producerDetails[0], Material = materials[3], SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 150, PackagingTonnageGreenMedical = 150 },
            new() { ProducerDetail = producerDetails[1], Material = materials[0], SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.Household                , PackagingTonnage =  50, PackagingTonnageRed          = 50 },
            new() { ProducerDetail = producerDetails[1], Material = materials[0], SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.Household                , PackagingTonnage =  50, PackagingTonnageAmber        = 50 },
            new() { ProducerDetail = producerDetails[1], Material = materials[1], SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.PublicBin                , PackagingTonnage = 100, PackagingTonnageGreen        = 100 },
            new() { ProducerDetail = producerDetails[1], Material = materials[1], SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.PublicBin                , PackagingTonnage = 100, PackagingTonnageRedMedical   = 100 },
            new() { ProducerDetail = producerDetails[1], Material = materials[3], SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 150, PackagingTonnageAmberMedical = 150 },
            new() { ProducerDetail = producerDetails[1], Material = materials[3], SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 150, PackagingTonnageGreenMedical = 150 }
        ];
        dbContext.ProducerReportedMaterial.AddRange(producerReportedMaterials);

        var designatedRunInvoice = new List<ProducerDesignatedRunInvoiceInstruction>
        {
            new()
            {
                BillingInstructionId = "1_1",
                CalculatorRunId = runs[0].Id,
                CurrentYearInvoicedTotalAfterThisRun = 100,
                Id = 1,
                ProducerId = 1,
                InvoiceAmount = 100,
                OutstandingBalance = 100
            },
            new()
            {
                BillingInstructionId = "1_2",
                CalculatorRunId = runs[0].Id,
                CurrentYearInvoicedTotalAfterThisRun = 100,
                Id = 2,
                ProducerId = 2,
                InvoiceAmount = 100,
                OutstandingBalance = 100
            }
        };


        dbContext.ProducerDesignatedRunInvoiceInstruction.AddRange(designatedRunInvoice);


        var billingInstructionList = new List<ProducerResultFileSuggestedBillingInstruction>
        {
            new()
            {
                MaterialPercentageThresholdBreached = "1%",
                MaterialPoundThresholdBreached = "1",
                ProducerId = 1,
                SuggestedBillingInstruction = "Initial",
                SuggestedInvoiceAmount = 100,
                CalculatorRunId = runs[0].Id,
                BillingInstructionAcceptReject = "Accepted"
            },
            new()
            {
                MaterialPercentageThresholdBreached = "1%",
                MaterialPoundThresholdBreached = "1",
                ProducerId = 2,
                SuggestedBillingInstruction = "Initial",
                SuggestedInvoiceAmount = 100,
                CalculatorRunId = runs[0].Id,
                BillingInstructionAcceptReject = "Accepted"
            },
            new()
            {
                MaterialPercentageThresholdBreached = "1%",
                MaterialPoundThresholdBreached = "1",
                ProducerId = 3,
                SuggestedBillingInstruction = "Initial",
                SuggestedInvoiceAmount = 100,
                CalculatorRunId = runs[0].Id,
                BillingInstructionAcceptReject = "Accepted"
            }
        };
        dbContext.ProducerResultFileSuggestedBillingInstruction.AddRange(billingInstructionList);

        var materialInvoiceTonnage = new List<ProducerInvoicedMaterialNetTonnage>
        {
            new()
            {
                Id = 1,
                CalculatorRunId = runs[0].Id,
                MaterialId = 1,
                InvoicedNetTonnage = 100,
                ProducerId = 1
            },
            new()
            {
                Id = 2,
                CalculatorRunId = runs[0].Id,
                MaterialId = 2,
                InvoicedNetTonnage = 100,
                ProducerId = 1
            },
            new()
            {
                Id = 3,
                CalculatorRunId = runs[0].Id,
                MaterialId = 1,
                InvoicedNetTonnage = 100,
                ProducerId = 2
            },
            new()
            {
                Id = 4,
                CalculatorRunId = runs[0].Id,
                MaterialId = 2,
                InvoicedNetTonnage = 100,
                ProducerId = 2
            }
        };
        dbContext.ProducerInvoicedMaterialNetTonnage.AddRange(materialInvoiceTonnage);

        dbContext.SaveChanges();
    }
}
