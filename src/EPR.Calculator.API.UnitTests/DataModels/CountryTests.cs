namespace EPR.Calculator.API.UnitTests.DataModels
{
    using System.Collections.Generic;
    using AutoFixture;
    using EPR.Calculator.API.Data.DataModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CountryTests
    {
        public CountryTests()
        {
            this.Fixture = new Fixture();
            this.TestClass = this.Fixture.Create<Country>();
        }

        private Country TestClass { get; init; }

        private IFixture Fixture { get; init; }

        [TestMethod]
        public void CanSetAndGetId()
        {
            // Arrange
            var testValue = this.Fixture.Create<int>();

            // Act
            this.TestClass.Id = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.Id);
        }

        [TestMethod]
        public void CanSetAndGetCode()
        {
            // Arrange
            var testValue = this.Fixture.Create<string>();

            // Act
            this.TestClass.Code = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.Code);
        }

        [TestMethod]
        public void CanSetAndGetName()
        {
            // Arrange
            var testValue = this.Fixture.Create<string>();

            // Act
            this.TestClass.Name = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.Name);
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

        [TestMethod]
        public void CanGetCountryApportionments()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.CountryApportionments, typeof(ICollection<CountryApportionment>));
        }
    }
}