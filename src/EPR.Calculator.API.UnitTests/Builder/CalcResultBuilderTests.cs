namespace EPR.Calculator.API.UnitTests.Builder
{
    using System;
    using EPR.Calculator.API.Builder;
    using EPR.Calculator.API.Builder.Lapcap;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class CalcResultBuilderTests
    {
        private CalcResultBuilder testClass;
        private Mock<ICalcResultDetailBuilder> calcResultDetailBuilder;
        private Mock<ICalcResultLapcapDataBuilder> lapcapBuilder;
        private Mock<ICalcResultOnePlusFourApportionmentBuilder> mockOnePlusFourApportionmentBuilder;

        [TestInitialize]
        public void SetUp()
        {
            this.calcResultDetailBuilder = new Mock<ICalcResultDetailBuilder>();
            this.lapcapBuilder = new Mock<ICalcResultLapcapDataBuilder>();
            this.testClass = new CalcResultBuilder(calcResultDetailBuilder.Object, lapcapBuilder.Object, mockOnePlusFourApportionmentBuilder.Object);
        }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new CalcResultBuilder(calcResultDetailBuilder.Object, lapcapBuilder.Object, mockOnePlusFourApportionmentBuilder.Object);

            // Assert
            Assert.IsNotNull(instance);
        }
    }
}