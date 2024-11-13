using EPR.Calculator.API.Builder.LateReportingTonnages;
using EPR.Calculator.API.CommsCost;

namespace EPR.Calculator.API.UnitTests.Builder
{
    using System;
    using EPR.Calculator.API.Builder;
    using EPR.Calculator.API.Builder.LaDisposalCost;
    using EPR.Calculator.API.Builder.Lapcap;
    using EPR.Calculator.API.Builder.LateReportingTonnages;
    using EPR.Calculator.API.Builder.Summary;
    using EPR.Calculator.API.Builder.ParametersOther;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CalcResultBuilderTests
    {
        private Mock<ICalcResultDetailBuilder> calcResultDetailBuilder;
        private Mock<ICalcResultLapcapDataBuilder> lapcapBuilder;
        private Mock<ICalcResultSummaryBuilder> summaryBuilder;
        private Mock<ICalcResultLateReportingBuilder> mockLateReportingBuilder;
        private Mock<ICalcRunLaDisposalCostBuilder> runLaDisposalCostBuilder;
        private Mock<ICalcResultCommsCostBuilder> commsCostReportBuilder;
        private Mock<ICalcResultParameterOtherCostBuilder> calcResultParameterOtherCostBuilder;

        [TestInitialize]
        public void SetUp()
        {
            this.calcResultDetailBuilder = new Mock<ICalcResultDetailBuilder>();
            this.lapcapBuilder = new Mock<ICalcResultLapcapDataBuilder>();
            this.summaryBuilder = new Mock<ICalcResultSummaryBuilder>();
            this.mockLateReportingBuilder = new Mock<ICalcResultLateReportingBuilder>();
            this.runLaDisposalCostBuilder = new Mock<ICalcRunLaDisposalCostBuilder>();
            this.commsCostReportBuilder = new Mock<ICalcResultCommsCostBuilder>();
            this.calcResultParameterOtherCostBuilder = new Mock<ICalcResultParameterOtherCostBuilder>();
            //this.testClass = new CalcResultBuilder(
            //    calcResultDetailBuilder.Object,
            //    lapcapBuilder.Object,
            //    commsCostReportBuilder.Object,
            //    mockLateReportingBuilder.Object,
            //    calcResultParameterOtherCostBuilder.Object,
            //    summaryBuilder.Object);
        }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new CalcResultBuilder(
                calcResultDetailBuilder.Object,
                lapcapBuilder.Object,
                commsCostReportBuilder.Object,
                mockLateReportingBuilder.Object,
                calcResultParameterOtherCostBuilder.Object,
                summaryBuilder.Object);

            // Assert
            Assert.IsNotNull(instance);
        }
    }
}