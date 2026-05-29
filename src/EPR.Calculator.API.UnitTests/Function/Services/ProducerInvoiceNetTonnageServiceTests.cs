using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Mappers;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.Builder;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.Fixtures;

namespace EPR.Calculator.Service.Function.UnitTests.Services;

[TestClass]
public class ProducerInvoiceNetTonnageServiceTests
{
    private IFixture fixture = null!;
    private Mock<IProducerInvoiceTonnageMapper> producerInvoiceMapper = null!;
    private ProducerInvoiceNetTonnageService sut = null!;

    [TestInitialize]
    public void Init()
    {
        fixture = TestFixtures.New();
        producerInvoiceMapper = fixture.Freeze<Mock<IProducerInvoiceTonnageMapper>>();

        sut = fixture.Create<ProducerInvoiceNetTonnageService>();
    }

    [TestMethod]
    public async Task CanCallCreateProducerInvoiceNetTonnage1()
    {
        // Arrange
        var materials = TestDataHelper.GetMaterials();
        var calcResult = TestDataHelper.GetCalcResult();
        producerInvoiceMapper.Setup(m => m.Map(It.IsAny<ProducerInvoiceTonnage>())).Returns(fixture.Create<ProducerInvoicedMaterialNetTonnage>());

        // Act
        var result = await sut.CreateProducerInvoiceNetTonnage(calcResult, materials);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public async Task CanCallCreateProducerInvoiceTonnageWithNoProducers()
    {
        // Arrange
        var materials = TestDataHelper.GetMaterials();
        var calcResult = new CalcResult
        {
            ApplyModulation = true,
            CalcResultScaledupProducers = new CalcResultScaledupProducers(),
            CalcResultPartialObligations = new CalcResultPartialObligations(),
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
            CalcResultProjectedProducers = new CalcResultProjectedProducers()
        };

        // Act
        var result = await sut.CreateProducerInvoiceNetTonnage(calcResult, materials);
        Assert.IsFalse(result);
    }
}
