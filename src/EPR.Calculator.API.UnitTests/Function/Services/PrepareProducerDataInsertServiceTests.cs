using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.Service.Function.UnitTests.Services;

[TestClass]
public class PrepareProducerDataInsertServiceTests
{
    private IFixture fixture = null!;
    private Mock<IBillingInstructionService> billingInstructionService = null!;
    private Mock<IProducerInvoiceNetTonnageService> producerInvoiceNetTonnageService = null!;
    private PrepareProducerDataInsertService sut = null!;

    [TestInitialize]
    public void Init()
    {
        fixture = TestFixtures.New();
        billingInstructionService = fixture.Freeze<Mock<IBillingInstructionService>>();
        producerInvoiceNetTonnageService = fixture.Freeze<Mock<IProducerInvoiceNetTonnageService>>();

        sut = fixture.Create<PrepareProducerDataInsertService>();
    }

    [TestMethod]
    public async Task CanCallInsertProducerDataToDatabase()
    {
        // Arrange
        var calcResult = fixture.Create<CalcResult>();
        var materials = fixture.Create<IImmutableList<MaterialDetail>>();

        // Act
        var result = await sut.InsertProducerDataToDatabase(calcResult, materials);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public async Task CanCallInsertProducerDataToDatabaseWith()
    {
        // Arrange
        var calcResult = fixture.Create<CalcResult>();
        var materials = fixture.Create<IImmutableList<MaterialDetail>>();

        billingInstructionService.Setup(m => m.CreateBillingInstructions(It.IsAny<CalcResult>())).ReturnsAsync(true);
        producerInvoiceNetTonnageService.Setup(m => m.CreateProducerInvoiceNetTonnage(It.IsAny<CalcResult>(), It.IsAny<IImmutableList<MaterialDetail>>())).ReturnsAsync(true);

        // Act
        var result = await sut.InsertProducerDataToDatabase(calcResult, materials);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task CannotCallInsertProducerDataToDatabaseWithNullCalcResult()

    {
        // Arrange
        var calcResult = fixture.Create<CalcResult>();
        var materials = fixture.Create<IImmutableList<MaterialDetail>>();

        billingInstructionService.Setup(m => m.CreateBillingInstructions(It.IsAny<CalcResult>())).ThrowsAsync(new Exception());
        producerInvoiceNetTonnageService.Setup(m => m.CreateProducerInvoiceNetTonnage(It.IsAny<CalcResult>(), It.IsAny<IImmutableList<MaterialDetail>>())).ReturnsAsync(true);

        // Act
        var result = await sut.InsertProducerDataToDatabase(calcResult, materials);

        // Assert
        Assert.IsFalse(result);
    }
}
