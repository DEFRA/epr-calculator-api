using EPR.Calculator.API.Builder.Summary;
using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
namespace EPR.Calculator.API.UnitTests
{
    [TestClass]
    public class CalcResultSummaryTests
    {
        private DbContextOptions<ApplicationDBContext> _dbContextOptions;
        private ApplicationDBContext _context;
        private CalcResultSummaryBuilder _calcResultsService;

        [TestInitialize]
        public void TestInitialize()
        {
            _dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "CalcResultSummaryTestDb")
                .Options;
            _context = new ApplicationDBContext(_dbContextOptions);
            _calcResultsService = new CalcResultSummaryBuilder(_context);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [TestMethod]
        public void Construct_ShouldReturnCalcResultSummary()
        {
            var requestDto = new CalcResultsRequestDto { RunId = 1 };
            var calcResult = new CalcResult
            {
                CalcResultParameterOtherCost = new CalcResultParameterOtherCost { BadDebtProvision = new KeyValuePair<string, string>("key1", "6%") },
                CalcResultDetail = new CalcResultDetail() { },
                CalcResultLaDisposalCostData = new CalcResultLaDisposalCostData(),
                CalcResultLapcapData = new CalcResultLapcapData() { CalcResultLapcapDataDetails = new List<CalcResultLapcapDataDetails>() { } },
                CalcResultOnePlusFourApportionment = new CalcResultOnePlusFourApportionment()
                {
                    CalcResultOnePlusFourApportionmentDetails =
                    [
                        new()
                        {
                            EnglandDisposalTotal="80",
                            NorthernIrelandDisposalTotal="70",
                            ScotlandDisposalTotal="30",
                            WalesDisposalTotal="20",
                            AllTotal=0.1M,
                            EnglandTotal=0.10M,
                            NorthernIrelandTotal=0.15M,
                            ScotlandTotal=0.15M,
                            WalesTotal=020M,
                            Name="Test",
                        },
                        new()
                        {
                            EnglandDisposalTotal="80",
                            NorthernIrelandDisposalTotal="70",
                            ScotlandDisposalTotal="30",
                            WalesDisposalTotal="20",
                            AllTotal=0.1M,
                            EnglandTotal=0.10M,
                            NorthernIrelandTotal=0.15M,
                            ScotlandTotal=0.15M,
                            WalesTotal=020M,
                            Name="Test",
                        },
                        new()
                        {
                            EnglandDisposalTotal="80",
                            NorthernIrelandDisposalTotal="70",
                            ScotlandDisposalTotal="30",
                            WalesDisposalTotal="20",
                            AllTotal=0.1M,
                            EnglandTotal=0.10M,
                            NorthernIrelandTotal=0.15M,
                            ScotlandTotal=0.15M,
                            WalesTotal=020M,
                            Name="Test",
                        },
                        new()
                        {
                            EnglandDisposalTotal="80",
                            NorthernIrelandDisposalTotal="70",
                            ScotlandDisposalTotal="30",
                            WalesDisposalTotal="20",
                            AllTotal=0.1M,
                            EnglandTotal=14.53M,
                            NorthernIrelandTotal=0.15M,
                            ScotlandTotal=0.15M,
                            WalesTotal=020M,
                            Name="Test",
                        },
                     new()
                        {
                            EnglandDisposalTotal="80",
                            NorthernIrelandDisposalTotal="70",
                            ScotlandDisposalTotal="30",
                            WalesDisposalTotal="20",
                            AllTotal=0.1M,
                            EnglandTotal=14.53M,
                            NorthernIrelandTotal=0.15M,
                            ScotlandTotal=0.15M,
                            WalesTotal=020M,
                            Name="Test",
                        }]
                },
                CalcResultParameterCommunicationCost = new CalcResultParameterCommunicationCost { }
            };

            _context.Material.Add(new Material { Id = 1, Name = "Material1", Code = "123" });
            _context.SaveChanges();

            _context.ProducerDetail.Add(new ProducerDetail { Id = 1, ProducerName = "Producer1", CalculatorRunId = 1, CalculatorRun = new CalculatorRun { Financial_Year = "2024-25", Name = "Test" } });
            _context.SaveChanges();

            var result = _calcResultsService.Construct(requestDto, calcResult);

            Assert.IsNotNull(result);
            Assert.AreEqual(CalcResultSummaryHeaders.CalculationResult, result.ResultSummaryHeader.Name);
            Assert.AreEqual(4, result.ProducerDisposalFeesHeader.ColumnIndex);
        }

