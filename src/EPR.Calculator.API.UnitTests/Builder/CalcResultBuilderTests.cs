using EPR.Calculator.API.Builder.LateReportingTonnages;
using EPR.Calculator.API.Builder.OnePlusFourApportionment;

namespace EPR.Calculator.API.UnitTests.Builder
{
    using System;
    using EPR.Calculator.API.Builder;
    using EPR.Calculator.API.Builder.CommsCost;
    using EPR.Calculator.API.Builder.Detail;
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
        private readonly Mock<ICalcResultDetailBuilder> calcResultDetailBuilder;
        private readonly Mock<ICalcResultLapcapDataBuilder> lapcapBuilder;
        private readonly Mock<ICalcResultLateReportingBuilder> lateReportingBuilder;
        private readonly Mock<ICalcRunLaDisposalCostBuilder> runLaDisposalCostBuilder;
        private readonly Mock<ICalcResultCommsCostBuilder> commsCostReportBuilder;
        private readonly Mock<ICalcResultParameterOtherCostBuilder> calcResultParameterOtherCostBuilder;
        private readonly Mock<ICalcResultOnePlusFourApportionmentBuilder> onePlusFourApportionmentBuilder;
        private readonly Mock<ICalcResultSummaryBuilder> summaryBuilder;

        public CalcResultBuilderTests()
        {
            this.calcResultDetailBuilder = new Mock<ICalcResultDetailBuilder>();
            this.lapcapBuilder = new Mock<ICalcResultLapcapDataBuilder>();
            this.onePlusFourApportionmentBuilder=new Mock<ICalcResultOnePlusFourApportionmentBuilder> ();
            this.commsCostReportBuilder = new Mock<ICalcResultCommsCostBuilder>();
            this.lateReportingBuilder = new Mock<ICalcResultLateReportingBuilder>();
            this.calcResultParameterOtherCostBuilder = new Mock<ICalcResultParameterOtherCostBuilder>();
            this.runLaDisposalCostBuilder = new Mock<ICalcRunLaDisposalCostBuilder>();
            this.summaryBuilder = new Mock<ICalcResultSummaryBuilder>();
        }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new CalcResultBuilder(
                calcResultDetailBuilder.Object,
                lapcapBuilder.Object,
                calcResultParameterOtherCostBuilder.Object,
                onePlusFourApportionmentBuilder.Object,
                commsCostReportBuilder.Object,
                lateReportingBuilder.Object,
                runLaDisposalCostBuilder.Object,
                summaryBuilder.Object);

            // Assert
            Assert.IsNotNull(instance);
        }
    }
}