using System.Text;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Exporter.CsvExporter;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.CancelledProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.CommsCost;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Detail;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ErrorReport;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.LaDisposalCost;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Lapcap;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Modulation;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.OtherCosts;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.PartialObligations;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ProjectedProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ScaledupProducers;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.Builder;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter
{
    [TestClass]
    public class CalcResultsExporterTests
    {
        public CalcResultsExporterTests()
        {
            Fixture = new Fixture();
            MockLateReportingExporter = new();
            MockResultDetailexporter = new();
            MockOnePlusFourExporter = new();
            MockLaDisposalCostDataExporter = new();
            MockCalcResultModulationExporter = new();
            MockScaledupProducersExporter = new();
            MockPartialObligationsExporter = new();
            MockProjectedProducersExporter = new();
            MockLapcapDataExporter = new();
            MockParameterOtherCostExporter = new();
            MockCalcResultSummaryExporter = new();
            MockCalcResultCancelledProducersExporter = new();
            MockClassReportExporter = new();
            MockCommsCostExporter = new();
            TestClass = new CalcResultsExporter(
                MockLateReportingExporter.Object,
                MockResultDetailexporter.Object,
                MockOnePlusFourExporter.Object,
                MockLaDisposalCostDataExporter.Object,
                MockCalcResultModulationExporter.Object,
                MockScaledupProducersExporter.Object,
                MockPartialObligationsExporter.Object,
                MockProjectedProducersExporter.Object,
                MockLapcapDataExporter.Object,
                MockParameterOtherCostExporter.Object,
                MockCommsCostExporter.Object,
                MockCalcResultSummaryExporter.Object,
                MockCalcResultCancelledProducersExporter.Object,
                MockClassReportExporter.Object);
        }

        private Fixture Fixture { get; init; }

        private Mock<ICalcResultLateReportingExporter> MockLateReportingExporter { get; init; }

        private Mock<ICalcResultDetailExporter> MockResultDetailexporter { get; init; }

        private Mock<ICalcResultOnePlusFourApportionmentExporter> MockOnePlusFourExporter { get; init; }

        private Mock<ICalcResultLaDisposalCostExporter> MockLaDisposalCostDataExporter { get; init; }

        private Mock<ICalcResultModulationExporter> MockCalcResultModulationExporter { get; init; }

        private Mock<ICalcResultScaledupProducersExporter> MockScaledupProducersExporter { get; init; }

        private Mock<ICalcResultPartialObligationsExporter> MockPartialObligationsExporter { get; init; }

        private Mock<ICalcResultProjectedProducersExporter> MockProjectedProducersExporter { get; init; }

        private Mock<ICalcResultLapcapDataExporter> MockLapcapDataExporter { get; init; }

        private Mock<ICalcResultParameterOtherCostExporter> MockParameterOtherCostExporter { get; init; }

        private Mock<ICalcResultSummaryExporter> MockCalcResultSummaryExporter { get; init; }

        private Mock<ICalcResultCancelledProducersExporter> MockCalcResultCancelledProducersExporter { get; init; }

        private Mock<ICalcResultCommsCostExporter> MockCommsCostExporter { get; init; }

        private CalcResultsExporter TestClass { get; init; }

        private Mock<ICalcResultErrorReportExporter> MockClassReportExporter { get; init; }

        [TestMethod]
        public void Export_ShouldReturnCsvContent_WhenAllDataIsPresent()
        {
            // Arrange
            var calcResult = CreateCalcResult();
            var materials = TestDataHelper.GetMaterials();

            // Act
            var result = TestClass.Export(calcResult, materials);

            // Assert
            Assert.IsNotNull(result);

            MockLateReportingExporter.Verify(x => x.Export(It.IsAny<IImmutableList<MaterialDetail>>(), calcResult.CalcResultLateReportingTonnageData, It.IsAny<StringBuilder>()));
            MockCalcResultSummaryExporter.Verify(x => x.Export(It.IsAny<CalcResultSummary>(), It.IsAny<StringBuilder>(), It.IsAny<bool>()));
            MockLapcapDataExporter.Verify(x => x.Export(It.IsAny<CalcResultLapcapData>(), It.IsAny<ImmutableList<MaterialDetail>>(), It.IsAny<StringBuilder>()));
            MockResultDetailexporter.Verify(x => x.Export(It.IsAny<CalcResultDetail>(), It.IsAny<StringBuilder>()));
            MockLaDisposalCostDataExporter.Verify(x => x.Export(It.IsAny<bool>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<CalcResultLaDisposalCostData>(), It.IsAny<StringBuilder>()));
            MockCommsCostExporter.Verify(x => x.Export(It.IsAny<CalcResultCommsCost>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<StringBuilder>()));
            MockOnePlusFourExporter.Verify(x => x.Export(It.IsAny<CalcResultOnePlusFourApportionment>(), It.IsAny<StringBuilder>()));
            MockScaledupProducersExporter.Verify(x => x.Export(It.IsAny<CalcResultScaledupProducers>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<bool>(), It.IsAny<StringBuilder>()));
            MockPartialObligationsExporter.Verify(x => x.Export(It.IsAny<CalcResultPartialObligations>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<StringBuilder>(), It.IsAny<bool>()));
            MockProjectedProducersExporter.Verify(x => x.Export(It.IsAny<CalcResultProjectedProducers>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<StringBuilder>()), Times.Never);
            MockParameterOtherCostExporter.Verify(x => x.Export(It.IsAny<CalcResultParameterOtherCost>(), It.IsAny<StringBuilder>()));
        }

        [TestMethod]
        public void Export_ShouldReturnCsvContent_WhenAllDataIsPresent_WithProjectedProducers()
        {
            // Arrange
            var calcResult = CreateCalcResult();
            var materials = TestDataHelper.GetMaterials();
            calcResult.ApplyModulation = true;
            var materialDetails = TestDataHelper.GetMaterials();

            // Act
            var result = TestClass.Export(calcResult, materials);

            // Assert
            Assert.IsNotNull(result);

            MockScaledupProducersExporter.Verify(x => x.Export(It.IsAny<CalcResultScaledupProducers>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<bool>(), It.IsAny<StringBuilder>()), Times.Never);
            MockProjectedProducersExporter.Verify(x => x.Export(It.IsAny<CalcResultProjectedProducers>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<StringBuilder>()));
        }

        [TestMethod]
        public void AppendFileInfoTest()
        {
            var csvContent = new StringBuilder();
            CalcResultDetailExporter.AppendFileInfo(csvContent, "Label", "Filename,20/12/2024,User");
            Assert.IsTrue(csvContent.ToString().Contains("Label"));
            Assert.IsTrue(csvContent.ToString().Contains("Filename"));
            Assert.IsTrue(csvContent.ToString().Contains("20/12/2024"));
        }

        private static CalcResult CreateCalcResult()
        {
            return new CalcResult
            {
                ApplyModulation = false,
                CalcResultLapcapData = new CalcResultLapcapData
                {
                    ByMaterial = new(){
                        ["AL"] = new ByCountryCost
                        {
                            England         = 13280.45m,
                            Wales           = 210.28m,
                            Scotland        = 91.00m,
                            NorthernIreland = 91.00m
                        }
                    }
                },
                CalcResultLateReportingTonnageData = new CalcResultLateReportingTonnage
                {
                    ByMaterial = new Dictionary<string, CalcResultLateReportingTonnageDetail>
                    {
                        ["AL"] = new() { Red = 1000.00m, Amber = 2000.00m, Green = 5000.00m, Total = 8000.00m },
                        ["PL"] = new() { Red = 1000.00m, Amber =  500.00m, Green =  500.00m, Total = 2000.00m },
                    }
                },
                CalcResultParameterOtherCost = new CalcResultParameterOtherCost
                {
                    BadDebtValue = 6m,
                    LaDataPrepCharge = new() { England = 40, Wales = 30, Scotland = 20, NorthernIreland = 10 },
                    SaOperatingCost = new() { England = 40, Wales = 30, Scotland = 20, NorthernIreland = 10 },
                    SchemeSetupCost = new() { England = 40, Wales = 30, Scotland = 20, NorthernIreland = 10 },
                },
                CalcResultOnePlusFourApportionment = new CalcResultOnePlusFourApportionment
                {
                    LaDisposalCost = new() { England = 0.10M, Wales = 020M, NorthernIreland = 0.15M, Scotland = 0.15M },
                    LADataPrepCharge = new() { England = 0.10M, Wales = 020M, Scotland = 0.15M, NorthernIreland = 0.15M }
                },
                CalcResultCommsCostReportDetail = TestDataHelper.GetCalcResultCommsCostReportDetail(),
                CalcResultLaDisposalCostData = new CalcResultLaDisposalCostData() { ByMaterial = [] },
                CalcResultScaledupProducers = new CalcResultScaledupProducers
                {
                    ScaledupProducers = GetCalcResultScaledupProducerList(),
                },
                CalcResultPartialObligations = new CalcResultPartialObligations(),
                CalcResultSummary = new CalcResultSummary
                {
                    ResultSummaryHeader = new CalcResultSummaryHeader
                    {
                        Name = "SummaryData",
                    },
                    ProducerDisposalFeesHeaders = new List<CalcResultSummaryHeader>
                    {
                       new CalcResultSummaryHeader
                       {
                           Name = "Producer disposal fees header",
                           ColumnIndex = 1,
                       },
                    },
                    MaterialBreakdownHeaders = new List<CalcResultSummaryHeader>
                    {
                       new CalcResultSummaryHeader
                       {
                           Name = "Material breakdown header",
                           ColumnIndex = 1,
                       },
                    },
                    ColumnHeaders = new List<CalcResultSummaryHeader>
                    {
                       new CalcResultSummaryHeader
                       {
                           Name = "Column header",
                           ColumnIndex = 1,
                       },
                    },
                    ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>
                    {
                        new CalcResultSummaryProducerDisposalFees
                        {
                            ProducerCommsFeesByMaterial =
                                new Fixture().Create<Dictionary<string, CalcResultSummaryProducerCommsFeesCostByMaterial>>(),
                            ProducerDisposalFeesByMaterial =
                                new Fixture().Create<Dictionary<string, CalcResultSummaryProducerDisposalFeesByMaterial>>(),
                            ProducerId = "1",
                            ProducerName = "Test",
                            TotalProducerDisposalFeeWithBadDebtProvision = 100,
                            TotalProducerCommsFeeWithBadDebtProvision = 100,
                            SubsidiaryId = "1",
                            ProducerOverallPercentageOfCostsForOnePlus2A2B2C = 1,
                        },
                    },
                },
                CalcResultDetail = new CalcResultDetail
                {
                    RunId = 1,
                    RunDate = DateTime.UtcNow,
                    RunName = "CalculatorRunName",
                    RelativeYear = new RelativeYear(2024)
                },
                CalcResultProjectedProducers = new CalcResultProjectedProducers(),
            };
        }

        private static Dictionary<string, CalcResultScaledupProducerTonnage> GetScaledupProducerTonnageByMaterial()
        {
            var tonnageByMaterial = new Dictionary<string, CalcResultScaledupProducerTonnage>();

            tonnageByMaterial.Add(
                "AL",
                new CalcResultScaledupProducerTonnage
                {
                    ReportedHouseholdPackagingWasteTonnage = 1000,
                    ReportedPublicBinTonnage = 100,
                    TotalReportedTonnage = 1100,
                    ReportedSelfManagedConsumerWasteTonnage = 500,
                    NetReportedTonnage = 1100,
                    ScaledupReportedHouseholdPackagingWasteTonnage = 2000,
                    ScaledupReportedPublicBinTonnage = 200,
                    ScaledupTotalReportedTonnage = 2200,
                    ScaledupReportedSelfManagedConsumerWasteTonnage = 1000,
                    ScaledupNetReportedTonnage = 2200,
                });

            return tonnageByMaterial;
        }

        private static ImmutableList<CalcResultScaledupProducer> GetCalcResultScaledupProducerList()
        {
            return [
                new CalcResultScaledupProducer
                {
                    ProducerId = 101001,
                    SubsidiaryId = string.Empty,
                    ProducerName = "Allied Packaging",
                    Level = "1",
                    SubmissionPeriodCode = "2024-P2",
                    DaysInSubmissionPeriod = 91,
                    DaysInWholePeriod = 91,
                    ScaleupFactor = 2,
                    ScaledupProducerTonnageByMaterial = GetScaledupProducerTonnageByMaterial(),
                }
            ];
        }
    }
}
