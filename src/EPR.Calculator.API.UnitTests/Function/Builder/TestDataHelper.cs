using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Enums;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;

namespace EPR.Calculator.Service.Function.UnitTests.Builder
{
    public static class TestDataHelper
    {
        public static CalcResult GetCalcResult(bool applyModulation = false)
        {
            return new CalcResult
            {
                ApplyModulation = applyModulation,
                CalcResultScaledupProducers = GetScaledupProducers(),
                CalcResultPartialObligations = GetPartialObligations(),
                CalcResultParameterOtherCost = GetCalcResultParameterOtherCost(),
                CalcResultDetail = GetCalcResultDetail(),
                CalcResultLapcapData = GetCalcResultLapcapData(),
                CalcResultLateReportingTonnageData = GetCalcResultLateReportingTonnage(),
                CalcResultSummary = GetCalcResultSummary(applyModulation),
                CalcResultProjectedProducers = new CalcResultProjectedProducers(),
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
                MaterialityIncrease   = new Materiality { Amount =  5000, Percentage =    2m },
                MaterialityDecrease   = new Materiality { Amount = -1000, Percentage =   -1m },
                TonnageChangeIncrease = new Materiality { Amount =    50, Percentage =    2m },
                TonnageChangeDecrease = new Materiality { Amount =   -10, Percentage = -0.5m }
            };
        }

        public static CalcResultDetail GetCalcResultDetail()
        {
            return new CalcResultDetail { RunId = 1, RelativeYear = new RelativeYear(2024) };
        }

        public static CalcResultLaDisposalCostData GetCalcResultLaDisposalCostData()
        {
            return new CalcResultLaDisposalCostData
            {
                ByMaterial = new Dictionary<string, CalcResultLaDisposalCostDataDetail>
                {
                    ["AL"] =
                        new CalcResultLaDisposalCostDataDetail
                        {
                            Cost = new() { England = 5000, Wales = 1750, Scotland = 2000, NorthernIreland = 1250 },
                            HouseholdPackagingWasteTonnage = 6980,
                            PublicBinTonnage = 2000,
                            HouseholdDrinkContainersTonnage = 0,
                            LateReportingTonnage = 8000
                        },
                    ["FC"] =
                        new CalcResultLaDisposalCostDataDetail
                        {
                            Cost = new() { England = 7500, Wales = 2100, Scotland = 3400, NorthernIreland = 1750 },
                            HouseholdPackagingWasteTonnage = 11850,
                            PublicBinTonnage = 2000,
                            HouseholdDrinkContainersTonnage = 0,
                            LateReportingTonnage = 7000
                        },
                    ["GL"] =
                        new CalcResultLaDisposalCostDataDetail
                        {
                            Cost = new() { England = 45000, Wales = 0, Scotland = 20700, NorthernIreland = 4500 },
                            HouseholdPackagingWasteTonnage = 4900,
                            PublicBinTonnage = 2000,
                            HouseholdDrinkContainersTonnage = 100,
                            LateReportingTonnage = 6000
                        },
                    ["PC"] =
                        new CalcResultLaDisposalCostDataDetail
                        {
                            Cost = new() { England = 12500, Wales = 2300, Scotland = 4500, NorthernIreland = 3400 },
                            HouseholdPackagingWasteTonnage = 4270,
                            PublicBinTonnage = 2000,
                            HouseholdDrinkContainersTonnage = 0,
                            LateReportingTonnage = 5000
                        },
                    ["PL"] =
                        new CalcResultLaDisposalCostDataDetail
                        {
                            Cost = new() { England = 23000, Wales = 4500, Scotland = 6700, NorthernIreland = 2100 },
                            HouseholdPackagingWasteTonnage = 12805,
                            PublicBinTonnage = 2000,
                            HouseholdDrinkContainersTonnage = 0,
                            LateReportingTonnage = 4000
                        },
                    ["ST"] =
                        new CalcResultLaDisposalCostDataDetail
                        {
                            Cost = new() { England = 13400, Wales = 0, Scotland = 7800, NorthernIreland = 0 },
                            HouseholdPackagingWasteTonnage = 7700,
                            PublicBinTonnage = 2000,
                            HouseholdDrinkContainersTonnage = 0,
                            LateReportingTonnage = 3000
                        },
                    ["WD"] =
                        new CalcResultLaDisposalCostDataDetail
                        {
                            Cost = new() { England = 0, Wales = 12000, Scotland = 0, NorthernIreland = 5600 },
                            HouseholdPackagingWasteTonnage = 6800,
                            PublicBinTonnage = 2000,
                            HouseholdDrinkContainersTonnage = 0,
                            LateReportingTonnage = 2000
                        },
                    ["OT"] =
                        new CalcResultLaDisposalCostDataDetail
                        {
                            Cost = new() { England = 3400, Wales = 2100, Scotland = 4200, NorthernIreland = 700 },
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
                    ["AL"] = new ByCountryCost { England =  5000, Wales =  1750, Scotland =  2000, NorthernIreland = 1250 },
                    ["FC"] = new ByCountryCost { England =  7500, Wales =  2100, Scotland =  3400, NorthernIreland = 1750 },
                    ["GL"] = new ByCountryCost { England = 45000, Wales =     0, Scotland = 20700, NorthernIreland = 4500 },
                    ["PC"] = new ByCountryCost { England = 12500, Wales =  2300, Scotland =  4500, NorthernIreland = 3400 },
                    ["PL"] = new ByCountryCost { England = 23000, Wales =  4500, Scotland =  6700, NorthernIreland = 2100 },
                    ["ST"] = new ByCountryCost { England = 13400, Wales =     0, Scotland =  7800, NorthernIreland =    0 },
                    ["WD"] = new ByCountryCost { England =     0, Wales = 12000, Scotland =     0, NorthernIreland = 5600 },
                    ["OT"] = new ByCountryCost { England =  3400, Wales =  2100, Scotland =  4200, NorthernIreland =  700 }
                }
            };
        }

        public static CalcResultOnePlusFourApportionment GetCalcResultOnePlusFourApportionment()
        {
            return new CalcResultOnePlusFourApportionment
            {
                LaDisposalCost   = new() { England = 30, Wales = 5, Scotland = 15, NorthernIreland = 35 },
                LADataPrepCharge = new() { England = 10, Wales = 5, Scotland =  0, NorthernIreland =  0 }
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
                    NorthernIreland =  8.98m
                },
                ByMaterial = new ()
                {
                    ["AL"] = new ()
                    {
                        Cost = ByCountryCost.Empty with { England = 4.347m },
                        TotalCost = 4.347m,
                        HouseholdPackagingWasteTonnage = 2.34m,
                        PublicBinTonnage = 4.56m,
                        HouseholdDrinksContainersTonnage = 0m,
                        LateReportingTonnage = 3.45m
                    },
                    ["GL"] = new ()
                    {
                        Cost = ByCountryCost.Empty,
                        TotalCost = 0m,
                        HouseholdPackagingWasteTonnage = 3.45m,
                        PublicBinTonnage = 5.67m,
                        HouseholdDrinksContainersTonnage = 1.23m,
                        LateReportingTonnage = 4.56m
                    }
                },
                CommsCostUkWide    = new () { England = 1500, Wales = 200, Scotland = 500, NorthernIreland = 331 },
                CommsCostByCountry = new () { England = 1400, Wales = 250, Scotland = 600, NorthernIreland = 280 }
            };
        }

        public static CalcResultLateReportingTonnage GetCalcResultLateReportingTonnage()
        {
            return new CalcResultLateReportingTonnage
            {
                ByMaterial = new Dictionary<string, CalcResultLateReportingTonnageDetail>
                {
                    ["AL"] = new() { Red = 1000.00m, Amber = 2000.00m, Green = 5000.00m, Total = 8000.00m },
                    ["FC"] = new() { Red =    5.00m, Amber =       0m, Green =    5.00m, Total =   10.00m },
                    ["GL"] = new() { Red =   10.00m, Amber =       0m, Green =    0.00m, Total =   10.00m },
                    ["PC"] = new() { Red =    0.00m, Amber =       0m, Green =    0.00m, Total =    0.00m },
                    ["PL"] = new() { Red = 1000.00m, Amber =  500.00m, Green =  500.00m, Total = 2000.00m },
                    ["ST"] = new() { Red =    0.00m, Amber =    0.00m, Green =    0.00m, Total =    0.00m },
                    ["WD"] = new() { Red =    0.00m, Amber =    0.00m, Green =    0.00m, Total =    0.00m },
                    ["OT"] = new() { Red =    0.00m, Amber =    0.00m, Green =    0.00m, Total =    0.00m }
                }
            };
        }

        public static CalcResultSummary GetCalcResultSummary(bool applyModulation = false)
        {
            return new CalcResultSummary
            {
                TotalFeeforLADisposalCostswoBadDebtprovision1 = 4423.39438M,
                BadDebtProvisionFor1 = 6021.3677166M,
                TotalFeeforLADisposalCostswithBadDebtprovision1 = 4688.7980428M,
                TotalFeeforCommsCostsbyMaterialwoBadDebtProvision2A = 1290.778M,
                BadDebtProvisionFor2A = 2098.887360M,
                TotalFeeforCommsCostsbyMaterialwithBadDebtprovision2A = 1368.22468M,
                SaOperatingCostsWoTitleSection3 = 3077.2243678810364M,
                BadDebtProvisionTitleSection3 = 3900.000000M,
                SaOperatingCostsWithTitleSection3 = 3261.8578299538985M,
                LaDataPrepCostsTitleSection4 = 1727.9798373485821M,
                LaDataPrepCostsBadDebtProvisionTitleSection4 = 103.67879024091492M,
                LaDataPrepCostsWithBadDebtProvisionTitleSection4 = 1831.658627589497M,
                SaSetupCostsTitleSection5 = 17500.00M,
                SaSetupCostsBadDebtProvisionTitleSection5 = 1050.00M,
                SaSetupCostsWithBadDebtProvisionTitleSection5 = 18550.00M,
                TwoCCommsCostsByCountryWithoutBadDebtProvision = 1339.100071422903M,
                TwoCBadDebtProvision = 80.34600428537418M,
                TwoCCommsCostsByCountryWithBadDebtProvision = 1419.446075708277M,
                CommsCostHeaderWithoutBadDebtFor2bTitle = 1339.100071422903M,
                CommsCostHeaderWithBadDebtFor2bTitle = 1419.446075708277M,
                CommsCostHeaderBadDebtProvisionFor2bTitle = 80.34600428537418M,
                TotalOnePlus2A2B2CFeeWithBadDebtProvision = 10230.2550766M,
                ProducerDisposalFees = GetProducerDisposalFees(applyModulation),
            };
        }

