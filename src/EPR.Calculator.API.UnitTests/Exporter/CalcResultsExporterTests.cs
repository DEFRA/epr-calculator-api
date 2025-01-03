using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EPR.Calculator.API.Tests.Controllers;
using EPR.Calculator.API.Data.Migrations;
using EPR.Calculator.API.Exporter;
using EPR.Calculator.API.Models;
using Microsoft.AspNetCore.Routing;
using EPR.Calculator.API.Constants;

namespace EPR.Calculator.API.UnitTests.Exporter
{
    [TestClass]
    public class CalcResultsExporterTests
    {
        [TestMethod]
        public void Export_ShouldReturnCsvContent_WhenAllDataIsPresent()
        {
            // Arrange
            var exporter = new CalcResultsExporter();
            var calcResult = CreateCalcResult();

            // Act
            var result = exporter.Export(calcResult);

            // Assert
            Assert.IsNotNull(result);
            // Additional assertions to verify the content of the CSV
        }

        [TestMethod]
        public void Export_DataFormatting_IsCorrect()
        {
            // Arrange
            var exporter = new CalcResultsExporter();
            var calcResult = CreateCalcResult();

            // Act
            var result = exporter.Export(calcResult);

            // Assert
            Assert.IsTrue(result.Contains("LAPCAP Data"));
            Assert.IsTrue(result.Contains("Late Reporting Tonnage"));
            Assert.IsTrue(result.Contains("Parameters - Other"));
            Assert.IsTrue(result.Contains("1 + 4 Apportionment %s"));
            Assert.IsTrue(result.Contains("4 LA Data Prep Charge"));
            Assert.IsTrue(result.Contains("5 Scheme set up cost Yearly Cost"));
            Assert.IsTrue(result.Contains("some test"));
        }

        [TestMethod]
        public void Export_CsvContent_HasCorrectNumberOfLineBreaks()
        {
            // Arrange
            var exporter = new CalcResultsExporter();
            var calcResult = CreateCalcResult();

            // Act
            var result = exporter.Export(calcResult);
            var lines = result.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            // Assert
            int expectedLineCount = 57; // Adjust this based on your expected number of lines
            Assert.AreEqual(expectedLineCount, lines.Length);
        }

        [TestMethod]
        public void Export_ShouldReturnEmptyString_WhenResultsIsNull()
        {
            // Arrange
            CalcResult results = null;
            var exporter = new CalcResultsExporter();

            // Act
            var csvContent = exporter.Export(results);

            // Assert
            Assert.AreEqual(string.Empty, csvContent, "The Export method should return an empty string when results is null.");
        }
   
        [TestMethod]
        public void Export_ShouldIncludeCalcResultRunNameDetails()
        {
            var results = CreateCalcResult();
            var exporter = new CalcResultsExporter();

            var result = exporter.Export(results);

            Assert.IsTrue(result.Contains("CalculatorRunName"));
        }

        [TestMethod]
        public void Export_ShouldIncludeLapcapData_WhenNotNull()
        {
            // Arrange
            var results = CreateCalcResult();
            var exporter = new CalcResultsExporter();


            // Act
            var result = exporter.Export(results);

            // Assert
            Assert.IsTrue(result.Contains("LAPCAP Data"));
        }

        [TestMethod]
        public void Export_ShouldIncludeLateReportingData_WhenNotNull()
        {
            // Arrange
            var results = CreateCalcResult();
            var exporter = new CalcResultsExporter();
            //Act
            var result = exporter.Export(results);

            //Assert
            Assert.IsTrue(result.Contains("Late Reporting Tonnage"));
        }

        [TestMethod]
        public void Export_ShouldIncludeOtherCosts_WhenNotNull()
        {
            // Arrange
            var results = CreateCalcResult();
            var exporter = new CalcResultsExporter();
            //Act
            var result = exporter.Export(results);

            //Assert
            Assert.IsTrue(result.Contains("Parameters - Other"));
        }

