using EPR.Calculator.API.Builder;
using EPR.Calculator.API.Builder.Lapcap;
using EPR.Calculator.API.Builder.ParametersOther;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.Calculator.API.UnitTests
{
    [TestClass]
    public class CalcResultBuilderTests
    {
        private Mock<ICalcResultDetailBuilder> mockCalcResultDetailBuilder;
        private Mock<ICalcResultLapcapDataBuilder> mockLapcapBuilder;
        private CalcResultBuilder calcResultBuilder;
        private Mock<ICalcResultParameterOtherCostBuilder> mockICalcResultParameterOtherCostBuilder;

        [TestInitialize]
        public void Setup()
        {
            mockCalcResultDetailBuilder = new Mock<ICalcResultDetailBuilder>();
            mockLapcapBuilder = new Mock<ICalcResultLapcapDataBuilder>();
            mockICalcResultParameterOtherCostBuilder = new Mock<ICalcResultParameterOtherCostBuilder>();
            calcResultBuilder = new CalcResultBuilder(
                mockCalcResultDetailBuilder.Object,
                mockLapcapBuilder.Object,
                mockICalcResultParameterOtherCostBuilder.Object);
        }

        [TestMethod]
        public void Build_ShouldReturnCalcResultWithDetailsAndLapcapData()
        {
            var resultsRequestDto = new CalcResultsRequestDto();
            var expectedDetail = new CalcResultDetail();
            var expectedLapcapData = new CalcResultLapcapData
            {
                Name = "SomeName",
                CalcResultLapcapDataDetails = new List<CalcResultLapcapDataDetails>() 
            };

            mockCalcResultDetailBuilder.Setup(m => m.Construct(resultsRequestDto)).Returns(expectedDetail);
            mockLapcapBuilder.Setup(m => m.Construct(resultsRequestDto)).Returns(expectedLapcapData);

            var result = calcResultBuilder.Build(resultsRequestDto);

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedDetail, result.CalcResultDetail);
            Assert.AreEqual(expectedLapcapData, result.CalcResultLapcapData);
        }
    }
}
