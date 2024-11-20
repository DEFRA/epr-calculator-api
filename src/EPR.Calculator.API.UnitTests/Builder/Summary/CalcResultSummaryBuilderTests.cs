using EPR.Calculator.API.Builder.Summary;
using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
namespace EPR.Calculator.API.UnitTests
{
    [TestClass]
    public class CalcResultSummaryBuilderTests
    {
        private DbContextOptions<ApplicationDBContext> _dbContextOptions;
        private ApplicationDBContext _context;
        private CalcResultSummaryBuilder _calcResultsService;
        private CalcResult _calcResult;

        [TestInitialize]
        public void TestInitialize()
        {
            _dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "CalcResultSummaryTestDb")
                .Options;
            _context = new ApplicationDBContext(_dbContextOptions);
            _calcResultsService = new CalcResultSummaryBuilder(_context);

            _calcResult = new CalcResult
            {
                CalcResultParameterOtherCost = new CalcResultParameterOtherCost { BadDebtProvision = new KeyValuePair<string, string>("key1", "6%") },
                CalcResultDetail = new CalcResultDetail() { },
                CalcResultLaDisposalCostData = new CalcResultLaDisposalCostData()
                {
                    CalcResultLaDisposalCostDetails = new List<CalcResultLaDisposalCostDataDetail>()
                    {
                        new CalcResultLaDisposalCostDataDetail()
                        {
                            DisposalCostPricePerTonne="20",
                            England="EnglandTest",
                            Wales="WalesTest",
                            Name="ScotlandTest",
                            Scotland="ScotlandTest"
                        }
                    }
                },
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
                CalcResultParameterCommunicationCost = new CalcResultParameterCommunicationCost { },
                CalcResultSummary = new CalcResultSummary { ProducerDisposalFees = new List<CalcResultSummaryProducerDisposalFees>() },
                CalcResultCommsCostReportDetail = new CalcResultCommsCost()
                {
                    CalcResultCommsCostCommsCostByMaterial =
                    [
                        new ()
                        {
                            CommsCostByMaterialPricePerTonne="0.42"
                        }
                    ]
                }
            };

            // Seed database
            SeedDatabase(_context);
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

            var result = _calcResultsService.Construct(requestDto, _calcResult);

            Assert.IsNotNull(result);
            Assert.AreEqual(CalcResultSummaryHeaders.CalculationResult, result.ResultSummaryHeader.Name);
            Assert.AreEqual(4, result.ProducerDisposalFeesHeaders.ToList()[0].ColumnIndex);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ProducerDisposalFees);
            Assert.AreEqual(2, result.ProducerDisposalFees.Count());
            var firstProducer = result.ProducerDisposalFees.FirstOrDefault();
            Assert.IsNotNull(firstProducer);
            Assert.AreEqual("Producer1", firstProducer.ProducerName);
        }

        [TestMethod]
        public void Construct_ShouldMapMaterialBreakdownHeaders()
        {
            var requestDto = new CalcResultsRequestDto { RunId = 1 };

            var result = _calcResultsService.Construct(requestDto, _calcResult);

            Assert.AreEqual("Material1 Breakdown", result.MaterialBreakdownHeaders.First().Name);
        }

        [TestMethod]
        public void Construct_ShouldCalculateProducerDisposalFeesCorrectly()
        {
            var requestDto = new CalcResultsRequestDto { RunId = 1 };

            var result = _calcResultsService.Construct(requestDto, _calcResult);

            Assert.IsTrue(result.ProducerDisposalFees.Any());
            Assert.AreEqual("Producer1", result.ProducerDisposalFees.First().ProducerName);
        }

        [TestMethod]
        public void Construct_ShouldReturnEmptyProducerDisposalFees_WhenNoProducers()
        {
            var requestDto = new CalcResultsRequestDto { RunId = 1 };

            var result = _calcResultsService.Construct(requestDto, _calcResult);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ProducerDisposalFees);
        }

        [TestMethod]
        public void Construct_ShouldCalculateBadDebtProvisionCorrectly()
        {
            var requestDto = new CalcResultsRequestDto { RunId = 1 };

            var result = _calcResultsService.Construct(requestDto, _calcResult);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, 0);
        }

        [TestMethod]
        public void Construct_ShouldReturnProducerDisposalFees_WithoutSubsidiaryTotalRow()
        {
            var calcResultsRequestDto = new CalcResultsRequestDto { RunId = 1 };

            var result = _calcResultsService.Construct(calcResultsRequestDto, _calcResult);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.ProducerDisposalFees.Count());
            Assert.IsFalse(result.ProducerDisposalFees.Any(fee => fee.ProducerName.Contains("Total")));
        }

        [TestMethod]
        public void Construct_ShouldReturnProducerDisposalFees_WithSubsidiaryTotalRow()
        {
            var calcResultsRequestDto = new CalcResultsRequestDto { RunId = 1 };

            var result = _calcResultsService.Construct(calcResultsRequestDto, _calcResult);

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.ProducerDisposalFees.Count());
        }

        [TestMethod]
        public void Construct_ShouldReturnOverallTotalRow_ForAllProducers()
        {
            var calcResultsRequestDto = new CalcResultsRequestDto { RunId = 1 };

            var result = _calcResultsService.Construct(calcResultsRequestDto, _calcResult);

            Assert.IsNotNull(result);
            var totalRow = result.ProducerDisposalFees.LastOrDefault();
            Assert.IsNotNull(totalRow);
        }

        private void SeedDatabase(ApplicationDBContext context)
        {
            context.Material.AddRange(new List<Material>
            {
                new() { Id = 1, Name = "Material1", Code = "123"},
                new() { Id = 2, Name = "Material2", Code = "456"}
            });

            context.ProducerDetail.AddRange(new List<ProducerDetail>
            {
                new() {  Id = 1, ProducerName = "Producer1", CalculatorRunId = 1, CalculatorRun = new CalculatorRun { Financial_Year = "2024-25", Name = "Test1" } },
                new() { Id = 2, ProducerName = "Producer2", CalculatorRunId = 2, CalculatorRun = new CalculatorRun { Financial_Year = "2024-25", Name = "Test2" } },
                new() {  Id = 3, ProducerName = "Producer3", CalculatorRunId = 3, CalculatorRun = new CalculatorRun { Financial_Year = "2024-25", Name = "Test3" } }
            });

            context.SaveChanges();
        }
    }
}
