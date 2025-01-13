namespace EPR.Calculator.API.UnitTests.DataModels
{
    using System;
    using AutoFixture;
    using EPR.Calculator.API.Data.DataModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class OrganisationDataTests
    {
        private OrganisationData TestClass { get; }

        private IFixture Fixture { get; }

        public OrganisationDataTests()
        {
            Fixture = new Fixture();
            this.TestClass = Fixture.Create<OrganisationData>();
        }

        [TestMethod]
        public void CanSetAndGetOrganisationId()
        {
            // Arrange
            var testValue = Fixture.Create<int?>();

            // Act
            this.TestClass.OrganisationId = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.OrganisationId);
        }

        [TestMethod]
        public void CanSetAndGetSubsidaryId()
        {
            // Arrange
            var testValue = Fixture.Create<string>();

            // Act
            this.TestClass.SubsidaryId = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.SubsidaryId);
        }

        [TestMethod]
        public void CanSetAndGetOrganisationName()
        {
            // Arrange
            var testValue = Fixture.Create<string>();

            // Act
            this.TestClass.OrganisationName = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.OrganisationName);
        }

        [TestMethod]
        public void CanSetAndGetSubmissionPeriodDesc()
        {
            // Arrange
            var testValue = Fixture.Create<string>();

            // Act
            this.TestClass.SubmissionPeriodDesc = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.SubmissionPeriodDesc);
        }

        [TestMethod]
        public void CanSetAndGetLoadTimestamp()
        {
            // Arrange
            var testValue = Fixture.Create<DateTime>();

            // Act
            this.TestClass.LoadTimestamp = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.LoadTimestamp);
        }
    }
}