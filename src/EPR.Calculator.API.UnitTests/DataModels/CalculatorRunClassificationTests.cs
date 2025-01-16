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
        private Fixture Fixture { get; } = new Fixture();

        private CalculatorRunClassification TestClass { get; init; }

        public CalculatorRunClassificationTests()
        {
            TestClass = Fixture.Create<CalculatorRunClassification>();
        }

        [TestMethod]
        public void CanSetAndGetId()
        {
            // Arrange
            var testValue = this.Fixture.Create<int>();

            // Act
            TestClass.Id = testValue;

            // Assert
            Assert.AreEqual(testValue, TestClass.Id);
        }

        [TestMethod]
        public void CanSetAndGetStatus()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            TestClass.Status = testValue;

            // Assert
            Assert.AreEqual(testValue, TestClass.Status);
        }

        [TestMethod]
        public void CanSetAndGetCreatedBy()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            TestClass.CreatedBy = testValue;

            // Assert
            Assert.AreEqual(testValue, TestClass.CreatedBy);
        }

        [TestMethod]
        public void CanGetCalculatorRunDetails()
        {
            // Assert
            Assert.IsInstanceOfType(TestClass.CalculatorRunDetails, typeof(ICollection<CalculatorRun>));
        }
    }
}