using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Utils;
using EPR.Calculator.Service.Function.Builder.CommsCost;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Builder;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class CalcResultCommsCostBuilderTest : TestsFor<CalcResultCommsCostBuilder>
{
    [TestMethod]
    public async Task ConstructTest()
    {
        var calcResult = TestDataHelper.GetCalcResult();

        var materialDetails = CreateMaterials();

        var runContext = TestDataHelper.CalculatorRun2024;
        runContext = runContext with
        {
            DefaultParameters = runContext.DefaultParameters with
            {
                CommunicationCosts = new CommunicationCosts
                {
                    ByMaterialCode = materialDetails.ToDictionary(m => m.Code, _ => 10m),
                    ByCountry = new ByCountryCostWithUk
                    {
                        UnitedKingdom = 100,
                        England = 40,
                        Wales = 30,
                        Scotland = 20,
                        NorthernIreland = 10
                    }
                },
            }
        };

        CreateNewRun(runContext);
        CreateProducerDetail(runContext);

        var apportionment = new CalcResultOnePlusFourApportionment
        {
            LaDisposalCost = new ByCountryCost
            {
                England = 40M,
                Wales = 20M,
                Scotland = 20M,
                NorthernIreland = 20M
            },
            LADataPrepCharge = ByCountryCost.Empty
        };
        var result = await testSubject.ConstructAsync(runContext, materialDetails, apportionment, calcResult.CalcResultLateReportingTonnageData);

        Assert.IsNotNull(result);

        var onePlusFourApp = result.OnePlusFourApportionment;
        Assert.IsNotNull(onePlusFourApp);

        Assert.AreEqual(40, onePlusFourApp.England);
        Assert.AreEqual(20, onePlusFourApp.Wales);
        Assert.AreEqual(20, onePlusFourApp.NorthernIreland);
        Assert.AreEqual(20, onePlusFourApp.Scotland);
        Assert.AreEqual(100, onePlusFourApp.Total);

        var materialCosts = result.ByMaterial;
        Assert.IsNotNull(materialCosts);
        Assert.HasCount(8, materialCosts);

        var aluminiumCost = materialCosts["AL"];
        Assert.AreEqual(4, aluminiumCost.Cost.England);
        Assert.AreEqual(2, aluminiumCost.Cost.Wales);
        Assert.AreEqual(2, aluminiumCost.Cost.Scotland);
        Assert.AreEqual(2, aluminiumCost.Cost.NorthernIreland);
        Assert.AreEqual(10, aluminiumCost.Cost.Total);
        Assert.AreEqual(1000, aluminiumCost.HouseholdPackagingWasteTonnage);
        Assert.AreEqual(8000, aluminiumCost.LateReportingTonnage);
        Assert.AreEqual(9000, aluminiumCost.TotalTonnage);
        Assert.AreEqual(0.0011m, MathUtils.RoundAwayFromZero(aluminiumCost.PricePerTonne, 4));

        var fibreCompositeCost = materialCosts["FC"];
        Assert.AreEqual(4, fibreCompositeCost.Cost.England);
        Assert.AreEqual(2, fibreCompositeCost.Cost.Wales);
        Assert.AreEqual(2, fibreCompositeCost.Cost.Scotland);
        Assert.AreEqual(2, fibreCompositeCost.Cost.NorthernIreland);
        Assert.AreEqual(10, fibreCompositeCost.Cost.Total);
        Assert.AreEqual(2000, fibreCompositeCost.HouseholdPackagingWasteTonnage);
        Assert.AreEqual(10, fibreCompositeCost.LateReportingTonnage);
        Assert.AreEqual(2210, fibreCompositeCost.TotalTonnage);
        Assert.AreEqual(0.0045m, MathUtils.RoundAwayFromZero(fibreCompositeCost.PricePerTonne, 4));

        var glassCost = materialCosts["GL"];
        Assert.AreEqual(4, glassCost.Cost.England);
        Assert.AreEqual(2, glassCost.Cost.Wales);
        Assert.AreEqual(2, glassCost.Cost.Scotland);
        Assert.AreEqual(2, glassCost.Cost.NorthernIreland);
        Assert.AreEqual(10, glassCost.Cost.Total);
        Assert.AreEqual(3000, glassCost.HouseholdPackagingWasteTonnage);
        Assert.AreEqual(10, glassCost.LateReportingTonnage);
        Assert.AreEqual(3210, glassCost.TotalTonnage);
        Assert.AreEqual(0.0031m, MathUtils.RoundAwayFromZero(glassCost.PricePerTonne, 4));
        Assert.AreEqual(200, glassCost.HouseholdDrinksContainersTonnage);

        var totalMaterialCost = result.Total;
        Assert.AreEqual(32, totalMaterialCost.Cost.England);
        Assert.AreEqual(16, totalMaterialCost.Cost.Wales);
        Assert.AreEqual(16, totalMaterialCost.Cost.Scotland);
        Assert.AreEqual(16, totalMaterialCost.Cost.NorthernIreland);
        Assert.AreEqual(80, totalMaterialCost.Cost.Total);
        Assert.AreEqual(36000, totalMaterialCost.HouseholdPackagingWasteTonnage);
        Assert.AreEqual(10020, totalMaterialCost.LateReportingTonnage);
        Assert.AreEqual(46420, totalMaterialCost.TotalTonnage);
    }

    [TestMethod]
    public async Task GetProducerReportedMaterials_ShouldReturnValidMaterials()
    {
        // Arrange
        var runContext = TestDataHelper.CalculatorRun2025;
        SeedDatabase(dbContext, runContext);

        // Act
        var result = await testSubject.GetProducerReportedMaterials(runContext);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(6, result.Count);
        Assert.IsTrue(result.Any(r => r.Material!.Code == "PL" && r.PackagingType == "HH" && r.PackagingTonnage == 50 && r.SubmissionPeriod == "2025-H1"));
        Assert.IsTrue(result.Any(r => r.Material!.Code == "PL" && r.PackagingType == "HH" && r.PackagingTonnage == 50 && r.SubmissionPeriod == "2025-H2"));
        Assert.IsTrue(result.Any(r => r.Material!.Code == "ST" && r.PackagingType == "PB" && r.PackagingTonnage == 100 && r.SubmissionPeriod == "2025-H1"));
        Assert.IsTrue(result.Any(r => r.Material!.Code == "ST" && r.PackagingType == "PB" && r.PackagingTonnage == 100 && r.SubmissionPeriod == "2025-H2"));
        Assert.IsTrue(result.Any(r => r.Material!.Code == "GL" && r.PackagingType == "HDC" && r.PackagingTonnage == 150 && r.SubmissionPeriod == "2025-H1"));
        Assert.IsTrue(result.Any(r => r.Material!.Code == "GL" && r.PackagingType == "HDC" && r.PackagingTonnage == 150 && r.SubmissionPeriod == "2025-H2"));
    }

    private void SeedDatabase(ApplicationDBContext context, RunContext runContext)
    {
        var run = new CalculatorRun { Id = runContext.RunId, RelativeYear = runContext.RelativeYear, Name = runContext.RunName };
        context.CalculatorRuns.Add(run);

        var producerDetail = new ProducerDetail { Id = 1, CalculatorRunId = runContext.RunId };
        context.ProducerDetail.Add(producerDetail);

        var materials = new List<Material>
        {
            new() { Id = 1, Name = "Plastic", Code = MaterialCodes.Plastic },
            new() { Id = 2, Name = "Steel", Code = MaterialCodes.Steel },
            new() { Id = 3, Name = "Glass", Code = MaterialCodes.Glass }
        };
        context.Material.AddRange(materials);

        var producerReportedMaterials = new List<ProducerMaterialPackaging>
        {
            new() { ProducerDetailId = 1, MaterialId = 1, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.Household, PackagingTonnage = 50 },
            new() { ProducerDetailId = 1, MaterialId = 1, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.Household, PackagingTonnage = 50 },
            new() { ProducerDetailId = 1, MaterialId = 2, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 100 },
            new() { ProducerDetailId = 1, MaterialId = 2, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 100 },
            new() { ProducerDetailId = 1, MaterialId = 3, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 150 },
            new() { ProducerDetailId = 1, MaterialId = 3, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 150 }
        };
        context.ProducerMaterialPackaging.AddRange(producerReportedMaterials);

        context.SaveChanges();
    }

    private void CreateProducerDetail(RunContext runContext)
    {
        var producerNames = new[]
        {
            "Allied Packaging",
            "Beeline Materials",
            "Cloud Boxes",
            "Decking and Shed",
            "Electric Things",
            "French Flooring",
            "Good Fruit Co",
            "Happy Shopper",
            "Icicle Foods",
            "Jumbo Box Store"
        };

        var producerId = 1;
        foreach (var producerName in producerNames)
        {
            dbContext.ProducerDetail.Add(new ProducerDetail
            {
                ProducerId = producerId++,
                SubsidiaryId = $"{producerId}-Sub",
                ProducerName = producerName,
                CalculatorRunId = runContext.RunId
            });
        }

        dbContext.SaveChanges();

        foreach (var subPeriod in new[] { "2025-H1", "2025-H2" })
        {
            for (var producerDetailId = 1; producerDetailId <= 10; producerDetailId++)
            {
                for (var materialId = 1; materialId < 9; materialId++)
                {
                    dbContext.ProducerMaterialPackaging.Add(new ProducerMaterialPackaging
                    {
                        MaterialId = materialId,
                        ProducerDetailId = producerDetailId,
                        PackagingType = "HH",
                        SubmissionPeriod = subPeriod,
                        PackagingTonnage = materialId * 50
                    });
                }
            }

            dbContext.ProducerMaterialPackaging.AddRange(new List<ProducerMaterialPackaging>
            {
                new()
                {
                    MaterialId = 3,
                    ProducerDetailId = 1,
                    PackagingType = "HDC",
                    SubmissionPeriod = subPeriod,
                    PackagingTonnage = 50
                },
                new()
                {
                    MaterialId = 3,
                    ProducerDetailId = 2,
                    PackagingType = "HDC",
                    SubmissionPeriod = subPeriod,
                    PackagingTonnage = 50
                },
                new()
                {
                    MaterialId = 2,
                    ProducerDetailId = 1,
                    PackagingType = "PB",
                    SubmissionPeriod = subPeriod,
                    PackagingTonnage = 100
                }
            });
        }

        dbContext.SaveChanges();
    }

    private void CreateNewRun(RunContext runContext)
    {
        var run = new CalculatorRun
        {
            Id = runContext.RunId,
            CalculatorRunClassificationId = (int)RunClassification.RUNNING,
            Name = runContext.RunName,
            RelativeYear = runContext.RelativeYear,
            CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
            CreatedBy = runContext.User,
            DefaultParameterSettingMasterId = 1
        };
        dbContext.CalculatorRuns.Add(run);
        dbContext.SaveChanges();
    }

    private IImmutableList<MaterialDetail> CreateMaterials()
    {
        var materials = TestDataHelper.GetMaterialDetails();

        dbContext.Material.AddRange(materials.Select(m =>
            new Material
            {
                Name = m.Name,
                Code = m.Code,
                Description = "ignored"
            }
        ));

        dbContext.SaveChanges();

        return materials;
    }
}
