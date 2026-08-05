using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder;
using EPR.Calculator.Service.Function.Builder.CancelledProducers;
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
public class ResultBuilderTests : TestsFor<ResultBuilder>
{
    private Mock<ICalcResultDetailBuilder> mockCalcResultDetailBuilder = null!;
    private Mock<ICalcResultParameterOtherCostBuilder> mockCalcResultParameterOtherCostBuilder = null!;
    private Mock<ICalcResultCancelledProducersBuilder> mockCalcResultCancelledProducersBuilder = null!;
    private Mock<ICalcResultPartialObligationBuilder> mockCalcResultPartialObligationBuilder = null!;
    private Mock<ICalcResultProjectedProducersBuilder> mockCalcResultProjectedProducersBuilder = null!;
    private Mock<ICalcResultScaledupProducersBuilder> mockCalcResultScaledupProducersBuilder = null!;
    private Mock<ICalcRunLaDisposalCostBuilder> mockCalcRunLaDisposalCostBuilder = null!;
    private Mock<ICalcResultCommsCostBuilder> mockCommsCostReportBuilder = null!;
    private Mock<ICalcResultLapcapDataBuilder> mockLapcapBuilder = null!;
    private Mock<ICalcResultLateReportingBuilder> mockLateReportingBuilder = null!;
    private Mock<ICalcResultOnePlusFourApportionmentBuilder> mockOnePlusFourApportionmentBuilder = null!;
    private Mock<IReportedProducerService> mockReportedProducerService = null!;
    private Mock<ISelfManagedConsumerWasteService> mockSelfManagedConsumerWasteService = null!;
    private Mock<IProducerFeesBuilder> mockSummaryBuilder = null!;
    private Mock<ICalcResultWriter> mockCalcResultWriter = null!;

