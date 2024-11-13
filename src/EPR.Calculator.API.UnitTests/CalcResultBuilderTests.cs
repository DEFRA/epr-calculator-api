using EPR.Calculator.API.Builder;
using EPR.Calculator.API.Builder.LaDisposalCost;
using EPR.Calculator.API.Builder.Lapcap;
using EPR.Calculator.API.Builder.LateReportingTonnages;
using EPR.Calculator.API.Builder.OnePlusFourApportionment;
using EPR.Calculator.API.Builder.ParametersOther;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using EPR.Calculator.API.Builder.CommsCost;

namespace EPR.Calculator.API.UnitTests
{
    [TestClass]
    public class CalcResultBuilderTests
    {
        private Mock<ICalcResultDetailBuilder> mockCalcResultDetailBuilder;
        private Mock<ICalcResultLapcapDataBuilder> mockLapcapBuilder;
        private Mock<ICalcResultCommsCostBuilder> mockCommsCostReportBuilder;
        private Mock<ICalcResultLateReportingBuilder> mockLateReportingBuilder;
        private CalcResultBuilder calcResultBuilder;
        private Mock<ICalcResultOnePlusFourApportionmentBuilder> mockOnePlusFourApportionmentBuilder;
        private Mock<ICalcResultParameterOtherCostBuilder> mockICalcResultParameterOtherCostBuilder;
        private Mock<ICalcRunLaDisposalCostBuilder> mockCalcRunLaDisposalCostBuilder;
        private Mock<ICalcResultOnePlusFourApportionmentBuilder> mockICalcResultOnePlusFourApportionmentBuilder;


        [TestInitialize]
        public void Setup()
        {
            mockCalcResultDetailBuilder = new Mock<ICalcResultDetailBuilder>();
            mockLapcapBuilder = new Mock<ICalcResultLapcapDataBuilder>();
            mockCommsCostReportBuilder = new Mock<ICalcResultCommsCostBuilder>();
            mockLateReportingBuilder = new Mock<ICalcResultLateReportingBuilder>();
            mockICalcResultParameterOtherCostBuilder = new Mock<ICalcResultParameterOtherCostBuilder>();
            mockICalcResultOnePlusFourApportionmentBuilder = new Mock<ICalcResultOnePlusFourApportionmentBuilder>();
            mockCalcRunLaDisposalCostBuilder = new Mock<ICalcRunLaDisposalCostBuilder>();
            calcResultBuilder = new CalcResultBuilder(
                mockCalcResultDetailBuilder.Object,
                mockLapcapBuilder.Object,
                mockICalcResultParameterOtherCostBuilder.Object,
                mockICalcResultOnePlusFourApportionmentBuilder.Object,
                mockCommsCostReportBuilder.Object,
                mockLateReportingBuilder.Object,
                mockCalcRunLaDisposalCostBuilder.Object);
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
