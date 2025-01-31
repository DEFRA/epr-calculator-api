using AutoFixture;
using EPR.Calculator.API.Builder.Summary.CommsCostTwoA;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.Calculator.API.UnitTests.Builder.Summary.CommsCostTwoA
{
  [TestClass]
  public class CalcResultSummaryCommsCostTwoATests
  {
    private readonly List<ProducerDetail> _producers;
    private readonly MaterialDetail _material;
    private readonly CalcResult _calcResult;
    private Fixture Fixture { get; init; } = new Fixture();

    public CalcResultSummaryCommsCostTwoATests()
    {
      _material = GetMaterial();
      _producers = GetProducers();
      _calcResult = new CalcResult
      {
        CalcResultParameterOtherCost = TestDataHelper.GetCalcResultParameterOtherCost(),
        CalcResultDetail = TestDataHelper.GetCalcResultDetail(),
        CalcResultLaDisposalCostData = TestDataHelper.GetCalcResultLaDisposalCostData(),
        CalcResultLapcapData = TestDataHelper.GetCalcResultLapcapData(),
        CalcResultOnePlusFourApportionment = TestDataHelper.GetCalcResultOnePlusFourApportionment(),
        CalcResultParameterCommunicationCost = GetCalcResultParameterCommunicationCost(),
        CalcResultSummary = TestDataHelper.GetCalcResultSummary(),
        CalcResultCommsCostReportDetail = TestDataHelper.GetCalcResultCommsCostReportDetail(),
        CalcResultLateReportingTonnageData = GetCalcResultLateReportingTonnage(),
      };
    }

    [TestMethod]
    public void GetEnglandWithBadDebtProvisionForCommsTotal_WhenNoProducers_ShouldReturn0()
    {
      // Arrange
      _producers.Clear();
      decimal expectedCost1 = 0m;

      // Act
      decimal totalCost = CalcResultSummaryCommsCostTwoA.GetEnglandWithBadDebtProvisionForCommsTotal(_producers, _material, _calcResult);

      // Assert
      Assert.AreEqual(expectedCost1, totalCost);
    }

    [TestMethod]
    public void GetEnglandWithBadDebtProvisionForCommsTotal_ShouldReturnCorrectTotal()
    {
      // Arrange
      decimal expectedCost1 = 1139.71200m;

      // Act
      decimal totalCost = CalcResultSummaryCommsCostTwoA.GetEnglandWithBadDebtProvisionForCommsTotal(_producers, _material, _calcResult);

      // Assert
      Assert.AreEqual(expectedCost1, totalCost);
    }

    [TestMethod]
    public void GetWalesWithBadDebtProvisionForCommsTotal_ShouldReturnCorrectTotal()
    {
      // Arrange
      decimal expectedCost1 = 284.92800m;

      // Act
      decimal totalCost = CalcResultSummaryCommsCostTwoA.GetWalesWithBadDebtProvisionForCommsTotal(_producers, _material, _calcResult);

      // Assert
      Assert.AreEqual(expectedCost1, totalCost);
    }

    [TestMethod]
    public void GetScotlandWithBadDebtProvisionForCommsTotal_ShouldReturnCorrectTotal()
    {
      // Arrange
      decimal expectedCost1 = 427.39200m;

      // Act
      decimal totalCost = CalcResultSummaryCommsCostTwoA.GetScotlandWithBadDebtProvisionForCommsTotal(_producers, _material, _calcResult);

      // Assert
      Assert.AreEqual(expectedCost1, totalCost);
    }

    [TestMethod]
    public void GetNorthernIrelandWithBadDebtProvisionForCommsTotal_ShouldReturnCorrectTotal()
    {
      // Arrange
      decimal expectedCost1 = 997.24800m;

      // Act
      decimal totalCost = CalcResultSummaryCommsCostTwoA.GetNorthernIrelandWithBadDebtProvisionForCommsTotal(_producers, _material, _calcResult);

      // Assert
      Assert.AreEqual(expectedCost1, totalCost);
    }

    [TestMethod]
    public void GetProducerTotalCostWithoutBadDebtProvisionTotal_ShouldReturnCorrectTotal()
    {
      // Arrange
      decimal expectedCost1 = 1344.00m;

      // Act
      decimal totalCost = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostWithoutBadDebtProvisionTotal(_producers, _material, _calcResult);

      // Assert
      Assert.AreEqual(expectedCost1, totalCost);
    }

    [TestMethod]
    public void GetBadDebtProvisionForCommsCostTotal_ShouldReturnCorrectTotal()
    {
      // Arrange
      decimal expectedCost1 = 80.64m;

      // Act
      decimal totalCost = CalcResultSummaryCommsCostTwoA.GetBadDebtProvisionForCommsCostTotal(_producers, _material, _calcResult);

      // Assert
      Assert.AreEqual(expectedCost1, totalCost);
    }

    [TestMethod]
    public void GetEnglandWithBadDebtProvisionForComms_ShouldReturnCorrectTotal()
    {
      // Arrange
      decimal expectedCost1 = 427.392m;

      // Act
      decimal totalCost = CalcResultSummaryCommsCostTwoA.GetEnglandWithBadDebtProvisionForComms(_producers[0], _material, _calcResult);

      // Assert
      Assert.AreEqual(expectedCost1, totalCost);
    }

    [TestMethod]
    public void GetWalesWithBadDebtProvisionForComms_ShouldReturnCorrectTotal()
    {
      // Arrange
      decimal expectedCost1 = 106.848m;

      // Act
      decimal totalCost = CalcResultSummaryCommsCostTwoA.GetWalesWithBadDebtProvisionForComms(_producers[0], _material, _calcResult);

      // Assert
      Assert.AreEqual(expectedCost1, totalCost);
    }

    [TestMethod]
    public void GetScotlandWithBadDebtProvisionForComms_ShouldReturnCorrectTotal()
    {
      // Arrange
      decimal expectedCost1 = 160.272m;

      // Act
      decimal totalCost = CalcResultSummaryCommsCostTwoA.GetScotlandWithBadDebtProvisionForComms(_producers[0], _material, _calcResult);

      // Assert
      Assert.AreEqual(expectedCost1, totalCost);
    }

    [TestMethod]
    public void GetNorthernIrelandWithBadDebtProvisionForComms_ShouldReturnCorrectTotal()
    {
      // Arrange
      decimal expectedCost1 = 373.96800m;

      // Act
      decimal totalCost = CalcResultSummaryCommsCostTwoA.GetNorthernIrelandWithBadDebtProvisionForComms(_producers[0], _material, _calcResult);

      // Assert
      Assert.AreEqual(expectedCost1, totalCost);
    }

    [TestMethod]
    public void GetPriceperTonneForComms_ShouldReturnCorrectTotal()
    {
      // Arrange
      decimal expectedCost1 = 0.42m;

      // Act
      decimal totalCost = CalcResultSummaryCommsCostTwoA.GetPriceperTonneForComms(_material, _calcResult);

      // Assert
      Assert.AreEqual(expectedCost1, totalCost);
    }

    [TestMethod]
    public void GetPriceperTonneForComms_WhenNoMaterialMatch_ShouldReturn0()
    {
      // Arrange
      _material.Name = "Aluminium";
      decimal expectedCost1 = 0m;
      // Act
      decimal totalCost = CalcResultSummaryCommsCostTwoA.GetPriceperTonneForComms(_material, _calcResult);

      // Assert
      Assert.AreEqual(expectedCost1, totalCost);
    }

    [TestMethod]
    public void GetPriceperTonneForComms_WhenCostIsNotDecimal_ShouldReturn0()
    {
      // Arrange      
      _calcResult.CalcResultCommsCostReportDetail.CalcResultCommsCostCommsCostByMaterial = [
                        new ()
                        {
                            CommsCostByMaterialPricePerTonne="0.42A",
                            Name ="Household"
                        }
                    ];
      decimal expectedCost1 = 0m;

      // Act
      decimal totalCost = CalcResultSummaryCommsCostTwoA.GetPriceperTonneForComms(_material, _calcResult);

      // Assert
      Assert.AreEqual(expectedCost1, totalCost);
    }

    [TestMethod]
    public void GetProducerTotalCostwithBadDebtProvision_ShouldReturnCorrectTotal()
    {
      // Arrange
      decimal expectedCost1 = 534.2400m;

      // Act
      decimal totalCost = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostwithBadDebtProvision(_producers[0], _material, _calcResult);

      // Assert
      Assert.AreEqual(expectedCost1, totalCost);
    }

    [TestMethod]
    public void GetProducerTotalCostWithoutBadDebtProvision_ShouldReturnCorrectTotal()
    {
      // Arrange
      decimal expectedCost1 = 504.00m;

      // Act
      decimal totalCost = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostWithoutBadDebtProvision(_producers[0], _material, _calcResult);

      // Assert
      Assert.AreEqual(expectedCost1, totalCost);
    }

    [TestMethod]
    public void GetProducerTotalCostwithBadDebtProvisionTotal_ShouldReturnCorrectTotal()
    {
      // Arrange
      decimal expectedCost1 = 1424.6400m;

      // Act
      decimal totalCost = CalcResultSummaryCommsCostTwoA.GetProducerTotalCostwithBadDebtProvisionTotal(_producers, _material, _calcResult);

      // Assert
      Assert.AreEqual(expectedCost1, totalCost);
    }

    [TestMethod]
    public void GetTotalReportedTonnageTotalForHDCShouldReturnCorrectTotal()
    {
        // Arrange
        decimal expectedCost1 = 150;
        var material = GetHDCMaterial();

        // Act
        decimal totalCost = CalcResultSummaryCommsCostTwoA.GetTotalReportedTonnageTotal(_producers, material);

        // Assert
        Assert.AreEqual(expectedCost1, totalCost);
    }

    [TestMethod]
    public void GetTotalReportedTonnageTotalShouldReturnCorrectTotal()
    {
        // Arrange
        decimal expectedCost1 = 3200;

        // Act
        decimal totalCost = CalcResultSummaryCommsCostTwoA.GetTotalReportedTonnageTotal(_producers, _material);

        // Assert
        Assert.AreEqual(expectedCost1, totalCost);
    }

        private CalcResultParameterCommunicationCost GetCalcResultParameterCommunicationCost()
    {
      return this.Fixture.Create<CalcResultParameterCommunicationCost>();
    }

    private CalcResultLateReportingTonnage GetCalcResultLateReportingTonnage()
    {
      return this.Fixture.Create<CalcResultLateReportingTonnage>();
    }

    private static List<ProducerDetail> GetProducers()
    {
      var producers = TestDataHelper.GetProducers();

      producers[0].ProducerReportedMaterials.Add(new ProducerReportedMaterial
      {
        MaterialId = 1,
        ProducerDetailId = 1,
        PackagingType = "HH",
        PackagingTonnage = (1 * 100),
        Material = new Material
        {
          Id = 1,
          Code = "HH",
          Name = "Material1",
          Description = "Material1"
        }
      });

        producers[0].ProducerReportedMaterials.Add(new ProducerReportedMaterial
        {
            MaterialId = 2,
            ProducerDetailId = 2,
            PackagingType = "HDC",
            PackagingTonnage = (1 * 100),
            Material = new Material
            {
                Id = 2,
                Code = "GL",
                Name = "Material2",
                Description = "Material2"
            }
        });

        producers[0].ProducerReportedMaterials.Add(new ProducerReportedMaterial
         {
                MaterialId = 3,
                ProducerDetailId = 3,
                PackagingType = "PB",
                PackagingTonnage = (2 * 100),
                Material = new Material
                {
                    Id = 1,
                    Code = "AL",
                    Name = "Material1",
                    Description = "Material1"
                }
            });

            return producers;
    }

    private static MaterialDetail GetMaterial()
    {
      var material = new MaterialDetail
      {
        Id = 1,
        Code = "AL",
        Name = "Material1",
        Description = "Material1"
      };
      return material;
    }

    private static MaterialDetail GetHDCMaterial()
    {
        var material = new MaterialDetail
        {
            Id = 2,
            Code = "GL",
            Name = "Material2",
            Description = "Material2"
        };
        return material;
    }

    }
}
