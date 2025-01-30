namespace EPR.Calculator.API.UnitTests.DataModels
{
    using AutoFixture;
    using EPR.Calculator.API.Data.DataModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CountryApportionmentTests
    {
        private CountryApportionment TestClass { get; }

        private IFixture Fixture { get; }

        public CountryApportionmentTests()
        {
            Fixture = new Fixture();
            Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => Fixture.Behaviors.Remove(b));
            Fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            this.TestClass = Fixture.Create<CountryApportionment>();
        }

        [TestMethod]
        public void CanSetAndGetId()
        {
            // Arrange
            var testValue = Fixture.Create<int>();

            // Act
            this.TestClass.Id = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.Id);
        }

        [TestMethod]
        public void CanSetAndGetApportionment()
        {
            // Arrange
            var testValue = Fixture.Create<decimal>();

            // Act
            this.TestClass.Apportionment = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.Apportionment);
        }

        [TestMethod]
        public void CanSetAndGetCountryId()
        {
            // Arrange
            var testValue = Fixture.Create<int>();

            // Act
            this.TestClass.CountryId = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.CountryId);
        }

        [TestMethod]
        public void CanSetAndGetCostTypeId()
        {
            // Arrange
            var testValue = Fixture.Create<int>();

            // Act
            this.TestClass.CostTypeId = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.CostTypeId);
        }

        [TestMethod]
        public void CanSetAndGetCalculatorRunId()
        {
            // Arrange
            var testValue = Fixture.Create<int>();

            // Act
            this.TestClass.CalculatorRunId = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.CalculatorRunId);
        }

        [TestMethod]
        public void CanSetAndGetCountry()
        {
            // Arrange
            var testValue = Fixture.Create<Country>();

            // Act
            this.TestClass.Country = testValue;

            // Assert
            Assert.AreSame(testValue, this.TestClass.Country);
        }

        [TestMethod]
        public void CanSetAndGetCostType()
        {
            // Arrange
            var testValue = Fixture.Create<CostType>();

            // Act
            this.TestClass.CostType = testValue;

            // Assert
            Assert.AreSame(testValue, this.TestClass.CostType);
        }

        [TestMethod]
        public void CanSetAndGetCalculatorRun()
        {
            // Arrange
            var testValue = Fixture.Create<CalculatorRun>();

            // Act
            this.TestClass.CalculatorRun = testValue;

            // Assert
            Assert.AreSame(testValue, this.TestClass.CalculatorRun);
        }
    }
}