    protected override void TestInitialize()
    {
        mockCalcResultDetailBuilder = fixture.Freeze<Mock<ICalcResultDetailBuilder>>();
        mockLapcapBuilder = fixture.Freeze<Mock<ICalcResultLapcapDataBuilder>>();
        mockSummaryBuilder = fixture.Freeze<Mock<IProducerFeesBuilder>>();
        mockCalcRunLaDisposalCostBuilder = fixture.Freeze<Mock<ICalcRunLaDisposalCostBuilder>>();
        mockCommsCostReportBuilder = fixture.Freeze<Mock<ICalcResultCommsCostBuilder>>();
        mockLateReportingBuilder = fixture.Freeze<Mock<ICalcResultLateReportingBuilder>>();
        mockCalcResultParameterOtherCostBuilder = fixture.Freeze<Mock<ICalcResultParameterOtherCostBuilder>>();
        mockOnePlusFourApportionmentBuilder = fixture.Freeze<Mock<ICalcResultOnePlusFourApportionmentBuilder>>();
        mockCalcResultScaledupProducersBuilder = fixture.Freeze<Mock<ICalcResultScaledupProducersBuilder>>();
        mockCalcResultCancelledProducersBuilder = fixture.Freeze<Mock<ICalcResultCancelledProducersBuilder>>();
        mockCalcResultPartialObligationBuilder = fixture.Freeze<Mock<ICalcResultPartialObligationBuilder>>();
        mockCalcResultProjectedProducersBuilder = fixture.Freeze<Mock<ICalcResultProjectedProducersBuilder>>();
        mockSelfManagedConsumerWasteService = fixture.Freeze<Mock<ISelfManagedConsumerWasteService>>();
        mockReportedProducerService = fixture.Freeze<Mock<IReportedProducerService>>();
        mockCalcResultWriter = fixture.Freeze<Mock<ICalcResultWriter>>();
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
        var mockProducerFees = new Mock<ProducerFees>();
        var mockCancelledProducers = new Mock<List<CalcResultCancelledProducer>>();

        mockCalcResultDetailBuilder.Setup(m => m.ConstructAsync(runContext))
            .ReturnsAsync(mockResultDetail.Object);
        mockCalcResultCancelledProducersBuilder.Setup(m => m.ConstructAsync(runContext, It.IsAny<IReadOnlyCollection<MaterialDetail>>()))
            .ReturnsAsync(mockCancelledProducers.Object);
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
            .ReturnsAsync(mockProducerFees.Object);

        mockSelfManagedConsumerWasteService.Setup(x => x.Calculate(
                It.IsAny<RunContext>(),
                It.IsAny<IEnumerable<MaterialDetail>>()))
            .ReturnsAsync(new SelfManagedConsumerWaste
            {
                CalculatorRunId = 1,
                ProducerTotals = [],
                TotalByMaterial = new Dictionary<string, SelfManagedConsumerWasteData>()
            });

        var result = await testSubject.BuildAsync(runContext, CancellationToken.None);

        Assert.IsNotNull(result);
        Assert.AreEqual(mockResultDetail.Object, result.CalcResultDetail);
        Assert.AreEqual(mockLapcapData.Object, result.CalcResultLapcapData);
        Assert.AreEqual(mockOtherParams.Object, result.CalcResultParameterOtherCost);
        Assert.AreEqual(mockOnePlusFourApp.Object, result.CalcResultOnePlusFourApportionment);
        Assert.AreEqual(mockCalcResultCommsCost.Object, result.CalcResultCommsCostReportDetail);
        Assert.AreEqual(mockCalcResultLateReportingTonnage, result.CalcResultLateReportingTonnageData);
        Assert.AreEqual(mockCalcResultLaDisposalCostData.Object, result.CalcResultLaDisposalCostData);
        Assert.AreSame(mockCalcResultScaledUpProducersData.Object, result.CalcResultScaledupProducers);
        Assert.AreSame(mockCalcResultPartialObligationsData.Object, result.CalcResultPartialObligations);
        Assert.AreNotEqual(mockCalcResultProjectedProducersData.Object, result.CalcResultProjectedProducers);
        Assert.AreEqual(mockProducerFees.Object, result.ProducerFees);
        Assert.AreSame(mockCancelledProducers.Object, result.CalcResultCancelledProducers);

        mockCalcRunLaDisposalCostBuilder.Verify(m => m.ConstructAsync(runContext, It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<CalcResultLapcapData>(), It.IsAny<CalcResultLateReportingTonnage>(), It.IsAny<SelfManagedConsumerWaste>()), Times.Once);
        mockCalcResultScaledupProducersBuilder.Verify(m => m.ConstructAsync(runContext, It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<List<L1Producer>>()), Times.Once);
        mockCalcResultProjectedProducersBuilder.Verify(m => m.Construct(runContext, It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<List<L1Producer>>()), Times.Never);
        mockCalcResultPartialObligationBuilder.Verify(m => m.ConstructAsync(runContext, It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<List<L1Producer>>()), Times.Once);

        mockCalcResultWriter.Verify(m => m.StoreLapcapData(runContext.RunId, mockLapcapData.Object, CancellationToken.None), Times.Once);
        mockCalcResultWriter.Verify(m => m.StoreLateReportingTonnage(runContext.RunId, mockCalcResultLateReportingTonnage, CancellationToken.None), Times.Once);
        mockCalcResultWriter.Verify(m => m.StoreParameterOtherCost(runContext.RunId, mockOtherParams.Object, CancellationToken.None), Times.Once);
        mockCalcResultWriter.Verify(m => m.StoreOnePlusFourApportionment(runContext.RunId, mockOnePlusFourApp.Object, CancellationToken.None), Times.Once);
        mockCalcResultWriter.Verify(m => m.StoreLaDisposalCostData(runContext.RunId, mockCalcResultLaDisposalCostData.Object, CancellationToken.None), Times.Once);
        mockCalcResultWriter.Verify(m => m.StoreCommsCost(runContext.RunId, mockCalcResultCommsCost.Object, CancellationToken.None), Times.Once);
        mockCalcResultWriter.Verify(m => m.StoreCancelledProducers(runContext.RunId, mockCancelledProducers.Object, CancellationToken.None), Times.Once);
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

        var result = await testSubject.BuildAsync(runContext, CancellationToken.None);

        Assert.IsNotNull(result);
        Assert.AreNotEqual(mockCalcResultScaledUpProducersData.Object, result.CalcResultScaledupProducers);
        Assert.AreSame(mockCalcResultProjectedProducersData.Object, result.CalcResultProjectedProducers);

        mockCalcResultScaledupProducersBuilder.Verify(m => m.ConstructAsync(runContext, It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<List<L1Producer>>()), Times.Never);
        mockCalcResultProjectedProducersBuilder.Verify(m => m.Construct(runContext, It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<List<L1Producer>>()), Times.Once);
        mockCalcResultWriter.Verify(m => m.StoreProjectedH1Data(runContext.RunId, It.IsAny<IReadOnlyList<CalcResultH1ProjectedProducer>>(), CancellationToken.None), Times.Once);
        mockCalcResultWriter.Verify(m => m.StoreProjectedH2Data(runContext.RunId, It.IsAny<IReadOnlyList<CalcResultH2ProjectedProducer>>(), CancellationToken.None), Times.Once);
        mockCalcResultWriter.Verify(m => m.StorePartialData(runContext.RunId, It.IsAny<IReadOnlyList<CalcResultPartialObligation>>(), CancellationToken.None), Times.Once);
        mockCalcResultWriter.Verify(m => m.StoreProducerMaterialPackaging(It.IsAny<List<L1Producer>>(), CancellationToken.None), Times.Once);
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

        var result = await testSubject.BuildAsync(runContext, CancellationToken.None);

        Assert.IsNotNull(result);
        Assert.AreSame(mockCalcResultScaledUpProducersData.Object, result.CalcResultScaledupProducers);
        Assert.AreNotEqual(mockCalcResultProjectedProducersData.Object, result.CalcResultProjectedProducers);

        mockCalcResultScaledupProducersBuilder.Verify(m => m.ConstructAsync(runContext,It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<List<L1Producer>>()), Times.Once);
        mockCalcResultProjectedProducersBuilder.Verify(m => m.Construct(runContext,It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<List<L1Producer>>()), Times.Never);
        mockCalcResultWriter.Verify(m => m.StoreScaledData(runContext.RunId, It.IsAny<IReadOnlyList<CalcResultScaledupProducer>>(), CancellationToken.None), Times.Once);
        mockCalcResultWriter.Verify(m => m.StorePartialData(runContext.RunId, It.IsAny<IReadOnlyList<CalcResultPartialObligation>>(), CancellationToken.None), Times.Once);
        mockCalcResultWriter.Verify(m => m.StoreProducerMaterialPackaging(It.IsAny<List<L1Producer>>(), CancellationToken.None), Times.Once);
    }

    [TestMethod]
    public async Task Build_ShouldReturnCalcResult_Write_ResultFile()
    {
        var runContext = TestDataHelper.CalculatorRun2026;
        var result = await testSubject.BuildAsync(runContext, CancellationToken.None);

        Assert.IsNotNull(result);

        mockCalcResultWriter.Verify(m => m.StoreProducerFees(runContext.RunId, result.ProducerFees, CancellationToken.None), Times.Once);
        mockCalcResultWriter.Verify(m => m.StoreSmcw(runContext.RunId, result.Smcw!, CancellationToken.None), Times.Once);
        mockCalcResultWriter.Verify(m => m.StoreModulationResult(runContext.RunId, result.CalcResultModulation!, CancellationToken.None), Times.Once);
    }
}
