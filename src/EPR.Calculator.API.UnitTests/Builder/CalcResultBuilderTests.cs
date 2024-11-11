namespace EPR.Calculator.API.UnitTests.Builder
{
    using System;
    using EPR.Calculator.API.Builder;
    using EPR.Calculator.API.Builder.LaDisposalCost;
    using EPR.Calculator.API.Builder.Lapcap;
    using EPR.Calculator.API.Builder.LateReportingTonnages;
    using EPR.Calculator.API.Builder.Summary;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CalcResultBuilderTests
    {
        private CalcResultBuilder testClass;
        private Mock<ICalcResultDetailBuilder> calcResultDetailBuilder;
        private Mock<ICalcResultLapcapDataBuilder> lapcapBuilder;
        private Mock<ICalcResultSummaryBuilder> summaryBuilder;
        private Mock<ICalcResultLateReportingBuilder> mockLateReportingBuilder;
        private Mock<ICalcRunLaDisposalCostBuilder> runLaDisposalCostBuilder;

        [TestInitialize]
        public void SetUp()
        {
            this.calcResultDetailBuilder = new Mock<ICalcResultDetailBuilder>();
            this.lapcapBuilder = new Mock<ICalcResultLapcapDataBuilder>();
            this.summaryBuilder = new Mock<ICalcResultSummaryBuilder>();
            this.mockLateReportingBuilder = new Mock<ICalcResultLateReportingBuilder>();
            this.runLaDisposalCostBuilder = new Mock<ICalcRunLaDisposalCostBuilder>();
            this.testClass = new CalcResultBuilder(calcResultDetailBuilder.Object, lapcapBuilder.Object, mockLateReportingBuilder.Object, runLaDisposalCostBuilder.Object, summaryBuilder.Object);
        }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new CalcResultBuilder(calcResultDetailBuilder.Object, lapcapBuilder.Object, mockLateReportingBuilder.Object, runLaDisposalCostBuilder.Object, summaryBuilder.Object);

            // Assert
            Assert.IsNotNull(instance);
        }
    }
}