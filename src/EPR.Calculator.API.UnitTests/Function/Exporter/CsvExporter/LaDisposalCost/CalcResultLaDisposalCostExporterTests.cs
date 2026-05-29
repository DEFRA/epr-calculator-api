using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.LaDisposalCost;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.Builder;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter.LaDisposalCost
{
    [TestClass]
    public class CalcResultLaDisposalCostExporterTests
    {
        private CalcResultLaDisposalCostExporter exporter;

        public CalcResultLaDisposalCostExporterTests()
        {
            exporter = new CalcResultLaDisposalCostExporter();
        }

        [TestMethod]
        public void Export_ShouldIncludeLaDisposalCostData_PreModulation()
        {
            // Arrange
            var calcResultLaDisposalCostData = GetCalcResultLaDisposalCostData();

            var csvContent = new StringBuilder();

            // Act
            exporter.Export(applyModulation: false, TestDataHelper.GetMaterials(), calcResultLaDisposalCostData, csvContent);

            // Assert
            var result = csvContent.ToString().Split("\n").Select(s => s.TrimEnd(',')).ToArray();
            Console.WriteLine(string.Join("\n", result));

            var expected = new[] {
                new string[] {},
                new string[] {},
                new[] { "LA Disposal Cost Data" },
                new[] { "Material",
                        "England",
                        "Wales",
                        "Scotland",
                        "Northern Ireland",
                        "Total",
                        "Producer Household Packaging Tonnage",
                        "Public Bin Tonnage",
                        "Household Drinks Containers Tonnage",
                        "Late Reporting Tonnage",
                        "Producer Household Tonnage + Late Reporting Tonnage + Public Bin Tonnage + Household Drinks Containers Tonnage",
                        "Disposal Cost Price Per Tonne"
                },
                new[] { "Aluminium"      ,"£100.00", "£50.00","£0.00","£0.00","£150.00","0.000","0.000","100.123","0.000","100.123",  "£1.4982" },
                new[] { "Fibre composite","£100.00", "£50.00","£0.00","£0.00","£150.00","2.000","0.000",     null,"0.000",  "0.770","£194.8052" },
                new[] { "Total"          ,"£200.00","£100.00","£0.00","£0.00","£300.00","2.000","0.000","100.123","0.000","100.893" },
                new string[] { }
            };

            CsvTestUtils.AssertCsv(expected, result);
        }

        [TestMethod]
        public void Export_ShouldIncludeLaDisposalCostData_Modulation()
        {
            // Arrange
            var calcResultLaDisposalCostData = GetCalcResultLaDisposalCostData();

            var csvContent = new StringBuilder();

            // Act
            exporter.Export(applyModulation: true, TestDataHelper.GetMaterials(), calcResultLaDisposalCostData, csvContent);

            // Assert
            var result = csvContent.ToString().Split("\n").Select(s => s.TrimEnd(',')).ToArray();
            Console.WriteLine(string.Join("\n", result));

            var expected = new[] {
                new string[] {},
                new string[] {},
                new[] { "LA Disposal Cost Data" },
                new[] { "Material",
                        "England",
                        "Wales",
                        "Scotland",
                        "Northern Ireland",
                        "Total",
                        "Producer Household Packaging Tonnage",
                        "Public Bin Tonnage",
                        "Household Drinks Containers Tonnage",
                        "Late Reporting Tonnage",
                        "Actioned Self Managed Consumer Waste",
                        "Net Tonnage + Late Reporting Tonnage",
                        "Disposal Cost Price Per Tonne"
                },
                new[] { "Aluminium"      ,"£100.00", "£50.00","£0.00","£0.00","£150.00","0.000","0.000","100.123","0.000","0.000","100.123",  "£1.4982" },
                new[] { "Fibre composite","£100.00", "£50.00","£0.00","£0.00","£150.00","2.000","0.000",     null,"0.000","1.230",  "0.770","£194.8052" },
                new[] { "Total"          ,"£200.00","£100.00","£0.00","£0.00","£300.00","2.000","0.000","100.123","0.000","1.230", "100.893" },
                new string[] { }
            };

            CsvTestUtils.AssertCsv(expected, result);
        }

        private CalcResultLaDisposalCostData GetCalcResultLaDisposalCostData()
        {
            return new CalcResultLaDisposalCostData
            {
                ByMaterial = new Dictionary<string, CalcResultLaDisposalCostDataDetail>
                {
                    ["AL"] = new CalcResultLaDisposalCostDataDetail
                    {
                        Cost = new() { England = 100m, Wales = 50m, Scotland = 0m, NorthernIreland = 0m },
                        HouseholdPackagingWasteTonnage = 0m,
                        PublicBinTonnage = 0m,
                        HouseholdDrinkContainersTonnage = 100.12345m
                    },
                    ["FC"] = new CalcResultLaDisposalCostDataDetail
                    {
                        Cost = new() { England = 100m, Wales = 50m, Scotland = 0m, NorthernIreland = 0m },
                        HouseholdPackagingWasteTonnage = 2m,
                        PublicBinTonnage = 0m,
                        HouseholdDrinkContainersTonnage = 0m,
                        ActionedSelfManagedConsumerWasteTonnage = 1.23m
                    }
                }
            };
        }
    }
}
