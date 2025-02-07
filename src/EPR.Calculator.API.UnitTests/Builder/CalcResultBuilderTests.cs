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
using EPR.Calculator.API.Builder.ScaledupProducers;
using EPR.Calculator.API.Data;

namespace EPR.Calculator.API.UnitTests
{
    [TestClass]
    public class CalcResultBuilderTests
    {
        private readonly Mock<ICalcResultDetailBuilder> mockCalcResultDetailBuilder;
        private readonly Mock<ICalcResultLapcapDataBuilder> mockLapcapBuilder;
        private readonly Mock<ICalcResultLateReportingBuilder> mockLateReportingBuilder;
        private readonly Mock<ICalcRunLaDisposalCostBuilder> mockCalcRunLaDisposalCostBuilder;
        private readonly Mock<ICalcResultCommsCostBuilder> mockCommsCostReportBuilder;
        private readonly Mock<ICalcResultSummaryBuilder> mockSummaryBuilder;
        private readonly CalcResultBuilder calcResultBuilder;

        private readonly Mock<ICalcResultParameterOtherCostBuilder> mockCalcResultParameterOtherCostBuilder;
        private readonly Mock<ICalcResultOnePlusFourApportionmentBuilder> mockOnePlusFourApportionmentBuilder;
        private readonly Mock<ICalcResultScaledupProducersBuilder> mockCalcResultScaledupProducersBuilder;

        private readonly Mock<ApplicationDBContext> mockContext;

        public CalcResultBuilderTests()
        {
            mockCalcResultDetailBuilder = new Mock<ICalcResultDetailBuilder>();
            mockLapcapBuilder = new Mock<ICalcResultLapcapDataBuilder>();
            mockSummaryBuilder = new Mock<ICalcResultSummaryBuilder>();
            mockCalcRunLaDisposalCostBuilder = new Mock<ICalcRunLaDisposalCostBuilder>();
            mockCommsCostReportBuilder = new Mock<ICalcResultCommsCostBuilder>();
            mockLateReportingBuilder = new Mock<ICalcResultLateReportingBuilder>();
            mockCalcResultParameterOtherCostBuilder = new Mock<ICalcResultParameterOtherCostBuilder>();
            mockOnePlusFourApportionmentBuilder = new Mock<ICalcResultOnePlusFourApportionmentBuilder>();
            mockCalcRunLaDisposalCostBuilder = new Mock<ICalcRunLaDisposalCostBuilder>();
            mockCalcResultScaledupProducersBuilder = new Mock<ICalcResultScaledupProducersBuilder>();

            mockContext = new Mock<ApplicationDBContext>();

            calcResultBuilder = new CalcResultBuilder(
                mockContext.Object,
                mockCalcResultDetailBuilder.Object,
                mockLapcapBuilder.Object,
                mockCalcResultParameterOtherCostBuilder.Object,
                mockOnePlusFourApportionmentBuilder.Object,
                mockCommsCostReportBuilder.Object,
                mockLateReportingBuilder.Object,
                mockCalcRunLaDisposalCostBuilder.Object,
                mockCalcResultScaledupProducersBuilder.Object,
                mockSummaryBuilder.Object);
        }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new CalcResultBuilder(
                mockContext.Object,
                mockCalcResultDetailBuilder.Object,
                mockLapcapBuilder.Object,
                mockCalcResultParameterOtherCostBuilder.Object,
                mockOnePlusFourApportionmentBuilder.Object,
                mockCommsCostReportBuilder.Object,
                mockLateReportingBuilder.Object,
                mockCalcRunLaDisposalCostBuilder.Object,
                mockCalcResultScaledupProducersBuilder.Object,
                mockSummaryBuilder.Object);

            // Assert
            Assert.IsNotNull(instance);
        }

