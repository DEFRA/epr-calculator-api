namespace EPR.Calculator.API.UnitTests.DataModels
{
    using System;
    using AutoFixture;
    using EPR.Calculator.API.Data.DataModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalculatorRunOrganisationDataDetailTests
    {
        private Fixture Fixture { get; } = new Fixture();

        private CalculatorRunOrganisationDataDetail TestClass { get; init; }

        public CalculatorRunOrganisationDataDetailTests()
        {
            TestClass = Fixture.Create<CalculatorRunOrganisationDataDetail>();
        }

        [TestMethod]
        public void CanSetAndGetId()
        {
            // Arrange
            var testValue = Fixture.Create<int>();

            // Act
            TestClass.Id = testValue;

            // Assert
            Assert.AreEqual(testValue, TestClass.Id);
        }

        [TestMethod]
        public void CanSetAndGetOrganisationId()
        {
            // Arrange
            var testValue = Fixture.Create<int?>();

            // Act
            TestClass.OrganisationId = testValue;

            // Assert
            Assert.AreEqual(testValue, TestClass.OrganisationId);
        }

        [TestMethod]
        public void CanSetAndGetSubsidaryId()
        {
            // Arrange
            var testValue = Fixture.Create<string>();

            // Act
            TestClass.SubsidaryId = testValue;

            // Assert
            Assert.AreEqual(testValue, TestClass.SubsidaryId);
        }

        [TestMethod]
        public void CanSetAndGetOrganisationName()
        {
            // Arrange
            var testValue = Fixture.Create<string>();

            // Act
            TestClass.OrganisationName = testValue;

            // Assert
            Assert.AreEqual(testValue, TestClass.OrganisationName);
        }

        [TestMethod]
        public void CanSetAndGetSubmissionPeriodDesc()
        {
            // Arrange
            var testValue = Fixture.Create<string>();

            // Act
            TestClass.SubmissionPeriodDesc = testValue;

            // Assert
            Assert.AreEqual(testValue, TestClass.SubmissionPeriodDesc);
        }

        [TestMethod]
        public void CanSetAndGetLoadTimeStamp()
        {
            // Arrange
            var testValue = Fixture.Create<DateTime>();

            // Act
            TestClass.LoadTimeStamp = testValue;

            // Assert
            Assert.AreEqual(testValue, TestClass.LoadTimeStamp);
        }

        [TestMethod]
        public void CanSetAndGetCalculatorRunOrganisationDataMasterId()
        {
            // Arrange
            var testValue = Fixture.Create<int>();

            // Act
            TestClass.CalculatorRunOrganisationDataMasterId = testValue;

            // Assert
            Assert.AreEqual(testValue, TestClass.CalculatorRunOrganisationDataMasterId);
        }

        [TestMethod]
        public void CanSetAndGetCalculatorRunOrganisationDataMaster()
        {
            // Arrange
            var testValue = Fixture.Create<CalculatorRunOrganisationDataMaster>();

            // Act
            TestClass.CalculatorRunOrganisationDataMaster = testValue;

            // Assert
            Assert.AreSame(testValue, TestClass.CalculatorRunOrganisationDataMaster);
        }
    }
}