namespace EPR.Calculator.API.UnitTests.DataModels
{
    using System;
    using AutoFixture;
    using EPR.Calculator.API.Data.DataModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalculatorRunOrganisationDataDetailTests
    {
        public CalculatorRunOrganisationDataDetailTests()
        {
            this.TestClass = this.Fixture.Create<CalculatorRunOrganisationDataDetail>();
        }

        private Fixture Fixture { get; } = new Fixture();

        private CalculatorRunOrganisationDataDetail TestClass { get; init; }

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
        public void CanSetAndGetOrganisationId()
        {
            // Arrange
            var testValue = this.Fixture.Create<int?>();

            // Act
            this.TestClass.OrganisationId = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.OrganisationId);
        }

        [TestMethod]
        public void CanSetAndGetSubsidaryId()
        {
            // Arrange
            var testValue = this.Fixture.Create<string>();

            // Act
            this.TestClass.SubsidaryId = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.SubsidaryId);
        }

        [TestMethod]
        public void CanSetAndGetOrganisationName()
        {
            // Arrange
            var testValue = this.Fixture.Create<string>();

            // Act
            this.TestClass.OrganisationName = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.OrganisationName);
        }

        [TestMethod]
        public void CanSetAndGetTradingName()
        {
            // Arrange
            var testValue = this.Fixture.Create<string>();

            // Act
            this.TestClass.TradingName = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.TradingName);
        }

        [TestMethod]
        public void CanSetAndGetSubmissionPeriodDesc()
        {
            // Arrange
            var testValue = this.Fixture.Create<string>();

            // Act
            this.TestClass.SubmissionPeriodDesc = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.SubmissionPeriodDesc);
        }

        [TestMethod]
        public void CanSetAndGetLoadTimeStamp()
        {
            // Arrange
            var testValue = this.Fixture.Create<DateTime>();

            // Act
            this.TestClass.LoadTimeStamp = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.LoadTimeStamp);
        }

        [TestMethod]
        public void CanSetAndGetCalculatorRunOrganisationDataMasterId()
        {
            // Arrange
            var testValue = this.Fixture.Create<int>();

            // Act
            this.TestClass.CalculatorRunOrganisationDataMasterId = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.CalculatorRunOrganisationDataMasterId);
        }

        [TestMethod]
        public void CanSetAndGetCalculatorRunOrganisationDataMaster()
        {
            // Arrange
            var testValue = this.Fixture.Create<CalculatorRunOrganisationDataMaster>();

            // Act
            this.TestClass.CalculatorRunOrganisationDataMaster = testValue;

            // Assert
            Assert.AreSame(testValue, this.TestClass.CalculatorRunOrganisationDataMaster);
        }
    }
}