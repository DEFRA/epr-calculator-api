namespace EPR.Calculator.API.UnitTests.DataModels
{
    using System;
    using System.Collections.Generic;
    using AutoFixture;
    using EPR.Calculator.API.Data.DataModels;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalculatorRunOrganisationDataMasterTests
    {
        private Fixture Fixture { get; } = new Fixture();

        private CalculatorRunOrganisationDataMaster TestClass { get; init; }

        public CalculatorRunOrganisationDataMasterTests()
        {
            TestClass = Fixture.Create<CalculatorRunOrganisationDataMaster>();
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
        public void CanSetAndGetCalendarYear()
        {
            // Arrange
            var testValue = Fixture.Create<string>();

            // Act
            TestClass.CalendarYear = testValue;

            // Assert
            Assert.AreEqual(testValue, TestClass.CalendarYear);
        }

        [TestMethod]
        public void CanSetAndGetEffectiveFrom()
        {
            // Arrange
            var testValue = Fixture.Create<DateTime>();

            // Act
            TestClass.EffectiveFrom = testValue;

            // Assert
            Assert.AreEqual(testValue, TestClass.EffectiveFrom);
        }

        [TestMethod]
        public void CanSetAndGetEffectiveTo()
        {
            // Arrange
            var testValue = Fixture.Create<DateTime?>();

            // Act
            TestClass.EffectiveTo = testValue;

            // Assert
            Assert.AreEqual(testValue, TestClass.EffectiveTo);
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
        public void CanGetDetails()
        {
            // Assert
            Assert.IsInstanceOfType(TestClass.Details, typeof(ICollection<CalculatorRunOrganisationDataDetail>));
        }

        [TestMethod]
        public void CanGetRunDetails()
        {
            // Assert
            Assert.IsNull(TestClass.RunDetails);
        }
    }
}