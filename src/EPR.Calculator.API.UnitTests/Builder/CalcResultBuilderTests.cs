using EPR.Calculator.API.Builder.LateReportingTonnages;
using EPR.Calculator.API.CommsCost;

namespace EPR.Calculator.API.UnitTests.Builder
{
    using System;
    using EPR.Calculator.API.Builder;
    using EPR.Calculator.API.Builder.LaDisposalCost;
    using EPR.Calculator.API.Builder.Lapcap;
    using EPR.Calculator.API.Builder.ParametersOther;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CalcResultBuilderTests
    {
        private Mock<ICalcResultDetailBuilder> calcResultDetailBuilder;
        private Mock<ICalcResultLapcapDataBuilder> lapcapBuilder;
        private Mock<ICalcResultOnePlusFourApportionmentBuilder> mockOnePlusFourApportionmentBuilder;
        private Mock<ICalcResultParameterOtherCostBuilder> mockICalcResultParameterOtherCostBuilder;
        private Mock<ICalcResultCommsCostBuilder> commsCostReportBuilder;
        private Mock<ICalcResultLateReportingBuilder> mockLateReportingBuilder;
        private Mock<ICalcResultParameterOtherCostBuilder> mockCalcResultParameterOtherCostBuilder;
        private Mock<ICalcRunLaDisposalCostBuilder> mockRunLaDisposalCostBuilder;

        [TestInitialize]
        public void SetUp()
        {
            this.calcResultDetailBuilder = new Mock<ICalcResultDetailBuilder>();
            this.lapcapBuilder = new Mock<ICalcResultLapcapDataBuilder>();
            this.mockICalcResultParameterOtherCostBuilder = new Mock<ICalcResultParameterOtherCostBuilder>();
            this.mockOnePlusFourApportionmentBuilder=new Mock<ICalcResultOnePlusFourApportionmentBuilder> ();
            this.commsCostReportBuilder = new Mock<ICalcResultCommsCostBuilder>();
            this.mockLateReportingBuilder = new Mock<ICalcResultLateReportingBuilder>();
            this.mockCalcResultParameterOtherCostBuilder = new Mock<ICalcResultParameterOtherCostBuilder>();
            this.mockRunLaDisposalCostBuilder = new Mock<ICalcRunLaDisposalCostBuilder>();
        }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new CalcResultBuilder(
                calcResultDetailBuilder.Object,
                lapcapBuilder.Object,
                mockICalcResultParameterOtherCostBuilder.Object,
                mockOnePlusFourApportionmentBuilder.Object,
                commsCostReportBuilder.Object,
                mockLateReportingBuilder.Object,
                mockRunLaDisposalCostBuilder.Object);

            // Assert
            Assert.IsNotNull(instance);
        }
    }
}