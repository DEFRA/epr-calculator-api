using EPR.Calculator.API.Builder;
using EPR.Calculator.API.Builder.LaDisposalCost;
using EPR.Calculator.API.Builder.Lapcap;
using EPR.Calculator.API.Builder.LateReportingTonnages;
using EPR.Calculator.API.Builder.Summary;
using EPR.Calculator.API.Builder.OnePlusFourApportionment;
using EPR.Calculator.API.Builder.ParametersOther;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using EPR.Calculator.API.Builder.CommsCost;
using EPR.Calculator.API.Builder.Detail;

namespace EPR.Calculator.API.UnitTests
{
    [TestClass]
    public class CalcResultBuilderTests
    {
        private Mock<ICalcResultDetailBuilder> mockCalcResultDetailBuilder;
        private Mock<ICalcResultLapcapDataBuilder> mockLapcapBuilder;
        private Mock<ICalcResultLateReportingBuilder> mockLateReportingBuilder;
        private Mock<ICalcRunLaDisposalCostBuilder> mockCalcRunLaDisposalCostBuilder;
        private Mock<ICalcResultCommsCostBuilder> mockCommsCostReportBuilder;
        private Mock<ICalcResultSummaryBuilder> mockSummaryBuilder;
        private CalcResultBuilder calcResultBuilder;
        
        private Mock<ICalcResultParameterOtherCostBuilder> mockICalcResultParameterOtherCostBuilder;
        private Mock<ICalcResultOnePlusFourApportionmentBuilder> mockOnePlusFourApportionmentBuilder;

        [TestInitialize]
        public void Setup()
        {
            mockCalcResultDetailBuilder = new Mock<ICalcResultDetailBuilder>();
            mockLapcapBuilder = new Mock<ICalcResultLapcapDataBuilder>();
            mockSummaryBuilder = new Mock<ICalcResultSummaryBuilder>();
            mockLateReportingBuilder = new Mock<ICalcResultLateReportingBuilder>();
            mockCalcRunLaDisposalCostBuilder = new Mock<ICalcRunLaDisposalCostBuilder>();
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
                mockICalcResultParameterOtherCostBuilder.Object,
                mockCalcRunLaDisposalCostBuilder.Object,
                mockSummaryBuilder.Object);
        }

        //[TestMethod]
        //public void Build_ShouldReturnCalcResultWithDetailsAndLapcapData()
        //{
        //    var resultsRequestDto = new CalcResultsRequestDto();
        //    var expectedDetail = new CalcResultDetail();
        //    var expectedLapcapData = new CalcResultLapcapData
        //    {
        //        Name = "SomeName",
        //        CalcResultLapcapDataDetails = new List<CalcResultLapcapDataDetails>() 
        //    };

        //    mockCalcResultDetailBuilder.Setup(m => m.Construct(resultsRequestDto)).Returns(expectedDetail);
        //    mockLapcapBuilder.Setup(m => m.Construct(resultsRequestDto)).Returns(expectedLapcapData);

        //    var result = calcResultBuilder.Build(resultsRequestDto);

        //    Assert.IsNotNull(result);
        //    Assert.AreEqual(expectedDetail, result.CalcResultDetail);
        //    Assert.AreEqual(expectedLapcapData, result.CalcResultLapcapData);
        //}
    }
}
