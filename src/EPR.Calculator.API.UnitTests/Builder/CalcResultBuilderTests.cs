namespace EPR.Calculator.API.UnitTests.Builder
{
    using System;
    using EPR.Calculator.API.Builder;
    using EPR.Calculator.API.Builder.Lapcap;
    using EPR.Calculator.API.Builder.LateReportingTonnages;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CalcResultBuilderTests
    {
        private CalcResultBuilder testClass;
        private Mock<ICalcResultDetailBuilder> calcResultDetailBuilder;
        private Mock<ICalcResultLapcapDataBuilder> lapcapBuilder;
        private Mock<ICalcResultLateReportingBuilder> mockLateReportingBuilder;
        private Mock<ICalcRunLaDisposalCostBuilder> runLaDisposalCostBuilder;

        [TestInitialize]
        public void SetUp()
        {
            this.calcResultDetailBuilder = new Mock<ICalcResultDetailBuilder>();
            this.lapcapBuilder = new Mock<ICalcResultLapcapDataBuilder>();
            this.mockLateReportingBuilder = new Mock<ICalcResultLateReportingBuilder>();
            this.testClass = new CalcResultBuilder(calcResultDetailBuilder.Object, lapcapBuilder.Object, mockLateReportingBuilder.Object);
            this.testClass = new CalcResultBuilder(calcResultDetailBuilder.Object, lapcapBuilder.Object, runLaDisposalCostBuilder.Object);
        }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new CalcResultBuilder(calcResultDetailBuilder.Object, lapcapBuilder.Object, mockLateReportingBuilder.Object);

            // Assert
            Assert.IsNotNull(instance);
        }
    }
}