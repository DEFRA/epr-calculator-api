namespace EPR.Calculator.API.UnitTests.DataModels
{
    using System.Collections.Generic;
    using AutoFixture;
    using EPR.Calculator.API.Data.DataModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class MaterialTests
    {
        public MaterialTests()
        {
            this.Fixture = new Fixture();
            this.TestClass = this.Fixture.Create<Material>();
        }

        private Material TestClass { get; }

        private IFixture Fixture { get; }

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
        public void CanSetAndGetCode()
        {
            // Arrange
            var testValue = this.Fixture.Create<string>();

            // Act
            this.TestClass.Code = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.Code);
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
        public void CanSetAndGetDescription()
        {
            // Arrange
            var testValue = this.Fixture.Create<string>();

            // Act
            this.TestClass.Description = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.Description);
        }

        [TestMethod]
        public void CanGetProducerReportedMaterials()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.ProducerReportedMaterials, typeof(ICollection<ProducerReportedMaterial>));
        }
    }
}