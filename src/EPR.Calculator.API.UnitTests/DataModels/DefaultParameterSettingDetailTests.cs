namespace EPR.Calculator.API.UnitTests.DataModels
{
    using System;
    using AutoFixture;
    using EPR.Calculator.API.Data.DataModels;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class DefaultParameterSettingDetailTests
    {
        private DefaultParameterSettingDetail TestClass { get; }

        private IFixture Fixture { get; }

        public DefaultParameterSettingDetailTests()
        {
            Fixture = new Fixture();
            this.TestClass = Fixture.Create<DefaultParameterSettingDetail>();
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
        public void CanSetAndGetDefaultParameterSettingMasterId()
        {
            // Arrange
            var testValue = Fixture.Create<int>();

            // Act
            this.TestClass.DefaultParameterSettingMasterId = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.DefaultParameterSettingMasterId);
        }

        [TestMethod]
        public void CanSetAndGetDefaultParameterSettingMaster()
        {
            // Arrange
            var testValue = Fixture.Create<DefaultParameterSettingMaster>();

            // Act
            this.TestClass.DefaultParameterSettingMaster = testValue;

            // Assert
            Assert.AreSame(testValue, this.TestClass.DefaultParameterSettingMaster);
        }

        [TestMethod]
        public void CanSetAndGetParameterUniqueReferenceId()
        {
            // Arrange
            var testValue = Fixture.Create<string>();

            // Act
            this.TestClass.ParameterUniqueReferenceId = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.ParameterUniqueReferenceId);
        }

        [TestMethod]
        public void CanSetAndGetParameterUniqueReference()
        {
            // Arrange
            var testValue = Fixture.Create<DefaultParameterTemplateMaster>();

            // Act
            this.TestClass.ParameterUniqueReference = testValue;

            // Assert
            Assert.AreSame(testValue, this.TestClass.ParameterUniqueReference);
        }

        [TestMethod]
        public void CanSetAndGetParameterValue()
        {
            // Arrange
            var testValue = Fixture.Create<decimal>();

            // Act
            this.TestClass.ParameterValue = testValue;

            // Assert
            Assert.AreEqual(testValue, this.TestClass.ParameterValue);
        }
    }
}