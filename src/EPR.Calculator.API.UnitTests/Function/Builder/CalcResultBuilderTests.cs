using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
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
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.Service.Function.UnitTests.Builder
{
    [TestClass]
    public class CalcResultBuilderTests
    {
        private IFixture fixture = null!;
        private Mock<ICalcResultDetailBuilder> mockCalcResultDetailBuilder = null!;
        private Mock<ICalcResultLapcapDataBuilder> mockLapcapBuilder = null!;
        private Mock<ICalcResultLateReportingBuilder> mockLateReportingBuilder = null!;
        private Mock<ICalcRunLaDisposalCostBuilder> mockCalcRunLaDisposalCostBuilder = null!;
        private Mock<ICalcResultCommsCostBuilder> mockCommsCostReportBuilder = null!;
        private Mock<ICalcResultSummaryBuilder> mockSummaryBuilder = null!;
        private CalcResultBuilder calcResultBuilder = null!;
        private Mock<ICalcResultParameterOtherCostBuilder> mockCalcResultParameterOtherCostBuilder = null!;
        private Mock<ICalcResultOnePlusFourApportionmentBuilder> mockOnePlusFourApportionmentBuilder = null!;
        private Mock<ICalcResultScaledupProducersBuilder> mockCalcResultScaledupProducersBuilder = null!;
        private Mock<ICalcResultPartialObligationBuilder> mockCalcResultPartialObligationBuilder = null!;
        private Mock<ICalcResultProjectedProducersBuilder> mockCalcResultProjectedProducersBuilder = null!;
        private Mock<IProjectedProducersService> mockProjectedProducerService = null!;
        private Mock<ISelfManagedConsumerWasteService> mockSelfManagedConsumerWasteService = null!;
        private Mock<IReportedProducerService> mockReportedProducerService = null!;

        [TestInitialize]
        public void Init()
        {
            fixture = TestFixtures.New();
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

            calcResultBuilder = fixture.Create<CalcResultBuilder>();
        }


        [TestMethod]
        public async Task Build_ShouldReturnCalcResult()
        {
            var resultsRequestDto = new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2025)};
            var applyModulation = false;
            var mockProducers1 = new List<L1Producer>
            {
                new L1Producer(1, [new ProducerDetail { ProducerId = 1, SubsidiaryId = null }])
            };
            var mockProducers2 = new List<L1Producer>
            {
                new L1Producer(2, [new ProducerDetail { ProducerId = 2, SubsidiaryId = null }])
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
            var mockMaterials = ImmutableList<MaterialDetail>.Empty;

            mockCalcResultDetailBuilder.Setup(m => m.ConstructAsync(resultsRequestDto))
                .ReturnsAsync(mockResultDetail.Object);
            mockLapcapBuilder.Setup(m => m.ConstructAsync(It.IsAny<IEnumerable<MaterialDetail>>(), resultsRequestDto))
                .ReturnsAsync(mockLapcapData.Object);
            mockCalcResultParameterOtherCostBuilder.Setup(m => m.ConstructAsync(resultsRequestDto))
                .ReturnsAsync(mockOtherParams.Object);
            mockOnePlusFourApportionmentBuilder.Setup(m => m.Construct(resultsRequestDto, It.IsAny<CalcResult>()))
                .Returns(mockOnePlusFourApp.Object);
            mockCommsCostReportBuilder
                .Setup(m => m.ConstructAsync(It.IsAny<IImmutableList<MaterialDetail>>(), resultsRequestDto, It.IsAny<CalcResultOnePlusFourApportionment>(), It.IsAny<CalcResultLateReportingTonnage>()))
                .ReturnsAsync(mockCalcResultCommsCost.Object);
            mockLateReportingBuilder.Setup(m => m.ConstructAsync(It.IsAny<IImmutableList<MaterialDetail>>(), resultsRequestDto))
                .ReturnsAsync(mockCalcResultLateReportingTonnage);
            mockCalcRunLaDisposalCostBuilder.Setup(m => m.ConstructAsync(resultsRequestDto, It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<CalcResultLapcapData>(), It.IsAny<CalcResultLateReportingTonnage>(), It.IsAny<SelfManagedConsumerWaste>(), It.IsAny<bool>()))
                .ReturnsAsync(mockCalcResultLaDisposalCostData.Object);
            mockReportedProducerService.Setup(m => m.GetProducers(resultsRequestDto.RunId))
                .ReturnsAsync(mockProducers1);
            mockCalcResultScaledupProducersBuilder.Setup(m => m.ConstructAsync(It.IsAny<IImmutableList<MaterialDetail>>(), mockProducers1, resultsRequestDto))
                .ReturnsAsync((mockProducers2, mockCalcResultScaledUpProducersData.Object));
            mockCalcResultPartialObligationBuilder.Setup(m => m.ConstructAsync(It.IsAny<IImmutableList<MaterialDetail>>(), mockProducers2, resultsRequestDto, applyModulation))
                .ReturnsAsync((mockProducers2, mockCalcResultPartialObligationsData.Object));
            mockSummaryBuilder.Setup(x => x.ConstructAsync(It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<int>(), It.IsAny<RelativeYear>(), It.IsAny<bool>(), It.IsAny<CalcResult>(), It.IsAny<SelfManagedConsumerWaste>()))
                .ReturnsAsync(mockCalcResultSummary.Object);

            mockSelfManagedConsumerWasteService.Setup(x => x.Calculate(
                    It.IsAny<CalcResultsRequestDto>(),
                    It.IsAny<IEnumerable<MaterialDetail>>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(new SelfManagedConsumerWaste
                {
                    ProducerTotals = [],
                    OverallTotalPerMaterials = []
                });

            var result = await calcResultBuilder.BuildAsync(resultsRequestDto, mockMaterials);

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

            mockCalcRunLaDisposalCostBuilder.Verify(m => m.ConstructAsync(resultsRequestDto, It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<CalcResultLapcapData>(), It.IsAny<CalcResultLateReportingTonnage>(), It.IsAny<SelfManagedConsumerWaste>(), It.IsAny<bool>()), Times.Once);
            mockCalcResultScaledupProducersBuilder.Verify(m => m.ConstructAsync(It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<List<L1Producer>>(), resultsRequestDto), Times.Once);
            mockCalcResultProjectedProducersBuilder.Verify(m => m.Construct(It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<List<L1Producer>>(), resultsRequestDto), Times.Never);
            mockCalcResultPartialObligationBuilder.Verify(m => m.ConstructAsync(It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<List<L1Producer>>(), resultsRequestDto, It.IsAny<bool>()), Times.Once);
        }

        [TestMethod]
        public async Task Build_ShouldReturnCalcResult_WithProjectedProducers()
        {
            var resultsRequestDto = new CalcResultsRequestDto() { RunId = 1, RelativeYear = new RelativeYear(2026), IsBillingFile = false };
            var applyModulation = true;
            var mockProducers1 = new List<L1Producer>
            {
                new L1Producer(1, [new ProducerDetail { ProducerId = 1, SubsidiaryId = null }])
            };
            var mockProducers2 = new List<L1Producer>
            {
                new L1Producer(2, [new ProducerDetail { ProducerId = 2, SubsidiaryId = null }])
            };
            var mockMaterials = ImmutableList<MaterialDetail>.Empty;

            var mockCalcResultScaledUpProducersData = fixture.Freeze<Mock<CalcResultScaledupProducers>>();
            var mockCalcResultProjectedProducersData = fixture.Freeze<Mock<CalcResultProjectedProducers>>();
            var mockCalcResultPartialObligationsData = fixture.Freeze<Mock<CalcResultPartialObligations>>();

            mockReportedProducerService.Setup(m => m.GetProducers(resultsRequestDto.RunId))
                .ReturnsAsync(mockProducers1);
            mockCalcResultProjectedProducersBuilder.Setup(m => m.Construct(It.IsAny<IImmutableList<MaterialDetail>>(), mockProducers1, resultsRequestDto))
                .Returns((mockProducers2, mockCalcResultProjectedProducersData.Object));
            mockCalcResultPartialObligationBuilder.Setup(m => m.ConstructAsync(It.IsAny<IImmutableList<MaterialDetail>>(), mockProducers2, resultsRequestDto, applyModulation))
                .ReturnsAsync((mockProducers2, mockCalcResultPartialObligationsData.Object));

            var result = await calcResultBuilder.BuildAsync(resultsRequestDto, mockMaterials);

            Assert.IsNotNull(result);
            Assert.AreNotEqual(mockCalcResultScaledUpProducersData.Object, result.CalcResultScaledupProducers);
            Assert.AreSame(mockCalcResultProjectedProducersData.Object, result.CalcResultProjectedProducers);

            mockCalcResultScaledupProducersBuilder.Verify(m => m.ConstructAsync(It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<List<L1Producer>>(), resultsRequestDto), Times.Never);
            mockCalcResultProjectedProducersBuilder.Verify(m => m.Construct(It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<List<L1Producer>>(), resultsRequestDto), Times.Once);
            mockProjectedProducerService.Verify(m => m.StoreProjectedProducers(1, It.IsAny<List<L1Producer>>()), Times.Once);
        }

        [TestMethod]
        public async Task Build_ShouldReturnCalcResult_WithProjectedProducers_Billing()
        {
            var resultsRequestDto = new CalcResultsRequestDto() { RunId = 1, RelativeYear = new RelativeYear(2026), IsBillingFile = true };
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

            mockReportedProducerService.Setup(m => m.GetProducers(resultsRequestDto.RunId))
                .ReturnsAsync(mockProducers1);

            mockCalcResultProjectedProducersBuilder.Setup(m => m.Construct(It.IsAny<IImmutableList<MaterialDetail>>(), mockProducers1, resultsRequestDto))
                .Returns((mockProducers2, mockCalcResultProjectedProducersData.Object));

            var result = await calcResultBuilder.BuildAsync(resultsRequestDto, mockMaterials);

            Assert.IsNotNull(result);
            Assert.AreSame(mockCalcResultProjectedProducersData.Object, result.CalcResultProjectedProducers);
            Assert.AreNotEqual(mockCalcResultScaledUpProducersData.Object, result.CalcResultScaledupProducers);

            mockCalcResultScaledupProducersBuilder.Verify(m => m.ConstructAsync(It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<List<L1Producer>>(), resultsRequestDto), Times.Never);
            mockCalcResultProjectedProducersBuilder.Verify(m => m.Construct(It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<List<L1Producer>>(), resultsRequestDto), Times.Once);
            mockProjectedProducerService.Verify(m => m.StoreProjectedProducers(1, It.IsAny<List<L1Producer>>()), Times.Never);
        }
    }
}
