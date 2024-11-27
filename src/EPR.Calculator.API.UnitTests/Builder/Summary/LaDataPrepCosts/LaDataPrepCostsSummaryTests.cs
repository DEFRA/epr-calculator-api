namespace EPR.Calculator.API.UnitTests.Builder.Summary.LaDataPrepCosts
{
    using System;
    using AutoFixture;
    using EPR.Calculator.API.Builder.Summary.LaDataPrepCosts;
    using EPR.Calculator.API.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class LaDataPrepCostsSummaryTests
    {
        [TestMethod]
        public void CanCallGetHeaders()
        {
            // Act
            var result = LaDataPrepCostsSummary.GetHeaders();

            // Assert
            Assert.Fail("Create or modify test");
        }

        [TestMethod]
        public void CanCallGetLaDataPrepCostsWithoutBadDebtProvision()
        {
            // Arrange
            var fixture = new Fixture();
            var calcResult = fixture.Create<CalcResult>();

            // Act
            var result = LaDataPrepCostsSummary.GetLaDataPrepCostsWithoutBadDebtProvision(calcResult);

            // Assert
            Assert.Fail("Create or modify test");
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsWithoutBadDebtProvisionWithNullCalcResult()
        {
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsSummary.GetLaDataPrepCostsWithoutBadDebtProvision(default(CalcResult)));
        }

        [TestMethod]
        public void CanCallGetBadDebtProvision()
        {
            // Arrange
            var fixture = new Fixture();
            var calcResult = fixture.Create<CalcResult>();

            // Act
            var result = LaDataPrepCostsSummary.GetBadDebtProvision(calcResult);

            // Assert
            Assert.Fail("Create or modify test");
        }

        [TestMethod]
        public void CannotCallGetBadDebtProvisionWithNullCalcResult()
        {
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsSummary.GetBadDebtProvision(default(CalcResult)));
        }

        [TestMethod]
        public void CanCallGetLaDataPrepCostsWithBadDebtProvision()
        {
            // Arrange
            var fixture = new Fixture();
            var calcResult = fixture.Create<CalcResult>();

            // Act
            var result = LaDataPrepCostsSummary.GetLaDataPrepCostsWithBadDebtProvision(calcResult);

            // Assert
            Assert.Fail("Create or modify test");
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsWithBadDebtProvisionWithNullCalcResult()
        {
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsSummary.GetLaDataPrepCostsWithBadDebtProvision(default(CalcResult)));
        }
    }
}