using System.Text;
using EPR.Calculator.Service.Function.Exporter.CsvExporter;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.CancelledProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.CommsCost;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Detail;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.LaDisposalCost;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Lapcap;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.OtherCosts;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.PartialObligations;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ProjectedProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.ScaledupProducers;
using EPR.Calculator.Service.Function.Exporter.CsvExporter.Summary;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Exporter.CsvExporter;

[TestCategory(TestCategories.BillingRuns)]
[TestClass]
public class BillingFileExporterTests : TestsFor<BillingFileExporter>
{
    private Mock<ICalcResultCancelledProducersExporter> cancelledProducersExporter = null!;
    private Mock<ICalcResultCommsCostExporter> commsCostExporter = null!;
    private Mock<ICalcResultLaDisposalCostExporter> laDisposalCostExporter = null!;
    private Mock<ICalcResultLapcapDataExporter> lapcapDataExporter = null!;
    private Mock<ICalcResultLateReportingExporter> lateReportingExporter = null!;
    private Mock<ICalcResultOnePlusFourApportionmentExporter> onePlusFourApportionmentExporter = null!;
    private Mock<ICalcResultParameterOtherCostExporter> parameterOtherCostsExporter = null!;
    private Mock<ICalcResultPartialObligationsExporter> partialObligationsExporter = null!;
    private Mock<ICalcResultProjectedProducersExporter> projectedProducersExporter = null!;
    private Mock<ICalcResultDetailExporter> resultDetailExporter = null!;
    private Mock<ICalcResultScaledupProducersExporter> scaledUpProducersExporter = null!;
    private Mock<ICalcResultSummaryExporter> summaryExporter = null!;

    protected override void TestInitialize()
    {
        lateReportingExporter = fixture.Freeze<Mock<ICalcResultLateReportingExporter>>();
        resultDetailExporter = fixture.Freeze<Mock<ICalcResultDetailExporter>>();
        onePlusFourApportionmentExporter = fixture.Freeze<Mock<ICalcResultOnePlusFourApportionmentExporter>>();
        laDisposalCostExporter = fixture.Freeze<Mock<ICalcResultLaDisposalCostExporter>>();
        scaledUpProducersExporter = fixture.Freeze<Mock<ICalcResultScaledupProducersExporter>>();
        partialObligationsExporter = fixture.Freeze<Mock<ICalcResultPartialObligationsExporter>>();
        projectedProducersExporter = fixture.Freeze<Mock<ICalcResultProjectedProducersExporter>>();
        lapcapDataExporter = fixture.Freeze<Mock<ICalcResultLapcapDataExporter>>();
        parameterOtherCostsExporter = fixture.Freeze<Mock<ICalcResultParameterOtherCostExporter>>();
        commsCostExporter = fixture.Freeze<Mock<ICalcResultCommsCostExporter>>();
        summaryExporter = fixture.Freeze<Mock<ICalcResultSummaryExporter>>();
        cancelledProducersExporter = fixture.Freeze<Mock<ICalcResultCancelledProducersExporter>>();
    }

    [TestMethod]
    public async Task CanCallExport()
    {
        // Arrange
        var calcResult = fixture.Create<CalcResult>();
        var runContext = TestDataHelper.BillingRun2025;

        // Act
        var result = await testSubject.Export(runContext, calcResult);

        // Assert
        resultDetailExporter.Verify(mock => mock.Export(It.IsAny<CalcResultDetail>(), It.IsAny<StringBuilder>()));
        onePlusFourApportionmentExporter.Verify(mock => mock.Export(It.IsAny<CalcResultOnePlusFourApportionment>(), It.IsAny<StringBuilder>()));
        lateReportingExporter.Verify(mock => mock.Export(It.IsAny<CalcResultLateReportingTonnage>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<StringBuilder>()));
        scaledUpProducersExporter.Verify(mock => mock.Export(It.IsAny<CalcResultScaledupProducers>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<bool>(), It.IsAny<StringBuilder>()));
        partialObligationsExporter.Verify(mock => mock.Export(runContext, It.IsAny<CalcResultPartialObligations>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<StringBuilder>()));
        projectedProducersExporter.Verify(mock => mock.Export(It.IsAny<CalcResultProjectedProducers>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<StringBuilder>()), Times.Never);
        lapcapDataExporter.Verify(mock => mock.Export(It.IsAny<CalcResultLapcapData>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<StringBuilder>()));
        parameterOtherCostsExporter.Verify(mock => mock.Export(It.IsAny<CalcResultParameterOtherCost>(), It.IsAny<StringBuilder>()));
        summaryExporter.Verify(mock => mock.Export(runContext, It.IsAny<CalcResultSummary>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<StringBuilder>()));
        laDisposalCostExporter.Verify(mock => mock.Export(runContext, It.IsAny<CalcResultLaDisposalCostData>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<StringBuilder>()));
        commsCostExporter.Verify(mock => mock.Export(It.IsAny<CalcResultCommsCost>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<StringBuilder>()));
        cancelledProducersExporter.Verify(mock => mock.Export(It.IsAny<CalcResultCancelledProducersResponse>(), It.IsAny<StringBuilder>()));

        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task CanCallExport_Modulation()
    {
        // Arrange
        var calcResult = fixture.Create<CalcResult>();
        var runContext = TestDataHelper.BillingRun2026;

        // Act
        var result = await testSubject.Export(runContext, calcResult);

        // Assert
        resultDetailExporter.Verify(mock => mock.Export(It.IsAny<CalcResultDetail>(), It.IsAny<StringBuilder>()));
        onePlusFourApportionmentExporter.Verify(mock => mock.Export(It.IsAny<CalcResultOnePlusFourApportionment>(), It.IsAny<StringBuilder>()));
        lateReportingExporter.Verify(mock => mock.Export(It.IsAny<CalcResultLateReportingTonnage>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<StringBuilder>()));
        scaledUpProducersExporter.Verify(mock => mock.Export(It.IsAny<CalcResultScaledupProducers>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<bool>(), It.IsAny<StringBuilder>()), Times.Never);
        partialObligationsExporter.Verify(mock => mock.Export(runContext, It.IsAny<CalcResultPartialObligations>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<StringBuilder>()));
        projectedProducersExporter.Verify(mock => mock.Export(It.IsAny<CalcResultProjectedProducers>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<StringBuilder>()));
        lapcapDataExporter.Verify(mock => mock.Export(It.IsAny<CalcResultLapcapData>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<StringBuilder>()));
        parameterOtherCostsExporter.Verify(mock => mock.Export(It.IsAny<CalcResultParameterOtherCost>(), It.IsAny<StringBuilder>()));
        summaryExporter.Verify(mock => mock.Export(runContext, It.IsAny<CalcResultSummary>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<StringBuilder>()));
        laDisposalCostExporter.Verify(mock => mock.Export(runContext, It.IsAny<CalcResultLaDisposalCostData>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<StringBuilder>()));
        commsCostExporter.Verify(mock => mock.Export(It.IsAny<CalcResultCommsCost>(), It.IsAny<IImmutableList<MaterialDetail>>(), It.IsAny<StringBuilder>()));
        cancelledProducersExporter.Verify(mock => mock.Export(It.IsAny<CalcResultCancelledProducersResponse>(), It.IsAny<StringBuilder>()));

        Assert.IsNotNull(result);
    }
}