        public static List<CalcResultSummaryProducerDisposalFees> GetProducerDisposalFees(bool applyModulation = false)
        {
            return new List<CalcResultSummaryProducerDisposalFees>
            {
                new CalcResultSummaryProducerDisposalFees
                {
                    ProducerId = "1",
                    ProducerIdInt = 1,
                    SubsidiaryId = string.Empty,
                    ProducerName = "Allied Packaging",
                    Level = "1",
                    isTotalRow = false,
                    TotalProducerDisposalFee = 4423.39438m,
                    BadDebtProvision = 265.4036628m,
                    TotalProducerDisposalFeeWithBadDebtProvision = 4688.7980428m,
                    EnglandTotal = 2534.2359097426884m,
                    WalesTotal = 571.2417008090152m,
                    ScotlandTotal = 1137.8673076088023m,
                    NorthernIrelandTotal = 445.4531246394942m,
                    TotalProducerCommsFee = 1290.778m,
                    BadDebtProvisionComms = 77.44668m,
                    TotalProducerCommsFeeWithBadDebtProvision = 1368.22468m,
                    EnglandTotalComms = 718.2251815154783m,
                    WalesTotalComms = 181.2690740598454m,
                    ScotlandTotalComms = 332.8499847265775m,
                    NorthernIrelandTotalComms = 135.88043969809883m,
                    LocalAuthorityDisposalCostsSectionOne = new CalcResultSummaryBadDebtProvision
                    {
                        TotalProducerFeeWithoutBadDebtProvision = 4423.39438m,
                        BadDebtProvision = 265.4036628m,
                        TotalProducerFeeWithBadDebtProvision = 4688.7980428m,
                        EnglandTotalWithBadDebtProvision = 2534.2359097426884m,
                        WalesTotalWithBadDebtProvision = 571.2417008090152m,
                        ScotlandTotalWithBadDebtProvision = 1137.8673076088023m,
                        NorthernIrelandTotalWithBadDebtProvision = 445.4531246394942m
                    },
                    CommunicationCostsSectionTwoA = new CalcResultSummaryBadDebtProvision
                    {
                        TotalProducerFeeWithoutBadDebtProvision = 1290.778m,
                        BadDebtProvision = 77.44668m,
                        TotalProducerFeeWithBadDebtProvision = 1368.22468m,
                        EnglandTotalWithBadDebtProvision = 718.2251815154783m,
                        WalesTotalWithBadDebtProvision = 181.2690740598454m,
                        ScotlandTotalWithBadDebtProvision = 332.8499847265775m,
                        NorthernIrelandTotalWithBadDebtProvision = 135.88043969809883m
                    },
                    TwoCTotalProducerFeeForCommsCostsWithoutBadDebt = 1339.100071422903m,
                    TwoCBadDebtProvision = 80.34600428537418m,
                    TwoCTotalProducerFeeForCommsCostsWithBadDebt = 1419.446075708277m,
                    TwoCEnglandTotalWithBadDebt = 607.4748035870169m,
                    TwoCWalesTotalWithBadDebt = 300.7301007856519m,
                    TwoCScotlandTotalWithBadDebt = 360.87612094278234m,
                    TwoCNorthernIrelandTotalWithBadDebt = 150.36505039282596m,
                    PercentageofProducerReportedTonnagevsAllProducers = 5.6741528450123m,
                    ProducerTotalOnePlus2A2B2CWithBadDeptProvision = 10491.167766844124m,
                    ProducerOverallPercentageOfCostsForOnePlus2A2B2C = 4.7341913352015945m,
                    SchemeAdministratorOperatingCosts = new CalcResultSummaryBadDebtProvision
                    {
                        TotalProducerFeeWithoutBadDebtProvision = 3077.2243678810364m,
                        BadDebtProvision = 184.6334620728622m,
                        TotalProducerFeeWithBadDebtProvision = 3261.8578299538985m,
                        EnglandTotalWithBadDebtProvision = 1712.2541832180282m,
                        WalesTotalWithBadDebtProvision = 432.1468228710047m,
                        ScotlandTotalWithBadDebtProvision = 793.5168432560496m,
                        NorthernIrelandTotalWithBadDebtProvision = 323.93998060881614m
                    },
                    LocalAuthorityDataPreparationCosts = new CalcResultSummaryBadDebtProvision
                    {
                        TotalProducerFeeWithoutBadDebtProvision = 1727.9798373485821m,
                        BadDebtProvision = 103.67879024091492m,
                        TotalProducerFeeWithBadDebtProvision = 1831.658627589497m,
                        EnglandTotalWithBadDebtProvision = 802.9188504501905m,
                        WalesTotalWithBadDebtProvision = 351.2769970719583m,
                        ScotlandTotalWithBadDebtProvision = 451.6418533782321m,
                        NorthernIrelandTotalWithBadDebtProvision = 225.82092668911605m
                    },
                    OneOffSchemeAdministrationSetupCosts = new CalcResultSummaryBadDebtProvision
                    {
                        TotalProducerFeeWithoutBadDebtProvision = 2970.7050628390007m,
                        BadDebtProvision = 178.24230377034004m,
                        TotalProducerFeeWithBadDebtProvision = 3148.947366609341m,
                        EnglandTotalWithBadDebtProvision = 1652.983846106635m,
                        WalesTotalWithBadDebtProvision = 417.1878943870084m,
                        ScotlandTotalWithBadDebtProvision = 766.0489525279556m,
                        NorthernIrelandTotalWithBadDebtProvision = 312.72667358774174m
                    },
                    TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision
                    {
                        TotalProducerFeeWithoutBadDebtProvision = 9897.32808192842m,
                        BadDebtProvision = 593.8396849157051m,
                        TotalProducerFeeWithBadDebtProvision = 10491.167766844124m,
                        EnglandTotalWithBadDebtProvision = 5442.448434925617m,
                        WalesTotalWithBadDebtProvision = 1452.6428880194774m,
                        ScotlandTotalWithBadDebtProvision = 2564.98356493499m,
                        NorthernIrelandTotalWithBadDebtProvision = 1031.0928789640386m
                    },
                    BillingInstructionSection = new CalcResultSummaryBillingInstruction
                    {
                        CurrentYearInvoiceTotalToDate = 1250.89m,
                        TonnageChangeSinceLastInvoice = "Tonnage Changed",
                        LiabilityDifference = 580.73m,
                        MaterialThresholdBreached = string.Empty,
                        TonnageThresholdBreached = string.Empty,
                        PercentageLiabilityDifference = 123.45m,
                        MaterialPercentageThresholdBreached = string.Empty,
                        TonnagePercentageThresholdBreached = string.Empty,
                        SuggestedBillingInstruction = string.Empty,
                        SuggestedInvoiceAmount = 4039m
                    },
                    CommunicationCostsSectionTwoB = new CalcResultSummaryBadDebtProvision
                    {
                        TotalProducerFeeWithoutBadDebtProvision = 2844.0556305055156m,
                        BadDebtProvision = 170.64333783033092m,
                        TotalProducerFeeWithBadDebtProvision = 3014.6989683358465m,
                        EnglandTotalWithBadDebtProvision = 1582.5125400804336m,
                        WalesTotalWithBadDebtProvision = 399.4020123649648m,
                        ScotlandTotalWithBadDebtProvision = 733.3901516568284m,
                        NorthernIrelandTotalWithBadDebtProvision = 299.39426423361965m
                    },
                    ProducerDisposalFeesByMaterial = GetProducerDisposalFeesByMaterial(applyModulation),
                    ProducerCommsFeesByMaterial = GetProducerCommsFeesByMaterial(),
                    TonnageChangeCount = "0",
                    TonnageChangeAdvice = "",
                },
            };
        }

