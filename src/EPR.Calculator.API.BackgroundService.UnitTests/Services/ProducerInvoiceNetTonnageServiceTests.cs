using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers;
using EPR.Calculator.API.BackgroundService.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.API.BackgroundService.UnitTests.Services;

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
        await Should.NotThrowAsync(testSubject.CreateProducerInvoiceNetTonnage(runContext, calcResult, CancellationToken.None));
    }
}
