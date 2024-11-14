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
        private Mock<ICalcResultDetailBuilder> calcResultDetailBuilder;
        private Mock<ICalcResultLapcapDataBuilder> lapcapBuilder;
        private Mock<ICalcResultLateReportingBuilder> lateReportingBuilder;
        private Mock<ICalcRunLaDisposalCostBuilder> runLaDisposalCostBuilder;
        private Mock<ICalcResultCommsCostBuilder> commsCostReportBuilder;
        private Mock<ICalcResultParameterOtherCostBuilder> calcResultParameterOtherCostBuilder;
        private Mock<ICalcResultOnePlusFourApportionmentBuilder> onePlusFourApportionmentBuilder;
        private Mock<ICalcResultSummaryBuilder> summaryBuilder;

        [TestInitialize]
        public void SetUp()
        {
            this.calcResultDetailBuilder = new Mock<ICalcResultDetailBuilder>();
            this.lapcapBuilder = new Mock<ICalcResultLapcapDataBuilder>();
            this.onePlusFourApportionmentBuilder=new Mock<ICalcResultOnePlusFourApportionmentBuilder> ();
            this.commsCostReportBuilder = new Mock<ICalcResultCommsCostBuilder>();
            this.lateReportingBuilder = new Mock<ICalcResultLateReportingBuilder>();
            this.calcResultParameterOtherCostBuilder = new Mock<ICalcResultParameterOtherCostBuilder>();
            this.runLaDisposalCostBuilder = new Mock<ICalcRunLaDisposalCostBuilder>();
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