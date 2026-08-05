using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder;
using EPR.Calculator.Service.Function.Builder.Modulation;
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

[TestCategory(TestCategories.BillingBuilder)]
[TestClass]
public class BillingBuilderTests : TestsFor<BillingBuilder>
{
    private Mock<ICalcResultPartialObligationBuilder> mockCalcResultPartialObligationBuilder = null!;
    private Mock<ICalcResultProjectedProducersBuilder> mockCalcResultProjectedProducersBuilder = null!;
    private Mock<ICalcResultScaledupProducersBuilder> mockCalcResultScaledupProducersBuilder = null!;
    private Mock<ISelfManagedConsumerWasteService> mockSelfManagedConsumerWasteService = null!;
    private Mock<IProducerFeesBuilder> mockSummaryBuilder = null!;
    private Mock<ICalcResultReader> mockCalcResultReader = null!;
    private Mock<ICalcResultModulationBuilder> mockModulationBuilder = null!;

    protected override void TestInitialize()
    {
        mockSummaryBuilder = fixture.Freeze<Mock<IProducerFeesBuilder>>();
        mockCalcResultScaledupProducersBuilder = fixture.Freeze<Mock<ICalcResultScaledupProducersBuilder>>();
        mockCalcResultPartialObligationBuilder = fixture.Freeze<Mock<ICalcResultPartialObligationBuilder>>();
        mockCalcResultProjectedProducersBuilder = fixture.Freeze<Mock<ICalcResultProjectedProducersBuilder>>();
        mockSelfManagedConsumerWasteService = fixture.Freeze<Mock<ISelfManagedConsumerWasteService>>();
        mockCalcResultReader = fixture.Freeze<Mock<ICalcResultReader>>();
        mockModulationBuilder = fixture.Freeze<Mock<ICalcResultModulationBuilder>>();
    }

    [TestMethod]
    public async Task Build_ShouldReturnCalcResult()
    {
        var runContext = TestDataHelper.BillingRun2026;
        var mockCalcResultProjectedProducersH1Data = new Mock<List<CalcResultH1ProjectedProducer>>();
        var mockCalcResultProjectedProducersH2Data = new Mock<List<CalcResultH2ProjectedProducer>>();
        var mockCalcResultPartialData = new Mock<List<CalcResultPartialObligation>>();
        var mockSummary = new Mock<ProducerFees>();
        var mockSmcw = new Mock<SelfManagedConsumerWaste>();
        var mockMod = new Mock<ModulationResult>();
        var mockLapcapData = new Mock<CalcResultLapcapData>();
        var mockLateReportingTonnage = new Mock<CalcResultLateReportingTonnage>();
        var mockParameterOtherCost = new Mock<CalcResultParameterOtherCost>();
        var mockOnePlusFourApportionment = new Mock<CalcResultOnePlusFourApportionment>();
        var mockLaDisposalCostData = new Mock<CalcResultLaDisposalCostData>();
        var mockCommsCost = new Mock<CalcResultCommsCost>();
        var mockCancelledProducers = new Mock<List<CalcResultCancelledProducer>>();

        mockCalcResultReader.Setup(m => m.ReadCancelledProducers(runContext.RunId, CancellationToken.None))
            .ReturnsAsync(mockCancelledProducers.Object);
        mockCalcResultReader.Setup(m => m.ReadH1ProjectedData(runContext.RunId, CancellationToken.None))
            .ReturnsAsync(mockCalcResultProjectedProducersH1Data.Object);
        mockCalcResultReader.Setup(m => m.ReadH2ProjectedData(runContext.RunId, CancellationToken.None))
            .ReturnsAsync(mockCalcResultProjectedProducersH2Data.Object);
        mockCalcResultReader.Setup(m => m.ReadPartialData(runContext.RunId, CancellationToken.None))
            .ReturnsAsync(mockCalcResultPartialData.Object);
        mockCalcResultReader.Setup(m => m.ReadProducerFees(runContext.RunId, CancellationToken.None))
            .ReturnsAsync(mockSummary.Object);
        mockCalcResultReader.Setup(m => m.ReadSmcw(runContext.RunId, CancellationToken.None))
            .ReturnsAsync(mockSmcw.Object);
        mockCalcResultReader.Setup(m => m.ReadModulationResult(runContext.RunId, CancellationToken.None))
            .ReturnsAsync(mockMod.Object);
        mockCalcResultReader.Setup(m => m.ReadLapcapData(runContext.RunId, CancellationToken.None))
            .ReturnsAsync(mockLapcapData.Object);
        mockCalcResultReader.Setup(m => m.ReadLateReportingTonnage(runContext.RunId, CancellationToken.None))
            .ReturnsAsync(mockLateReportingTonnage.Object);
        mockCalcResultReader.Setup(m => m.ReadParameterOtherCost(runContext.RunId, CancellationToken.None))
            .ReturnsAsync(mockParameterOtherCost.Object);
        mockCalcResultReader.Setup(m => m.ReadOnePlusFourApportionment(runContext.RunId, CancellationToken.None))
            .ReturnsAsync(mockOnePlusFourApportionment.Object);
        mockCalcResultReader.Setup(m => m.ReadLaDisposalCostData(runContext.RunId, CancellationToken.None))
            .ReturnsAsync(mockLaDisposalCostData.Object);
        mockCalcResultReader.Setup(m => m.ReadCommsCost(runContext.RunId, CancellationToken.None))
            .ReturnsAsync(mockCommsCost.Object);

        var result = await testSubject.BuildAsync(runContext, CancellationToken.None);

        Assert.IsNotNull(result);
        Assert.AreSame(mockSmcw.Object, result.Smcw);
        Assert.AreSame(mockMod.Object, result.CalcResultModulation);
        Assert.AreSame(mockSummary.Object, result.ProducerFees);
        Assert.AreSame(mockLapcapData.Object, result.CalcResultLapcapData);
        Assert.AreSame(mockLateReportingTonnage.Object, result.CalcResultLateReportingTonnageData);
        Assert.AreSame(mockParameterOtherCost.Object, result.CalcResultParameterOtherCost);
        Assert.AreSame(mockOnePlusFourApportionment.Object, result.CalcResultOnePlusFourApportionment);
        Assert.AreSame(mockLaDisposalCostData.Object, result.CalcResultLaDisposalCostData);
        Assert.AreSame(mockCommsCost.Object, result.CalcResultCommsCostReportDetail);
        CollectionAssert.AreEqual(mockCancelledProducers.Object, result.CalcResultCancelledProducers.ToList()); // List should not be the same - filtered by rejected

        mockSelfManagedConsumerWasteService.Verify(m => m.Calculate(runContext, It.IsAny<IImmutableList<MaterialDetail>>()), Times.Never);
        mockModulationBuilder.Verify(m => m.ConstructAsync(It.IsAny<RunContext>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<CalcResultLaDisposalCostData>(), It.IsAny<SelfManagedConsumerWaste>()), Times.Never);
        mockSummaryBuilder.Verify(m => m.ConstructAsync(runContext, It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<CalcResult>(), It.IsAny<SelfManagedConsumerWaste>()), Times.Never);
    }

