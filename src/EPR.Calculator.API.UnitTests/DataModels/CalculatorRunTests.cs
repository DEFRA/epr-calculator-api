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
        public CalculatorRunTests()
        {
            this.TestClass = this.Fixture.Create<CalculatorRun>();
        }

        private CalculatorRun TestClass { get; init; }

        private Fixture Fixture { get; } = new Fixture();

        [TestMethod]
        public void CanSetAndGetCalculatorRunClassificationId()
        {
            // Arrange
            var testValue = this.Fixture.Create<int>();

            // Act
            this.TestClass.CalculatorRunClassificationId = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.CalculatorRunClassificationId);
        }

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
        public void CanSetAndGetName()
        {
            // Arrange
            var testValue = this.Fixture.Create<string>();

            // Act
            this.TestClass.Name = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.Name);
        }

        [TestMethod]
        public void CanSetAndGetFinancial_Year()
        {
            // Arrange
            var testValue = this.Fixture.Create<string>();

            // Act
            this.TestClass.Financial_Year = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.Financial_Year);
        }

        [TestMethod]
        public void CanSetAndGetCreatedBy()
        {
            // Arrange
            var testValue = this.Fixture.Create<string>();

            // Act
            this.TestClass.CreatedBy = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.CreatedBy);
        }

        [TestMethod]
        public void CanSetAndGetCreatedAt()
        {
            // Arrange
            var testValue = this.Fixture.Create<DateTime>();

            // Act
            this.TestClass.CreatedAt = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.CreatedAt);
        }

        [TestMethod]
        public void CanSetAndGetUpdatedBy()
        {
            // Arrange
            var testValue = this.Fixture.Create<string>();

            // Act
            this.TestClass.UpdatedBy = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.UpdatedBy);
        }

        [TestMethod]
        public void CanSetAndGetUpdatedAt()
        {
            // Arrange
            var testValue = this.Fixture.Create<DateTime?>();

            // Act
            this.TestClass.UpdatedAt = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.UpdatedAt);
        }

        [TestMethod]
        public void CanSetAndGetCalculatorRunPomDataMasterId()
        {
            // Arrange
            var testValue = this.Fixture.Create<int?>();

            // Act
            this.TestClass.CalculatorRunPomDataMasterId = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.CalculatorRunPomDataMasterId);
        }

        [TestMethod]
        public void CanSetAndGetCalculatorRunOrganisationDataMasterId()
        {
            // Arrange
            var testValue = this.Fixture.Create<int?>();

            // Act
            this.TestClass.CalculatorRunOrganisationDataMasterId = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.CalculatorRunOrganisationDataMasterId);
        }

        [TestMethod]
        public void CanSetAndGetLapcapDataMasterId()
        {
            // Arrange
            var testValue = this.Fixture.Create<int?>();

            // Act
            this.TestClass.LapcapDataMasterId = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.LapcapDataMasterId);
        }

        [TestMethod]
        public void CanSetAndGetDefaultParameterSettingMasterId()
        {
            // Arrange
            var testValue = this.Fixture.Create<int?>();

            // Act
            this.TestClass.DefaultParameterSettingMasterId = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.DefaultParameterSettingMasterId);
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

        [TestMethod]
        public void CanSetAndGetLapcapDataMaster()
        {
            // Arrange
            var testValue = this.Fixture.Create<LapcapDataMaster>();

            // Act
            this.TestClass.LapcapDataMaster = testValue;

            // Assert
            Assert.AreSame(testValue, this.TestClass.LapcapDataMaster);
        }

        [TestMethod]
        public void CanSetAndGetDefaultParameterSettingMaster()
        {
            // Arrange
            var testValue = this.Fixture.Create<DefaultParameterSettingMaster>();

            // Act
            this.TestClass.DefaultParameterSettingMaster = testValue;

            // Assert
            Assert.AreSame(testValue, this.TestClass.DefaultParameterSettingMaster);
        }

        [TestMethod]
        public void CanGetProducerDetails()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.ProducerDetails, typeof(ICollection<ProducerDetail>));
        }

        [TestMethod]
        public void CanGetCountryApportionments()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.CountryApportionments, typeof(ICollection<CountryApportionment>));
        }
    }
}