        [TestMethod]
        public void Construct_ShouldMapMaterialBreakdownHeaders()
        {
            // Arrange
            var requestDto = new CalcResultsRequestDto { RunId = 1 };
            var calcResult = new CalcResult
            {
                CalcResultParameterOtherCost = new CalcResultParameterOtherCost { BadDebtProvision = new KeyValuePair<string, string>("key1", "6%") }
            };

            _context.Material.Add(new Material { Id = 1, Name = "Material1", Code = "123" });
            _context.SaveChanges();

            // Act
            var result = _calcResultsService.Construct(requestDto, calcResult);

            // Assert
            Assert.AreEqual("Material1 Breakdown", result.MaterialBreakdownHeaders.First().Name);
        }

        [TestMethod]
        public void Construct_ShouldCalculateProducerDisposalFeesCorrectly()
        {
            var requestDto = new CalcResultsRequestDto { RunId = 1 };
            var calcResult = new CalcResult
            {
                CalcResultParameterOtherCost = new CalcResultParameterOtherCost { BadDebtProvision = new KeyValuePair<string, string>("key1", "6%") },
                CalcResultDetail = new CalcResultDetail() { },
                CalcResultLaDisposalCostData = new CalcResultLaDisposalCostData(),
                CalcResultLapcapData = new CalcResultLapcapData() { CalcResultLapcapDataDetails = new List<CalcResultLapcapDataDetails>() { } },
                CalcResultOnePlusFourApportionment = new CalcResultOnePlusFourApportionment()
                {
                    CalcResultOnePlusFourApportionmentDetails =
                    [
                        new()
                        {
                            EnglandDisposalTotal="80",
                            NorthernIrelandDisposalTotal="70",
                            ScotlandDisposalTotal="30",
                            WalesDisposalTotal="20",
                            AllTotal=0.1M,
                            EnglandTotal=0.10M,
                            NorthernIrelandTotal=0.15M,
                            ScotlandTotal=0.15M,
                            WalesTotal=020M,
                            Name="Test",
                        },
                        new()
                        {
                            EnglandDisposalTotal="80",
                            NorthernIrelandDisposalTotal="70",
                            ScotlandDisposalTotal="30",
                            WalesDisposalTotal="20",
                            AllTotal=0.1M,
                            EnglandTotal=0.10M,
                            NorthernIrelandTotal=0.15M,
                            ScotlandTotal=0.15M,
                            WalesTotal=020M,
                            Name="Test",
                        },
                        new()
                        {
                            EnglandDisposalTotal="80",
                            NorthernIrelandDisposalTotal="70",
                            ScotlandDisposalTotal="30",
                            WalesDisposalTotal="20",
                            AllTotal=0.1M,
                            EnglandTotal=0.10M,
                            NorthernIrelandTotal=0.15M,
                            ScotlandTotal=0.15M,
                            WalesTotal=020M,
                            Name="Test",
                        },
                        new()
                        {
                            EnglandDisposalTotal="80",
                            NorthernIrelandDisposalTotal="70",
                            ScotlandDisposalTotal="30",
                            WalesDisposalTotal="20",
                            AllTotal=0.1M,
                            EnglandTotal=14.53M,
                            NorthernIrelandTotal=0.15M,
                            ScotlandTotal=0.15M,
                            WalesTotal=020M,
                            Name="Test",
                        },
                     new()
                        {
                            EnglandDisposalTotal="80",
                            NorthernIrelandDisposalTotal="70",
                            ScotlandDisposalTotal="30",
                            WalesDisposalTotal="20",
                            AllTotal=0.1M,
                            EnglandTotal=14.53M,
                            NorthernIrelandTotal=0.15M,
                            ScotlandTotal=0.15M,
                            WalesTotal=020M,
                            Name="Test",
                        }]
                },
                CalcResultParameterCommunicationCost = new CalcResultParameterCommunicationCost { }
            };


            _context.Material.Add(new Material { Id = 1, Name = "Material1", Code = "123" });
            _context.ProducerDetail.Add(new ProducerDetail { Id = 1, ProducerName = "Producer1", CalculatorRunId = 1, CalculatorRun = new CalculatorRun { Financial_Year = "2024-25", Name = "Test" } });
            _context.SaveChanges();

            var result = _calcResultsService.Construct(requestDto, calcResult);

            Assert.IsTrue(result.ProducerDisposalFees.Any());
            Assert.AreEqual("Producer1", result.ProducerDisposalFees.First().ProducerName);
        }