    [TestMethod]
    public async Task Build_ShouldReturnCalcResult_WithProjectedProducers()
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

        mockCalcResultReader.Setup(m => m.ReadH1ProjectedData(runContext.RunId, CancellationToken.None))
            .ReturnsAsync(mockCalcResultProjectedProducersH1Data.Object);
        mockCalcResultReader.Setup(m => m.ReadH2ProjectedData(runContext.RunId, CancellationToken.None))
            .ReturnsAsync(mockCalcResultProjectedProducersH2Data.Object);
        mockCalcResultReader.Setup(m => m.ReadPartialData(runContext.RunId, CancellationToken.None))
            .ReturnsAsync(mockCalcResultPartialData.Object);

        var result = await testSubject.BuildAsync(runContext, CancellationToken.None);


        Assert.IsNotNull(result);
        Assert.AreSame(mockCalcResultProjectedProducersH1Data.Object.ToImmutableList(), result.CalcResultProjectedProducers.H1ProjectedProducers);
        Assert.AreSame(mockCalcResultProjectedProducersH2Data.Object.ToImmutableList(), result.CalcResultProjectedProducers.H2ProjectedProducers);
        Assert.AreSame(mockCalcResultPartialData.Object.ToImmutableList(), result.CalcResultPartialObligations.PartialObligations);

        mockCalcResultScaledupProducersBuilder.Verify(m => m.ConstructAsync(runContext, It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<List<L1Producer>>()), Times.Never);
        mockCalcResultProjectedProducersBuilder.Verify(m => m.Construct(runContext, It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<List<L1Producer>>()), Times.Never);
        mockCalcResultPartialObligationBuilder.Verify(m => m.ConstructAsync(runContext,It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<List<L1Producer>>()), Times.Never);
    }

    [TestMethod]
    public async Task Build_ShouldReturnCalcResult_WithScaledUpProducers()
    {
        var runContext = TestDataHelper.BillingRun2025;
        var mockCalcResultScaledUpData = new Mock<List<CalcResultScaledupProducer>>();
        var mockCalcResultPartialData = new Mock<List<CalcResultPartialObligation>>();

        mockCalcResultReader.Setup(m => m.ReadScaledData(runContext.RunId, CancellationToken.None))
            .ReturnsAsync(mockCalcResultScaledUpData.Object);
        mockCalcResultReader.Setup(m => m.ReadPartialData(runContext.RunId, CancellationToken.None))
            .ReturnsAsync(mockCalcResultPartialData.Object);

        var result = await testSubject.BuildAsync(runContext, CancellationToken.None);

        Assert.IsNotNull(result);
        Assert.AreSame(mockCalcResultScaledUpData.Object.ToImmutableList(), result.CalcResultScaledupProducers.ScaledupProducers);
        Assert.AreSame(mockCalcResultPartialData.Object.ToImmutableList(), result.CalcResultPartialObligations.PartialObligations);

        mockCalcResultScaledupProducersBuilder.Verify(m => m.ConstructAsync(runContext,It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<List<L1Producer>>()), Times.Never);
        mockCalcResultPartialObligationBuilder.Verify(m => m.ConstructAsync(runContext,It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<List<L1Producer>>()), Times.Never);
        mockCalcResultProjectedProducersBuilder.Verify(m => m.Construct(runContext,It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<List<L1Producer>>()), Times.Never);
    }
}
