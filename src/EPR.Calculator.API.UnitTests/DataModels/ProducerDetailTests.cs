namespace EPR.Calculator.API.UnitTests.DataModels
{
    using System.Collections.Generic;
    using AutoFixture;
    using EPR.Calculator.API.Data.DataModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class ProducerDetailTests
    {
        private ProducerDetail TestClass { get; }

        private IFixture Fixture { get; }

        public ProducerDetailTests()
        {
            Fixture = new Fixture();
            this.TestClass = Fixture.Create<ProducerDetail>();
        }

        [TestMethod]
        public void CanSetAndGetId()
        {
            // Arrange
            var testValue = Fixture.Create<int>();

            // Act
            this.TestClass.Id = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.Id);
        }

        [TestMethod]
        public void CanSetAndGetProducerId()
        {
            // Arrange
            var testValue = Fixture.Create<int>();

            // Act
            this.TestClass.ProducerId = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.ProducerId);
        }

        [TestMethod]
        public void CanSetAndGetSubsidiaryId()
        {
            // Arrange
            var testValue = Fixture.Create<string>();

            // Act
            this.TestClass.SubsidiaryId = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.SubsidiaryId);
        }

        [TestMethod]
        public void CanSetAndGetProducerName()
        {
            // Arrange
            var testValue = Fixture.Create<string>();

            // Act
            this.TestClass.ProducerName = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.ProducerName);
        }

        [TestMethod]
        public void CanSetAndGetCalculatorRunId()
        {
            // Arrange
            var testValue = Fixture.Create<int>();

            // Act
            this.TestClass.CalculatorRunId = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.CalculatorRunId);
        }

        [TestMethod]
        public void CanGetProducerReportedMaterials()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.ProducerReportedMaterials, typeof(ICollection<ProducerReportedMaterial>));
        }

        [TestMethod]
        public void CanSetAndGetCalculatorRun()
        {
            // Arrange
            var testValue = Fixture.Create<CalculatorRun>();

            // Act
            this.TestClass.CalculatorRun = testValue;

            // Assert
            Assert.AreSame(testValue, this.TestClass.CalculatorRun);
        }
    }
}