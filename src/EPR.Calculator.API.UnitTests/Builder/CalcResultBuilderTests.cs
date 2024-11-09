namespace EPR.Calculator.API.UnitTests.Builder
{
    using System;
    using EPR.Calculator.API.Builder;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CalcResultBuilderTests
    {
        private CalcResultBuilder testClass;
        private Mock<ICalcResultDetailBuilder> calcResultDetailBuilder;
        private Mock<ICalcResultLapcapDataBuilder> lapcapBuilder;
        private Mock<ICalcResultSummaryBuilder> summaryBuilder;

        [TestInitialize]
        public void SetUp()
        {
            this.calcResultDetailBuilder = new Mock<ICalcResultDetailBuilder>();
            this.lapcapBuilder = new Mock<ICalcResultLapcapDataBuilder>();
            this.summaryBuilder = new Mock<ICalcResultSummaryBuilder>();
            this.testClass = new CalcResultBuilder(calcResultDetailBuilder.Object, lapcapBuilder.Object, summaryBuilder.Object);
        }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new CalcResultBuilder(calcResultDetailBuilder.Object, lapcapBuilder.Object, summaryBuilder.Object);

            // Assert
            Assert.IsNotNull(instance);
        }
    }
}