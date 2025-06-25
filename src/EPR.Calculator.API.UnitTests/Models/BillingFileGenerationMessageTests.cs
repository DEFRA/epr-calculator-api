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
        private readonly BillingFileGenerationMessage testClass;

        public BillingFileGenerationMessageTests()
        {
            this.testClass = new BillingFileGenerationMessage();
        }

        [TestMethod]
        public void CanSetAndGetRunId()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<int>();

            // Act
            this.testClass.CalculatorRunId = testValue;

            // Assert
            this.testClass.CalculatorRunId.Should().Be(testValue);
        }

        [TestMethod]
        public void CanSetAndGetApprovedBy()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            this.testClass.ApprovedBy = testValue;

            // Assert
            this.testClass.ApprovedBy.Should().Be(testValue);
        }

        [TestMethod]
        public void CanSetAndGetMessageType()
        {
            // Arrange
            var fixture = new Fixture();

            var testValue = fixture.Create<string>();

            // Act
            this.testClass.MessageType = testValue;

            // Assert
            this.testClass.MessageType.Should().Be(testValue);
        }
    }
}