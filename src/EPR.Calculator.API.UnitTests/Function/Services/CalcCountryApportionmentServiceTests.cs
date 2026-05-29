using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    [TestClass]
    public class CalcCountryApportionmentServiceTests
    {
        private CalcCountryApportionmentService _testClass = null!;
        private ApplicationDBContext _context = null!;

        [TestInitialize]
        public void SetUp()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                           .UseInMemoryDatabase(databaseName: "PayCal")
                           .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                           .Options;

            _context = new ApplicationDBContext(dbContextOptions);

            _context.Database.EnsureCreated();

            if(!_context.Country.Any())
            {
                _context.Country.Add(new Country { Name = "England", Code = "Eng" });
                _context.Country.Add(new Country { Name = "Scotland", Code = "Sct" });
                _context.Country.Add(new Country { Name = "Northern Ireland", Code = "NI" });
                _context.Country.Add(new Country { Name = "Wales", Code = "Wales" });
                _context.SaveChanges();
            }
            _testClass = new CalcCountryApportionmentService(_context);
        }

        [TestCleanup]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new CalcCountryApportionmentService(_context);

            // Assert
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public async Task CanCallSaveChangesAsync()
        {
            // Arrange
            var fixture = new Fixture();
            var countryApportionmentServiceDto = fixture.Create<CalcCountryApportionmentServiceDto>();

            countryApportionmentServiceDto.Countries = _context.Country.ToList();

            countryApportionmentServiceDto.CostTypeId = 1;
            countryApportionmentServiceDto.EnglandCost = 1000;
            countryApportionmentServiceDto.WalesCost = 2000;
            countryApportionmentServiceDto.NorthernIrelandCost = 3000;
            countryApportionmentServiceDto.ScotlandCost = 4000;

            // Act
            await _testClass.SaveChangesAsync(countryApportionmentServiceDto);

            var apportionment = _context.CountryApportionment.ToList();
            Assert.AreEqual(4, apportionment.Count());

            Assert.IsTrue(apportionment.Exists(x => 
            x.CountryId == 1
            && 
            x.Apportionment == countryApportionmentServiceDto.EnglandCost
            &&
            x.CostTypeId == countryApportionmentServiceDto.CostTypeId
            &&
            x.CalculatorRunId == countryApportionmentServiceDto.RunId
            &&
            x.Apportionment == countryApportionmentServiceDto.EnglandCost
            ));
            Assert.IsTrue(
                apportionment.Exists(x => x.CountryId == 4
                &&
                x.CostTypeId == countryApportionmentServiceDto.CostTypeId
                &&
                x.CalculatorRunId == countryApportionmentServiceDto.RunId
                &&
                x.Apportionment == countryApportionmentServiceDto.WalesCost
            ));
            Assert.IsTrue(
                apportionment.Exists(x =>
                x.CountryId == 3
                &&
                x.CostTypeId == countryApportionmentServiceDto.CostTypeId
                &&
                x.CalculatorRunId == countryApportionmentServiceDto.RunId
                &&
                x.Apportionment == countryApportionmentServiceDto.NorthernIrelandCost));
            Assert.IsTrue(apportionment.Exists(x => x.CountryId == 2 && x.Apportionment == countryApportionmentServiceDto.ScotlandCost));
        }
    }
}