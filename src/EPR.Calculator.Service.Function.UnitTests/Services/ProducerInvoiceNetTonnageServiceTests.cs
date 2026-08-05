using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.Service.Function.Exceptions;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;
using EPR.Calculator.API.Data.DataModels;

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
}
