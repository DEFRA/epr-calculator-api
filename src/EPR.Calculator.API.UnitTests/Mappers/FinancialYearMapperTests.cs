namespace EPR.Calculator.API.UnitTests.Mappers
{
    using System;
    using AutoFixture;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.API.Dtos;
    using EPR.Calculator.API.Mappers;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class FinancialYearMapperTests
    {
        private IFixture Fixture { get; init; } = new Fixture();

        [TestMethod]
        public void CanCallMap()
        {
            // Arrange
            var financialYear = this.Fixture.Create<CalculatorRunFinancialYear>();

            // Act
            var result = FinancialYearMapper.Map(financialYear);

            // Assert
            Assert.IsInstanceOfType<FinancialYearDto>(result);
        }

        [TestMethod]
        public void MapPerformsMapping()
        {
            // Arrange
            var financialYear = this.Fixture.Create<CalculatorRunFinancialYear>();

            // Act
            var result = FinancialYearMapper.Map(financialYear);

            // Assert
            Assert.AreSame(financialYear.Name, result.Name);
            Assert.AreSame(financialYear.Description, result.Description);
        }
    }
}