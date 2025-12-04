namespace EPR.Calculator.API.UnitTests.DataModels
{
    using System;
    using AutoFixture;
    using EPR.Calculator.API.Data.DataModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalculatorRunPomDataDetailTests
    {
        public CalculatorRunPomDataDetailTests()
        {
            this.TestClass = this.Fixture.Create<CalculatorRunPomDataDetail>();
        }

        private Fixture Fixture { get; } = new Fixture();

        private CalculatorRunPomDataDetail TestClass { get; init; }

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
        public void CanSetAndGetSubsidiaryId()
        {
            // Arrange
            var testValue = this.Fixture.Create<string>();

            // Act
            this.TestClass.SubsidiaryId = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.SubsidiaryId);
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

        [TestMethod]
        public void CanSetAndGetCalculatorRunPomDataMasterId()
        {
            // Arrange
            var testValue = this.Fixture.Create<int>();

            // Act
            this.TestClass.CalculatorRunPomDataMasterId = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.CalculatorRunPomDataMasterId);
        }

        [TestMethod]
        public void CanSetAndGetCalculatorRunPomDataMaster()
        {
            // Arrange
            var testValue = this.Fixture.Create<CalculatorRunPomDataMaster>();

            // Act
            this.TestClass.CalculatorRunPomDataMaster = testValue;

            // Assert
            Assert.AreSame(testValue, this.TestClass.CalculatorRunPomDataMaster);
        }
    }
}