        public static List<CalcResultSummaryProducerDisposalFees> GetProducerDisposalFeesForOverAllTotal(bool applyModulation = false)
        {
            return new List<CalcResultSummaryProducerDisposalFees>
            {
                new CalcResultSummaryProducerDisposalFees
                {
                    ProducerId = "1",
                    ProducerIdInt = 1,
                    SubsidiaryId = string.Empty,
                    ProducerName = "Allied Packaging",
                    Level = "1",
                    isTotalRow = false,
                    TotalProducerDisposalFee = 4423.39438m,
                    BadDebtProvision = 265.4036628m,
                    TotalProducerDisposalFeeWithBadDebtProvision = 4688.7980428m,
                    EnglandTotal = 2534.2359097426884m,
                    WalesTotal = 571.2417008090152m,
                    ScotlandTotal = 1137.8673076088023m,
                    NorthernIrelandTotal = 445.4531246394942m,
                    TotalProducerCommsFee = 1290.778m,
                    BadDebtProvisionComms = 77.44668m,
                    TotalProducerCommsFeeWithBadDebtProvision = 1368.22468m,
                    EnglandTotalComms = 718.2251815154783m,
                    WalesTotalComms = 181.2690740598454m,
                    ScotlandTotalComms = 332.8499847265775m,
                    NorthernIrelandTotalComms = 135.88043969809883m,
                    LocalAuthorityDisposalCostsSectionOne = new CalcResultSummaryBadDebtProvision
                    {
                        TotalProducerFeeWithoutBadDebtProvision = 4423.39438m,
                        BadDebtProvision = 265.4036628m,
                        TotalProducerFeeWithBadDebtProvision = 4688.7980428m,
                        EnglandTotalWithBadDebtProvision = 2534.2359097426884m,
                        WalesTotalWithBadDebtProvision = 571.2417008090152m,
                        ScotlandTotalWithBadDebtProvision = 1137.8673076088023m,
                        NorthernIrelandTotalWithBadDebtProvision = 445.4531246394942m
                    },
                    CommunicationCostsSectionTwoA = new CalcResultSummaryBadDebtProvision
                    {
                        TotalProducerFeeWithoutBadDebtProvision = 1290.778m,
                        BadDebtProvision = 77.44668m,
                        TotalProducerFeeWithBadDebtProvision = 1368.22468m,
                        EnglandTotalWithBadDebtProvision = 718.2251815154783m,
                        WalesTotalWithBadDebtProvision = 181.2690740598454m,
                        ScotlandTotalWithBadDebtProvision = 332.8499847265775m,
                        NorthernIrelandTotalWithBadDebtProvision = 135.88043969809883m
                    },
                    TwoCTotalProducerFeeForCommsCostsWithoutBadDebt = 1339.100071422903m,
                    TwoCBadDebtProvision = 80.34600428537418m,
                    TwoCTotalProducerFeeForCommsCostsWithBadDebt = 1419.446075708277m,
                    TwoCEnglandTotalWithBadDebt = 607.4748035870169m,
                    TwoCWalesTotalWithBadDebt = 300.7301007856519m,
                    TwoCScotlandTotalWithBadDebt = 360.87612094278234m,
                    TwoCNorthernIrelandTotalWithBadDebt = 150.36505039282596m,
                    PercentageofProducerReportedTonnagevsAllProducers = 5.6741528450123m,
                    ProducerTotalOnePlus2A2B2CWithBadDeptProvision = 10491.167766844124m,
                    ProducerOverallPercentageOfCostsForOnePlus2A2B2C = 4.7341913352015945m,
                    SchemeAdministratorOperatingCosts = new CalcResultSummaryBadDebtProvision
                    {
                        TotalProducerFeeWithoutBadDebtProvision = 3077.2243678810364m,
                        BadDebtProvision = 184.6334620728622m,
                        TotalProducerFeeWithBadDebtProvision = 3261.8578299538985m,
                        EnglandTotalWithBadDebtProvision = 1712.2541832180282m,
                        WalesTotalWithBadDebtProvision = 432.1468228710047m,
                        ScotlandTotalWithBadDebtProvision = 793.5168432560496m,
                        NorthernIrelandTotalWithBadDebtProvision = 323.93998060881614m
                    },
                    LocalAuthorityDataPreparationCosts = new CalcResultSummaryBadDebtProvision
                    {
                        TotalProducerFeeWithoutBadDebtProvision = 1727.9798373485821m,
                        BadDebtProvision = 103.67879024091492m,
                        TotalProducerFeeWithBadDebtProvision = 1831.658627589497m,
                        EnglandTotalWithBadDebtProvision = 802.9188504501905m,
                        WalesTotalWithBadDebtProvision = 351.2769970719583m,
                        ScotlandTotalWithBadDebtProvision = 451.6418533782321m,
                        NorthernIrelandTotalWithBadDebtProvision = 225.82092668911605m
                    },
                    OneOffSchemeAdministrationSetupCosts = new CalcResultSummaryBadDebtProvision
                    {
                        TotalProducerFeeWithoutBadDebtProvision = 2970.7050628390007m,
                        BadDebtProvision = 178.24230377034004m,
                        TotalProducerFeeWithBadDebtProvision = 3148.947366609341m,
                        EnglandTotalWithBadDebtProvision = 1652.983846106635m,
                        WalesTotalWithBadDebtProvision = 417.1878943870084m,
                        ScotlandTotalWithBadDebtProvision = 766.0489525279556m,
                        NorthernIrelandTotalWithBadDebtProvision = 312.72667358774174m
                    },
                    TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision
                    {
                        TotalProducerFeeWithoutBadDebtProvision = 9897.32808192842m,
                        BadDebtProvision = 593.8396849157051m,
                        TotalProducerFeeWithBadDebtProvision = 10491.167766844124m,
                        EnglandTotalWithBadDebtProvision = 5442.448434925617m,
                        WalesTotalWithBadDebtProvision = 1452.6428880194774m,
                        ScotlandTotalWithBadDebtProvision = 2564.98356493499m,
                        NorthernIrelandTotalWithBadDebtProvision = 1031.0928789640386m
                    },
                    BillingInstructionSection = new CalcResultSummaryBillingInstruction
                    {
                        CurrentYearInvoiceTotalToDate = null,
                        TonnageChangeSinceLastInvoice = null,
                        LiabilityDifference = 580.73m,
                        MaterialThresholdBreached = string.Empty,
                        TonnageThresholdBreached = string.Empty,
                        PercentageLiabilityDifference = null,
                        MaterialPercentageThresholdBreached = string.Empty,
                        TonnagePercentageThresholdBreached = string.Empty,
                        SuggestedBillingInstruction = string.Empty,
                        SuggestedInvoiceAmount = 4039m
                    },
                    CommunicationCostsSectionTwoB = new CalcResultSummaryBadDebtProvision
                    {
                        TotalProducerFeeWithoutBadDebtProvision = 2844.0556305055156m,
                        BadDebtProvision = 170.64333783033092m,
                        TotalProducerFeeWithBadDebtProvision = 3014.6989683358465m,
                        EnglandTotalWithBadDebtProvision = 1582.5125400804336m,
                        WalesTotalWithBadDebtProvision = 399.4020123649648m,
                        ScotlandTotalWithBadDebtProvision = 733.3901516568284m,
                        NorthernIrelandTotalWithBadDebtProvision = 299.39426423361965m
                    },
                    ProducerDisposalFeesByMaterial = GetProducerDisposalFeesByMaterial(applyModulation),
                    ProducerCommsFeesByMaterial = GetProducerCommsFeesByMaterial(),
                    TonnageChangeCount = "0",
                    TonnageChangeAdvice = "",
                    isOverallTotalRow = true,
                },
            };
        }

