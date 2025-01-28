namespace EPR.Calculator.API.UnitTests.Builder
{
    using EPR.Calculator.API.Builder.LaDisposalCost;
    using EPR.Calculator.API.Constants;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.API.Dtos;
    using EPR.Calculator.API.Enums;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using System;

    [TestClass]
    public class CalcRunLaDisposalCostBuilderTests
    {
        private readonly CalcRunLaDisposalCostBuilder builder;
        private readonly ApplicationDBContext dbContext;

        public CalcRunLaDisposalCostBuilderTests()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "PayCal")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            dbContext = new ApplicationDBContext(dbContextOptions);
            dbContext.Database.EnsureCreated();

            dbContext.ProducerReportedMaterial.AddRange(GetProducerReportedMaterials());
            dbContext.SaveChanges();

            builder = new CalcRunLaDisposalCostBuilder(dbContext);           
        }

        [TestCleanup]
        public void TearDown()
        {
            dbContext?.Database.EnsureDeleted();
        }

        [TestMethod]
        public void Should_Return_LA_Disposal_Costs()
        {
            // Assign
            var resultsDto = new CalcResultsRequestDto { RunId = 2 };
            var calcResult = TestDataHelper.GetCalcResult();

            // Act
            var results = builder.Construct(resultsDto, calcResult);
            results.Wait();
            var lapcapDisposalCostResults = results.Result;

            // Assert
            Assert.IsNotNull(lapcapDisposalCostResults);
            Assert.AreEqual(CommonConstants.LADisposalCostData, lapcapDisposalCostResults.Name);
            Assert.AreEqual(10, lapcapDisposalCostResults.CalcResultLaDisposalCostDetails?.Count());
        }

        [TestMethod]
        public void Should_Return_HeaderRow()
        {
            // Assign
            var resultsDto = new CalcResultsRequestDto { RunId = 2 };
            var calcResult = TestDataHelper.GetCalcResult();

            // Act
            var results = builder.Construct(resultsDto, calcResult);
            results.Wait();
            var lapcapDisposalCostResults = results.Result;

            // Assert
            var headerRow = lapcapDisposalCostResults.CalcResultLaDisposalCostDetails?.Single(x => x.OrderId == 1);
            Assert.IsNotNull(headerRow);
            Assert.AreEqual(CommonConstants.Material, headerRow.Name);
            Assert.AreEqual(CommonConstants.England, headerRow.England);
            Assert.AreEqual(CommonConstants.Wales, headerRow.Wales);
            Assert.AreEqual(CommonConstants.Scotland, headerRow.Scotland);
            Assert.AreEqual(CommonConstants.NorthernIreland, headerRow.NorthernIreland);
            Assert.AreEqual(CommonConstants.Total, headerRow.Total);
            Assert.AreEqual(CommonConstants.ProducerReportedHouseholdPackagingWasteTonnage, headerRow.ProducerReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual(CommonConstants.ReportedPublicBinTonnage, headerRow.ReportedPublicBinTonnage);
            Assert.AreEqual(CommonConstants.HouseholdDrinkContainers, headerRow.HouseholdDrinkContainers);
            Assert.AreEqual(CommonConstants.LateReportingTonnage, headerRow.LateReportingTonnage);
            Assert.AreEqual(CommonConstants.ProducerReportedTotalTonnage, headerRow.ProducerReportedTotalTonnage);
            Assert.AreEqual(CommonConstants.DisposalCostPricePerTonne, headerRow.DisposalCostPricePerTonne);
        }

        [TestMethod]
        public void Should_Return_Material_Data_With_PublicBin()
        {
            // Assign
            var resultsDto = new CalcResultsRequestDto { RunId = 2 };
            var calcResult = TestDataHelper.GetCalcResult();

            // Act
            var results = builder.Construct(resultsDto, calcResult);
            results.Wait();
            var lapcapDisposalCostResults = results.Result;

            // Assert
            var laDisposalCost = lapcapDisposalCostResults.CalcResultLaDisposalCostDetails?.Single(x => x.Name == MaterialNames.Plastic);
            Assert.IsNotNull(laDisposalCost);
            Assert.AreEqual(MaterialNames.Plastic, laDisposalCost.Name);
            Assert.AreEqual("£23,000.00", laDisposalCost.England);
            Assert.AreEqual("£4,500.00", laDisposalCost.Wales);
            Assert.AreEqual("£6,700.00", laDisposalCost.Scotland);
            Assert.AreEqual("£2,100.00", laDisposalCost.NorthernIreland);
            Assert.AreEqual("£36,300.00", laDisposalCost.Total);
            Assert.AreEqual("2000.00", laDisposalCost.ProducerReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual("2000.00", laDisposalCost.ReportedPublicBinTonnage);
            Assert.AreEqual(string.Empty, laDisposalCost.HouseholdDrinkContainers);
            Assert.AreEqual("2000.00", laDisposalCost.LateReportingTonnage);
            Assert.AreEqual("6000.00", laDisposalCost.ProducerReportedTotalTonnage);
            Assert.AreEqual("£6.0500", laDisposalCost.DisposalCostPricePerTonne);
        }

        [TestMethod]
        public void Should_Return_Material_Data_With_Household_Drink_Containers()
        {
            // Assign
            var resultsDto = new CalcResultsRequestDto { RunId = 2 };
            var calcResult = TestDataHelper.GetCalcResult();

            // Act
            var results = builder.Construct(resultsDto, calcResult);
            results.Wait();
            var lapcapDisposalCostResults = results.Result;

            // Assert
            var laDisposalCost = lapcapDisposalCostResults.CalcResultLaDisposalCostDetails?.Single(x => x.Name == MaterialNames.Glass);
            Assert.IsNotNull(laDisposalCost);
            Assert.AreEqual(MaterialNames.Glass, laDisposalCost.Name);
            Assert.AreEqual("£45,000.00", laDisposalCost.England);
            Assert.AreEqual("£0.00", laDisposalCost.Wales);
            Assert.AreEqual("£20,700.00", laDisposalCost.Scotland);
            Assert.AreEqual("£4,500.00", laDisposalCost.NorthernIreland);
            Assert.AreEqual("£70,200.00", laDisposalCost.Total);
            Assert.AreEqual("2000.00", laDisposalCost.ProducerReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual("0", laDisposalCost.ReportedPublicBinTonnage);
            Assert.AreEqual("2000.00", laDisposalCost.HouseholdDrinkContainers);
            Assert.AreEqual("0", laDisposalCost.LateReportingTonnage);
            Assert.AreEqual("4000.00", laDisposalCost.ProducerReportedTotalTonnage);
            Assert.AreEqual("£17.5500", laDisposalCost.DisposalCostPricePerTonne);
        }

        private static IList<ProducerReportedMaterial> GetProducerReportedMaterials()
        {
            var run = new CalculatorRun
            {
                Id = 2,
                CalculatorRunClassificationId = (int)RunClassification.RUNNING,
                Name = "Test Run",
                Financial_Year = "2024-25",
                CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                CreatedBy = "Test User",
                LapcapDataMasterId = 2
            };

            var aluminiumMaterial = new Material()
            {
                Code = MaterialCodes.Aluminium,
                Name = MaterialNames.Aluminium,
                Description = MaterialNames.Aluminium
            };
            var plasticMaterial = new Material()
            {
                Code = MaterialCodes.Plastic,
                Name = MaterialNames.Plastic,
                Description = MaterialNames.Plastic
            };
            var glassMaterial = new Material()
            {
                Code = MaterialCodes.Glass,
                Name = MaterialNames.Glass,
                Description = MaterialNames.Glass
            };

            var producer = new ProducerDetail
            {
                CalculatorRunId = 2,
                ProducerId = 1,
                ProducerName = "Producer Name",
                CalculatorRun = run
            };

            return new List<ProducerReportedMaterial>()
            {
                new()
                {
                    Material = aluminiumMaterial,
                    PackagingTonnage = 1000.00m,
                    PackagingType = PackagingTypes.Household,
                    MaterialId = 1,
                    ProducerDetail = producer
                },
                new()
                {
                    Material = plasticMaterial,
                    PackagingTonnage = 2000.00m,
                    PackagingType = PackagingTypes.Household,
                    MaterialId = 2,
                    ProducerDetail = producer
                },
                new()
                {
                    Material = plasticMaterial,
                    PackagingTonnage = 2000.00m,
                    PackagingType = PackagingTypes.ConsumerWaste,
                    MaterialId = 2,
                    ProducerDetail = producer
                },
                new()
                {
                    Material = plasticMaterial,
                    PackagingTonnage = 2000.00m,
                    PackagingType = PackagingTypes.PublicBin,
                    MaterialId = 2,
                    ProducerDetail = producer
                },
                new()
                {
                    Material = glassMaterial,
                    PackagingTonnage = 2000.00m,
                    PackagingType = PackagingTypes.Household,
                    MaterialId = 3,
                    ProducerDetail = producer
                },
                new()
                {
                    Material = glassMaterial,
                    PackagingTonnage = 2000.00m,
                    PackagingType = PackagingTypes.HouseholdDrinksContainers,
                    MaterialId = 3,
                    ProducerDetail = producer
                }
            };
        }
    }
}