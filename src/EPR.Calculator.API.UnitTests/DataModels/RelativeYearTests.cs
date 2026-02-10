namespace EPR.Calculator.API.UnitTests.DataModels
{
    using AutoFixture;
    using EPR.Calculator.API.Data.DataModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class RelativeYearTests
    {
        public RelativeYearTests()
        {
            this.Fixture = new Fixture();
            this.TestClass = this.Fixture.Create<CalculatorRunRelativeYear>();
        }

        private CalculatorRunRelativeYear TestClass { get; init; }

        private IFixture Fixture { get; init; }

        [TestMethod]
        public void CanCallToString()
        {
            // Act
            var result = this.TestClass.ToString();

            // Assert
            Assert.AreSame(result, this.TestClass.Value.ToString());
        }

        [TestMethod]
        public void CanSetAndGetName()
        {
            // Arrange
            var testValue = this.Fixture.Create<int>();

            // Act
            this.TestClass.Value = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.Value);
        }

        [TestMethod]
        public void CanSetAndGetDescription()
        {
            // Arrange
            var testValue = this.Fixture.Create<string>();

            // Act
            this.TestClass.Description = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.Description);
        }
    }
}