using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Builder.LaDisposalCost;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Features.Common;
using EPR.Calculator.Service.Function.Services;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers;
using EPR.Calculator.Service.Function.UnitTests.TestHelpers.TestData;

namespace EPR.Calculator.Service.Function.UnitTests.Builder;

[TestCategory(TestCategories.ResultBuilder)]
[TestClass]
public class CalcRunLaDisposalCostBuilderTests : TestsFor<CalcRunLaDisposalCostBuilder>
{
    private SelfManagedConsumerWaste smcw = null!;

    protected override void TestInitialize()
    {
        smcw = new SelfManagedConsumerWaste
        {
            ProducerTotals = [],
            OverallTotalPerMaterials = []
        };
    }

    [TestMethod]
    public void Should_Set_And_Get_MaterialName()
    {
        // Arrange
        var producerData = new ProducerData { MaterialName = "Plastic", PackagingType = "PB" };

        // Act
        var result = producerData.MaterialName;

        // Assert
        Assert.AreEqual("Plastic", result);
    }

    [TestMethod]
    public void Should_Set_And_Get_PackagingType()
    {
        // Arrange
        var producerData = new ProducerData { PackagingType = "HDC", MaterialName = "Glass" };

        // Act
        var result = producerData.PackagingType;

        // Assert
        Assert.AreEqual("HDC", result);
    }

    [TestMethod]
    public void Should_Set_And_Get_Tonnage()
    {
        // Arrange
        var producerData = new ProducerData { Tonnage = 100.5m, MaterialName = "Plastic", PackagingType = "PB" };

        // Act
        var result = producerData.Tonnage;

        // Assert
        Assert.AreEqual(100.5m, result);
    }

    [TestMethod]
    public void Should_Set_And_Get_ProducerDetail()
    {
        // Arrange
        var producerDetail = new ProducerDetail { ProducerId = 1, ProducerName = "Test Producer" };
        var producerData = new ProducerData { MaterialName = "Plastic", PackagingType = "PB", ProducerDetail = producerDetail };

        // Act
        var result = producerData.ProducerDetail;

        // Assert
        Assert.AreEqual(producerDetail, result);
    }

    [TestMethod]
    public async Task Should_Return_LA_Disposal_Costs()
    {
        // Assign
        var runContext = TestDataHelper.CalculatorRun2025;
        var calcResult = TestDataHelper.GetCalcResult();

        // Act
        var lapcapDisposalCostResults = await testSubject.ConstructAsync(runContext, TestDataHelper.GetMaterialDetails(), calcResult.CalcResultLapcapData, calcResult.CalcResultLateReportingTonnageData, smcw);

        // Assert
        Assert.IsNotNull(lapcapDisposalCostResults);
        Assert.AreEqual(8, lapcapDisposalCostResults.ByMaterial.Count);
    }

    [TestMethod]
    public async Task Should_Return_Material_Data_With_PublicBin()
    {
        // Assign
        var runContext = TestDataHelper.CalculatorRun2025;
        var calcResult = TestDataHelper.GetCalcResult();
        SeedDatabase(runContext);


        // Act
        var lapcapDisposalCostResults = await testSubject.ConstructAsync(runContext, TestDataHelper.GetMaterialDetails(), calcResult.CalcResultLapcapData, calcResult.CalcResultLateReportingTonnageData, smcw);

        // Assert
        var laDisposalCost = lapcapDisposalCostResults.ByMaterial["PL"];

        Assert.AreEqual(23000, laDisposalCost.Cost.England);
        Assert.AreEqual(4500, laDisposalCost.Cost.Wales);
        Assert.AreEqual(6700, laDisposalCost.Cost.Scotland);
        Assert.AreEqual(2100, laDisposalCost.Cost.NorthernIreland);
        Assert.AreEqual(36300, laDisposalCost.Cost.Total);

        Assert.AreEqual(400, laDisposalCost.HouseholdPackagingWasteTonnage);
        Assert.AreEqual(0, laDisposalCost.PublicBinTonnage);
        Assert.AreEqual(0, laDisposalCost.HouseholdDrinkContainersTonnage);
        Assert.AreEqual(2000, laDisposalCost.LateReportingTonnage);
        Assert.AreEqual(2400, laDisposalCost.TotalTonnage);
    }