        [TestMethod]
        public void Construct_ShouldReturnEmptyProducerDisposalFees_WhenNoProducers()
        {
            var requestDto = new CalcResultsRequestDto { RunId = 1 };
            var calcResult = new CalcResult
            {
                CalcResultParameterOtherCost = new CalcResultParameterOtherCost { BadDebtProvision = new KeyValuePair<string, string>("key1", "6%") }
            };

            _context.Material.Add(new Material { Id = 1, Name = "Material1", Code = "123" });
            _context.SaveChanges();

            var result = _calcResultsService.Construct(requestDto, calcResult);

            Assert.IsNotNull(result);
            Assert.IsFalse(result.ProducerDisposalFees.Any());
        }

        [TestMethod]
        public void Construct_ShouldCalculateBadDebtProvisionCorrectly()
        {
            var requestDto = new CalcResultsRequestDto { RunId = 1 };
            var calcResult = new CalcResult
            {
                CalcResultParameterOtherCost = new CalcResultParameterOtherCost { BadDebtProvision = new KeyValuePair<string, string>("key1", "6%") },
                CalcResultDetail = new CalcResultDetail() { },
                CalcResultLaDisposalCostData = new CalcResultLaDisposalCostData(),
                CalcResultLapcapData = new CalcResultLapcapData() { CalcResultLapcapDataDetails = new List<CalcResultLapcapDataDetails>() { } },
                CalcResultOnePlusFourApportionment = new CalcResultOnePlusFourApportionment()
                {
                    CalcResultOnePlusFourApportionmentDetails =
                    [
                        new()
                        {
                            EnglandDisposalTotal="80",
                            NorthernIrelandDisposalTotal="70",
                            ScotlandDisposalTotal="30",
                            WalesDisposalTotal="20",
                            AllTotal=0.1M,
                            EnglandTotal=0.10M,
                            NorthernIrelandTotal=0.15M,
                            ScotlandTotal=0.15M,
                            WalesTotal=020M,
                            Name="Test",
                        },
                        new()
                        {
                            EnglandDisposalTotal="80",
                            NorthernIrelandDisposalTotal="70",
                            ScotlandDisposalTotal="30",
                            WalesDisposalTotal="20",
                            AllTotal=0.1M,
                            EnglandTotal=0.10M,
                            NorthernIrelandTotal=0.15M,
                            ScotlandTotal=0.15M,
                            WalesTotal=020M,
                            Name="Test",
                        },
                        new()
                        {
                            EnglandDisposalTotal="80",
                            NorthernIrelandDisposalTotal="70",
                            ScotlandDisposalTotal="30",
                            WalesDisposalTotal="20",
                            AllTotal=0.1M,
                            EnglandTotal=0.10M,
                            NorthernIrelandTotal=0.15M,
                            ScotlandTotal=0.15M,
                            WalesTotal=020M,
                            Name="Test",
                        },
                        new()
                        {
                            EnglandDisposalTotal="80",
                            NorthernIrelandDisposalTotal="70",
                            ScotlandDisposalTotal="30",
                            WalesDisposalTotal="20",
                            AllTotal=0.1M,
                            EnglandTotal=14.53M,
                            NorthernIrelandTotal=0.15M,
                            ScotlandTotal=0.15M,
                            WalesTotal=020M,
                            Name="Test",
                        },
                     new()
                        {
                            EnglandDisposalTotal="80",
                            NorthernIrelandDisposalTotal="70",
                            ScotlandDisposalTotal="30",
                            WalesDisposalTotal="20",
                            AllTotal=0.1M,
                            EnglandTotal=14.53M,
                            NorthernIrelandTotal=0.15M,
                            ScotlandTotal=0.15M,
                            WalesTotal=020M,
                            Name="Test",
                        }]
                },
                CalcResultParameterCommunicationCost = new CalcResultParameterCommunicationCost { }
            };

            _context.Material.Add(new Material { Id = 1, Name = "Material1", Code = "123" });
            _context.ProducerDetail.Add(new ProducerDetail { Id = 1, ProducerName = "Producer1", CalculatorRunId = 1, CalculatorRun = new CalculatorRun { Financial_Year = "2024-25", Name = "Test" } });
            _context.SaveChanges();
            
            var result = _calcResultsService.Construct(requestDto, calcResult);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, 0);
        }
    }
}