        [TestMethod]
        public void Export_ShouldIncludeOnePlusFourApportionment_WhenNotNull()
        { // Arrange
            var results = CreateCalcResult();
            var exporter = new CalcResultsExporter();
            //Act
            var result = exporter.Export(results);

            //Assert
            Assert.IsTrue(result.Contains("1 + 4 Apportionment %s"));
        }

        [TestMethod]
        public void Export_ShouldIncludeCommCost_WhenNotNull()
        {
            // Arrange
            var results = CreateCalcResult();
            var exporter = new CalcResultsExporter();
            //Act
            var result = exporter.Export(results);

            //Assert
            Assert.IsTrue(result.Contains("4 LA Data Prep Charge"));
        }

        [TestMethod]
        public void Export_ShouldIncludeLaDisposalCostData_WhenNotNull()
        {
            // Arrange
            var results = CreateCalcResult();
            var exporter = new CalcResultsExporter();

            //Act
            var result = exporter.Export(results);

            //Assert
            Assert.IsTrue(result.Contains("5 Scheme set up cost Yearly Cost"));
        }

        [TestMethod]
        public void Export_ShouldIncludeSummaryData_WhenNotNull()
        { 
            // Arrange
            var results = CreateCalcResult();
            var exporter = new CalcResultsExporter();

            //Act
            var result = exporter.Export(results);

            //Assert

            Assert.IsTrue(result.Contains("SummaryData"));
        }

        [TestMethod]
        public void AppendDataIfNotNull_ShouldCallAppendMethod_WhenDataIsNotNull()
        {
            var csvContent = new StringBuilder();
            var exporter = new CalcResultsExporter();
            var data = new object();

            exporter.AppendDataIfNotNull(data, (d, sb) => sb.Append("Data"), csvContent);

            Assert.IsTrue(csvContent.ToString().Contains("Data"));
        }

        [TestMethod]
        public void AppendDataIfNotNull_ShouldNotCallAppendMethod_WhenDataIsNull()
        {
            var csvContent = new StringBuilder();
            var exporter = new CalcResultsExporter();
            object data = null;

            exporter.AppendDataIfNotNull(data, (d, sb) => sb.Append("Data"), csvContent);

            Assert.IsFalse(csvContent.ToString().Contains("Data"));
        }

        [TestMethod]
        public void Export_ShouldGenerateEmptyCsv_WhenNoData()
        {
            // Arrange
            var results = new CalcResult
            {
                CalcResultLapcapData = null,
                CalcResultLateReportingTonnageData = null,
                CalcResultParameterOtherCost = null
            };
            var exporter = new CalcResultsExporter();

            // Act
            if (results != null)
            {
                var csvContent = exporter.Export(results);

                // Assert
                Assert.IsFalse(string.IsNullOrEmpty(csvContent), "CSV content should not be empty.");
                Assert.IsFalse(csvContent.Contains("LapcapData"), "CSV content should not contain LapcapData.");
                Assert.IsFalse(csvContent.Contains("LateReportingData"),
                    "CSV content should not contain LateReportingData.");
                Assert.IsFalse(csvContent.Contains("OtherCosts"), "CSV content should not contain OtherCosts.");
                Assert.IsFalse(csvContent.Contains("OnePlusFourApportionment"),
                    "CSV content should not contain OnePlusFourApportionment.");
                Assert.IsFalse(csvContent.Contains("CommsCost"), "CSV content should not contain CommsCost.");
                Assert.IsFalse(csvContent.Contains("LaDisposalCostData"),
                    "CSV content should not contain LaDisposalCostData.");
                Assert.IsFalse(csvContent.Contains("SummaryData"), "CSV content should not contain SummaryData.");
            }
        }