    [TestMethod]
    public async Task Should_Apply_Modulations()
    {
        // Assign
        var runContext = TestDataHelper.CalculatorRun2026;
        var calcResult = TestDataHelper.GetCalcResult(true);
        SeedDatabase(runContext);

        static SelfManagedConsumerWasteData MkSelfManagedConsumerWasteData(decimal red, decimal amber, decimal green) =>
            new()
            {
                SelfManagedConsumerWasteTonnage = amber,
                ActionedSelfManagedConsumerWasteTonnage = (total: amber, red: 0, amber, green: 0),
                ResidualSelfManagedConsumerWasteTonnage = 0,
                NetReportedTonnage = (total: red + green, red, amber: 0, green)
            };

        var smcw = new SelfManagedConsumerWaste
        {
            ProducerTotals = [],
            OverallTotalPerMaterials = new Dictionary<string, SelfManagedConsumerWasteData>
            {
                [MaterialCodes.Aluminium] = MkSelfManagedConsumerWasteData(220, 330, 550),
                [MaterialCodes.FibreComposite] = MkSelfManagedConsumerWasteData(275, 55, 55),
                [MaterialCodes.Glass] = MkSelfManagedConsumerWasteData(110, 220, 220),
                [MaterialCodes.Plastic] = MkSelfManagedConsumerWasteData(400, 1050, 2400),
                [MaterialCodes.PaperOrCard] = MkSelfManagedConsumerWasteData(2150, 275, 270),
                [MaterialCodes.Steel] = MkSelfManagedConsumerWasteData(33, 40, 74),
                [MaterialCodes.Wood] = MkSelfManagedConsumerWasteData(265, 0, 0),
                [MaterialCodes.OtherMaterials] = MkSelfManagedConsumerWasteData(30, 0, 0)
            }
        };
        // Act
        var lapcapDisposalCostResults = await testSubject.ConstructAsync(runContext, TestDataHelper.GetMaterialDetails(), calcResult.CalcResultLapcapData, calcResult.CalcResultLateReportingTonnageData, smcw);

        // Assert
        var plastic = lapcapDisposalCostResults.ByMaterial["PL"];
        Assert.AreEqual(1050, plastic.ActionedSelfManagedConsumerWasteTonnage);
        Assert.AreEqual(1350, plastic.TotalTonnage);

        var steel = lapcapDisposalCostResults.ByMaterial["ST"];
        Assert.AreEqual(40, steel.ActionedSelfManagedConsumerWasteTonnage);
        Assert.AreEqual(360, steel.TotalTonnage);

        var glass = lapcapDisposalCostResults.ByMaterial["GL"];
        Assert.AreEqual(220, glass.ActionedSelfManagedConsumerWasteTonnage);
        Assert.AreEqual(290, glass.TotalTonnage);
    }

    [TestMethod]
    public async Task Should_Return_Material_Data_With_Household_Drink_Containers()
    {
        // Assign
        var runContext = TestDataHelper.CalculatorRun2025;
        var calcResult = TestDataHelper.GetCalcResult();
        SeedDatabase(runContext);

        // Act
        var lapcapDisposalCostResults = await testSubject.ConstructAsync(runContext, TestDataHelper.GetMaterialDetails(), calcResult.CalcResultLapcapData, calcResult.CalcResultLateReportingTonnageData, smcw);

        // Assert
        var laDisposalCost = lapcapDisposalCostResults.ByMaterial["GL"];
        Assert.AreEqual(45000, laDisposalCost.Cost.England);
        Assert.AreEqual(0, laDisposalCost.Cost.Wales);
        Assert.AreEqual(20700, laDisposalCost.Cost.Scotland);
        Assert.AreEqual(4500, laDisposalCost.Cost.NorthernIreland);
        Assert.AreEqual(70200, laDisposalCost.Cost.Total);
        Assert.AreEqual(0, laDisposalCost.HouseholdPackagingWasteTonnage);
        Assert.AreEqual(0, laDisposalCost.PublicBinTonnage);
        Assert.AreEqual(500, laDisposalCost.HouseholdDrinkContainersTonnage);
        Assert.AreEqual(10, laDisposalCost.LateReportingTonnage);
        Assert.AreEqual(510, laDisposalCost.TotalTonnage);
    }

