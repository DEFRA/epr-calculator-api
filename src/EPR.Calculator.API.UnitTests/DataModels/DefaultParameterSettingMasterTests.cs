namespace EPR.Calculator.API.UnitTests.DataModels
{
    using System;
    using System.Collections.Generic;
    using AutoFixture;
    using EPR.Calculator.API.Data.DataModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DefaultParameterSettingMasterTests
    {
        public DefaultParameterSettingMasterTests()
        {
           this.Fixture = new Fixture();
           this.TestClass = new DefaultParameterSettingMaster();
        }

        private DefaultParameterSettingMaster TestClass { get; }

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
        public void CanSetAndGetParameterYear()
        {
            // Arrange
            var testValue = this.Fixture.Create<string>();

            // Act
            this.TestClass.ParameterYear = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.ParameterYear);
        }

        [TestMethod]
        public void CanSetAndGetEffectiveFrom()
        {
            // Arrange
            var testValue = this.Fixture.Create<DateTime>();

            // Act
            this.TestClass.EffectiveFrom = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.EffectiveFrom);
        }

        [TestMethod]
        public void CanSetAndGetEffectiveTo()
        {
            // Arrange
            var testValue = this.Fixture.Create<DateTime?>();

            // Act
            this.TestClass.EffectiveTo = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.EffectiveTo);
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
        public void CanSetAndGetParameterFileName()
        {
            // Arrange
            var testValue = this.Fixture.Create<string>();

            // Act
            this.TestClass.ParameterFileName = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.ParameterFileName);
        }

        [TestMethod]
        public void CanGetDetails()
        {
            // Assert
            Assert.IsInstanceOfType(this.TestClass.Details, typeof(ICollection<DefaultParameterSettingDetail>));
        }

        [TestMethod]
        public void CanGetRunDetails()
        {
            // Assert
            Assert.IsNull(this.TestClass.RunDetails);
        }
    }
}