        //[TestMethod]
        //public void Build_ShouldReturnCalcResult()
        //{
        //    var resultsRequestDto = new CalcResultsRequestDto();
        //    var mockResultDetail = new Mock<CalcResultDetail>();
        //    var mockLapcapData = new Mock<CalcResultLapcapData>();
        //    var mockOtherParams = new Mock<CalcResultParameterOtherCost>();
        //    var mockOnePlusFourApp = new Mock<CalcResultOnePlusFourApportionment>();
        //    var mockCalcResultCommsCost = new Mock<CalcResultCommsCost>();
        //    var mockCalcResultLateReportingTonnage = new Mock<CalcResultLateReportingTonnage>();
        //    var mockCalcResultLaDisposalCostData = new Mock<CalcResultLaDisposalCostData>();
        //    var mockCalcResultScaledupProducers = new Mock<CalcResultScaledupProducers>();
        //    var mockCalcResultSummary = new Mock<CalcResultSummary>();

        //    mockCalcResultDetailBuilder.Setup(m => m.Construct(resultsRequestDto)).ReturnsAsync(mockResultDetail.Object);
        //    mockLapcapBuilder.Setup(m => m.Construct(resultsRequestDto)).ReturnsAsync(mockLapcapData.Object);
        //    mockCalcResultParameterOtherCostBuilder.Setup(m => m.Construct(resultsRequestDto))
        //        .ReturnsAsync(mockOtherParams.Object);
        //    mockOnePlusFourApportionmentBuilder.Setup(m => m.Construct(resultsRequestDto, It.IsAny<CalcResult>()))
        //        .Returns(mockOnePlusFourApp.Object);
        //    mockCommsCostReportBuilder
        //        .Setup(m => m.Construct(resultsRequestDto, It.IsAny<CalcResultOnePlusFourApportionment>()))
        //        .ReturnsAsync(mockCalcResultCommsCost.Object);
        //    mockLateReportingBuilder.Setup(m => m.Construct(resultsRequestDto))
        //        .ReturnsAsync(mockCalcResultLateReportingTonnage.Object);
        //    mockCalcRunLaDisposalCostBuilder.Setup(m => m.Construct(resultsRequestDto, It.IsAny<CalcResult>()))
        //        .ReturnsAsync(mockCalcResultLaDisposalCostData.Object);
        //    mockCalcResultScaledupProducersBuilder.Setup(m => m.Construct(resultsRequestDto, It.IsAny<CalcResult>(), It.IsAny<IEnumerable<ScaledupProducer>>())
        //        .ReturnsAsync(mockCalcResultScaledupProducers.Object);
        //    mockSummaryBuilder.Setup(x => x.Construct(resultsRequestDto, It.IsAny<CalcResult>()))
        //        .ReturnsAsync(mockCalcResultSummary.Object);

        //    var results = calcResultBuilder.Build(resultsRequestDto);
        //    results.Wait();
        //    var result = results.Result;

        //    Assert.IsNotNull(result);
        //    Assert.AreEqual(mockResultDetail.Object, result.CalcResultDetail);
        //    Assert.AreEqual(mockLapcapData.Object, result.CalcResultLapcapData);
        //    Assert.AreEqual(mockOtherParams.Object, result.CalcResultParameterOtherCost);
        //    Assert.AreEqual(mockOnePlusFourApp.Object, result.CalcResultOnePlusFourApportionment);
        //    Assert.AreEqual(mockCalcResultCommsCost.Object, result.CalcResultCommsCostReportDetail);
        //    Assert.AreEqual(mockCalcResultLateReportingTonnage.Object, result.CalcResultLateReportingTonnageData);
        //    Assert.AreEqual(mockCalcResultLaDisposalCostData.Object, result.CalcResultLaDisposalCostData);
        //    Assert.AreEqual(mockCalcResultScaledupProducersBuilder.Object, result.CalcResultScaledupProducers);
        //    Assert.AreEqual(mockCalcResultSummary.Object, result.CalcResultSummary);
        //}
    }
}
