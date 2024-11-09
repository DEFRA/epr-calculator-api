using EPR.Calculator.API.Builder;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Exporter;
using EPR.Calculator.API.Models;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Validators;
using EPR.Calculator.API.Wrapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.Calculator.API.UnitTests
{
    [TestClass]
    public class CalcResultServiceTests
    {
        private Mock<ICalcResultBuilder> mockCalcResultBuilder;
        private Mock<ICalcResultsExporter<CalcResult>> mockExporter;
        private Mock<ICalcResultDetailBuilder> mockDetailBuilder;
        private Mock<ICalcResultLapcapDataBuilder> mockLapcapBuilder;
        private Mock<ICalcResultSummaryBuilder> mockSummaryBuilder;


        private Mock<ApplicationDBContext> mockContext;
        private CalculatorInternalController controller;
        private CalcResultBuilder calcResultBuilder;
        private CalcResultDetailBuilder detailBuilder;
        private CalcResultsExporter exporter;
        protected ApplicationDBContext? dbContext;
        protected IOrgAndPomWrapper? wrapper;

        [TestInitialize]
        public void Setup()
        {
            mockCalcResultBuilder = new Mock<ICalcResultBuilder>();
            mockExporter = new Mock<ICalcResultsExporter<CalcResult>>();
            wrapper = new Mock<IOrgAndPomWrapper>().Object;
            controller = new CalculatorInternalController(
               dbContext,
               new RpdStatusDataValidator(wrapper),
               wrapper,
               mockCalcResultBuilder.Object,
               mockExporter.Object,
               new Mock<ITransposePomAndOrgDataService>().Object
            );

            mockDetailBuilder = new Mock<ICalcResultDetailBuilder>();
            mockLapcapBuilder = new Mock<ICalcResultLapcapDataBuilder>();
            mockSummaryBuilder = new Mock<ICalcResultSummaryBuilder>();
            calcResultBuilder = new CalcResultBuilder(mockDetailBuilder.Object, mockLapcapBuilder.Object, mockSummaryBuilder.Object);
            mockContext = new Mock<ApplicationDBContext>();
            detailBuilder = new CalcResultDetailBuilder(mockContext.Object);

        }

        //[TestMethod]
        //public void PrepareCalcResults_ShouldReturnCreatedStatus()
        //{
        //    var requestDto = new CalcResultsRequestDto();
        //    var calcResult = new CalcResult();
        //    mockCalcResultBuilder.Setup(b => b.Build(requestDto)).Returns(calcResult);

        //    var result = controller.PrepareCalcResults(requestDto) as ObjectResult;

        //    Assert.IsNotNull(result);
        //    Assert.AreEqual(201, result.StatusCode);
        //    mockExporter.Verify(e => e.Export(calcResult), Times.Once);
        //}

        //[TestMethod]
        //public void Build_ShouldReturnCalcResultWithDetail()
        //{
        //    var requestDto = new CalcResultsRequestDto();
        //    var detail = new CalcResultDetail();
        //    mockDetailBuilder.Setup(d => d.Construct(requestDto)).Returns(detail);

        //    var result = calcResultBuilder.Build(requestDto);

        //    Assert.IsNotNull(result);
        //    Assert.AreEqual(detail, result.CalcResultDetail);
        //}
    }
}

