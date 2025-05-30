namespace EPR.Calculator.API.UnitTests.DataModels
{
    using System;
    using AutoFixture;
    using EPR.Calculator.API.Data.DataModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class PomDataTests
    {
        public PomDataTests()
        {
            this.Fixture = new Fixture();
            this.TestClass = this.Fixture.Create<PomData>();
        }

        private PomData TestClass { get; }

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
        public void CanSetAndGetSubmissionPeriod()
        {
            // Arrange
            var testValue = this.Fixture.Create<string>();

            // Act
            this.TestClass.SubmissionPeriod = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.SubmissionPeriod);
        }

        [TestMethod]
        public void CanSetAndGetPackagingActivity()
        {
            // Arrange
            var testValue = this.Fixture.Create<string>();

            // Act
            this.TestClass.PackagingActivity = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.PackagingActivity);
        }

        [TestMethod]
        public void CanSetAndGetPackagingType()
        {
            // Arrange
            var testValue = this.Fixture.Create<string>();

            // Act
            this.TestClass.PackagingType = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.PackagingType);
        }

        [TestMethod]
        public void CanSetAndGetPackagingClass()
        {
            // Arrange
            var testValue = this.Fixture.Create<string>();

            // Act
            this.TestClass.PackagingClass = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.PackagingClass);
        }

        [TestMethod]
        public void CanSetAndGetPackagingMaterial()
        {
            // Arrange
            var testValue = this.Fixture.Create<string>();

            // Act
            this.TestClass.PackagingMaterial = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.PackagingMaterial);
        }

        [TestMethod]
        public void CanSetAndGetPackagingMaterialWeight()
        {
            // Arrange
            var testValue = this.Fixture.Create<double?>();

            // Act
            this.TestClass.PackagingMaterialWeight = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.PackagingMaterialWeight);
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
    }
}