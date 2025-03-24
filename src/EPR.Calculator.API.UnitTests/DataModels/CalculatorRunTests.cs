namespace EPR.Calculator.API.UnitTests.DataModels
{
    using System;
    using System.Collections.Generic;
    using AutoFixture;
    using EPR.Calculator.API.Data.DataModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalculatorRunTests
    {
        private CalculatorRun TestClass { get; init; }

        private Fixture Fixture { get; } = new Fixture();

        public CalculatorRunTests()
        {
            TestClass = this.Fixture.Create<CalculatorRun>();
        }

        [TestMethod]
        public void CanSetAndGetCalculatorRunClassificationId()
        {
            // Arrange

            var testValue = Fixture.Create<int>();

            // Act
            TestClass.CalculatorRunClassificationId = testValue;

            // Assert
            Assert.AreEqual(testValue, TestClass.CalculatorRunClassificationId);
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
        public void CanSetAndGetName()
        {
            // Arrange
            var testValue = Fixture.Create<string>();

            // Act
            TestClass.Name = testValue;

            // Assert
            Assert.AreEqual(testValue, TestClass.Name);
        }

        [TestMethod]
        public void CanSetAndGetFinancial_Year()
        {
            // Arrange
            var testValue = Fixture.Create<CalculatorRunFinancialYear>();

            // Act
            TestClass.Financial_Year = testValue;

            // Assert
            Assert.AreEqual(testValue, TestClass.Financial_Year);
        }

        [TestMethod]
        public void CanSetAndGetCreatedBy()
        {
            // Arrange
            var testValue = Fixture.Create<string>();

            // Act
            TestClass.CreatedBy = testValue;

            // Assert
            Assert.AreEqual(testValue, TestClass.CreatedBy);
        }

        [TestMethod]
        public void CanSetAndGetCreatedAt()
        {
            // Arrange
            var testValue = Fixture.Create<DateTime>();

            // Act
            TestClass.CreatedAt = testValue;

            // Assert
            Assert.AreEqual(testValue, TestClass.CreatedAt);
        }

        [TestMethod]
        public void CanSetAndGetUpdatedBy()
        {
            // Arrange
            var testValue = Fixture.Create<string>();

            // Act
            TestClass.UpdatedBy = testValue;

            // Assert
            Assert.AreEqual(testValue, TestClass.UpdatedBy);
        }

        [TestMethod]
        public void CanSetAndGetUpdatedAt()
        {
            // Arrange
            var testValue = Fixture.Create<DateTime?>();

            // Act
            TestClass.UpdatedAt = testValue;

            // Assert
            Assert.AreEqual(testValue, TestClass.UpdatedAt);
        }

        [TestMethod]
        public void CanSetAndGetCalculatorRunPomDataMasterId()
        {
            // Arrange
            var testValue = Fixture.Create<int?>();

            // Act
            TestClass.CalculatorRunPomDataMasterId = testValue;

            // Assert
            Assert.AreEqual(testValue, TestClass.CalculatorRunPomDataMasterId);
        }

        [TestMethod]
        public void CanSetAndGetCalculatorRunOrganisationDataMasterId()
        {
            // Arrange
            var testValue = Fixture.Create<int?>();

            // Act
            TestClass.CalculatorRunOrganisationDataMasterId = testValue;

            // Assert
            Assert.AreEqual(testValue, TestClass.CalculatorRunOrganisationDataMasterId);
        }

        [TestMethod]
        public void CanSetAndGetLapcapDataMasterId()
        {
            // Arrange
            var testValue = Fixture.Create<int?>();

            // Act
            TestClass.LapcapDataMasterId = testValue;

            // Assert
            Assert.AreEqual(testValue, TestClass.LapcapDataMasterId);
        }

        [TestMethod]
        public void CanSetAndGetDefaultParameterSettingMasterId()
        {
            // Arrange
            var testValue = Fixture.Create<int?>();

            // Act
            TestClass.DefaultParameterSettingMasterId = testValue;

            // Assert
            Assert.AreEqual(testValue, TestClass.DefaultParameterSettingMasterId);
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

        [TestMethod]
        public void CanSetAndGetLapcapDataMaster()
        {
            // Arrange
            var testValue = Fixture.Create<LapcapDataMaster>();

            // Act
            TestClass.LapcapDataMaster = testValue;

            // Assert
            Assert.AreSame(testValue, TestClass.LapcapDataMaster);
        }

        [TestMethod]
        public void CanSetAndGetDefaultParameterSettingMaster()
        {
            // Arrange
            var testValue = Fixture.Create<DefaultParameterSettingMaster>();

            // Act
            TestClass.DefaultParameterSettingMaster = testValue;

            // Assert
            Assert.AreSame(testValue, TestClass.DefaultParameterSettingMaster);
        }

        [TestMethod]
        public void CanGetProducerDetails()
        {
            // Assert
            Assert.IsInstanceOfType(TestClass.ProducerDetails, typeof(ICollection<ProducerDetail>));
        }

        [TestMethod]
        public void CanGetCountryApportionments()
        {
            // Assert
            Assert.IsInstanceOfType(TestClass.CountryApportionments, typeof(ICollection<CountryApportionment>));
        }
    }
}