namespace EPR.Calculator.API.UnitTests.DataModels
{
    using System;
    using System.Collections.Generic;
    using AutoFixture;
    using EPR.Calculator.API.Data.DataModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class LapcapDataTemplateMasterTests
    {
        private LapcapDataTemplateMaster TestClass { get; }

        private IFixture Fixture { get; }

        public LapcapDataTemplateMasterTests()
        {
            Fixture = new Fixture();
            this.TestClass = Fixture.Create<LapcapDataTemplateMaster>();
        }

        [TestMethod]
        public void CanSetAndGetUniqueReference()
        {
            // Arrange
            var testValue = Fixture.Create<string>();

            // Act
            this.TestClass.UniqueReference = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.UniqueReference);
        }

        [TestMethod]
        public void CanSetAndGetCountry()
        {
            // Arrange
            var testValue = Fixture.Create<string>();

            // Act
            this.TestClass.Country = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.Country);
        }

        [TestMethod]
        public void CanSetAndGetMaterial()
        {
            // Arrange
            var testValue = Fixture.Create<string>();

            // Act
            this.TestClass.Material = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.Material);
        }

        [TestMethod]
        public void CanSetAndGetTotalCostFrom()
        {
            // Arrange
            var testValue = Fixture.Create<decimal>();

            // Act
            this.TestClass.TotalCostFrom = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.TotalCostFrom);
        }

        [TestMethod]
        public void CanSetAndGetTotalCostTo()
        {
            // Arrange
            var testValue = Fixture.Create<decimal>();

            // Act
            this.TestClass.TotalCostTo = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.TotalCostTo);
        }

        [TestMethod]
        public void CanGetDetails()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.Details, typeof(ICollection<LapcapDataDetail>));
        }
    }
}