        private CalcResult CreateCalcResult()
        {
            return new CalcResult
            {
                CalcResultLapcapData = new CalcResultLapcapData
                {
                    Name = "LAPCAP Data",
                    CalcResultLapcapDataDetails = new List<CalcResultLapcapDataDetails>
            {
                new CalcResultLapcapDataDetails
                {
                    Name = "Total",
                    EnglandDisposalCost = "£13,280.45",
                    WalesDisposalCost = "£210.28",
                    ScotlandDisposalCost = "£161.07",
                    NorthernIrelandDisposalCost = "£91.00",
                    TotalDisposalCost = "£13,742.80",
                    EnglandCost = 13280.45m,
                    WalesCost = 210.28m,
                    ScotlandCost = 91.00m,
                    NorthernIrelandCost = 91.00m,
                    TotalCost = 13742.80m
                }
            }
                },
                CalcResultLateReportingTonnageData = new CalcResultLateReportingTonnage
                {
                    Name = "Late Reporting Tonnage",
                    MaterialHeading = string.Empty,
                    TonnageHeading = string.Empty,
                    CalcResultLateReportingTonnageDetails = new List<CalcResultLateReportingTonnageDetail>
            {
                new CalcResultLateReportingTonnageDetail
                {
                    Name = "Aluminium",
                    TotalLateReportingTonnage = 8000.00m
                },
                new CalcResultLateReportingTonnageDetail
                {
                    Name = "Plastic",
                    TotalLateReportingTonnage = 2000.00m
                },
                new CalcResultLateReportingTonnageDetail
                {
                    Name = "Total",
                    TotalLateReportingTonnage = 10000.00m
                }
            }
                },
                CalcResultParameterOtherCost = new CalcResultParameterOtherCost
                {
                    BadDebtProvision = new KeyValuePair<string, string>("key1", "6%"),
                    BadDebtValue = 6m,
                    Details = new List<CalcResultParameterOtherCostDetail>
            {
                new CalcResultParameterOtherCostDetail
                {
                    Name = "4 LA Data Prep Charge",
                    OrderId = 1,
                    England = "£40.00",
                    EnglandValue = 40,
                    Wales = "£30.00",
                    WalesValue = 30,
                    Scotland = "£20.00",
                    ScotlandValue = 20,
                    NorthernIreland = "£10.00",
                    NorthernIrelandValue = 10,
                    Total = "£100.00",
                    TotalValue = 100
                }
            },
                    Materiality = new List<CalcResultMateriality>
            {
                new CalcResultMateriality
                {
                    Amount = "Amount £s",
                    AmountValue = 0,
                    Percentage = "%",
                    PercentageValue = 0,
                    SevenMateriality = "7 Materiality"
                }
            },
                    Name = "Parameters - Other",
                    SaOperatingCost = new List<CalcResultParameterOtherCostDetail>
            {
                new CalcResultParameterOtherCostDetail
                {
                    Name = string.Empty,
                    OrderId = 1,
                    England = "£40.00",
                    EnglandValue = 40,
                    Wales = "£30.00",
                    WalesValue = 30,
                    Scotland = "£20.00",
                    ScotlandValue = 20,
                    NorthernIreland = "£10.00",
                    NorthernIrelandValue = 10,
                    Total = "£100.00",
                    TotalValue = 100
                }
            },
                    SchemeSetupCost = new CalcResultParameterOtherCostDetail
                    {
                        Name = "5 Scheme set up cost Yearly Cost",
                        OrderId = 1,
                        England = "£40.00",
                        EnglandValue = 40,
                        Wales = "£30.00",
                        WalesValue = 30,
                        Scotland = "£20.00",
                        ScotlandValue = 20,
                        NorthernIreland = "£10.00",
                        NorthernIrelandValue = 10,
                        Total = "£100.00",
                        TotalValue = 100
                    }
                },
                CalcResultOnePlusFourApportionment = new CalcResultOnePlusFourApportionment
                {
                    CalcResultOnePlusFourApportionmentDetails = new List<CalcResultOnePlusFourApportionmentDetail>
            {
                new CalcResultOnePlusFourApportionmentDetail
                {
                    EnglandDisposalTotal = "80",
                    NorthernIrelandDisposalTotal = "70",
                    ScotlandDisposalTotal = "30",
                    WalesDisposalTotal = "20",
                    AllTotal = 0.1M,
                    EnglandTotal = 14.53M,
                    NorthernIrelandTotal = 0.15M,
                    ScotlandTotal = 1.15M,
                    WalesTotal = 0.20M,
                    Name = "1 + 4 Apportionment %s",
                },
                new CalcResultOnePlusFourApportionmentDetail
                {
                    EnglandDisposalTotal = "80",
                    NorthernIrelandDisposalTotal = "70",
                    ScotlandDisposalTotal = "30",
                    WalesDisposalTotal = "20",
                    AllTotal = 0.1M,
                    EnglandTotal = 0.10M,
                    NorthernIrelandTotal = 0.15M,
                    ScotlandTotal = 0.15M,
                    WalesTotal = 0.20M,
                    Name = "Test",
                }
            },
                    Name = "some test"
                },
                CalcResultCommsCostReportDetail = new CalcResultCommsCost
                {
                    CalcResultCommsCostCommsCostByMaterial = new List<CalcResultCommsCostCommsCostByMaterial>
            {
                new CalcResultCommsCostCommsCostByMaterial
                {
                    CommsCostByMaterialPricePerTonne = "0.42",
                    Name = "Aluminium",
                },
                new CalcResultCommsCostCommsCostByMaterial
                {
                    CommsCostByMaterialPricePerTonne = "0.3",
                    Name = "Glass",
                }
            }
                },
                CalcResultLaDisposalCostData = new CalcResultLaDisposalCostData
                {
                    CalcResultLaDisposalCostDetails = new List<CalcResultLaDisposalCostDataDetail>
            {
                new CalcResultLaDisposalCostDataDetail
                {
                    DisposalCostPricePerTonne = "20",
                    England = "EnglandTest",
                    Wales = "WalesTest",
                    Name = "ScotlandTest",
                    Scotland = "ScotlandTest",
                    Material = "Material1",
                    NorthernIreland = "NorthernIrelandTest",
                    Total = "null",
                    ProducerReportedHouseholdPackagingWasteTonnage = "null"
                },
                new CalcResultLaDisposalCostDataDetail
                {
                    DisposalCostPricePerTonne = "20",
                    England = "EnglandTest",
                    Wales = "WalesTest",
                    Name = "Material1",
                    Scotland = "ScotlandTest",
                    NorthernIreland = "NorthernIrelandTest",
                    Total = "null",
                    ProducerReportedHouseholdPackagingWasteTonnage = "null",
                },
                new CalcResultLaDisposalCostDataDetail
                {
                    DisposalCostPricePerTonne = "10",
                    England = "EnglandTest",
                    Wales = "WalesTest",
                    Name = "Material2",
                    Scotland = "ScotlandTest",
                    NorthernIreland = "NorthernIrelandTest",
                    Total = "100",
                    ProducerReportedHouseholdPackagingWasteTonnage = "null",
                }
            },
                    Name = "some test"
                },
                CalcResultSummary = new CalcResultSummary
                {
                    ResultSummaryHeader = new CalcResultSummaryHeader
                    {
                        Name = "SummaryData",
                    },
                    ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>
            {
                new CalcResultSummaryProducerDisposalFees
                {
                    ProducerCommsFeesByMaterial = new Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>(),
                    ProducerDisposalFeesByMaterial = new Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>(),
                    ProducerId = "1",
                    ProducerName = "Test",
                    TotalProducerDisposalFeeWithBadDebtProvision = 100,
                    TotalProducerCommsFeeWithBadDebtProvision = 100,
                    SubsidiaryId = "1",
                    ProducerOverallPercentageOfCostsForOnePlus2A2B2C = 1
                }
            }
                },
                CalcResultDetail = new CalcResultDetail
                {
                    RunId = 1,
                    RunDate = DateTime.Now,
                    RunName = "CalculatorRunName"
                }
            };
        }
    }
}