    [TestMethod]
    public async Task Should_Calculate_ProducerDataTotal_For_Total_Material()
    {
        // Assign
        var runContext = TestDataHelper.CalculatorRun2025;
        var calcResult = TestDataHelper.GetCalcResult();
        SeedDatabase(runContext);

        // Act
        var lapcapDisposalCostResults = await testSubject.ConstructAsync(runContext, TestDataHelper.GetMaterialDetails(), calcResult.CalcResultLapcapData, calcResult.CalcResultLateReportingTonnageData, smcw);

        // Assert
        var laDisposalCost = lapcapDisposalCostResults.Total;
        Assert.IsNotNull(laDisposalCost);
        Assert.AreEqual(400, laDisposalCost.HouseholdPackagingWasteTonnage);
    }


    [TestMethod]
    public async Task Should_Calculate_ProducerDataTotal_For_Specific_Material()
    {
        // Assign
        var runContext = TestDataHelper.CalculatorRun2025;
        var calcResult = TestDataHelper.GetCalcResult();
        SeedDatabase(runContext);

        // Act
        var lapcapDisposalCostResults = await testSubject.ConstructAsync(runContext, TestDataHelper.GetMaterialDetails(), calcResult.CalcResultLapcapData, calcResult.CalcResultLateReportingTonnageData, smcw);

        // Assert
        var plastic = lapcapDisposalCostResults.ByMaterial["PL"];
        Assert.AreEqual(400, plastic.HouseholdPackagingWasteTonnage);
    }

    private void SeedDatabase(RunContext runContext)
    {
        var run = new CalculatorRun { Id = runContext.RunId, RelativeYear = runContext.RelativeYear, Name = runContext.RunName };
        dbContext.CalculatorRuns.Add(run);

        var producerDetail = new List<ProducerDetail>
        {
            new() { Id = 3, CalculatorRunId = runContext.RunId },
            new() { Id = 2, CalculatorRunId = runContext.RunId }
        };
        dbContext.ProducerDetail.AddRange(producerDetail);

        var materials = new List<Material>
        {
            new() { Id = 1, Name = "Plastic", Code = MaterialCodes.Plastic },
            new() { Id = 2, Name = "Steel", Code = MaterialCodes.Steel },
            new() { Id = 3, Name = "Glass", Code = MaterialCodes.Glass }
        };
        dbContext.Material.AddRange(materials);

        var producerReportedMaterials = new List<ProducerReportedMaterialProjected>
        {
            new() { ProducerDetailId = 3, MaterialId = 1, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.Household, PackagingTonnage = 25 },
            new() { ProducerDetailId = 3, MaterialId = 1, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.Household, PackagingTonnage = 75 },
            new() { ProducerDetailId = 2, MaterialId = 1, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.Household, PackagingTonnage = 100 },
            new() { ProducerDetailId = 2, MaterialId = 1, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.Household, PackagingTonnage = 200 },
            new() { ProducerDetailId = 3, MaterialId = 2, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 100 },
            new() { ProducerDetailId = 3, MaterialId = 2, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 100 },
            new() { ProducerDetailId = 2, MaterialId = 2, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 50 },
            new() { ProducerDetailId = 2, MaterialId = 2, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 150 },
            new() { ProducerDetailId = 1, MaterialId = 3, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 150 },
            new() { ProducerDetailId = 1, MaterialId = 3, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 150 },
            new() { ProducerDetailId = 2, MaterialId = 3, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 200 },
            new() { ProducerDetailId = 2, MaterialId = 3, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 300 }
        };
        dbContext.ProducerReportedMaterialProjected.AddRange(producerReportedMaterials);

        dbContext.SaveChanges();
    }

    public class ProducerData
    {
        public int ProducerId { get; set; }

        public required string MaterialName { get; set; }

        public required string PackagingType { get; set; }

        public decimal Tonnage { get; set; }

        public ProducerDetail? ProducerDetail { get; set; }
    }
}
