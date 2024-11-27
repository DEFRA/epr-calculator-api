namespace EPR.Calculator.API.UnitTests.Builder.Summary.LaDataPrepCosts
{
    using System;
    using System.Collections.Generic;
    using AutoFixture;
    using EPR.Calculator.API.Builder.Summary.LaDataPrepCosts;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.API.Models;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class LaDataPrepCostsProducerTests
    {
        [TestMethod]
        public void CanCallGetHeaders()
        {
            // Act
            var result = LaDataPrepCostsProducer.GetHeaders();

            // Assert
            Assert.Fail("Create or modify test");
        }

        [TestMethod]
        public void CanCallGetLaDataPrepCostsProducerFeeWithoutBadDebtProvision()
        {
            // Arrange
            var fixture = new Fixture();
            var producers = new Mock<IEnumerable<ProducerDetail>>().Object;
            var materials = new Mock<IEnumerable<MaterialDetail>>().Object;
            var calcResult = fixture.Create<CalcResult>();
            var materialCostSummary = fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>>();
            var materialCommsCostSummary = fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>>();

            // Act
            var result = LaDataPrepCostsProducer.GetLaDataPrepCostsProducerFeeWithoutBadDebtProvision(producers, materials, calcResult, materialCostSummary, materialCommsCostSummary);

            // Assert
            Assert.Fail("Create or modify test");
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsProducerFeeWithoutBadDebtProvisionWithNullProducers()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsProducer.GetLaDataPrepCostsProducerFeeWithoutBadDebtProvision(default(IEnumerable<ProducerDetail>), new Mock<IEnumerable<MaterialDetail>>().Object, fixture.Create<CalcResult>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>>()));
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsProducerFeeWithoutBadDebtProvisionWithNullMaterials()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsProducer.GetLaDataPrepCostsProducerFeeWithoutBadDebtProvision(new Mock<IEnumerable<ProducerDetail>>().Object, default(IEnumerable<MaterialDetail>), fixture.Create<CalcResult>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>>()));
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsProducerFeeWithoutBadDebtProvisionWithNullCalcResult()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsProducer.GetLaDataPrepCostsProducerFeeWithoutBadDebtProvision(new Mock<IEnumerable<ProducerDetail>>().Object, new Mock<IEnumerable<MaterialDetail>>().Object, default(CalcResult), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>>()));
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsProducerFeeWithoutBadDebtProvisionWithNullMaterialCostSummary()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsProducer.GetLaDataPrepCostsProducerFeeWithoutBadDebtProvision(new Mock<IEnumerable<ProducerDetail>>().Object, new Mock<IEnumerable<MaterialDetail>>().Object, fixture.Create<CalcResult>(), default(Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>>()));
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsProducerFeeWithoutBadDebtProvisionWithNullMaterialCommsCostSummary()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsProducer.GetLaDataPrepCostsProducerFeeWithoutBadDebtProvision(new Mock<IEnumerable<ProducerDetail>>().Object, new Mock<IEnumerable<MaterialDetail>>().Object, fixture.Create<CalcResult>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>>(), default(Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>)));
        }

        [TestMethod]
        public void CanCallGetLaDataPrepCostsBadDebtProvision()
        {
            // Arrange
            var fixture = new Fixture();
            var producers = new Mock<IEnumerable<ProducerDetail>>().Object;
            var materials = new Mock<IEnumerable<MaterialDetail>>().Object;
            var calcResult = fixture.Create<CalcResult>();
            var materialCostSummary = fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>>();
            var materialCommsCostSummary = fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>>();

            // Act
            var result = LaDataPrepCostsProducer.GetLaDataPrepCostsBadDebtProvision(producers, materials, calcResult, materialCostSummary, materialCommsCostSummary);

            // Assert
            Assert.Fail("Create or modify test");
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsBadDebtProvisionWithNullProducers()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsProducer.GetLaDataPrepCostsBadDebtProvision(default(IEnumerable<ProducerDetail>), new Mock<IEnumerable<MaterialDetail>>().Object, fixture.Create<CalcResult>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>>()));
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsBadDebtProvisionWithNullMaterials()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsProducer.GetLaDataPrepCostsBadDebtProvision(new Mock<IEnumerable<ProducerDetail>>().Object, default(IEnumerable<MaterialDetail>), fixture.Create<CalcResult>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>>()));
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsBadDebtProvisionWithNullCalcResult()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsProducer.GetLaDataPrepCostsBadDebtProvision(new Mock<IEnumerable<ProducerDetail>>().Object, new Mock<IEnumerable<MaterialDetail>>().Object, default(CalcResult), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>>()));
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsBadDebtProvisionWithNullMaterialCostSummary()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsProducer.GetLaDataPrepCostsBadDebtProvision(new Mock<IEnumerable<ProducerDetail>>().Object, new Mock<IEnumerable<MaterialDetail>>().Object, fixture.Create<CalcResult>(), default(Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>>()));
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsBadDebtProvisionWithNullMaterialCommsCostSummary()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsProducer.GetLaDataPrepCostsBadDebtProvision(new Mock<IEnumerable<ProducerDetail>>().Object, new Mock<IEnumerable<MaterialDetail>>().Object, fixture.Create<CalcResult>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>>(), default(Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>)));
        }

        [TestMethod]
        public void CanCallGetLaDataPrepCostsProducerFeeWithBadDebtProvision()
        {
            // Arrange
            var fixture = new Fixture();
            var producers = new Mock<IEnumerable<ProducerDetail>>().Object;
            var materials = new Mock<IEnumerable<MaterialDetail>>().Object;
            var calcResult = fixture.Create<CalcResult>();
            var materialCostSummary = fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>>();
            var materialCommsCostSummary = fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>>();

            // Act
            var result = LaDataPrepCostsProducer.GetLaDataPrepCostsProducerFeeWithBadDebtProvision(producers, materials, calcResult, materialCostSummary, materialCommsCostSummary);

            // Assert
            Assert.Fail("Create or modify test");
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsProducerFeeWithBadDebtProvisionWithNullProducers()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsProducer.GetLaDataPrepCostsProducerFeeWithBadDebtProvision(default(IEnumerable<ProducerDetail>), new Mock<IEnumerable<MaterialDetail>>().Object, fixture.Create<CalcResult>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>>()));
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsProducerFeeWithBadDebtProvisionWithNullMaterials()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsProducer.GetLaDataPrepCostsProducerFeeWithBadDebtProvision(new Mock<IEnumerable<ProducerDetail>>().Object, default(IEnumerable<MaterialDetail>), fixture.Create<CalcResult>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>>()));
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsProducerFeeWithBadDebtProvisionWithNullCalcResult()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsProducer.GetLaDataPrepCostsProducerFeeWithBadDebtProvision(new Mock<IEnumerable<ProducerDetail>>().Object, new Mock<IEnumerable<MaterialDetail>>().Object, default(CalcResult), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>>()));
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsProducerFeeWithBadDebtProvisionWithNullMaterialCostSummary()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsProducer.GetLaDataPrepCostsProducerFeeWithBadDebtProvision(new Mock<IEnumerable<ProducerDetail>>().Object, new Mock<IEnumerable<MaterialDetail>>().Object, fixture.Create<CalcResult>(), default(Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>>()));
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsProducerFeeWithBadDebtProvisionWithNullMaterialCommsCostSummary()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsProducer.GetLaDataPrepCostsProducerFeeWithBadDebtProvision(new Mock<IEnumerable<ProducerDetail>>().Object, new Mock<IEnumerable<MaterialDetail>>().Object, fixture.Create<CalcResult>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>>(), default(Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>)));
        }

        [TestMethod]
        public void CanCallGetLaDataPrepCostsProducerFeeWithoutBadDebtProvisionTotal()
        {
            // Arrange
            var fixture = new Fixture();
            var producers = new Mock<IEnumerable<ProducerDetail>>().Object;
            var materials = new Mock<IEnumerable<MaterialDetail>>().Object;
            var calcResult = fixture.Create<CalcResult>();
            var materialCostSummary = fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>>();
            var materialCommsCostSummary = fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>>();

            // Act
            var result = LaDataPrepCostsProducer.GetLaDataPrepCostsProducerFeeWithoutBadDebtProvisionTotal(producers, materials, calcResult, materialCostSummary, materialCommsCostSummary);

            // Assert
            Assert.Fail("Create or modify test");
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsProducerFeeWithoutBadDebtProvisionTotalWithNullProducers()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsProducer.GetLaDataPrepCostsProducerFeeWithoutBadDebtProvisionTotal(default(IEnumerable<ProducerDetail>), new Mock<IEnumerable<MaterialDetail>>().Object, fixture.Create<CalcResult>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>>()));
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsProducerFeeWithoutBadDebtProvisionTotalWithNullMaterials()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsProducer.GetLaDataPrepCostsProducerFeeWithoutBadDebtProvisionTotal(new Mock<IEnumerable<ProducerDetail>>().Object, default(IEnumerable<MaterialDetail>), fixture.Create<CalcResult>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>>()));
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsProducerFeeWithoutBadDebtProvisionTotalWithNullCalcResult()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsProducer.GetLaDataPrepCostsProducerFeeWithoutBadDebtProvisionTotal(new Mock<IEnumerable<ProducerDetail>>().Object, new Mock<IEnumerable<MaterialDetail>>().Object, default(CalcResult), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>>()));
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsProducerFeeWithoutBadDebtProvisionTotalWithNullMaterialCostSummary()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsProducer.GetLaDataPrepCostsProducerFeeWithoutBadDebtProvisionTotal(new Mock<IEnumerable<ProducerDetail>>().Object, new Mock<IEnumerable<MaterialDetail>>().Object, fixture.Create<CalcResult>(), default(Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>>()));
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsProducerFeeWithoutBadDebtProvisionTotalWithNullMaterialCommsCostSummary()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsProducer.GetLaDataPrepCostsProducerFeeWithoutBadDebtProvisionTotal(new Mock<IEnumerable<ProducerDetail>>().Object, new Mock<IEnumerable<MaterialDetail>>().Object, fixture.Create<CalcResult>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>>(), default(Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>)));
        }

        [TestMethod]
        public void CanCallGetLaDataPrepCostsBadDebtProvisionTotal()
        {
            // Act
            var result = LaDataPrepCostsProducer.GetLaDataPrepCostsBadDebtProvisionTotal();

            // Assert
            Assert.Fail("Create or modify test");
        }

        [TestMethod]
        public void CanCallGetLaDataPrepCostsProducerFeeWithBadDebtProvisionTotal()
        {
            // Act
            var result = LaDataPrepCostsProducer.GetLaDataPrepCostsProducerFeeWithBadDebtProvisionTotal();

            // Assert
            Assert.Fail("Create or modify test");
        }

        [TestMethod]
        public void CanCallGetLaDataPrepCostsEnglandTotalWithBadDebtProvision()
        {
            // Arrange
            var fixture = new Fixture();
            var producers = new Mock<IEnumerable<ProducerDetail>>().Object;
            var materials = new Mock<IEnumerable<MaterialDetail>>().Object;
            var calcResult = fixture.Create<CalcResult>();
            var materialCostSummary = fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>>();
            var materialCommsCostSummary = fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>>();

            // Act
            var result = LaDataPrepCostsProducer.GetLaDataPrepCostsEnglandTotalWithBadDebtProvision(producers, materials, calcResult, materialCostSummary, materialCommsCostSummary);

            // Assert
            Assert.Fail("Create or modify test");
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsEnglandTotalWithBadDebtProvisionWithNullProducers()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsProducer.GetLaDataPrepCostsEnglandTotalWithBadDebtProvision(default(IEnumerable<ProducerDetail>), new Mock<IEnumerable<MaterialDetail>>().Object, fixture.Create<CalcResult>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>>()));
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsEnglandTotalWithBadDebtProvisionWithNullMaterials()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsProducer.GetLaDataPrepCostsEnglandTotalWithBadDebtProvision(new Mock<IEnumerable<ProducerDetail>>().Object, default(IEnumerable<MaterialDetail>), fixture.Create<CalcResult>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>>()));
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsEnglandTotalWithBadDebtProvisionWithNullCalcResult()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsProducer.GetLaDataPrepCostsEnglandTotalWithBadDebtProvision(new Mock<IEnumerable<ProducerDetail>>().Object, new Mock<IEnumerable<MaterialDetail>>().Object, default(CalcResult), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>>()));
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsEnglandTotalWithBadDebtProvisionWithNullMaterialCostSummary()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsProducer.GetLaDataPrepCostsEnglandTotalWithBadDebtProvision(new Mock<IEnumerable<ProducerDetail>>().Object, new Mock<IEnumerable<MaterialDetail>>().Object, fixture.Create<CalcResult>(), default(Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>>()));
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsEnglandTotalWithBadDebtProvisionWithNullMaterialCommsCostSummary()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsProducer.GetLaDataPrepCostsEnglandTotalWithBadDebtProvision(new Mock<IEnumerable<ProducerDetail>>().Object, new Mock<IEnumerable<MaterialDetail>>().Object, fixture.Create<CalcResult>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>>(), default(Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>)));
        }

        [TestMethod]
        public void CanCallGetLaDataPrepCostsEnglandOverallTotalWithBadDebtProvision()
        {
            // Act
            var result = LaDataPrepCostsProducer.GetLaDataPrepCostsEnglandOverallTotalWithBadDebtProvision();

            // Assert
            Assert.Fail("Create or modify test");
        }

        [TestMethod]
        public void CanCallGetLaDataPrepCostsWalesTotalWithBadDebtProvision()
        {
            // Arrange
            var fixture = new Fixture();
            var producers = new Mock<IEnumerable<ProducerDetail>>().Object;
            var materials = new Mock<IEnumerable<MaterialDetail>>().Object;
            var calcResult = fixture.Create<CalcResult>();
            var materialCostSummary = fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>>();
            var materialCommsCostSummary = fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>>();

            // Act
            var result = LaDataPrepCostsProducer.GetLaDataPrepCostsWalesTotalWithBadDebtProvision(producers, materials, calcResult, materialCostSummary, materialCommsCostSummary);

            // Assert
            Assert.Fail("Create or modify test");
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsWalesTotalWithBadDebtProvisionWithNullProducers()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsProducer.GetLaDataPrepCostsWalesTotalWithBadDebtProvision(default(IEnumerable<ProducerDetail>), new Mock<IEnumerable<MaterialDetail>>().Object, fixture.Create<CalcResult>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>>()));
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsWalesTotalWithBadDebtProvisionWithNullMaterials()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsProducer.GetLaDataPrepCostsWalesTotalWithBadDebtProvision(new Mock<IEnumerable<ProducerDetail>>().Object, default(IEnumerable<MaterialDetail>), fixture.Create<CalcResult>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>>()));
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsWalesTotalWithBadDebtProvisionWithNullCalcResult()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsProducer.GetLaDataPrepCostsWalesTotalWithBadDebtProvision(new Mock<IEnumerable<ProducerDetail>>().Object, new Mock<IEnumerable<MaterialDetail>>().Object, default(CalcResult), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>>()));
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsWalesTotalWithBadDebtProvisionWithNullMaterialCostSummary()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsProducer.GetLaDataPrepCostsWalesTotalWithBadDebtProvision(new Mock<IEnumerable<ProducerDetail>>().Object, new Mock<IEnumerable<MaterialDetail>>().Object, fixture.Create<CalcResult>(), default(Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>>()));
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsWalesTotalWithBadDebtProvisionWithNullMaterialCommsCostSummary()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsProducer.GetLaDataPrepCostsWalesTotalWithBadDebtProvision(new Mock<IEnumerable<ProducerDetail>>().Object, new Mock<IEnumerable<MaterialDetail>>().Object, fixture.Create<CalcResult>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>>(), default(Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>)));
        }

        [TestMethod]
        public void CanCallGetLaDataPrepCostsWalesOverallTotalWithBadDebtProvision()
        {
            // Act
            var result = LaDataPrepCostsProducer.GetLaDataPrepCostsWalesOverallTotalWithBadDebtProvision();

            // Assert
            Assert.Fail("Create or modify test");
        }

        [TestMethod]
        public void CanCallGetLaDataPrepCostsScotlandTotalWithBadDebtProvision()
        {
            // Arrange
            var fixture = new Fixture();
            var producers = new Mock<IEnumerable<ProducerDetail>>().Object;
            var materials = new Mock<IEnumerable<MaterialDetail>>().Object;
            var calcResult = fixture.Create<CalcResult>();
            var materialCostSummary = fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>>();
            var materialCommsCostSummary = fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>>();

            // Act
            var result = LaDataPrepCostsProducer.GetLaDataPrepCostsScotlandTotalWithBadDebtProvision(producers, materials, calcResult, materialCostSummary, materialCommsCostSummary);

            // Assert
            Assert.Fail("Create or modify test");
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsScotlandTotalWithBadDebtProvisionWithNullProducers()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsProducer.GetLaDataPrepCostsScotlandTotalWithBadDebtProvision(default(IEnumerable<ProducerDetail>), new Mock<IEnumerable<MaterialDetail>>().Object, fixture.Create<CalcResult>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>>()));
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsScotlandTotalWithBadDebtProvisionWithNullMaterials()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsProducer.GetLaDataPrepCostsScotlandTotalWithBadDebtProvision(new Mock<IEnumerable<ProducerDetail>>().Object, default(IEnumerable<MaterialDetail>), fixture.Create<CalcResult>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>>()));
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsScotlandTotalWithBadDebtProvisionWithNullCalcResult()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsProducer.GetLaDataPrepCostsScotlandTotalWithBadDebtProvision(new Mock<IEnumerable<ProducerDetail>>().Object, new Mock<IEnumerable<MaterialDetail>>().Object, default(CalcResult), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>>()));
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsScotlandTotalWithBadDebtProvisionWithNullMaterialCostSummary()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsProducer.GetLaDataPrepCostsScotlandTotalWithBadDebtProvision(new Mock<IEnumerable<ProducerDetail>>().Object, new Mock<IEnumerable<MaterialDetail>>().Object, fixture.Create<CalcResult>(), default(Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>>()));
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsScotlandTotalWithBadDebtProvisionWithNullMaterialCommsCostSummary()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsProducer.GetLaDataPrepCostsScotlandTotalWithBadDebtProvision(new Mock<IEnumerable<ProducerDetail>>().Object, new Mock<IEnumerable<MaterialDetail>>().Object, fixture.Create<CalcResult>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>>(), default(Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>)));
        }

        [TestMethod]
        public void CanCallGetLaDataPrepCostsScotlandOverallTotalWithBadDebtProvision()
        {
            // Act
            var result = LaDataPrepCostsProducer.GetLaDataPrepCostsScotlandOverallTotalWithBadDebtProvision();

            // Assert
            Assert.Fail("Create or modify test");
        }

        [TestMethod]
        public void CanCallGetLaDataPrepCostsNorthernIrelandTotalWithBadDebtProvision()
        {
            // Arrange
            var fixture = new Fixture();
            var producers = new Mock<IEnumerable<ProducerDetail>>().Object;
            var materials = new Mock<IEnumerable<MaterialDetail>>().Object;
            var calcResult = fixture.Create<CalcResult>();
            var materialCostSummary = fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>>();
            var materialCommsCostSummary = fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>>();

            // Act
            var result = LaDataPrepCostsProducer.GetLaDataPrepCostsNorthernIrelandTotalWithBadDebtProvision(producers, materials, calcResult, materialCostSummary, materialCommsCostSummary);

            // Assert
            Assert.Fail("Create or modify test");
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsNorthernIrelandTotalWithBadDebtProvisionWithNullProducers()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsProducer.GetLaDataPrepCostsNorthernIrelandTotalWithBadDebtProvision(default(IEnumerable<ProducerDetail>), new Mock<IEnumerable<MaterialDetail>>().Object, fixture.Create<CalcResult>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>>()));
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsNorthernIrelandTotalWithBadDebtProvisionWithNullMaterials()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsProducer.GetLaDataPrepCostsNorthernIrelandTotalWithBadDebtProvision(new Mock<IEnumerable<ProducerDetail>>().Object, default(IEnumerable<MaterialDetail>), fixture.Create<CalcResult>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>>()));
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsNorthernIrelandTotalWithBadDebtProvisionWithNullCalcResult()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsProducer.GetLaDataPrepCostsNorthernIrelandTotalWithBadDebtProvision(new Mock<IEnumerable<ProducerDetail>>().Object, new Mock<IEnumerable<MaterialDetail>>().Object, default(CalcResult), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>>()));
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsNorthernIrelandTotalWithBadDebtProvisionWithNullMaterialCostSummary()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsProducer.GetLaDataPrepCostsNorthernIrelandTotalWithBadDebtProvision(new Mock<IEnumerable<ProducerDetail>>().Object, new Mock<IEnumerable<MaterialDetail>>().Object, fixture.Create<CalcResult>(), default(Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>>()));
        }

        [TestMethod]
        public void CannotCallGetLaDataPrepCostsNorthernIrelandTotalWithBadDebtProvisionWithNullMaterialCommsCostSummary()
        {
            // Arrange
            var fixture = new Fixture();
            Assert.ThrowsException<ArgumentNullException>(() => LaDataPrepCostsProducer.GetLaDataPrepCostsNorthernIrelandTotalWithBadDebtProvision(new Mock<IEnumerable<ProducerDetail>>().Object, new Mock<IEnumerable<MaterialDetail>>().Object, fixture.Create<CalcResult>(), fixture.Create<Dictionary<MaterialDetail, CalcResultSummaryProducerDisposalFeesByMaterial>>(), default(Dictionary<MaterialDetail, CalcResultSummaryProducerCommsFeesCostByMaterial>)));
        }

        [TestMethod]
        public void CanCallGetLaDataPrepCostsNorthernIrelandOverallTotalWithBadDebtProvision()
        {
            // Act
            var result = LaDataPrepCostsProducer.GetLaDataPrepCostsNorthernIrelandOverallTotalWithBadDebtProvision();

            // Assert
            Assert.Fail("Create or modify test");
        }
    }
}