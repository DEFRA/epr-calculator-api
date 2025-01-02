namespace EPR.Calculator.API.UnitTests.Builder
{
    using System;
    using EPR.Calculator.API.Builder.Detail;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.API.Dtos;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalcResultDetailBuilderTests
    {
        private readonly ApplicationDBContext _context;
        private readonly CalcResultDetailBuilder _builder;

        public CalcResultDetailBuilderTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDBContext(options);
            _builder = new CalcResultDetailBuilder(_context);
            SeedDatabase();
        }

        [TestCleanup]
        public void TearDown()
        {
            _context.Dispose();
        }

        private void SeedDatabase()
        {
            var calculatorRun = new CalculatorRun
            {
                Id = 1,
                Name = "TestRun",
                CreatedBy = "TestUser",
                CreatedAt = new DateTime(2023, 1, 1),
                Financial_Year = "2023",
                CalculatorRunOrganisationDataMaster = new CalculatorRunOrganisationDataMaster { CreatedBy = "", CalendarYear = "2024", EffectiveFrom = new DateTime(2023, 1, 1), CreatedAt = new DateTime(2023, 1, 1) },
                CalculatorRunPomDataMaster = new CalculatorRunPomDataMaster { CreatedBy = "", CalendarYear = "2024", EffectiveFrom = new DateTime(2023, 1, 1), CreatedAt = new DateTime(2023, 1, 1) },
                LapcapDataMaster = new LapcapDataMaster
                {
                    LapcapFileName = "LapcapFile.csv",
                    CreatedAt = new DateTime(2023, 1, 1),
                    CreatedBy = "TestUser",
                    ProjectionYear = "2024-25"
                },
                DefaultParameterSettingMaster = new DefaultParameterSettingMaster
                {
                    ParameterFileName = "Parameters.csv",
                    CreatedAt = new DateTime(2023, 1, 1),
                    CreatedBy = "TestUser",
                },
            };

            _context.CalculatorRuns.Add(calculatorRun);
            _context.SaveChanges();
        }

        [TestMethod]
        public void Construct_AllPropertiesPresent_ReturnsCorrectData()
        {
            var result = _builder.Construct(new CalcResultsRequestDto() { RunId = 1 });

            Assert.AreEqual(1, result.RunId);
            Assert.AreEqual("TestRun", result.RunName);
            Assert.AreEqual("TestUser", result.RunBy);
            Assert.AreEqual(new DateTime(2023, 1, 1), result.RunDate);
            Assert.AreEqual("2023", result.FinancialYear);
            Assert.AreEqual("01/01/2023 00:00", result.RpdFileORG);
            Assert.AreEqual("01/01/2023 00:00", result.RpdFilePOM);
            Assert.AreEqual("LapcapFile.csv,01/01/2023 00:00,TestUser", result.LapcapFile);
            Assert.AreEqual("Parameters.csv,01/01/2023 00:00,TestUser", result.ParametersFile);
        }

        [TestMethod]
        public void Construct_MissingOptionalProperties_ReturnsPartialData()
        {
            _context.CalculatorRuns.RemoveRange(_context.CalculatorRuns);
            _context.SaveChangesAsync();

            var calculatorRun = new CalculatorRun
            {
                Id = 2,
                Name = "RunWithMissingProps",
                CreatedBy = "TestUser2",
                CreatedAt = new DateTime(2023, 2, 1),
                Financial_Year = "2023"
            };

            _context.CalculatorRuns.Add(calculatorRun);
            _context.SaveChangesAsync();

            var result = _builder.Construct(new CalcResultsRequestDto() { RunId = 2 });

            Assert.AreEqual(2, result.RunId);
            Assert.AreEqual("RunWithMissingProps", result.RunName);
            Assert.AreEqual("TestUser2", result.RunBy);
            Assert.AreEqual(new DateTime(2023, 2, 1), result.RunDate);
            Assert.AreEqual("2023", result.FinancialYear);
            Assert.IsTrue(string.IsNullOrEmpty(result.RpdFileORG));
            Assert.IsTrue(string.IsNullOrEmpty(result.RpdFilePOM));
            Assert.IsTrue(string.IsNullOrEmpty(result.LapcapFile));
            Assert.IsTrue(string.IsNullOrEmpty(result.ParametersFile));
        }
    }
}