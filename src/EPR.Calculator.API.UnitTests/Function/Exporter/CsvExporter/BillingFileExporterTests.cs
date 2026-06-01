using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.CancelledProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.CommsCost;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Detail;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.LaDisposalCost;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Lapcap;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Modulation;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.OtherCosts;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.PartialObligations;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ProjectedProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.RejectedProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ScaledupProducers;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter
{
    [TestClass]
    public class BillingFileExporterTests
    {
        private BillingFileExporter testClass;
        private Mock<ICalcResultLateReportingExporter> _lateReportingExporter;
        private Mock<ICalcResultDetailExporter> _resultDetailexporter;
        private Mock<ICalcResultOnePlusFourApportionmentExporter> _onePlusFourApportionmentExporter;
        private Mock<ICalcResultLaDisposalCostExporter> _laDisposalCostExporter;
        private Mock<ICalcResultModulationExporter> _modulationExporter;
        private Mock<ICalcResultScaledupProducersExporter> _calcResultScaledupProducersExporter;
        private Mock<ICalcResultPartialObligationsExporter> _calcResultPartialObligationsExporter;
        private Mock<ICalcResultProjectedProducersExporter> _calcResultProjectedProducersExporter;
        private Mock<ICalcResultLapcapDataExporter> _lapcapDataExporter;
        private Mock<ICalcResultParameterOtherCostExporter> _parameterOtherCosts;
        private Mock<ICalcResultCommsCostExporter> _commsCostExporter;
        private Mock<ICalcResultSummaryExporter> _calcResultSummaryExporter;
        private Mock<ICalcResultCancelledProducersExporter> _calcResultCancelledProducersExporter;
        private Mock<ICalcResultRejectedProducersExporter> _calcResultRejectedProducersExporter;

        public BillingFileExporterTests()
        {
            _lateReportingExporter = new Mock<ICalcResultLateReportingExporter>();
            _resultDetailexporter = new Mock<ICalcResultDetailExporter>();
            _onePlusFourApportionmentExporter = new Mock<ICalcResultOnePlusFourApportionmentExporter>();
            _laDisposalCostExporter = new Mock<ICalcResultLaDisposalCostExporter>();
            _modulationExporter = new Mock<ICalcResultModulationExporter>();
            _calcResultScaledupProducersExporter = new Mock<ICalcResultScaledupProducersExporter>();
            _calcResultPartialObligationsExporter = new Mock<ICalcResultPartialObligationsExporter>();
            _calcResultProjectedProducersExporter = new Mock<ICalcResultProjectedProducersExporter>();
            _lapcapDataExporter = new Mock<ICalcResultLapcapDataExporter>();
            _parameterOtherCosts = new Mock<ICalcResultParameterOtherCostExporter>();
            _commsCostExporter = new Mock<ICalcResultCommsCostExporter>();
            _calcResultSummaryExporter = new Mock<ICalcResultSummaryExporter>();
            _calcResultCancelledProducersExporter = new Mock<ICalcResultCancelledProducersExporter>();
            _calcResultRejectedProducersExporter = new Mock<ICalcResultRejectedProducersExporter>();
            testClass = new BillingFileExporter(
                _lateReportingExporter.Object,
                _resultDetailexporter.Object,
                _onePlusFourApportionmentExporter.Object,
                _laDisposalCostExporter.Object,
                _modulationExporter.Object,
                _calcResultScaledupProducersExporter.Object,
                _calcResultPartialObligationsExporter.Object,
                _calcResultProjectedProducersExporter.Object,
                _lapcapDataExporter.Object,
                _parameterOtherCosts.Object,
                _commsCostExporter.Object,
                _calcResultSummaryExporter.Object,
                _calcResultCancelledProducersExporter.Object,
                _calcResultRejectedProducersExporter.Object);
        }

        [TestMethod]
        public void CanConstruct()
        {
            // Act
            var instance = new BillingFileExporter(
                _lateReportingExporter.Object,
                _resultDetailexporter.Object,
                _onePlusFourApportionmentExporter.Object,
                _laDisposalCostExporter.Object,
                _modulationExporter.Object,
                _calcResultScaledupProducersExporter.Object,
                _calcResultPartialObligationsExporter.Object,
                _calcResultProjectedProducersExporter.Object,
                _lapcapDataExporter.Object,
                _parameterOtherCosts.Object,
                _commsCostExporter.Object,
                _calcResultSummaryExporter.Object,
                _calcResultCancelledProducersExporter.Object,
                _calcResultRejectedProducersExporter.Object);

            // Assert
            Assert.IsNotNull(instance);
        }

        [TestMethod]
        public void CanCallExport()
        {
            // Arrange
            var fixture = new Fixture().Customize(new ImmutableCollectionsCustomization());
            var results = fixture.Create<CalcResult>();
            var materials = fixture.Create<IImmutableList<MaterialDetail>>();
            results.ApplyModulation = false;
            var acceptedProducerIds = results.CalcResultScaledupProducers.ScaledupProducers!.Select(t => t.ProducerId).Take(1).ToImmutableHashSet() ?? [];

            _resultDetailexporter.Setup(mock => mock.Export(It.IsAny<CalcResultDetail>(), It.IsAny<StringBuilder>())).Verifiable();
            _onePlusFourApportionmentExporter.Setup(mock => mock.Export(It.IsAny<CalcResultOnePlusFourApportionment>(), It.IsAny<StringBuilder>())).Verifiable();
            _lateReportingExporter.Setup(mock => mock.Export(It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<CalcResultLateReportingTonnage>(), It.IsAny<StringBuilder>()));
            _calcResultScaledupProducersExporter.Setup(mock => mock.Export(It.IsAny<CalcResultScaledupProducers>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<bool>(), It.IsAny<StringBuilder>())).Verifiable();
            _calcResultPartialObligationsExporter.Setup(mock => mock.Export(It.IsAny<CalcResultPartialObligations>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<StringBuilder>(), It.IsAny<bool>())).Verifiable();
            _lapcapDataExporter.Setup(mock => mock.Export(It.IsAny<CalcResultLapcapData>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<StringBuilder>())).Verifiable();
            _parameterOtherCosts.Setup(mock => mock.Export(It.IsAny<CalcResultParameterOtherCost>(), It.IsAny<StringBuilder>())).Verifiable();
            _calcResultSummaryExporter.Setup(mock => mock.Export(It.IsAny<CalcResultSummary>(), It.IsAny<StringBuilder>(), It.IsAny<bool>())).Verifiable();
            _laDisposalCostExporter.Setup(mock => mock.Export(It.IsAny<bool>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<CalcResultLaDisposalCostData>(), It.IsAny<StringBuilder>())).Verifiable();
            _commsCostExporter.Setup(mock => mock.Export(It.IsAny<CalcResultCommsCost>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<StringBuilder>())).Verifiable();
            _calcResultCancelledProducersExporter.Setup(mock => mock.Export(It.IsAny<CalcResultCancelledProducersResponse>(), It.IsAny<StringBuilder>())).Verifiable();

            // Act
            var result = testClass.Export(results, materials, acceptedProducerIds);

            // Assert
            _resultDetailexporter.Verify(mock => mock.Export(It.IsAny<CalcResultDetail>(), It.IsAny<StringBuilder>()));
            _onePlusFourApportionmentExporter.Verify(mock => mock.Export(It.IsAny<CalcResultOnePlusFourApportionment>(), It.IsAny<StringBuilder>()));
            _lateReportingExporter.Verify(mock => mock.Export(It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<CalcResultLateReportingTonnage>(), It.IsAny<StringBuilder>()));
            _calcResultScaledupProducersExporter.Verify(mock => mock.Export(It.IsAny<CalcResultScaledupProducers>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<bool>(), It.IsAny<StringBuilder>()));
            _calcResultPartialObligationsExporter.Verify(mock => mock.Export(It.IsAny<CalcResultPartialObligations>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<StringBuilder>(), It.IsAny<bool>()));
            _calcResultProjectedProducersExporter.Verify(mock => mock.Export(It.IsAny<CalcResultProjectedProducers>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<StringBuilder>()), Times.Never);
            _lapcapDataExporter.Verify(mock => mock.Export(It.IsAny<CalcResultLapcapData>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<StringBuilder>()));
            _parameterOtherCosts.Verify(mock => mock.Export(It.IsAny<CalcResultParameterOtherCost>(), It.IsAny<StringBuilder>()));
            _calcResultSummaryExporter.Verify(mock => mock.Export(It.IsAny<CalcResultSummary>(), It.IsAny<StringBuilder>(), It.IsAny<bool>()));
            _laDisposalCostExporter.Verify(mock => mock.Export(It.IsAny<bool>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<CalcResultLaDisposalCostData>(), It.IsAny<StringBuilder>()));
            _commsCostExporter.Verify(mock => mock.Export(It.IsAny<CalcResultCommsCost>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<StringBuilder>()));
            _calcResultCancelledProducersExporter.Verify(mock => mock.Export(It.IsAny<CalcResultCancelledProducersResponse>(), It.IsAny<StringBuilder>()));

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void CanCallExport_Modulation()
        {
            // Arrange
            var fixture = new Fixture().Customize(new ImmutableCollectionsCustomization());
            var results = fixture.Create<CalcResult>();
            var materials = fixture.Create<IImmutableList<MaterialDetail>>();
            results.ApplyModulation = true;
            var acceptedProducerIds = results.CalcResultProjectedProducers.H2ProjectedProducers!.Select(t => t.ProducerId).Take(1).ToImmutableHashSet();

            _resultDetailexporter.Setup(mock => mock.Export(It.IsAny<CalcResultDetail>(), It.IsAny<StringBuilder>())).Verifiable();
            _onePlusFourApportionmentExporter.Setup(mock => mock.Export(It.IsAny<CalcResultOnePlusFourApportionment>(), It.IsAny<StringBuilder>())).Verifiable();
            _lateReportingExporter.Setup(mock => mock.Export(It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<CalcResultLateReportingTonnage>(), It.IsAny<StringBuilder>()));
            _calcResultScaledupProducersExporter.Setup(mock => mock.Export(It.IsAny<CalcResultScaledupProducers>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<bool>(), It.IsAny<StringBuilder>())).Verifiable();
            _calcResultPartialObligationsExporter.Setup(mock => mock.Export(It.IsAny<CalcResultPartialObligations>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<StringBuilder>(), It.IsAny<bool>())).Verifiable();
            _calcResultProjectedProducersExporter.Setup(mock => mock.Export(It.IsAny<CalcResultProjectedProducers>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<StringBuilder>())).Verifiable();
            _lapcapDataExporter.Setup(mock => mock.Export(It.IsAny<CalcResultLapcapData>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<StringBuilder>())).Verifiable();
            _parameterOtherCosts.Setup(mock => mock.Export(It.IsAny<CalcResultParameterOtherCost>(), It.IsAny<StringBuilder>())).Verifiable();
            _calcResultSummaryExporter.Setup(mock => mock.Export(It.IsAny<CalcResultSummary>(), It.IsAny<StringBuilder>(), It.IsAny<bool>())).Verifiable();
            _laDisposalCostExporter.Setup(mock => mock.Export(It.IsAny<bool>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<CalcResultLaDisposalCostData>(), It.IsAny<StringBuilder>())).Verifiable();
            _commsCostExporter.Setup(mock => mock.Export(It.IsAny<CalcResultCommsCost>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<StringBuilder>())).Verifiable();
            _calcResultCancelledProducersExporter.Setup(mock => mock.Export(It.IsAny<CalcResultCancelledProducersResponse>(), It.IsAny<StringBuilder>())).Verifiable();

            // Act
            var result = testClass.Export(results, materials, acceptedProducerIds);

            // Assert
            _resultDetailexporter.Verify(mock => mock.Export(It.IsAny<CalcResultDetail>(), It.IsAny<StringBuilder>()));
            _onePlusFourApportionmentExporter.Verify(mock => mock.Export(It.IsAny<CalcResultOnePlusFourApportionment>(), It.IsAny<StringBuilder>()));
            _lateReportingExporter.Verify(mock => mock.Export(It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<CalcResultLateReportingTonnage>(), It.IsAny<StringBuilder>()));
            _calcResultScaledupProducersExporter.Verify(mock => mock.Export(It.IsAny<CalcResultScaledupProducers>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<bool>(), It.IsAny<StringBuilder>()), Times.Never);
            _calcResultPartialObligationsExporter.Verify(mock => mock.Export(It.IsAny<CalcResultPartialObligations>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<StringBuilder>(), It.IsAny<bool>()));
            _calcResultProjectedProducersExporter.Verify(mock => mock.Export(It.IsAny<CalcResultProjectedProducers>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<StringBuilder>()));
            _lapcapDataExporter.Verify(mock => mock.Export(It.IsAny<CalcResultLapcapData>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<StringBuilder>()));
            _parameterOtherCosts.Verify(mock => mock.Export(It.IsAny<CalcResultParameterOtherCost>(), It.IsAny<StringBuilder>()));
            _calcResultSummaryExporter.Verify(mock => mock.Export(It.IsAny<CalcResultSummary>(), It.IsAny<StringBuilder>(), It.IsAny<bool>()));
            _laDisposalCostExporter.Verify(mock => mock.Export(It.IsAny<bool>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<CalcResultLaDisposalCostData>(), It.IsAny<StringBuilder>()));
            _commsCostExporter.Verify(mock => mock.Export(It.IsAny<CalcResultCommsCost>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<StringBuilder>()));
            _calcResultCancelledProducersExporter.Verify(mock => mock.Export(It.IsAny<CalcResultCancelledProducersResponse>(), It.IsAny<StringBuilder>()));

            Assert.IsNotNull(result);
        }


        [TestMethod]
        public void CanCallGetScaledUpProducersForExport()
        {
            // Arrange
            var fixture = new Fixture().Customize(new ImmutableCollectionsCustomization());
            var producers = fixture.Create<CalcResultScaledupProducers>();
            var acceptedProducerIds = producers.ScaledupProducers!.Select(t => t.ProducerId).Take(2).ToImmutableHashSet();

            // Act
            var result = testClass.GetScaledUpProducersForExport(producers, acceptedProducerIds);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.ScaledupProducers?.Count() > 0);
        }
    }
}
