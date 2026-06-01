using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using EPR.Calculator.Service.Function.Builder.LaDisposalCost;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EPR.Calculator.Service.Function.UnitTests.Builder
{
    [TestClass]
    public class CalcRunLaDisposalCostBuilderTests
    {
        private readonly CalcRunLaDisposalCostBuilder builder;
        private readonly ApplicationDBContext dbContext;
        private readonly SelfManagedConsumerWaste smcw;

        public class ProducerData
        {
            public int ProducerId { get; set; }

            public required string MaterialName { get; set; }

            public required string PackagingType { get; set; }

            public decimal Tonnage { get; set; }

            public ProducerDetail? ProducerDetail { get; set; }
        }

        public CalcRunLaDisposalCostBuilderTests()
        {
            Fixture = new Fixture();
            var dbContextOptions =
                new DbContextOptionsBuilder<ApplicationDBContext>()
                    .UseInMemoryDatabase(databaseName: "PayCal")
                    .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                    .Options;

            dbContext = new ApplicationDBContext(dbContextOptions);
            dbContext.Database.EnsureCreated();
            builder = new CalcRunLaDisposalCostBuilder(dbContext);

            smcw = new SelfManagedConsumerWaste
            {
                ProducerTotals = [],
                OverallTotalPerMaterials = []
            };
        }

        private Fixture Fixture { get; init; }

        [TestCleanup]
        public void TearDown()
        {
            dbContext.Database.EnsureDeleted();
            dbContext.Dispose();
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
            var resultsDto = new CalcResultsRequestDto { RunId = 2, RelativeYear = new RelativeYear(2025) };
            var calcResult = TestDataHelper.GetCalcResult();

            // Act
            var lapcapDisposalCostResults = await builder.ConstructAsync(resultsDto, TestDataHelper.GetMaterials(), calcResult.CalcResultLapcapData, calcResult.CalcResultLateReportingTonnageData, smcw, calcResult.ApplyModulation);

            // Assert
            Assert.IsNotNull(lapcapDisposalCostResults);
            Assert.AreEqual(8, lapcapDisposalCostResults.ByMaterial.Count);
        }

        [TestMethod]
        public async Task Should_Return_Material_Data_With_PublicBin()
        {
            // Assign
            var resultsDto = new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2025) };
            var calcResult = TestDataHelper.GetCalcResult();
            SeedDatabase(dbContext);

            // Act
            var lapcapDisposalCostResults = await builder.ConstructAsync(resultsDto, TestDataHelper.GetMaterials(), calcResult.CalcResultLapcapData, calcResult.CalcResultLateReportingTonnageData, smcw, calcResult.ApplyModulation);

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
            var resultsDto = new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2025) };
            var calcResult = TestDataHelper.GetCalcResult(applyModulation: true);
            SeedDatabase(dbContext);

            static SelfManagedConsumerWasteData MkSelfManagedConsumerWasteData(decimal red, decimal amber, decimal green)
            {
                return new SelfManagedConsumerWasteData
                {
                    SelfManagedConsumerWasteTonnage         = amber,
                    ActionedSelfManagedConsumerWasteTonnage = (total: amber      , red: 0, amber   , green: 0),
                    ResidualSelfManagedConsumerWasteTonnage = 0,
                    NetReportedTonnage                      = (total: red + green, red   , amber: 0, green)
                };
            }

            var smcw = new SelfManagedConsumerWaste
                {
                    ProducerTotals = [],
                    OverallTotalPerMaterials = new Dictionary<string, SelfManagedConsumerWasteData>
                    {
                        [MaterialCodes.Aluminium]      = MkSelfManagedConsumerWasteData(red:  220, amber:  330, green:  550),
                        [MaterialCodes.FibreComposite] = MkSelfManagedConsumerWasteData(red:  275, amber:   55, green:   55),
                        [MaterialCodes.Glass]          = MkSelfManagedConsumerWasteData(red:  110, amber:  220, green:  220),
                        [MaterialCodes.Plastic]        = MkSelfManagedConsumerWasteData(red:  400, amber: 1050, green: 2400),
                        [MaterialCodes.PaperOrCard]    = MkSelfManagedConsumerWasteData(red: 2150, amber:  275, green:  270),
                        [MaterialCodes.Steel]          = MkSelfManagedConsumerWasteData(red:   33, amber:   40, green:   74),
                        [MaterialCodes.Wood]           = MkSelfManagedConsumerWasteData(red:  265, amber:    0, green:    0),
                        [MaterialCodes.OtherMaterials] = MkSelfManagedConsumerWasteData(red:   30, amber:    0, green:    0)
                    }
                };
            // Act
            var lapcapDisposalCostResults = await builder.ConstructAsync(resultsDto, TestDataHelper.GetMaterials(), calcResult.CalcResultLapcapData, calcResult.CalcResultLateReportingTonnageData, smcw, calcResult.ApplyModulation);

            // Assert
            var plastic = lapcapDisposalCostResults.ByMaterial["PL"];
            Assert.AreEqual(1050, plastic.ActionedSelfManagedConsumerWasteTonnage);
            Assert.AreEqual(1350, plastic.TotalTonnage);

            var steel = lapcapDisposalCostResults.ByMaterial["ST"];
            Assert.AreEqual(40 , steel.ActionedSelfManagedConsumerWasteTonnage);
            Assert.AreEqual(360, steel.TotalTonnage);

            var glass = lapcapDisposalCostResults.ByMaterial["GL"];
            Assert.AreEqual(220, glass.ActionedSelfManagedConsumerWasteTonnage);
            Assert.AreEqual(290, glass.TotalTonnage);
        }

        [TestMethod]
        public async Task Should_Return_Material_Data_With_Household_Drink_Containers()
        {
            // Assign
            var resultsDto = new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2025) };
            var calcResult = TestDataHelper.GetCalcResult();
            SeedDatabase(dbContext);

            // Act
            var lapcapDisposalCostResults = await builder.ConstructAsync(resultsDto, TestDataHelper.GetMaterials(), calcResult.CalcResultLapcapData, calcResult.CalcResultLateReportingTonnageData, smcw, calcResult.ApplyModulation);

            // Assert
            var laDisposalCost = lapcapDisposalCostResults.ByMaterial["GL"];
            Assert.AreEqual(45000, laDisposalCost.Cost.England);
            Assert.AreEqual(    0, laDisposalCost.Cost.Wales);
            Assert.AreEqual(20700, laDisposalCost.Cost.Scotland);
            Assert.AreEqual( 4500, laDisposalCost.Cost.NorthernIreland);
            Assert.AreEqual(70200, laDisposalCost.Cost.Total);
            Assert.AreEqual(    0, laDisposalCost.HouseholdPackagingWasteTonnage);
            Assert.AreEqual(    0, laDisposalCost.PublicBinTonnage);
            Assert.AreEqual(  500, laDisposalCost.HouseholdDrinkContainersTonnage);
            Assert.AreEqual(   10, laDisposalCost.LateReportingTonnage);
            Assert.AreEqual(  510, laDisposalCost.TotalTonnage);
        }

        [TestMethod]
        public async Task Should_Calculate_ProducerDataTotal_For_Total_Material()
        {
            // Arrange
            // Assign
            var resultsDto = new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2025) };
            var calcResult = TestDataHelper.GetCalcResult();
            SeedDatabase(dbContext);

            // Act
            var lapcapDisposalCostResults = await builder.ConstructAsync(resultsDto, TestDataHelper.GetMaterials(), calcResult.CalcResultLapcapData, calcResult.CalcResultLateReportingTonnageData, smcw, calcResult.ApplyModulation);

            // Assert
            var laDisposalCost = lapcapDisposalCostResults.Total;
            Assert.IsNotNull(laDisposalCost);
            Assert.AreEqual(400, laDisposalCost.HouseholdPackagingWasteTonnage);
        }


        [TestMethod]
        public async Task Should_Calculate_ProducerDataTotal_For_Specific_Material()
        {
            // Assign
            var resultsDto = new CalcResultsRequestDto { RunId = 1, RelativeYear = new RelativeYear(2025) };
            var calcResult = TestDataHelper.GetCalcResult();
            SeedDatabase(dbContext);

            // Act
            var lapcapDisposalCostResults = await builder.ConstructAsync(resultsDto, TestDataHelper.GetMaterials(), calcResult.CalcResultLapcapData, calcResult.CalcResultLateReportingTonnageData, smcw, calcResult.ApplyModulation);

            // Assert
            var plastic = lapcapDisposalCostResults.ByMaterial["PL"];
            Assert.AreEqual(400, plastic.HouseholdPackagingWasteTonnage);
        }

        private static void SeedDatabase(ApplicationDBContext context)
        {
            var run = new CalculatorRun { Id = 1, RelativeYear = new RelativeYear(2024), Name = "CalculatorRunTest1" };
            context.CalculatorRuns.Add(run);

            var producerDetail = new List<ProducerDetail>
            {
                new ProducerDetail
                {
                    Id = 3, CalculatorRunId = 1,
                },
                new ProducerDetail
                {
                    Id = 2, CalculatorRunId = 1,
                },
            };
            context.ProducerDetail.AddRange(producerDetail);

            var materials = new List<Material>
            {
                new Material { Id = 1, Name = "Plastic", Code = MaterialCodes.Plastic },
                new Material { Id = 2, Name = "Steel", Code = MaterialCodes.Steel },
                new Material { Id = 3, Name = "Glass", Code = MaterialCodes.Glass },
            };
            context.Material.AddRange(materials);

            var producerReportedMaterials = new List<ProducerReportedMaterialProjected>
            {
                new ProducerReportedMaterialProjected { ProducerDetailId = 3, MaterialId = 1, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.Household, PackagingTonnage = 25 },
                new ProducerReportedMaterialProjected { ProducerDetailId = 3, MaterialId = 1, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.Household, PackagingTonnage = 75 },
                new ProducerReportedMaterialProjected { ProducerDetailId = 2, MaterialId = 1, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.Household, PackagingTonnage = 100 },
                new ProducerReportedMaterialProjected { ProducerDetailId = 2, MaterialId = 1, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.Household, PackagingTonnage = 200 },
                new ProducerReportedMaterialProjected { ProducerDetailId = 3, MaterialId = 2, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 100 },
                new ProducerReportedMaterialProjected { ProducerDetailId = 3, MaterialId = 2, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 100 },
                new ProducerReportedMaterialProjected { ProducerDetailId = 2, MaterialId = 2, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 50 },
                new ProducerReportedMaterialProjected { ProducerDetailId = 2, MaterialId = 2, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 150 },
                new ProducerReportedMaterialProjected { ProducerDetailId = 1, MaterialId = 3, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 150 },
                new ProducerReportedMaterialProjected { ProducerDetailId = 1, MaterialId = 3, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 150 },
                new ProducerReportedMaterialProjected { ProducerDetailId = 2, MaterialId = 3, SubmissionPeriod = "2025-H1", PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 200 },
                new ProducerReportedMaterialProjected { ProducerDetailId = 2, MaterialId = 3, SubmissionPeriod = "2025-H2", PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 300 },
            };
            context.ProducerReportedMaterialProjected.AddRange(producerReportedMaterials);

            context.SaveChanges();
        }
    }
}
