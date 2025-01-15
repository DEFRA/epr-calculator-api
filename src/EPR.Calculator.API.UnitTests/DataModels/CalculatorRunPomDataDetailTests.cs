namespace EPR.Calculator.API.UnitTests.DataModels
{
    using System;
    using AutoFixture;
    using EPR.Calculator.API.Data.DataModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalculatorRunPomDataDetailTests
    {
        private Fixture Fixture { get; } = new Fixture();

        private CalculatorRunPomDataDetail TestClass { get; init; }

        public CalculatorRunPomDataDetailTests()
        {
            TestClass = Fixture.Create<CalculatorRunPomDataDetail>();
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
        public void CanSetAndGetSubmissionPeriod()
        {
            // Arrange
            var testValue = Fixture.Create<string>();

            // Act
            TestClass.SubmissionPeriod = testValue;

            // Assert
            Assert.AreEqual(testValue, TestClass.SubmissionPeriod);
        }

        [TestMethod]
        public void CanSetAndGetPackagingActivity()
        {
            // Arrange
            var testValue = Fixture.Create<string>();

            // Act
            TestClass.PackagingActivity = testValue;

            // Assert
            Assert.AreEqual(testValue, TestClass.PackagingActivity);
        }

        [TestMethod]
        public void CanSetAndGetPackagingType()
        {
            // Arrange
            var testValue = Fixture.Create<string>();

            // Act
            TestClass.PackagingType = testValue;

            // Assert
            Assert.AreEqual(testValue, TestClass.PackagingType);
        }

        [TestMethod]
        public void CanSetAndGetPackagingClass()
        {
            // Arrange
            var testValue = Fixture.Create<string>();

            // Act
            TestClass.PackagingClass = testValue;

            // Assert
            Assert.AreEqual(testValue, TestClass.PackagingClass);
        }

        [TestMethod]
        public void CanSetAndGetPackagingMaterial()
        {
            // Arrange
            var testValue = Fixture.Create<string>();

            // Act
            TestClass.PackagingMaterial = testValue;

            // Assert
            Assert.AreEqual(testValue, TestClass.PackagingMaterial);
        }

        [TestMethod]
        public void CanSetAndGetPackagingMaterialWeight()
        {
            // Arrange
            var testValue = Fixture.Create<double?>();

            // Act
            TestClass.PackagingMaterialWeight = testValue;

            // Assert
            Assert.AreEqual(testValue, TestClass.PackagingMaterialWeight);
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
        public void CanSetAndGetCalculatorRunPomDataMasterId()
        {
            // Arrange
            var testValue = Fixture.Create<int>();

            // Act
            TestClass.CalculatorRunPomDataMasterId = testValue;

            // Assert
            Assert.AreEqual(testValue, TestClass.CalculatorRunPomDataMasterId);
        }

        [TestMethod]
        public void CanSetAndGetCalculatorRunPomDataMaster()
        {
            // Arrange
            var testValue = Fixture.Create<CalculatorRunPomDataMaster>();

            // Act
            TestClass.CalculatorRunPomDataMaster = testValue;

            // Assert
            Assert.AreSame(testValue, TestClass.CalculatorRunPomDataMaster);
        }
    }
}