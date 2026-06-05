using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Exceptions;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Services;

[TestClass]
public class ProducerInvoiceNetTonnageServiceTests : TestsFor<ProducerInvoiceNetTonnageService>
{
    [TestMethod]
    public async Task Should_create_net_tonnages()
    {
        // Arrange
        var runContext = TestDataHelper.CalculatorRun2025;
        var calcResult = TestDataHelper.GetCalcResult();

        // Act & Assert
        await Should.NotThrowAsync(testSubject.CreateProducerInvoiceNetTonnage(runContext, calcResult));
    }

    [TestMethod]
    public async Task Should_throw_when_no_producers()
    {
        // Arrange
        var runContext = TestDataHelper.CalculatorRun2025;
        var calcResult = new CalcResult
        {
            CalcResultScaledupProducers = new CalcResultScaledupProducers(){
                ScaledupProducers = ImmutableList<CalcResultScaledupProducer>.Empty
            },
            CalcResultPartialObligations = new CalcResultPartialObligations(){
                PartialObligations = ImmutableList<CalcResultPartialObligation>.Empty
            },
            CalcResultDetail = new CalcResultDetail
            {
                RunId = 4,
                RunDate = DateTime.UtcNow,
                RunName = "RunName",
                RelativeYear = new RelativeYear(2024)
            },
            CalcResultLapcapData = new CalcResultLapcapData
            {
                ByMaterial = []
            },
            CalcResultParameterOtherCost = new CalcResultParameterOtherCost
            {
                SchemeSetupCost = new ByCountryCost
                {
                    England = 0,
                    Wales = 0,
                    Scotland = 0,
                    NorthernIreland = 0
                }
            },
            CalcResultLateReportingTonnageData = new() { ByMaterial = [] },
            CalcResultProjectedProducers = new CalcResultProjectedProducers(){
                H1ProjectedProducers = ImmutableList<CalcResultH1ProjectedProducer>.Empty,
                H2ProjectedProducers = ImmutableList<CalcResultH2ProjectedProducer>.Empty,
            },
        };

        // Act & Assert
        await Should.ThrowAsync<RunProcessingException>(testSubject.CreateProducerInvoiceNetTonnage(runContext, calcResult));
    }
}
