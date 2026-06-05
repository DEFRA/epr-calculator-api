using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder;
using EPR.Calculator.Service.Function.Builder.CommsCost;
using EPR.Calculator.Service.Function.Builder.Detail;
using EPR.Calculator.Service.Function.Builder.LaDisposalCost;
using EPR.Calculator.Service.Function.Builder.Lapcap;
using EPR.Calculator.Service.Function.Builder.LateReportingTonnages;
using EPR.Calculator.Service.Function.Builder.OnePlusFourApportionment;
using EPR.Calculator.Service.Function.Builder.ParametersOther;
using EPR.Calculator.Service.Function.Builder.PartialObligations;
using EPR.Calculator.Service.Function.Builder.ProjectedProducers;
using EPR.Calculator.Service.Function.Builder.ScaledupProducers;
using EPR.Calculator.Service.Function.Builder.Summary;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Builder;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class CalcResultBuilderTests : TestsFor<CalcResultBuilder>
{
    private Mock<ICalcResultDetailBuilder> mockCalcResultDetailBuilder = null!;
    private Mock<ICalcResultParameterOtherCostBuilder> mockCalcResultParameterOtherCostBuilder = null!;
    private Mock<ICalcResultPartialObligationBuilder> mockCalcResultPartialObligationBuilder = null!;
    private Mock<ICalcResultProjectedProducersBuilder> mockCalcResultProjectedProducersBuilder = null!;
    private Mock<ICalcResultScaledupProducersBuilder> mockCalcResultScaledupProducersBuilder = null!;
    private Mock<ICalcRunLaDisposalCostBuilder> mockCalcRunLaDisposalCostBuilder = null!;
    private Mock<ICalcResultCommsCostBuilder> mockCommsCostReportBuilder = null!;
    private Mock<ICalcResultLapcapDataBuilder> mockLapcapBuilder = null!;
    private Mock<ICalcResultLateReportingBuilder> mockLateReportingBuilder = null!;
    private Mock<ICalcResultOnePlusFourApportionmentBuilder> mockOnePlusFourApportionmentBuilder = null!;
    private Mock<IProjectedProducersService> mockProjectedProducerService = null!;
    private Mock<IReportedProducerService> mockReportedProducerService = null!;
    private Mock<ISelfManagedConsumerWasteService> mockSelfManagedConsumerWasteService = null!;
    private Mock<ICalcResultSummaryBuilder> mockSummaryBuilder = null!;
    private Mock<ICalcResultService> mockCalcResultService = null!;

    protected override void TestInitialize()
    {
        mockCalcResultDetailBuilder = fixture.Freeze<Mock<ICalcResultDetailBuilder>>();
        mockLapcapBuilder = fixture.Freeze<Mock<ICalcResultLapcapDataBuilder>>();
        mockSummaryBuilder = fixture.Freeze<Mock<ICalcResultSummaryBuilder>>();
        mockCalcRunLaDisposalCostBuilder = fixture.Freeze<Mock<ICalcRunLaDisposalCostBuilder>>();
        mockCommsCostReportBuilder = fixture.Freeze<Mock<ICalcResultCommsCostBuilder>>();
        mockLateReportingBuilder = fixture.Freeze<Mock<ICalcResultLateReportingBuilder>>();
        mockCalcResultParameterOtherCostBuilder = fixture.Freeze<Mock<ICalcResultParameterOtherCostBuilder>>();
        mockOnePlusFourApportionmentBuilder = fixture.Freeze<Mock<ICalcResultOnePlusFourApportionmentBuilder>>();
        mockCalcResultScaledupProducersBuilder = fixture.Freeze<Mock<ICalcResultScaledupProducersBuilder>>();
        mockCalcResultPartialObligationBuilder = fixture.Freeze<Mock<ICalcResultPartialObligationBuilder>>();
        mockCalcResultProjectedProducersBuilder = fixture.Freeze<Mock<ICalcResultProjectedProducersBuilder>>();
        mockProjectedProducerService = fixture.Freeze<Mock<IProjectedProducersService>>();
        mockSelfManagedConsumerWasteService = fixture.Freeze<Mock<ISelfManagedConsumerWasteService>>();
        mockReportedProducerService = fixture.Freeze<Mock<IReportedProducerService>>();
        mockCalcResultService = fixture.Freeze<Mock<ICalcResultService>>();
    }

    [TestMethod]
    public async Task Build_ShouldReturnCalcResult()
    {
        var runContext = TestDataHelper.CalculatorRun2025;
        var mockProducers1 = new List<L1Producer>
        {
            new(1, [new ProducerDetail { ProducerId = 1, SubsidiaryId = null }])
        };
        var mockProducers2 = new List<L1Producer>
        {
            new(2, [new ProducerDetail { ProducerId = 2, SubsidiaryId = null }])
        };

        var mockResultDetail = new Mock<CalcResultDetail>();
        var mockLapcapData = new Mock<CalcResultLapcapData>();
        var mockOtherParams = new Mock<CalcResultParameterOtherCost>();
        var mockOnePlusFourApp = new Mock<CalcResultOnePlusFourApportionment>();
        var mockCalcResultCommsCost = new Mock<CalcResultCommsCost>();
        var mockCalcResultLateReportingTonnage = fixture.Create<CalcResultLateReportingTonnage>();
        var mockCalcResultLaDisposalCostData = new Mock<CalcResultLaDisposalCostData>();
        var mockCalcResultScaledUpProducersData = new Mock<CalcResultScaledupProducers>();
        var mockCalcResultPartialObligationsData = new Mock<CalcResultPartialObligations>();
        var mockCalcResultProjectedProducersData = new Mock<CalcResultProjectedProducers>();
        var mockCalcResultSummary = new Mock<CalcResultSummary>();

        mockCalcResultDetailBuilder.Setup(m => m.ConstructAsync(runContext))
            .ReturnsAsync(mockResultDetail.Object);
        mockLapcapBuilder.Setup(m => m.ConstructAsync(runContext, It.IsAny<IEnumerable<MaterialDetail>>()))
            .ReturnsAsync(mockLapcapData.Object);
        mockCalcResultParameterOtherCostBuilder.Setup(m => m.ConstructAsync(runContext))
            .ReturnsAsync(mockOtherParams.Object);
        mockOnePlusFourApportionmentBuilder.Setup(m => m.Construct(It.IsAny<CalcResult>()))
            .Returns(mockOnePlusFourApp.Object);
        mockCommsCostReportBuilder
            .Setup(m => m.ConstructAsync(runContext, It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<CalcResultOnePlusFourApportionment>(), It.IsAny<CalcResultLateReportingTonnage>()))
            .ReturnsAsync(mockCalcResultCommsCost.Object);
        mockLateReportingBuilder.Setup(m => m.ConstructAsync(runContext, It.IsAny<IImmutableList<MaterialDetail>>()))
            .ReturnsAsync(mockCalcResultLateReportingTonnage);
        mockCalcRunLaDisposalCostBuilder.Setup(m => m.ConstructAsync(runContext, It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<CalcResultLapcapData>(), It.IsAny<CalcResultLateReportingTonnage>(), It.IsAny<SelfManagedConsumerWaste>()))
            .ReturnsAsync(mockCalcResultLaDisposalCostData.Object);
        mockReportedProducerService.Setup(m => m.GetProducers(runContext))
            .ReturnsAsync(mockProducers1);
        mockCalcResultScaledupProducersBuilder.Setup(m => m.ConstructAsync(runContext, It.IsAny<IImmutableList<MaterialDetail>>(), mockProducers1))
            .ReturnsAsync((mockProducers2, mockCalcResultScaledUpProducersData.Object));
        mockCalcResultPartialObligationBuilder.Setup(m => m.ConstructAsync(runContext, It.IsAny<IImmutableList<MaterialDetail>>(), mockProducers2))
            .ReturnsAsync((mockProducers2, mockCalcResultPartialObligationsData.Object));
        mockSummaryBuilder.Setup(x => x.ConstructAsync(runContext, It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<CalcResult>(), It.IsAny<SelfManagedConsumerWaste>()))
            .ReturnsAsync(mockCalcResultSummary.Object);

        mockSelfManagedConsumerWasteService.Setup(x => x.Calculate(
                It.IsAny<RunContext>(),
                It.IsAny<IEnumerable<MaterialDetail>>()))
            .ReturnsAsync(new SelfManagedConsumerWaste
            {
                ProducerTotals = [],
                OverallTotalPerMaterials = []
            });

        var result = await testSubject.BuildAsync(runContext);

        Assert.IsNotNull(result);
        Assert.AreEqual(mockResultDetail.Object, result.CalcResultDetail);
        Assert.AreEqual(mockLapcapData.Object, result.CalcResultLapcapData);
        Assert.AreEqual(mockOtherParams.Object, result.CalcResultParameterOtherCost);
        Assert.AreEqual(mockOnePlusFourApp.Object, result.CalcResultOnePlusFourApportionment);
        Assert.AreEqual(mockCalcResultCommsCost.Object, result.CalcResultCommsCostReportDetail);
        Assert.AreEqual(mockCalcResultLateReportingTonnage, result.CalcResultLateReportingTonnageData);
        Assert.AreEqual(mockCalcResultLaDisposalCostData.Object, result.CalcResultLaDisposalCostData);
        Assert.AreEqual(mockCalcResultScaledUpProducersData.Object, result.CalcResultScaledupProducers);
        Assert.AreEqual(mockCalcResultPartialObligationsData.Object, result.CalcResultPartialObligations);
        Assert.AreNotEqual(mockCalcResultProjectedProducersData.Object, result.CalcResultProjectedProducers);
        Assert.AreEqual(mockCalcResultSummary.Object, result.CalcResultSummary);

        mockCalcRunLaDisposalCostBuilder.Verify(m => m.ConstructAsync(runContext, It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<CalcResultLapcapData>(), It.IsAny<CalcResultLateReportingTonnage>(), It.IsAny<SelfManagedConsumerWaste>()), Times.Once);
        mockCalcResultScaledupProducersBuilder.Verify(m => m.ConstructAsync(runContext, It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<List<L1Producer>>()), Times.Once);
        mockCalcResultProjectedProducersBuilder.Verify(m => m.Construct(runContext, It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<List<L1Producer>>()), Times.Never);
        mockCalcResultPartialObligationBuilder.Verify(m => m.ConstructAsync(runContext, It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<List<L1Producer>>()), Times.Once);
    }

    [TestMethod]
    public async Task Build_ShouldReturnCalcResult_WithProjectedProducers()
    {
        var runContext = TestDataHelper.CalculatorRun2026;
        var mockProducers1 = new List<L1Producer>
        {
            new(1, [new ProducerDetail { ProducerId = 1, SubsidiaryId = null }])
        };
        var mockProducers2 = new List<L1Producer>
        {
            new(2, [new ProducerDetail { ProducerId = 2, SubsidiaryId = null }])
        };

        var mockCalcResultScaledUpProducersData = fixture.Freeze<Mock<CalcResultScaledupProducers>>();
        var mockCalcResultProjectedProducersData = fixture.Freeze<Mock<CalcResultProjectedProducers>>();
        var mockCalcResultPartialObligationsData = fixture.Freeze<Mock<CalcResultPartialObligations>>();

        mockReportedProducerService.Setup(m => m.GetProducers(runContext))
            .ReturnsAsync(mockProducers1);
        mockCalcResultProjectedProducersBuilder.Setup(m => m.Construct(runContext, It.IsAny<IImmutableList<MaterialDetail>>(), mockProducers1))
            .Returns((mockProducers2, mockCalcResultProjectedProducersData.Object));
        mockCalcResultPartialObligationBuilder.Setup(m => m.ConstructAsync(runContext, It.IsAny<IImmutableList<MaterialDetail>>(), mockProducers2))
            .ReturnsAsync((mockProducers2, mockCalcResultPartialObligationsData.Object));

        var result = await testSubject.BuildAsync(runContext);

        Assert.IsNotNull(result);
        Assert.AreNotEqual(mockCalcResultScaledUpProducersData.Object, result.CalcResultScaledupProducers);
        Assert.AreSame(mockCalcResultProjectedProducersData.Object, result.CalcResultProjectedProducers);

        mockCalcResultScaledupProducersBuilder.Verify(m => m.ConstructAsync(runContext, It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<List<L1Producer>>()), Times.Never);
        mockCalcResultProjectedProducersBuilder.Verify(m => m.Construct(runContext, It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<List<L1Producer>>()), Times.Once);
        mockCalcResultService.Verify(m => m.StoreProjectedH1Data(runContext.RunId, It.IsAny<IReadOnlyList<CalcResultH1ProjectedProducer>>()), Times.Once);
        mockCalcResultService.Verify(m => m.StoreProjectedH2Data(runContext.RunId, It.IsAny<IReadOnlyList<CalcResultH2ProjectedProducer>>()), Times.Once);
        mockCalcResultService.Verify(m => m.StorePartialData(runContext.RunId, It.IsAny<IReadOnlyList<CalcResultPartialObligation>>()), Times.Once);
        mockProjectedProducerService.Verify(m => m.StoreProjectedProducers(It.IsAny<List<L1Producer>>()), Times.Once);
    }

    [TestMethod]
    public async Task Build_ShouldReturnCalcResult_WithProjectedProducers_Billing()
    {
        var runContext = TestDataHelper.BillingRun2026;
        var mockProducers1 = new List<L1Producer>
        {
            new(1, [new ProducerDetail { ProducerId = 1, SubsidiaryId = null }])
        };
        var mockProducers2 = new List<L1Producer>
        {
            new(2, [new ProducerDetail { ProducerId = 2, SubsidiaryId = null }])
        };
        
        var mockCalcResultProjectedProducersH1Data = new Mock<List<CalcResultH1ProjectedProducer>>();
        var mockCalcResultProjectedProducersH2Data = new Mock<List<CalcResultH2ProjectedProducer>>();
        var mockCalcResultPartialData = new Mock<List<CalcResultPartialObligation>>();

        mockCalcResultService.Setup(m => m.ReadH1ProjectedData(runContext.RunId))
            .ReturnsAsync(mockCalcResultProjectedProducersH1Data.Object);
        mockCalcResultService.Setup(m => m.ReadH2ProjectedData(runContext.RunId))
            .ReturnsAsync(mockCalcResultProjectedProducersH2Data.Object);
        mockCalcResultService.Setup(m => m.ReadPartialData(runContext.RunId))
            .ReturnsAsync(mockCalcResultPartialData.Object);

        var result = await testSubject.BuildAsync(runContext);


        Assert.IsNotNull(result);
        Assert.AreSame(mockCalcResultProjectedProducersH1Data.Object.ToImmutableList(), result.CalcResultProjectedProducers.H1ProjectedProducers);
        Assert.AreSame(mockCalcResultProjectedProducersH2Data.Object.ToImmutableList(), result.CalcResultProjectedProducers.H2ProjectedProducers);
        Assert.AreSame(mockCalcResultPartialData.Object.ToImmutableList(), result.CalcResultPartialObligations.PartialObligations);

        mockCalcResultScaledupProducersBuilder.Verify(m => m.ConstructAsync(runContext, It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<List<L1Producer>>()), Times.Never);
        mockCalcResultProjectedProducersBuilder.Verify(m => m.Construct(runContext, It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<List<L1Producer>>()), Times.Never);
        mockCalcResultPartialObligationBuilder.Verify(m => m.ConstructAsync(runContext,It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<List<L1Producer>>()), Times.Never);
        mockProjectedProducerService.Verify(m => m.StoreProjectedProducers(It.IsAny<List<L1Producer>>()), Times.Never);
    }

    [TestMethod]
        public async Task Build_ShouldReturnCalcResult_WithScaledUpProducers()
        {
            var runContext = TestDataHelper.CalculatorRun2025;
            var mockProducers1 = new List<L1Producer>
            {
                new L1Producer(1, [new ProducerDetail { ProducerId = 1, SubsidiaryId = null }])
            };
            var mockProducers2 = new List<L1Producer>
            {
                new L1Producer(2, [new ProducerDetail { ProducerId = 2, SubsidiaryId = null }])
            };
            var mockMaterials = ImmutableList<MaterialDetail>.Empty;

            var mockCalcResultScaledUpProducersData = new Mock<CalcResultScaledupProducers>();
            var mockCalcResultProjectedProducersData = new Mock<CalcResultProjectedProducers>();
            var mockCalcResultPartialObligationsData = new Mock<CalcResultPartialObligations>();

            mockReportedProducerService.Setup(m => m.GetProducers(runContext))
                .ReturnsAsync(mockProducers1);
            mockCalcResultScaledupProducersBuilder.Setup(m => m.ConstructAsync(runContext, It.IsAny<IImmutableList<MaterialDetail>>(), mockProducers1))
                .ReturnsAsync((mockProducers2, mockCalcResultScaledUpProducersData.Object));
            mockCalcResultPartialObligationBuilder.Setup(m => m.ConstructAsync(runContext, It.IsAny<IImmutableList<MaterialDetail>>(), mockProducers2))
                .ReturnsAsync((mockProducers2, mockCalcResultPartialObligationsData.Object));

            var result = await testSubject.BuildAsync(runContext);

            Assert.IsNotNull(result);
            Assert.AreSame(mockCalcResultScaledUpProducersData.Object, result.CalcResultScaledupProducers);
            Assert.AreNotEqual(mockCalcResultProjectedProducersData.Object, result.CalcResultProjectedProducers);

            mockCalcResultScaledupProducersBuilder.Verify(m => m.ConstructAsync(runContext,It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<List<L1Producer>>()), Times.Once);
            mockCalcResultProjectedProducersBuilder.Verify(m => m.Construct(runContext,It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<List<L1Producer>>()), Times.Never);
            mockCalcResultService.Verify(m => m.StoreScaledData(runContext.RunId, It.IsAny<IReadOnlyList<CalcResultScaledupProducer>>()), Times.Once);
            mockCalcResultService.Verify(m => m.StorePartialData(runContext.RunId, It.IsAny<IReadOnlyList<CalcResultPartialObligation>>()), Times.Once);
            mockProjectedProducerService.Verify(m => m.StoreProjectedProducers(It.IsAny<List<L1Producer>>()), Times.Once);
        }

        [TestMethod]
        public async Task Build_ShouldReturnCalcResult_WithScaledUpProducers_Billing()
        {
            var runContext = TestDataHelper.BillingRun2025;
            var mockCalcResultScaledUpData = new Mock<List<CalcResultScaledupProducer>>();
            var mockCalcResultPartialData = new Mock<List<CalcResultPartialObligation>>();

            mockCalcResultService.Setup(m => m.ReadScaledData(runContext.RunId))
                .ReturnsAsync(mockCalcResultScaledUpData.Object);
            mockCalcResultService.Setup(m => m.ReadPartialData(runContext.RunId))
                .ReturnsAsync(mockCalcResultPartialData.Object);

            var result = await testSubject.BuildAsync(runContext);

            Assert.IsNotNull(result);
            Assert.AreSame(mockCalcResultScaledUpData.Object.ToImmutableList(), result.CalcResultScaledupProducers.ScaledupProducers);
            Assert.AreSame(mockCalcResultPartialData.Object.ToImmutableList(), result.CalcResultPartialObligations.PartialObligations);

            mockCalcResultScaledupProducersBuilder.Verify(m => m.ConstructAsync(runContext,It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<List<L1Producer>>()), Times.Never);
            mockCalcResultPartialObligationBuilder.Verify(m => m.ConstructAsync(runContext,It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<List<L1Producer>>()), Times.Never);
            mockCalcResultProjectedProducersBuilder.Verify(m => m.Construct(runContext,It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<List<L1Producer>>()), Times.Never);
            mockProjectedProducerService.Verify(m => m.StoreProjectedProducers(It.IsAny<List<L1Producer>>()), Times.Never);
        }
}