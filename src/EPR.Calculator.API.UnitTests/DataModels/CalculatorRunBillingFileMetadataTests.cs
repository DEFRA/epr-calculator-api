namespace EPR.Calculator.API.UnitTests.DataModels
{
    using System;
    using AutoFixture;
    using EPR.Calculator.API.Data.DataModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalculatorRunBillingFileMetadataTests
    {
        public CalculatorRunBillingFileMetadataTests()
        {
            this.Fixture = new Fixture();
            this.TestClass = this.Fixture.Create<CalculatorRunBillingFileMetadata>();
        }

        private CalculatorRunBillingFileMetadata TestClass { get; }

        private IFixture Fixture { get; }

        [TestMethod]
        public void CanSetAndGetId()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<int>();

            // Act
            this.TestClass.Id = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.Id);
        }

        [TestMethod]
        public void CanSetAndGetBillingCsvFileName()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            this.TestClass.BillingCsvFileName = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.BillingCsvFileName);
        }

        [TestMethod]
        public void CanSetAndGetBillingJsonFileName()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            this.TestClass.BillingJsonFileName = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.BillingJsonFileName);
        }

        [TestMethod]
        public void CanSetAndGetBillingFileCreatedDate()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<DateTime>();

            // Act
            this.TestClass.BillingFileCreatedDate = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.BillingFileCreatedDate);
        }

        [TestMethod]
        public void CanSetAndGetBillingFileCreatedBy()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            this.TestClass.BillingFileCreatedBy = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.BillingFileCreatedBy);
        }

        [TestMethod]
        public void CanSetAndGetBillingFileAuthorisedDate()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<DateTime?>();

            // Act
            this.TestClass.BillingFileAuthorisedDate = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.BillingFileAuthorisedDate);
        }

        [TestMethod]
        public void CanSetAndGetBillingFileAuthorisedBy()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            this.TestClass.BillingFileAuthorisedBy = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.BillingFileAuthorisedBy);
        }

        [TestMethod]
        public void CanSetAndGetCalculatorRunId()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<int>();

            // Act
            this.TestClass.CalculatorRunId = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.CalculatorRunId);
        }

        [TestMethod]
        public void CanSetAndGetCalculatorRun()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<CalculatorRun>();

            // Act
            this.TestClass.CalculatorRun = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.CalculatorRun);
        }
    }
}