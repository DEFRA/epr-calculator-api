namespace EPR.Calculator.API.UnitTests.DataModels
{
    using System;
    using AutoFixture;
    using EPR.Calculator.API.Data.DataModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class OrganisationDataTests
    {
        public OrganisationDataTests()
        {
            this.Fixture = new Fixture();
            this.TestClass = this.Fixture.Create<OrganisationData>();
        }

        private OrganisationData TestClass { get; }

        private IFixture Fixture { get; }

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
        public void CanSetAndGetLoadTimestamp()
        {
            // Arrange
            var testValue = this.Fixture.Create<DateTime>();

            // Act
            this.TestClass.LoadTimestamp = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.LoadTimestamp);
        }

        [TestMethod]
        public void CanSetAndGetLeaverCode()
        {
            // Arrange
            var testValue = this.Fixture.Create<string>();

            // Act
            this.TestClass.LeaverCode = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.LeaverCode);
        }

        [TestMethod]
        public void CanSetAndGetLeaverDate()
        {
            // Arrange
            var testValue = this.Fixture.Create<string>();

            // Act
            this.TestClass.LeaverDate = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.LeaverDate);
        }

        [TestMethod]
        public void CanSetAndGetJoinerDate()
        {
            // Arrange
            var testValue = this.Fixture.Create<string>();

            // Act
            this.TestClass.JoinerDate = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.JoinerDate);
        }

        [TestMethod]
        public void CanSetAndGetSubmittedOrgId()
        {
            // Arrange
            var testValue = this.Fixture.Create<string>();

            // Act
            this.TestClass.SubmitterOrgId = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.SubmitterOrgId);
        }
    }
}