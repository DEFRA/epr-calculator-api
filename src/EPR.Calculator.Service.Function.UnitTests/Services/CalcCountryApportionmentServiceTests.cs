using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;

namespace EPR.Calculator.Service.Function.UnitTests.Services
{
    [TestClass]
    public class CalcCountryApportionmentServiceTests : TestsFor<CalcCountryApportionmentService>
    {
        protected override void TestInitialize()
        {
            dbContext.Country.Add(new Country { Name = "England", Code = "Eng" });
            dbContext.Country.Add(new Country { Name = "Scotland", Code = "Sct" });
            dbContext.Country.Add(new Country { Name = "Northern Ireland", Code = "NI" });
            dbContext.Country.Add(new Country { Name = "Wales", Code = "Wales" });
        }

        [TestMethod]
        public async Task CanCallSaveChangesAsync()
        {
            // Arrange
            var fixture = new Fixture();
            var countryApportionmentServiceDto = fixture.Create<CalcCountryApportionmentServiceDto>();

            countryApportionmentServiceDto.Countries = dbContext.Country.ToList();

            countryApportionmentServiceDto.CostTypeId = 1;
            countryApportionmentServiceDto.EnglandCost = 1000;
            countryApportionmentServiceDto.WalesCost = 2000;
            countryApportionmentServiceDto.NorthernIrelandCost = 3000;
            countryApportionmentServiceDto.ScotlandCost = 4000;

            // Act
            await testSubject.SaveChangesAsync(countryApportionmentServiceDto);

            var apportionment = dbContext.CountryApportionment.ToList();
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
