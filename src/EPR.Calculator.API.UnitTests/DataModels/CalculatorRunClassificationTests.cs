namespace EPR.Calculator.API.UnitTests.DataModels
{
    using System;
    using System.Collections.Generic;
    using AutoFixture;
    using EPR.Calculator.API.Data.DataModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalculatorRunClassificationTests
    {
        public CalculatorRunClassificationTests()
        {
            this.TestClass = this.Fixture.Create<CalculatorRunClassification>();
        }

        private Fixture Fixture { get; } = new Fixture();

        private CalculatorRunClassification TestClass { get; init; }

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
        public void CanSetAndGetStatus()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            this.TestClass.Status = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.Status);
        }

        [TestMethod]
        public void CanSetAndGetCreatedBy()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            this.TestClass.CreatedBy = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.CreatedBy);
        }

        [TestMethod]
        public void CanGetCalculatorRunDetails()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.CalculatorRunDetails, typeof(ICollection<CalculatorRun>));
        }
    }
}