        public static List<CalcResultSummaryProducerDisposalFees> GetProducerDisposalFeesTonnageValueNull(bool applyModulation = false)
        {
            return new List<CalcResultSummaryProducerDisposalFees>
            {
                new CalcResultSummaryProducerDisposalFees
                {
                    ProducerId = "1",
                    ProducerIdInt = 1,
                    SubsidiaryId = string.Empty,
                    ProducerName = "Allied Packaging",
                    Level = "2",
                    isTotalRow = false,
                    TotalProducerDisposalFee = 4423.39438m,
                    BadDebtProvision = 265.4036628m,
                    TotalProducerDisposalFeeWithBadDebtProvision = 4688.7980428m,
                    EnglandTotal = 2534.2359097426884m,
                    WalesTotal = 571.2417008090152m,
                    ScotlandTotal = 1137.8673076088023m,
                    NorthernIrelandTotal = 445.4531246394942m,
                    TotalProducerCommsFee = 1290.778m,
                    BadDebtProvisionComms = 77.44668m,
                    TotalProducerCommsFeeWithBadDebtProvision = 1368.22468m,
                    EnglandTotalComms = 718.2251815154783m,
                    WalesTotalComms = 181.2690740598454m,
                    ScotlandTotalComms = 332.8499847265775m,
                    NorthernIrelandTotalComms = 135.88043969809883m,
                    LocalAuthorityDisposalCostsSectionOne = new CalcResultSummaryBadDebtProvision
                    {
                        TotalProducerFeeWithoutBadDebtProvision = 4423.39438m,
                        BadDebtProvision = 265.4036628m,
                        TotalProducerFeeWithBadDebtProvision = 4688.7980428m,
                        EnglandTotalWithBadDebtProvision = 2534.2359097426884m,
                        WalesTotalWithBadDebtProvision = 571.2417008090152m,
                        ScotlandTotalWithBadDebtProvision = 1137.8673076088023m,
                        NorthernIrelandTotalWithBadDebtProvision = 445.4531246394942m
                    },
                    CommunicationCostsSectionTwoA = new CalcResultSummaryBadDebtProvision
                    {
                        TotalProducerFeeWithoutBadDebtProvision = 1290.778m,
                        BadDebtProvision = 77.44668m,
                        TotalProducerFeeWithBadDebtProvision = 1368.22468m,
                        EnglandTotalWithBadDebtProvision = 718.2251815154783m,
                        WalesTotalWithBadDebtProvision = 181.2690740598454m,
                        ScotlandTotalWithBadDebtProvision = 332.8499847265775m,
                        NorthernIrelandTotalWithBadDebtProvision = 135.88043969809883m
                    },
                    TwoCTotalProducerFeeForCommsCostsWithoutBadDebt = 1339.100071422903m,
                    TwoCBadDebtProvision = 80.34600428537418m,
                    TwoCTotalProducerFeeForCommsCostsWithBadDebt = 1419.446075708277m,
                    TwoCEnglandTotalWithBadDebt = 607.4748035870169m,
                    TwoCWalesTotalWithBadDebt = 300.7301007856519m,
                    TwoCScotlandTotalWithBadDebt = 360.87612094278234m,
                    TwoCNorthernIrelandTotalWithBadDebt = 150.36505039282596m,
                    PercentageofProducerReportedTonnagevsAllProducers = 5.6741528450123m,
                    ProducerTotalOnePlus2A2B2CWithBadDeptProvision = 10491.167766844124m,
                    ProducerOverallPercentageOfCostsForOnePlus2A2B2C = 4.7341913352015945m,
                    SchemeAdministratorOperatingCosts = new CalcResultSummaryBadDebtProvision
                    {
                        TotalProducerFeeWithoutBadDebtProvision = 3077.2243678810364m,
                        BadDebtProvision = 184.6334620728622m,
                        TotalProducerFeeWithBadDebtProvision = 3261.8578299538985m,
                        EnglandTotalWithBadDebtProvision = 1712.2541832180282m,
                        WalesTotalWithBadDebtProvision = 432.1468228710047m,
                        ScotlandTotalWithBadDebtProvision = 793.5168432560496m,
                        NorthernIrelandTotalWithBadDebtProvision = 323.93998060881614m
                    },
                    LocalAuthorityDataPreparationCosts = new CalcResultSummaryBadDebtProvision
                    {
                        TotalProducerFeeWithoutBadDebtProvision = 1727.9798373485821m,
                        BadDebtProvision = 103.67879024091492m,
                        TotalProducerFeeWithBadDebtProvision = 1831.658627589497m,
                        EnglandTotalWithBadDebtProvision = 802.9188504501905m,
                        WalesTotalWithBadDebtProvision = 351.2769970719583m,
                        ScotlandTotalWithBadDebtProvision = 451.6418533782321m,
                        NorthernIrelandTotalWithBadDebtProvision = 225.82092668911605m
                    },
                    OneOffSchemeAdministrationSetupCosts = new CalcResultSummaryBadDebtProvision
                    {
                        TotalProducerFeeWithoutBadDebtProvision = 2970.7050628390007m,
                        BadDebtProvision = 178.24230377034004m,
                        TotalProducerFeeWithBadDebtProvision = 3148.947366609341m,
                        EnglandTotalWithBadDebtProvision = 1652.983846106635m,
                        WalesTotalWithBadDebtProvision = 417.1878943870084m,
                        ScotlandTotalWithBadDebtProvision = 766.0489525279556m,
                        NorthernIrelandTotalWithBadDebtProvision = 312.72667358774174m
                    },
                    TotalProducerBillBreakdownCosts = new CalcResultSummaryBadDebtProvision
                    {
                        TotalProducerFeeWithoutBadDebtProvision = 9897.32808192842m,
                        BadDebtProvision = 593.8396849157051m,
                        TotalProducerFeeWithBadDebtProvision = 10491.167766844124m,
                        EnglandTotalWithBadDebtProvision = 5442.448434925617m,
                        WalesTotalWithBadDebtProvision = 1452.6428880194774m,
                        ScotlandTotalWithBadDebtProvision = 2564.98356493499m,
                        NorthernIrelandTotalWithBadDebtProvision = 1031.0928789640386m
                    },
                    BillingInstructionSection = new CalcResultSummaryBillingInstruction
                    {
                        CurrentYearInvoiceTotalToDate = 1250.89m,
                        TonnageChangeSinceLastInvoice = string.Empty,
                        LiabilityDifference = 580.73m,
                        MaterialThresholdBreached = string.Empty,
                        TonnageThresholdBreached = string.Empty,
                        PercentageLiabilityDifference = null,
                        MaterialPercentageThresholdBreached = string.Empty,
                        TonnagePercentageThresholdBreached = string.Empty,
                        SuggestedBillingInstruction = string.Empty,
                        SuggestedInvoiceAmount = 4039m
                    },
                    CommunicationCostsSectionTwoB = new CalcResultSummaryBadDebtProvision
                    {
                        TotalProducerFeeWithoutBadDebtProvision = 2844.0556305055156m,
                        BadDebtProvision = 170.64333783033092m,
                        TotalProducerFeeWithBadDebtProvision = 3014.6989683358465m,
                        EnglandTotalWithBadDebtProvision = 1582.5125400804336m,
                        WalesTotalWithBadDebtProvision = 399.4020123649648m,
                        ScotlandTotalWithBadDebtProvision = 733.3901516568284m,
                        NorthernIrelandTotalWithBadDebtProvision = 299.39426423361965m
                    },
                    ProducerDisposalFeesByMaterial = GetProducerDisposalFeesByMaterial(applyModulation),
                    ProducerCommsFeesByMaterial = GetProducerCommsFeesByMaterial(),
                    TonnageChangeCount = null,
                    TonnageChangeAdvice = null,
                },
            };
        }
        public static Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial> GetProducerDisposalFeesByMaterial(bool applyModulation = false)
        {
            return new Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial>
            {
                {
                   "AL",
                   new CalcResultSummaryProducerDisposalFeesByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 1000,
                        SelfManagedConsumerWasteTonnage = 90,
                        TotalReportedTonnageRagRating = applyModulation
                            ? new Dictionary<RagRating, decimal>
                                {
                                    [RagRating.Red]          = 1,
                                    [RagRating.Amber]        = 2,
                                    [RagRating.Green]        = 3,
                                    [RagRating.RedMedical]   = 4,
                                    [RagRating.AmberMedical] = 5,
                                    [RagRating.GreenMedical] = 6
                                }
                            : new(),
                        NetReportedTonnage = applyModulation
                            ?(total: 910, red: 300, amber: 200, green: 410)
                            :(total: 910, red: null, amber: null, green: null),
                        PricePerTonne = applyModulation
                            ? (total: 0.6676m, red: 1, amber: 2, green: 3)
                            : (total: 0.6676m, red: null, amber: null, green: null),
                        ProducerDisposalFee = applyModulation
                            ? (total: 607.525000m, red: 4.525001m, amber: 5   , green: 6)
                            : (total: 607.525000m, red: null     , amber: null, green: null),
                        BadDebtProvision = 36.45m,
                        ProducerDisposalFeeWithBadDebtProvision = 643.97m,
                        EnglandWithBadDebtProvision = 348.06m,
                        WalesWithBadDebtProvision = 78.46m,
                        ScotlandWithBadDebtProvision = 156.28m,
                        NorthernIrelandWithBadDebtProvision = 61.18m,
                        PreviousInvoicedTonnage = null,
                        TonnageChange = 0,
                        ActionedSelfManagedConsumerWasteTonnage = (total: 90, red: 0, amber: 90, green: 0),
                    }
                },
                {
                    "FC",
                    new CalcResultSummaryProducerDisposalFeesByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 2000,
                        SelfManagedConsumerWasteTonnage = 140,
                        TotalReportedTonnageRagRating = applyModulation
                            ? new Dictionary<RagRating, decimal>
                                {
                                    [RagRating.Red]          = 1,
                                    [RagRating.Amber]        = 2,
                                    [RagRating.Green]        = 3,
                                    [RagRating.RedMedical]   = 4,
                                    [RagRating.AmberMedical] = 5,
                                    [RagRating.GreenMedical] = 6
                                }
                            : new(),
                        NetReportedTonnage = applyModulation
                            ?(total: 1860, red:  860, amber:    0, green: 1000)
                            :(total: 1860, red: null, amber: null, green: null),
                        PricePerTonne = applyModulation
                            ? (total: 0.7825m, red:    1, amber:    2, green:    3)
                            : (total: 0.7825m, red: null, amber: null, green: null),
                        ProducerDisposalFee = applyModulation
                            ? (total: 1455.45m, red:    4, amber:    5, green:    6)
                            : (total: 1455.45m, red: null, amber: null, green: null),
                        BadDebtProvision = 87.33m,
                        ProducerDisposalFeeWithBadDebtProvision = 1542.78m,
                        EnglandWithBadDebtProvision = 833.85m,
                        WalesWithBadDebtProvision = 187.96m,
                        ScotlandWithBadDebtProvision = 374.40m,
                        NorthernIrelandWithBadDebtProvision = 146.57m,
                        PreviousInvoicedTonnage = 0,
                        TonnageChange = 0,
                        ActionedSelfManagedConsumerWasteTonnage = (total: 140, red: 0, amber: 90, green: 140)
                    }
                },
                {
                   "GL",
                   new CalcResultSummaryProducerDisposalFeesByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 500,
                        SelfManagedConsumerWasteTonnage = 150,
                        TotalReportedTonnageRagRating = applyModulation
                            ? new Dictionary<RagRating, decimal>
                                {
                                    [RagRating.Red]          = 1,
                                    [RagRating.Amber]        = 2,
                                    [RagRating.Green]        = 3,
                                    [RagRating.RedMedical]   = 4,
                                    [RagRating.AmberMedical] = 5,
                                    [RagRating.GreenMedical] = 6
                                }
                            : new(),
                        NetReportedTonnage = applyModulation
                            ?(total: 350, red:  300, amber:   50, green:    0)
                            :(total: 350, red: null, amber: null, green: null),
                        PricePerTonne = applyModulation
                            ? (total: 6.4404m, red:    1, amber:    2, green:    3)
                            : (total: 6.4404m, red: null, amber: null, green: null),
                        ProducerDisposalFee = applyModulation
                            ? (total: 2254.14m, red:    4, amber:    5, green:    6)
                            : (total: 2254.14m, red: null, amber: null, green: null),
                        BadDebtProvision = 135.25m,
                        ProducerDisposalFeeWithBadDebtProvision = 2389.39m,
                        EnglandWithBadDebtProvision = 1291.43m,
                        WalesWithBadDebtProvision = 291.10m,
                        ScotlandWithBadDebtProvision = 579.85m,
                        NorthernIrelandWithBadDebtProvision = 227,
                        HouseholdDrinksContainersTonnage = 220,
                        PreviousInvoicedTonnage = 0,
                        TonnageChange = 0,
                        ActionedSelfManagedConsumerWasteTonnage = (total: 150, red: 50, amber: 100, green: 0)
                    }
                },
                {
                    "PC",
                    new CalcResultSummaryProducerDisposalFeesByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 20,
                        SelfManagedConsumerWasteTonnage = 2.200m,
                        TotalReportedTonnageRagRating = applyModulation
                            ? new Dictionary<RagRating, decimal>
                                {
                                    [RagRating.Red]          = 1,
                                    [RagRating.Amber]        = 2,
                                    [RagRating.Green]        = 3,
                                    [RagRating.RedMedical]   = 4,
                                    [RagRating.AmberMedical] = 5,
                                    [RagRating.GreenMedical] = 6
                                }
                            : new(),
                        NetReportedTonnage = applyModulation
                            ?(total: 17.800m, red:    0, amber:    0, green:    0)
                            :(total: 17.800m, red: null, amber: null, green: null),
                        PricePerTonne = applyModulation
                            ? (total: 2.4488m, red:    1, amber:    2, green:    3)
                            : (total: 2.4488m, red: null, amber: null, green: null),
                        ProducerDisposalFee = applyModulation
                            ? (total: 43.59m, red:    4, amber:    5, green:    6)
                            : (total: 43.59m, red: null, amber: null, green: null),
                        BadDebtProvision = 2.62m,
                        ProducerDisposalFeeWithBadDebtProvision = 46.20m,
                        EnglandWithBadDebtProvision = 24.97m,
                        WalesWithBadDebtProvision = 5.63m,
                        ScotlandWithBadDebtProvision = 11.21m,
                        NorthernIrelandWithBadDebtProvision = 4.39m,
                        PreviousInvoicedTonnage = 0,
                        TonnageChange = 0,
                        ActionedSelfManagedConsumerWasteTonnage = (total: 2.200m, red: 0, amber: 2.200m, green: 0)
                    }
                },
                {
                    "PL",
                    new CalcResultSummaryProducerDisposalFeesByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 5.000m,
                        SelfManagedConsumerWasteTonnage = 0.600m,
                        TotalReportedTonnageRagRating = applyModulation
                            ? new Dictionary<RagRating, decimal>
                                {
                                    [RagRating.Red]          = 1,
                                    [RagRating.Amber]        = 2,
                                    [RagRating.Green]        = 3,
                                    [RagRating.RedMedical]   = 4,
                                    [RagRating.AmberMedical] = 5,
                                    [RagRating.GreenMedical] = 6
                                }
                            : new(),
                        NetReportedTonnage = applyModulation
                            ?(total: 4.400m, red: 4.400m, amber:    0, green:    0)
                            :(total: 4.400m, red:   null, amber: null, green: null),
                        PricePerTonne = applyModulation
                            ? (total: 2.1601m, red:    1, amber:    2, green:    3)
                            : (total: 2.1601m, red: null, amber: null, green: null),
                        ProducerDisposalFee = applyModulation
                            ? (total: 9.50m, red:    4, amber:    5, green:    6)
                            : (total: 9.50m, red: null, amber: null, green: null),
                        BadDebtProvision = 0.57m,
                        ProducerDisposalFeeWithBadDebtProvision = 10.07m,
                        EnglandWithBadDebtProvision = 5.45m,
                        WalesWithBadDebtProvision = 1.23m,
                        ScotlandWithBadDebtProvision = 2.44m,
                        NorthernIrelandWithBadDebtProvision = 0.96m,
                        PreviousInvoicedTonnage = 0,
                        TonnageChange = 0,
                        ActionedSelfManagedConsumerWasteTonnage = (total: 0.600m, red: 0, amber: 0.600m, green: 0)
                    }
                },
                {
                    "ST",
                    new CalcResultSummaryProducerDisposalFeesByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 0.000m,
                        SelfManagedConsumerWasteTonnage = 0.000m,
                                    TotalReportedTonnageRagRating = applyModulation
                            ? new Dictionary<RagRating, decimal>
                                {
                                    [RagRating.Red]          = 1,
                                    [RagRating.Amber]        = 2,
                                    [RagRating.Green]        = 3,
                                    [RagRating.RedMedical]   = 4,
                                    [RagRating.AmberMedical] = 5,
                                    [RagRating.GreenMedical] = 6
                                }
                            : new(),
                        NetReportedTonnage = applyModulation
                            ?(total: 0, red:    0, amber:    0, green:    0)
                            :(total: 0, red: null, amber: null, green: null),
                        PricePerTonne = applyModulation
                            ? (total: 1.9813m, red:    1, amber:    2, green:    3)
                            : (total: 1.9813m, red: null, amber: null, green: null),
                        ProducerDisposalFee = applyModulation
                            ? (total: 0.00m, red:    4, amber:    5, green:    6)
                            : (total: 0.00m, red: null, amber: null, green: null),
                        BadDebtProvision = 0.00m,
                        ProducerDisposalFeeWithBadDebtProvision = 0.00m,
                        EnglandWithBadDebtProvision = 0.00m,
                        WalesWithBadDebtProvision = 0.00m,
                        ScotlandWithBadDebtProvision = 0.00m,
                        NorthernIrelandWithBadDebtProvision = 0.00m,
                        PreviousInvoicedTonnage = 0,
                        TonnageChange = 0,
                        ActionedSelfManagedConsumerWasteTonnage = (total: 0, red: 0, amber: 0, green: 0)
                    }
                },
                {
                   "WD",
                   new CalcResultSummaryProducerDisposalFeesByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 500.000m,
                        SelfManagedConsumerWasteTonnage = 95.000m,
                        TotalReportedTonnageRagRating = applyModulation
                            ? new Dictionary<RagRating, decimal>
                                {
                                    [RagRating.Red]          = 1,
                                    [RagRating.Amber]        = 2,
                                    [RagRating.Green]        = 3,
                                    [RagRating.RedMedical]   = 4,
                                    [RagRating.AmberMedical] = 5,
                                    [RagRating.GreenMedical] = 6
                                }
                            : new(),
                        NetReportedTonnage = applyModulation
                            ?(total: 405.000m, red:  300, amber:  100, green:    5)
                            :(total: 405.000m, red: null, amber: null, green: null),
                        PricePerTonne = applyModulation
                            ? (total: 2.0000m, red:    1, amber:    2, green:    3)
                            : (total: 2.0000m, red: null, amber: null, green: null),
                        ProducerDisposalFee = applyModulation
                            ? (total: 810.00m, red:    4, amber:    5, green:    6)
                            : (total: 810.00m, red: null, amber: null, green: null),
                        BadDebtProvision = 48.60m,
                        ProducerDisposalFeeWithBadDebtProvision = 858.60m,
                        EnglandWithBadDebtProvision = 464.06m,
                        WalesWithBadDebtProvision = 104.60m,
                        ScotlandWithBadDebtProvision = 208.36m,
                        NorthernIrelandWithBadDebtProvision = 81.57m,
                        PreviousInvoicedTonnage = 0,
                        TonnageChange = 0,
                        ActionedSelfManagedConsumerWasteTonnage = (total: 95, red: 0, amber: 95, green: 0)
                    }
                },
                {
                    "OT",
                    new CalcResultSummaryProducerDisposalFeesByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 50.000m,
                        SelfManagedConsumerWasteTonnage = 5.500m,
                        TotalReportedTonnageRagRating = applyModulation
                            ? new Dictionary<RagRating, decimal>
                                {
                                    [RagRating.Red]          = 1,
                                    [RagRating.Amber]        = 2,
                                    [RagRating.Green]        = 3,
                                    [RagRating.RedMedical]   = 4,
                                    [RagRating.AmberMedical] = 5,
                                    [RagRating.GreenMedical] = 6
                                }
                            : new(),
                        NetReportedTonnage = applyModulation
                            ?(total: 44.500m, red:    0, amber: 44.500m, green:    0)
                            :(total: 44.500m, red: null, amber:    null, green: null),
                        PricePerTonne = applyModulation
                            ? (total: 1.1954m, red:    1, amber:    2, green:    3)
                            : (total: 1.1954m, red: null, amber: null, green: null),
                        ProducerDisposalFee = applyModulation
                            ? (total: 53.20m, red:    4, amber:    5, green:    6)
                            : (total: 53.20m, red: null, amber: null, green: null),
                        BadDebtProvision = 3.19m,
                        ProducerDisposalFeeWithBadDebtProvision = 56.39m,
                        EnglandWithBadDebtProvision = 30.48m,
                        WalesWithBadDebtProvision = 6.87m,
                        ScotlandWithBadDebtProvision = 13.68m,
                        NorthernIrelandWithBadDebtProvision = 5.36m,
                        PreviousInvoicedTonnage = 0,
                        TonnageChange = 0,
                        ActionedSelfManagedConsumerWasteTonnage = (total: 5.500m, red: 0, amber: 5.500m, green: 0)
                    }
                },
            };
        }


        public static Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial> GetProducerCommsFeesByMaterial()
        {
            return new Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial>
            {
                {
                    "AL",
                    new CalcResultSummaryProducerCommsFeesCostByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 1000,
                        PriceperTonne = 0.1916m,
                        ProducerTotalCostWithoutBadDebtProvision = 191.60m,
                        BadDebtProvision = 11.50m,
                        ProducerTotalCostwithBadDebtProvision = 203.10m,
                        EnglandWithBadDebtProvision = 106.61m,
                        WalesWithBadDebtProvision = 26.91m,
                        ScotlandWithBadDebtProvision = 49.41m,
                        NorthernIrelandWithBadDebtProvision = 20.17m,
                    }
                },
                {
                    "FC",
                    new CalcResultSummaryProducerCommsFeesCostByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 2000.000m,
                        PriceperTonne = 0.4032m,
                        ProducerTotalCostWithoutBadDebtProvision = 806.40m,
                        BadDebtProvision = 48.38m,
                        ProducerTotalCostwithBadDebtProvision = 854.78m,
                        EnglandWithBadDebtProvision = 448.70m,
                        WalesWithBadDebtProvision = 113.25m,
                        ScotlandWithBadDebtProvision = 207.94m,
                        NorthernIrelandWithBadDebtProvision = 84.89m,
                    }
                },
                {
                    "GL",
                    new CalcResultSummaryProducerCommsFeesCostByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 500.000m,
                        PriceperTonne = 0.4404m,
                        ProducerTotalCostWithoutBadDebtProvision = 220.20m,
                        BadDebtProvision = 13.21m,
                        ProducerTotalCostwithBadDebtProvision = 233.41m,
                        EnglandWithBadDebtProvision = 122.53m,
                        WalesWithBadDebtProvision = 30.92m,
                        ScotlandWithBadDebtProvision = 56.78m,
                        NorthernIrelandWithBadDebtProvision = 23.18m,
                    }
                },
                {
                    "PC",
                    new CalcResultSummaryProducerCommsFeesCostByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 20.000m,
                        PriceperTonne = 1.1100m,
                        ProducerTotalCostWithoutBadDebtProvision = 22.20m,
                        BadDebtProvision = 1.33m,
                        ProducerTotalCostwithBadDebtProvision = 23.53m,
                        EnglandWithBadDebtProvision = 12.35m,
                        WalesWithBadDebtProvision = 3.12m,
                        ScotlandWithBadDebtProvision = 5.72m,
                        NorthernIrelandWithBadDebtProvision = 2.34m,
                    }
                },
                {
                    "PL",
                    new CalcResultSummaryProducerCommsFeesCostByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 5.000m,
                        PriceperTonne = 0.5356m,
                        ProducerTotalCostWithoutBadDebtProvision = 2.68m,
                        BadDebtProvision = 0.16m,
                        ProducerTotalCostwithBadDebtProvision = 2.84m,
                        EnglandWithBadDebtProvision = 1.49m,
                        WalesWithBadDebtProvision = 0.38m,
                        ScotlandWithBadDebtProvision = 0.69m,
                        NorthernIrelandWithBadDebtProvision = 0.28m,
                    }
                },
                {
                    "ST",
                    new CalcResultSummaryProducerCommsFeesCostByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 0.000m,
                        PriceperTonne = 0.8879m,
                        ProducerTotalCostWithoutBadDebtProvision = 0.00m,
                        BadDebtProvision = 0.00m,
                        ProducerTotalCostwithBadDebtProvision = 0.00m,
                        EnglandWithBadDebtProvision = 0.00m,
                        WalesWithBadDebtProvision = 0.00m,
                        ScotlandWithBadDebtProvision = 0.00m,
                        NorthernIrelandWithBadDebtProvision = 0.00m,
                    }
                },
                {
                   "WD",
                   new CalcResultSummaryProducerCommsFeesCostByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 500.000m,
                        PriceperTonne = 0.1364m,
                        ProducerTotalCostWithoutBadDebtProvision = 68.20m,
                        BadDebtProvision = 4.09m,
                        ProducerTotalCostwithBadDebtProvision = 72.29m,
                        EnglandWithBadDebtProvision = 37.95m,
                        WalesWithBadDebtProvision = 9.58m,
                        ScotlandWithBadDebtProvision = 17.59m,
                        NorthernIrelandWithBadDebtProvision = 7.18m,
                    }
                },
                {
                   "OT",
                    new CalcResultSummaryProducerCommsFeesCostByMaterial
                    {
                        HouseholdPackagingWasteTonnage = 50.000m,
                        PriceperTonne = 0.9540m,
                        ProducerTotalCostWithoutBadDebtProvision = 47.70m,
                        BadDebtProvision = 2.86m,
                        ProducerTotalCostwithBadDebtProvision = 50.56m,
                        EnglandWithBadDebtProvision = 26.54m,
                        WalesWithBadDebtProvision = 6.70m,
                        ScotlandWithBadDebtProvision = 12.30m,
                        NorthernIrelandWithBadDebtProvision = 5.02m,
                    }
                },
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
                                    ScaledupNetReportedTonnage = 360,
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
                                    ScaledupNetReportedTonnage = 360,
                                }
                            },
                        },
                    },
                ],
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
                                    HouseholdTonnage = 100,
                                    HouseholdRAMTonnage = new RAMTonnage(),
                                    PublicBinTonnage = 20,
                                    PublicBinRAMTonnage = new RAMTonnage(),
                                    TotalTonnage = 120,
                                    SelfManagedConsumerWasteTonnage = 60,
                                    PartialHouseholdTonnage = 50,
                                    PartialHouseholdRAMTonnage = new RAMTonnage(),
                                    PartialPublicBinTonnage = 10,
                                    PartialPublicBinRAMTonnage = new RAMTonnage(),
                                    PartialTotalTonnage = 60,
                                    PartialSelfManagedConsumerWasteTonnage = 30,
                                }
                            },
                            {
                                "GL",
                                new CalcResultPartialObligationTonnage
                                {
                                    HouseholdTonnage = 100,
                                    HouseholdRAMTonnage = new RAMTonnage(),
                                    PublicBinTonnage = 20,
                                    PublicBinRAMTonnage = new RAMTonnage(),
                                    HouseholdDrinksContainersTonnage = 70,
                                    HouseholdDrinksContainersRAMTonnage = new RAMTonnage(),
                                    TotalTonnage = 120,
                                    SelfManagedConsumerWasteTonnage = 60,
                                    PartialHouseholdTonnage = 50,
                                    PartialHouseholdRAMTonnage = new RAMTonnage(),
                                    PartialPublicBinTonnage = 10,
                                    PartialPublicBinRAMTonnage = new RAMTonnage(),
                                    PartialHouseholdDrinksContainersTonnage = 35,
                                    PartialHouseholdDrinksContainersRAMTonnage = new RAMTonnage(),
                                    PartialTotalTonnage = 60,
                                    PartialSelfManagedConsumerWasteTonnage = 30
                                }
                            },
                        },
                    },
                ],
            };
        }

        public static IImmutableList<MaterialDetail> GetMaterials()
        {
            return ImmutableList.Create(
                new MaterialDetail
                {
                    Id = 1,
                    Code = "AL",
                    Name = "Aluminium",
                    Description = "Aluminium",
                },
                new MaterialDetail
                {
                    Id = 2,
                    Code = "FC",
                    Name = "Fibre composite",
                    Description = "Fibre composite",
                },
                new MaterialDetail
                {
                    Id = 3,
                    Code = "GL",
                    Name = "Glass",
                    Description = "Glass",
                },
                new MaterialDetail
                {
                    Id = 4,
                    Code = "PC",
                    Name = "Paper or card",
                    Description = "Paper or card",
                },
                new MaterialDetail
                {
                    Id = 5,
                    Code = "PL",
                    Name = "Plastic",
                    Description = "Plastic",
                },
                new MaterialDetail
                {
                    Id = 6,
                    Code = "ST",
                    Name = "Steel",
                    Description = "Steel",
                },
                new MaterialDetail
                {
                    Id = 7,
                    Code = "WD",
                    Name = "Wood",
                    Description = "Wood",
                },
                new MaterialDetail
                {
                    Id = 8,
                    Code = "OT",
                    Name = "Other materials",
                    Description = "Other materials",
                }
            );
        }

        public static List<ProducerDetail> GetProducers()
        {
            var producers = new List<ProducerDetail>
            {
                new ProducerDetail
                {
                    Id = 1,
                    ProducerId = 1,
                    ProducerName = "Allied Packaging",
                    CalculatorRunId = 1,
                    CalculatorRun = new CalculatorRun { RelativeYear = new RelativeYear(204), Name = "Test Run 1" },
                },
                new ProducerDetail
                {
                    Id = 2,
                    ProducerId = 2,
                    ProducerName = "Beeline Materials",
                    CalculatorRunId = 1,
                    CalculatorRun = new CalculatorRun { RelativeYear = new RelativeYear(204), Name = "Test Run 1" },
                },
                new ProducerDetail
                {
                    Id = 3,
                    ProducerId = 3,
                    ProducerName = "Cloud Boxes",
                    CalculatorRunId = 1,
                    CalculatorRun = new CalculatorRun { RelativeYear = new RelativeYear(204), Name = "Test Run 1" },
                },
            };

            var producerReportedMaterials = GetProducerReportedMaterials();

            producers.ForEach(producer =>
            {
                producerReportedMaterials.ForEach(producerReportedMaterial =>
                {
                    producer.ProducerReportedMaterials.Add(producerReportedMaterial);
                });
            });

            return producers;
        }

        public static List<ProducerReportedMaterial> GetProducerReportedMaterials()
        {
            var prodMats = new List<ProducerReportedMaterial>();
            foreach (var subPeriod in new[] { "2025-H1", "2025-H2"}) {
                prodMats.AddRange(new[]{
                    new ProducerReportedMaterial
                    {
                        MaterialId = 1,
                        PackagingTonnage = 500.00m,
                        PackagingType = "HH",
                        SubmissionPeriod = subPeriod,
                        ProducerDetail = null,
                    },
                    new ProducerReportedMaterial
                    {
                        MaterialId = 1,
                        PackagingTonnage = 10.00m,
                        PackagingType = "CW",
                        SubmissionPeriod = subPeriod,
                        ProducerDetail = null,
                    },
                    new ProducerReportedMaterial
                    {
                        MaterialId = 5,
                        PackagingTonnage = 10.00m,
                        PackagingType = "PB",
                        SubmissionPeriod = subPeriod,
                        ProducerDetail = null,
                    },
                    new ProducerReportedMaterial
                    {
                        MaterialId = 3,
                        PackagingTonnage = 20.00m,
                        PackagingType = "HH",
                        SubmissionPeriod = subPeriod,
                        ProducerDetail = null,
                    },
                    new ProducerReportedMaterial
                    {
                        MaterialId = 3,
                        PackagingTonnage = 10.00m,
                        PackagingType = "HDC",
                        SubmissionPeriod = subPeriod,
                        ProducerDetail = null,
                    },
                    new ProducerReportedMaterial
                    {
                        MaterialId = 3,
                        PackagingTonnage = 50.00m,
                        PackagingType = "CW",
                        SubmissionPeriod = subPeriod,
                        ProducerDetail = null,
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
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-NIR",
                ParameterCategory = "Northern Ireland",
                ParameterType = "Communication costs by country",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-SCT",
                ParameterCategory = "Scotland",
                ParameterType = "Communication costs by country",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-UK",
                ParameterCategory = "United Kingdom",
                ParameterType = "Communication costs by country",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-WLS",
                ParameterCategory = "Wales",
                ParameterType = "Communication costs by country",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-AL",
                ParameterCategory = "Aluminium",
                ParameterType = "Communication costs by material",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-FC",
                ParameterCategory = "Fibre composite",
                ParameterType = "Communication costs by material",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-GL",
                ParameterCategory = "Glass",
                ParameterType = "Communication costs by material",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-PC",
                ParameterCategory = "Paper or card",
                ParameterType = "Communication costs by material",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-PL",
                ParameterCategory = "Plastic",
                ParameterType = "Communication costs by material",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-ST",
                ParameterCategory = "Steel",
                ParameterType = "Communication costs by material",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-WD",
                ParameterCategory = "Wood",
                ParameterType = "Communication costs by material",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-FC-R",
                ParameterCategory = "Fibre composite-R",
                ParameterType = "Late reporting tonnage",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-FC",
                ParameterCategory = "Fibre composite-A",
                ParameterType = "Late reporting tonnage",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-FC-G",
                ParameterCategory = "Fibre composite-G",
                ParameterType = "Late reporting tonnage",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-GL-R",
                ParameterCategory = "Glass-R",
                ParameterType = "Late reporting tonnage",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-GL",
                ParameterCategory = "Glass-A",
                ParameterType = "Late reporting tonnage",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-GL-G",
                ParameterCategory = "Glass-G",
                ParameterType = "Late reporting tonnage",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-AL-R",
                ParameterCategory = "Aluminium-R",
                ParameterType = "Late reporting tonnage",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-AL",
                ParameterCategory = "Aluminium-A",
                ParameterType = "Late reporting tonnage",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-AL-G",
                ParameterCategory = "Aluminium-G",
                ParameterType = "Late reporting tonnage",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-WD-R",
                ParameterCategory = "Wood-R",
                ParameterType = "Late reporting tonnage",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-WD",
                ParameterCategory = "Wood-A",
                ParameterType = "Late reporting tonnage",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-WD-G",
                ParameterCategory = "Wood-G",
                ParameterType = "Late reporting tonnage",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-ST-R",
                ParameterCategory = "Steel-R",
                ParameterType = "Late reporting tonnage",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-ST",
                ParameterCategory = "Steel-A",
                ParameterType = "Late reporting tonnage",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-ST-G",
                ParameterCategory = "Steel-G",
                ParameterType = "Late reporting tonnage",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-PC-R",
                ParameterCategory = "Paper or card-R",
                ParameterType = "Late reporting tonnage",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-PC",
                ParameterCategory = "Paper or card-A",
                ParameterType = "Late reporting tonnage",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-PL-G",
                ParameterCategory = "Plastic-G",
                ParameterType = "Late reporting tonnage",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-OT-R",
                ParameterCategory = "Other materials-R",
                ParameterType = "Late reporting tonnage",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-OT",
                ParameterCategory = "Other materials-A",
                ParameterType = "Late reporting tonnage",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LRET-OT-G",
                ParameterCategory = "Other materials-G",
                ParameterType = "Late reporting tonnage",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LAPC-ENG",
                ParameterCategory = "England",
                ParameterType = "Local authority data preparation costs",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LAPC-NIR",
                ParameterCategory = "Northern Ireland",
                ParameterType = "Local authority data preparation costs",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LAPC-SCT",
                ParameterCategory = "Scotland",
                ParameterType = "Local authority data preparation costs",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "LAPC-WLS",
                ParameterCategory = "Wales",
                ParameterType = "Local authority data preparation costs",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "MATT-AD",
                ParameterCategory = "Amount Decrease",
                ParameterType = "Materiality threshold",
                ValidRangeFrom = -999999999.990m,
                ValidRangeTo = 0.00m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "MATT-AI",
                ParameterCategory = "Amount Increase",
                ParameterType = "Materiality threshold",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "MATT-PD",
                ParameterCategory = "Percent Decrease",
                ParameterType = "Materiality threshold",
                ValidRangeFrom = -999.990m,
                ValidRangeTo = 0.00m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "MATT-PI",
                ParameterCategory = "Percent Increase",
                ParameterType = "Materiality threshold",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999.990m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "COMC-OT",
                ParameterCategory = "Other",
                ParameterType = "Other materials",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "BADEBT-P",
                ParameterCategory = "Bad debt provision",
                ParameterType = "Percentage",
                ValidRangeFrom = 0m,
                ValidRangeTo = 1000.000m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "SAOC-ENG",
                ParameterCategory = "England",
                ParameterType = "Scheme administrator operating costs",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "SAOC-NIR",
                ParameterCategory = "Northern Ireland",
                ParameterType = "Scheme administrator operating costs",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "SAOC-SCT",
                ParameterCategory = "Scotland",
                ParameterType = "Scheme administrator operating costs",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "SAOC-WLS",
                ParameterCategory = "Wales",
                ParameterType = "Scheme administrator operating costs",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "SCSC-ENG",
                ParameterCategory = "England",
                ParameterType = "Scheme setup costs",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "SCSC-NIR",
                ParameterCategory = "Northern Ireland",
                ParameterType = "Scheme setup costs",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "SCSC-SCT",
                ParameterCategory = "Scotland",
                ParameterType = "Scheme setup costs",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "SCSC-WLS",
                ParameterCategory = "Wales",
                ParameterType = "Scheme setup costs",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "TONT-AD",
                ParameterCategory = "Amount Decrease",
                ParameterType = "Tonnage change threshold",
                ValidRangeFrom = -999999999.990m,
                ValidRangeTo = 0.00m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "TONT-AI",
                ParameterCategory = "Amount Increase",
                ParameterType = "Tonnage change threshold",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999999999.99m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "TONT-PD",
                ParameterCategory = "Percent Decrease",
                ParameterType = "Tonnage change threshold",
                ValidRangeFrom = -999.990m,
                ValidRangeTo = 0.00m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "TONT-PI",
                ParameterCategory = "Percent Increase",
                ParameterType = "Tonnage change threshold",
                ValidRangeFrom = 0m,
                ValidRangeTo = 999.990m,
            });
            list.Add(new DefaultParameterTemplateMaster
            {
                ParameterUniqueReferenceId = "REDM-RF",
                ParameterCategory = "Modulation Factor",
                ParameterType = "Red modulation factor",
                ValidRangeFrom = 1.000m,
                ValidRangeTo = 2.000m,
            });
            return list;
        }

        public static IEnumerable<CalculatorRun> GetCaculatorRuns()
        {
            var list = new List<CalculatorRun>();
            list.Add(new CalculatorRun
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
            });
            list.Add(new CalculatorRun
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
            });
            return list;
        }

        public static IEnumerable<CalculatorRunOrganisationDataDetail> GetCalculatorRunOrganisationDataDetails()
        {
            var submitterId1 = Guid.NewGuid();
            var list = new List<CalculatorRunOrganisationDataDetail>();
            list.Add(new CalculatorRunOrganisationDataDetail
            {
                Id = 1,
                OrganisationId = 1,
                SubsidiaryId = null,
                OrganisationName = "Allied Packaging",
                TradingName = "Allied Trading",
                LoadTimeStamp = DateTime.UtcNow,
                ObligationStatus= ObligationStates.Obligated,
                SubmitterId = submitterId1,
                CalculatorRunOrganisationDataMasterId = GetCalculatorRunOrganisationDataMaster().ToList()[0].Id,
            });
            list.Add(new CalculatorRunOrganisationDataDetail
            {
                Id = 2,
                OrganisationId = 1,
                SubsidiaryId = "901",
                OrganisationName = "Allied Subsidiary",
                LoadTimeStamp = DateTime.UtcNow,
                CalculatorRunOrganisationDataMasterId = GetCalculatorRunOrganisationDataMaster().ToList()[0].Id,
            });
            list.Add(new CalculatorRunOrganisationDataDetail
            {
                Id = 3,
                OrganisationId = 2,
                SubsidiaryId = null,
                OrganisationName = "",
                TradingName = "",
                LoadTimeStamp = DateTime.UtcNow,
                CalculatorRunOrganisationDataMasterId = GetCalculatorRunOrganisationDataMaster().ToList()[0].Id,
            });
            list.Add(new CalculatorRunOrganisationDataDetail
            {
                Id = 4,
                OrganisationId = 2,
                SubsidiaryId = "Sub 2",
                OrganisationName = "",
                LoadTimeStamp = DateTime.UtcNow,
                CalculatorRunOrganisationDataMasterId = GetCalculatorRunOrganisationDataMaster().ToList()[0].Id,
            });
            list.Add(new CalculatorRunOrganisationDataDetail
            {
                Id = 5,
                OrganisationId = 1,
                SubsidiaryId = "Sub 1",
                OrganisationName = "Allied Packaging sub 1",
                TradingName = "Allied Trading sub 1",
                LoadTimeStamp = DateTime.UtcNow,
                ObligationStatus= ObligationStates.Obligated,
                SubmitterId = submitterId1,
                CalculatorRunOrganisationDataMasterId = GetCalculatorRunOrganisationDataMaster().ToList()[0].Id,
            });
            list.Add(new CalculatorRunOrganisationDataDetail
            {
                Id = 6,
                OrganisationId = 1,
                SubsidiaryId = "Sub 2",
                OrganisationName = "Allied Packaging sub 2",
                TradingName = "Allied Trading sub 2",
                LoadTimeStamp = DateTime.UtcNow,
                ObligationStatus= ObligationStates.Obligated,
                SubmitterId = submitterId1,
                CalculatorRunOrganisationDataMasterId = GetCalculatorRunOrganisationDataMaster().ToList()[0].Id,
            });
            list.Add(new CalculatorRunOrganisationDataDetail
            {
                Id = 7,
                OrganisationId = 1,
                SubsidiaryId = null,
                OrganisationName = "Allied Packaging",
                TradingName = "Allied Trading - Old Compliance Scheme",
                LoadTimeStamp = DateTime.UtcNow,
                ObligationStatus= ObligationStates.NotObligated,
                CalculatorRunOrganisationDataMasterId = GetCalculatorRunOrganisationDataMaster().ToList()[0].Id,
                SubmitterId = submitterId1
            });
            list.Add(new CalculatorRunOrganisationDataDetail
            {
                Id = 8,
                OrganisationId = 1,
                SubsidiaryId = "Sub 1",
                OrganisationName = "Allied Packaging sub 1 - Old Compliance Scheme",
                TradingName = "Allied Trading",
                ObligationStatus= ObligationStates.NotObligated,
                LoadTimeStamp = DateTime.UtcNow,
                CalculatorRunOrganisationDataMasterId = GetCalculatorRunOrganisationDataMaster().ToList()[0].Id,
                SubmitterId = submitterId1
            });
            list.Add(new CalculatorRunOrganisationDataDetail
            {
                Id = 9,
                OrganisationId = 1,
                SubsidiaryId = "Sub 2",
                OrganisationName = "Allied Packaging sub 2 - Old Compliance Scheme",
                TradingName = "Allied Trading",
                ObligationStatus= ObligationStates.NotObligated,
                LoadTimeStamp = DateTime.UtcNow,
                CalculatorRunOrganisationDataMasterId = GetCalculatorRunOrganisationDataMaster().ToList()[0].Id,
            });

            return list;
        }

        public static IEnumerable<CalculatorRunOrganisationDataMaster> GetCalculatorRunOrganisationDataMaster()
        {
            var list = new List<CalculatorRunOrganisationDataMaster>();
            list.Add(new CalculatorRunOrganisationDataMaster
            {
                Id = 1,
                RelativeYear = new RelativeYear(2024),
                EffectiveFrom = DateTime.UtcNow,
                EffectiveTo = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Test user",
            });
            list.Add(new CalculatorRunOrganisationDataMaster
            {
                Id = 2,
                RelativeYear = new RelativeYear(2024),
                EffectiveFrom = DateTime.UtcNow,
                EffectiveTo = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Test user",
            });
            return list;
        }

        public static IEnumerable<CalculatorRunPomDataDetail> GetCalculatorRunPomDataDetails()
        {
            var list = new List<CalculatorRunPomDataDetail>();
            list.Add(new CalculatorRunPomDataDetail
            {
                Id = 1,
                OrganisationId = 1,
                SubsidiaryId = null,
                SubmissionPeriod = "2024-P2",
                PackagingActivity = null,
                PackagingType = "HH",
                PackagingClass = "O1",
                PackagingMaterial = "AL",
                PackagingMaterialWeight = 1000,
                LoadTimeStamp = DateTime.UtcNow,
                CalculatorRunPomDataMasterId = 1,
                SubmissionPeriodDesc = "January to June 2024",
                CalculatorRunPomDataMaster = GetCalculatorRunPomDataMaster().ToList()[0]
            });
            list.Add(new CalculatorRunPomDataDetail
            {
                Id = 1,
                OrganisationId = 1,
                SubsidiaryId = null,
                SubmissionPeriod = "2024-P4",
                PackagingActivity = null,
                PackagingType = "HH",
                PackagingClass = "O1",
                PackagingMaterial = "AL",
                PackagingMaterialWeight = 2000,
                LoadTimeStamp = DateTime.UtcNow,
                CalculatorRunPomDataMasterId = 1,
                SubmissionPeriodDesc = "July to December 2024",
                CalculatorRunPomDataMaster = GetCalculatorRunPomDataMaster().ToList()[0]
            });
            return list;
        }

        public static IEnumerable<CalculatorRunPomDataMaster> GetCalculatorRunPomDataMaster()
        {
            var list = new List<CalculatorRunPomDataMaster>();
            list.Add(new CalculatorRunPomDataMaster
            {
                Id = 1,
                RelativeYear = new RelativeYear(2024),
                EffectiveFrom = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Test User",
            });
            list.Add(new CalculatorRunPomDataMaster
            {
                Id = 2,
                RelativeYear = new RelativeYear(2024),
                EffectiveFrom = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Test User",
            });
            return list;
        }

        public static void SeedDatabaseForInitialRun(ApplicationDBContext context)
        {
            //calculator runs
            var runs = new List<CalculatorRun> {
                new CalculatorRun { Id = 1, RelativeYear = new RelativeYear(2025), CalculatorRunClassificationId=7, Name = "CalculatorRunTest1" },
                new CalculatorRun { Id = 2, RelativeYear = new RelativeYear(2025), CalculatorRunClassificationId=2, Name = "CalculatorRunTest2" }
            };
            context.CalculatorRuns.AddRange(runs);

            context.CalculatorRunOrganisationDataMaster.AddRange(GetCalculatorRunOrganisationDataMaster());


            context.CalculatorRunOrganisationDataDetails.Add(new CalculatorRunOrganisationDataDetail
            {
                Id = 1,
                OrganisationId = 1,
                SubsidiaryId = null,
                OrganisationName = "Test1",
                LoadTimeStamp = DateTime.UtcNow,
                CalculatorRunOrganisationDataMasterId = GetCalculatorRunOrganisationDataMaster().ToList()[0].Id,
                TradingName = "TN1"
            });
            context.CalculatorRunOrganisationDataDetails.Add(new CalculatorRunOrganisationDataDetail
            {
                Id = 2,
                OrganisationId = 2,
                SubsidiaryId = null,
                OrganisationName = "Test2",
                LoadTimeStamp = DateTime.UtcNow,
                CalculatorRunOrganisationDataMasterId = GetCalculatorRunOrganisationDataMaster().ToList()[0].Id,
            });


            var producerDetails = new List<ProducerDetail>
            {
                new ProducerDetail { Id =1 , CalculatorRunId = 1, ProducerName="Test1", ProducerId = 1, TradingName = "TN1"},
                new ProducerDetail { Id =2 , CalculatorRunId = 1, ProducerName="Test2", ProducerId = 2, TradingName = "TN2"},
                new ProducerDetail { Id =3 , CalculatorRunId = 2, ProducerName="Test1", ProducerId = 1, TradingName = "TN3"},
                new ProducerDetail { Id =4 , CalculatorRunId = 1, ProducerName="Test3", ProducerId = 3, TradingName = "TN4"},
            };

            context.ProducerDetail.AddRange(producerDetails);

            var materials = new List<Material>
            {
                new Material { Id = 5, Name = "Plastic", Code = MaterialCodes.Plastic },
                new Material { Id = 6, Name = "Steel", Code = MaterialCodes.Steel },
                new Material { Id = 3, Name = "Glass", Code = MaterialCodes.Glass },
            };
            context.Material.AddRange(materials);

            var producerReportedMaterials = new List<ProducerReportedMaterial>
            {
                new ProducerReportedMaterial { ProducerDetailId = 1, MaterialId = 1, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.Household, PackagingTonnage = 50, PackagingTonnageRed = 50 },
                new ProducerReportedMaterial { ProducerDetailId = 1, MaterialId = 1, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.Household, PackagingTonnage = 50, PackagingTonnageAmber = 50 },
                new ProducerReportedMaterial { ProducerDetailId = 1, MaterialId = 2, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 100, PackagingTonnageGreen = 100 },
                new ProducerReportedMaterial { ProducerDetailId = 1, MaterialId = 2, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 100, PackagingTonnageRedMedical = 100 },
                new ProducerReportedMaterial { ProducerDetailId = 1, MaterialId = 3, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 150, PackagingTonnageAmberMedical = 150 },
                new ProducerReportedMaterial { ProducerDetailId = 1, MaterialId = 3, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 150, PackagingTonnageGreenMedical = 150 },
                new ProducerReportedMaterial { ProducerDetailId = 2, MaterialId = 1, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.Household, PackagingTonnage = 50, PackagingTonnageRed = 50 },
                new ProducerReportedMaterial { ProducerDetailId = 2, MaterialId = 1, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.Household, PackagingTonnage = 50, PackagingTonnageAmber = 50 },
                new ProducerReportedMaterial { ProducerDetailId = 2, MaterialId = 2, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 100, PackagingTonnageGreen = 100 },
                new ProducerReportedMaterial { ProducerDetailId = 2, MaterialId = 2, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 100, PackagingTonnageRedMedical = 100 },
                new ProducerReportedMaterial { ProducerDetailId = 2, MaterialId = 3, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 150, PackagingTonnageAmberMedical = 150 },
                new ProducerReportedMaterial { ProducerDetailId = 2, MaterialId = 3, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 150, PackagingTonnageGreenMedical = 150 },

            };
            context.ProducerReportedMaterial.AddRange(producerReportedMaterials);



            var designatedRunInvoice = new List<ProducerDesignatedRunInvoiceInstruction>
            {
                new ProducerDesignatedRunInvoiceInstruction
                {
                    BillingInstructionId = "1_1",
                    CalculatorRunId = 1,
                    CurrentYearInvoicedTotalAfterThisRun = 100,
                    Id = 1,
                    ProducerId = 1,
                    InvoiceAmount = 100,
                    OutstandingBalance = 100,

                },
                new ProducerDesignatedRunInvoiceInstruction
                {
                    BillingInstructionId = "1_2",
                    CalculatorRunId = 1,
                    CurrentYearInvoicedTotalAfterThisRun = 100,
                    Id = 2,
                    ProducerId = 2,
                    InvoiceAmount = 100,
                    OutstandingBalance = 100,

                },
            };


            context.ProducerDesignatedRunInvoiceInstruction.AddRange(designatedRunInvoice);


            var billingInstructionList = new List<ProducerResultFileSuggestedBillingInstruction>
            {
                new ProducerResultFileSuggestedBillingInstruction
                {
                    MaterialPercentageThresholdBreached = "1%",
                    MaterialPoundThresholdBreached = "1",
                    ProducerId = 1,
                    SuggestedBillingInstruction = "Initial",
                    SuggestedInvoiceAmount = 100,
                    CalculatorRunId = 1,
                    BillingInstructionAcceptReject = "Accepted"
                },
                new ProducerResultFileSuggestedBillingInstruction
                {
                    MaterialPercentageThresholdBreached = "1%",
                    MaterialPoundThresholdBreached = "1",
                    ProducerId = 2,
                    SuggestedBillingInstruction = "Initial",
                    SuggestedInvoiceAmount = 100,
                    CalculatorRunId = 1,
                    BillingInstructionAcceptReject = "Accepted"
                },
                new ProducerResultFileSuggestedBillingInstruction
                {
                    MaterialPercentageThresholdBreached = "1%",
                    MaterialPoundThresholdBreached = "1",
                    ProducerId = 3,
                    SuggestedBillingInstruction = "Initial",
                    SuggestedInvoiceAmount = 100,
                    CalculatorRunId = 1,
                    BillingInstructionAcceptReject = "Accepted"
                }
            };



            context.ProducerResultFileSuggestedBillingInstruction.AddRange(billingInstructionList);

            var materialInvoiceTonnage = new List<ProducerInvoicedMaterialNetTonnage>
            {
                 new ProducerInvoicedMaterialNetTonnage
                 {
                      CalculatorRunId =1,
                      MaterialId= 1,
                      InvoicedNetTonnage = 100,
                      ProducerId =1, Id=1

                 },
                new ProducerInvoicedMaterialNetTonnage
                {
                      CalculatorRunId =1,
                      MaterialId= 2,
                      InvoicedNetTonnage = 100,
                      ProducerId =1,
                    Id=2

                },
                new ProducerInvoicedMaterialNetTonnage
                {
                      CalculatorRunId =1,
                      MaterialId= 1,
                      InvoicedNetTonnage = 100,
                      ProducerId =2, Id=3

                 },
                new ProducerInvoicedMaterialNetTonnage
                {
                      CalculatorRunId =1,
                      MaterialId= 2,
                      InvoicedNetTonnage = 100,
                      ProducerId =2,
                    Id=4

                }
            };

            context.ProducerInvoicedMaterialNetTonnage.AddRange(materialInvoiceTonnage);

            context.SaveChanges();
        }

        public static void SeedDatabaseForUnclassified(ApplicationDBContext context)
        {
            //calculator runs
            var runs = new List<CalculatorRun> {
                new CalculatorRun { Id = 1, RelativeYear = new RelativeYear(2025), CalculatorRunClassificationId=2, Name = "CalculatorRunTest1" },
                new CalculatorRun { Id = 2, RelativeYear = new RelativeYear(2025), CalculatorRunClassificationId=2, Name = "CalculatorRunTest2" }
            };
            context.CalculatorRuns.AddRange(runs);


            var producerDetails = new List<ProducerDetail>
            {
                new ProducerDetail { Id =1 , CalculatorRunId = 1, ProducerName="Test1", ProducerId = 1, TradingName = "TN1"},
                new ProducerDetail { Id =2 , CalculatorRunId = 1, ProducerName="Test2", ProducerId = 2, TradingName = "TN2"},
                new ProducerDetail { Id =3 , CalculatorRunId = 2, ProducerName="Test1", ProducerId = 1, TradingName = "TN3"},
                new ProducerDetail { Id =4 , CalculatorRunId = 1, ProducerName="Test3", ProducerId = 3, TradingName = "TN4"},
            };

            context.ProducerDetail.AddRange(producerDetails);

            var materials = new List<Material>
            {
                new Material { Id = 5, Name = "Plastic", Code = MaterialCodes.Plastic },
                new Material { Id = 6, Name = "Steel", Code = MaterialCodes.Steel },
                new Material { Id = 3, Name = "Glass", Code = MaterialCodes.Glass },
            };
            context.Material.AddRange(materials);

            var producerReportedMaterials = new List<ProducerReportedMaterial>
            {
                new ProducerReportedMaterial { ProducerDetailId = 1, MaterialId = 1, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.Household, PackagingTonnage = 50 },
                new ProducerReportedMaterial { ProducerDetailId = 1, MaterialId = 1, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.Household, PackagingTonnage = 50 },
                new ProducerReportedMaterial { ProducerDetailId = 1, MaterialId = 2, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 100 },
                new ProducerReportedMaterial { ProducerDetailId = 1, MaterialId = 2, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 100 },
                new ProducerReportedMaterial { ProducerDetailId = 1, MaterialId = 3, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 150 },
                new ProducerReportedMaterial { ProducerDetailId = 1, MaterialId = 3, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 150 },
                new ProducerReportedMaterial { ProducerDetailId = 2, MaterialId = 1, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.Household, PackagingTonnage = 50 },
                new ProducerReportedMaterial { ProducerDetailId = 2, MaterialId = 1, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.Household, PackagingTonnage = 50 },
                new ProducerReportedMaterial { ProducerDetailId = 2, MaterialId = 2, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 100 },
                new ProducerReportedMaterial { ProducerDetailId = 2, MaterialId = 2, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 100 },
                new ProducerReportedMaterial { ProducerDetailId = 2, MaterialId = 3, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 150 },
                new ProducerReportedMaterial { ProducerDetailId = 2, MaterialId = 3, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 150 },
            };
            context.ProducerReportedMaterial.AddRange(producerReportedMaterials);


            var designatedRunInvoice = new List<ProducerDesignatedRunInvoiceInstruction>
            {
                new ProducerDesignatedRunInvoiceInstruction
                {
                    BillingInstructionId = "1_1",
                    CalculatorRunId = 1,
                    CurrentYearInvoicedTotalAfterThisRun = 100,
                    Id = 1,
                    ProducerId = 1,
                    InvoiceAmount = 100,
                    OutstandingBalance = 100,

                },
                new ProducerDesignatedRunInvoiceInstruction
                {
                    BillingInstructionId = "1_2",
                    CalculatorRunId = 1,
                    CurrentYearInvoicedTotalAfterThisRun = 100,
                    Id = 2,
                    ProducerId = 2,
                    InvoiceAmount = 100,
                    OutstandingBalance = 100,

                },
            };
            context.ProducerDesignatedRunInvoiceInstruction.AddRange(designatedRunInvoice);


            var billingInstructionList = new List<ProducerResultFileSuggestedBillingInstruction>
            {
                new ProducerResultFileSuggestedBillingInstruction
                {
                    MaterialPercentageThresholdBreached = "1%",
                    MaterialPoundThresholdBreached = "1",
                    ProducerId = 1,
                    SuggestedBillingInstruction = "Initial",
                    SuggestedInvoiceAmount = 100,
                    CalculatorRunId = 1,
                    BillingInstructionAcceptReject = "Accepted"
                },
                new ProducerResultFileSuggestedBillingInstruction
                {
                    MaterialPercentageThresholdBreached = "1%",
                    MaterialPoundThresholdBreached = "1",
                    ProducerId = 2,
                    SuggestedBillingInstruction = "Initial",
                    SuggestedInvoiceAmount = 100,
                    CalculatorRunId = 1,
                    BillingInstructionAcceptReject = "Accepted"
                },
                new ProducerResultFileSuggestedBillingInstruction
                {
                    MaterialPercentageThresholdBreached = "1%",
                    MaterialPoundThresholdBreached = "1",
                    ProducerId = 3,
                    SuggestedBillingInstruction = "Initial",
                    SuggestedInvoiceAmount = 100,
                    CalculatorRunId = 1,
                    BillingInstructionAcceptReject = "Accepted"
                }
            };
            context.ProducerResultFileSuggestedBillingInstruction.AddRange(billingInstructionList);

            var materialInvoiceTonnage = new List<ProducerInvoicedMaterialNetTonnage>
            {
                new ProducerInvoicedMaterialNetTonnage
                {
                    Id = 1,
                    CalculatorRunId = 1,
                    MaterialId = 1,
                    InvoicedNetTonnage = 100,
                    ProducerId = 1
                },
                new ProducerInvoicedMaterialNetTonnage
                {
                    Id = 2,
                    CalculatorRunId = 1,
                    MaterialId = 2,
                    InvoicedNetTonnage = 100,
                    ProducerId = 1
                },
                new ProducerInvoicedMaterialNetTonnage
                {
                    Id = 3,
                    CalculatorRunId = 1,
                    MaterialId = 1,
                    InvoicedNetTonnage = 100,
                    ProducerId = 2,
                },
                new ProducerInvoicedMaterialNetTonnage
                {
                    Id = 4,
                    CalculatorRunId = 1,
                    MaterialId = 2,
                    InvoicedNetTonnage = 100,
                    ProducerId = 2
                }
            };
            context.ProducerInvoicedMaterialNetTonnage.AddRange(materialInvoiceTonnage);

            context.SaveChanges();
        }
    }
}
