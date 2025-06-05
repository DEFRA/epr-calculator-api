namespace EPR.Calculator.API.UnitTests.Models
{
    using System;
    using AutoFixture;
    using EPR.Calculator.API.Models;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class BillingFileGenerationMessageTests
    {
        private BillingFileGenerationMessage _testClass;

        [TestInitialize]
        public void SetUp()
        {
            _testClass = new BillingFileGenerationMessage();
        }

        [TestMethod]
        public void CanSetAndGetRunId()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<int>();

            // Act
            _testClass.RunId = testValue;

            // Assert
            _testClass.RunId.Should().Be(testValue);
        }

        [TestMethod]
        public void CanSetAndGetApprovedBy()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            _testClass.ApprovedBy = testValue;

            // Assert
            _testClass.ApprovedBy.Should().Be(testValue);
        }

        [TestMethod]
        public void CanSetAndGetMessageType()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            _testClass.MessageType = testValue;

            // Assert
            _testClass.MessageType.Should().Be(testValue);
        }
    }
}