namespace EPR.Calculator.API.UnitTests.DataModels
{
    using System;
    using System.Collections.Generic;
    using AutoFixture;
    using EPR.Calculator.API.Data.DataModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class LapcapDataMasterTests
    {
        private LapcapDataMaster TestClass { get; }

        private IFixture Fixture { get; }

        public LapcapDataMasterTests()
        {
            Fixture = new Fixture();
            this.TestClass = Fixture.Create<LapcapDataMaster>();
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
        public void CanSetAndGetProjectionYear()
        {
            // Arrange
            var testValue = Fixture.Create<string>();

            // Act
            this.TestClass.ProjectionYear = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.ProjectionYear);
        }

        [TestMethod]
        public void CanSetAndGetEffectiveFrom()
        {
            // Arrange
            var testValue = Fixture.Create<DateTime>();

            // Act
            this.TestClass.EffectiveFrom = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.EffectiveFrom);
        }

        [TestMethod]
        public void CanSetAndGetEffectiveTo()
        {
            // Arrange
            var testValue = Fixture.Create<DateTime?>();

            // Act
            this.TestClass.EffectiveTo = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.EffectiveTo);
        }

        [TestMethod]
        public void CanSetAndGetCreatedBy()
        {
            // Arrange
            var testValue = Fixture.Create<string>();

            // Act
            this.TestClass.CreatedBy = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.CreatedBy);
        }

        [TestMethod]
        public void CanSetAndGetCreatedAt()
        {
            // Arrange
            var testValue = Fixture.Create<DateTime>();

            // Act
            this.TestClass.CreatedAt = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.CreatedAt);
        }

        [TestMethod]
        public void CanSetAndGetLapcapFileName()
        {
            // Arrange
            var testValue = Fixture.Create<string>();

            // Act
            this.TestClass.LapcapFileName = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.LapcapFileName);
        }

        [TestMethod]
        public void CanGetDetails()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.Details, typeof(ICollection<LapcapDataDetail>));
        }

        [TestMethod]
        public void CanGetRunDetails()
        {
            // Assert
            Assert.IsNull(TestClass.RunDetails);
        }
    }
}