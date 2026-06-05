using EPR.Calculator.Service.Function.Builder.OnePlusFourApportionment;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Builder;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class CalcResultOnePlusFourApportionmentBuilderTest : CalcResultOnePlusFourApportionmentBuilder
{
    private Fixture Fixture { get; } = new();

    [TestMethod]
    public void Construct_ShouldReturnCorrectApportionment()
    {
        // Arrange
        var runContext = TestDataHelper.CalculatorRun2024;
        var calcResult = new CalcResult
        {
            CalcResultDetail = new CalcResultDetail { RunId = runContext.RunId, RelativeYear = runContext.RelativeYear },
            CalcResultScaledupProducers = new CalcResultScaledupProducers(){
                    ScaledupProducers = ImmutableList<CalcResultScaledupProducer>.Empty
                },
            CalcResultPartialObligations = new CalcResultPartialObligations(){
                    PartialObligations = ImmutableList<CalcResultPartialObligation>.Empty,
                },
            CalcResultLapcapData = new CalcResultLapcapData
            {
                ByMaterial = new Dictionary<string, ByCountryCost>
                {
                    ["AL"] = new()
                    {
                        England = 13280.45m,
                        Wales = 210.28m,
                        Scotland = 91.00m,
                        NorthernIreland = 91.00m
                    }
                }
            },

            CalcResultParameterOtherCost = new CalcResultParameterOtherCost
            {
                LaDataPrepCharge = new ByCountryCost
                {
                    England = 115.45m,
                    Wales = 114.00m,
                    Scotland = 117.00m,
                    NorthernIreland = 19.00m
                }
            },
            CalcResultLateReportingTonnageData = Fixture.Create<CalcResultLateReportingTonnage>(),
            CalcResultProjectedProducers = new CalcResultProjectedProducers(){
                    H1ProjectedProducers = ImmutableList<CalcResultH1ProjectedProducer>.Empty,
                    H2ProjectedProducers = ImmutableList<CalcResultH2ProjectedProducer>.Empty,
                },
        };

        var resultCalc = Construct(calcResult);

        // Assert
        Assert.IsNotNull(calcResult);

        // Check disposal cost row
        var disposalRow = resultCalc.LaDisposalCost;
        Assert.AreEqual(13280.45m, disposalRow.England);
        Assert.AreEqual(210.28m, disposalRow.Wales);
        Assert.AreEqual(91.00m, disposalRow.Scotland);
        Assert.AreEqual(91, disposalRow.NorthernIreland);
        Assert.AreEqual(13672.73m, disposalRow.Total);

        // Check data preparation charge row
        var prepchargeRow = resultCalc.LADataPrepCharge;
        Assert.AreEqual(115.45m, prepchargeRow.England);
        Assert.AreEqual(114.00m, prepchargeRow.Wales);
        Assert.AreEqual(117.00m, prepchargeRow.Scotland);
        Assert.AreEqual(19.00m, prepchargeRow.NorthernIreland);
        Assert.AreEqual(365.45m, prepchargeRow.Total);

        // Check total row
        var totalRow = resultCalc.TotalOnePlusFour;
        Assert.AreEqual(13395.90m, totalRow.England);
        Assert.AreEqual(324.28m, totalRow.Wales);
        Assert.AreEqual(208.00m, totalRow.Scotland);
        Assert.AreEqual(110.00m, totalRow.NorthernIreland);
        Assert.AreEqual(14038.18m, totalRow.Total);

        // Check apportionment row
        var apportionmentRow = resultCalc.OnePlusFourApportionment;
        Assert.AreEqual(95.42476304m, Math.Round(apportionmentRow.England, 8));
        Assert.AreEqual(2.30998605m, Math.Round(apportionmentRow.Wales, 8));
        Assert.AreEqual(1.48167355m, Math.Round(apportionmentRow.Scotland, 8));
        Assert.AreEqual(0.78357736m, Math.Round(apportionmentRow.NorthernIreland, 8));
    }
}
