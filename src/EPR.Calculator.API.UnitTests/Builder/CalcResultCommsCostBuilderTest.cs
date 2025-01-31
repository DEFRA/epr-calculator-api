using AutoFixture;
using EPR.Calculator.API.Builder.CommsCost;
using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Models;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.Calculator.API.UnitTests.Builder
{
    [TestClass]
    public class CalcResultCommsCostBuilderTest
    {
        private readonly CalcResultCommsCostBuilder builder;
        private readonly ApplicationDBContext dbContext;

        public CalcResultCommsCostBuilderTest()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "PayCal")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            dbContext = new ApplicationDBContext(dbContextOptions);
            dbContext.Database.EnsureCreated();
            builder = new CalcResultCommsCostBuilder(dbContext);
        }

        private Fixture Fixture { get; init; } = new Fixture();

        [TestCleanup]
        public void TearDown()
        {
            dbContext?.Database.EnsureDeleted();
        }

        [TestMethod]
        public void ConstructTest()
        {
            CreateMaterials();
            CreateDefaultTemplate();
            CreateDefaultParameters();
            CreateNewRun();
            CreateProducerDetail();
            var resultsRequestDto = new CalcResultsRequestDto { RunId = 1 };
            var apportionment = new CalcResultOnePlusFourApportionment
            {
                Name = Fixture.Create<string>(),
                CalcResultOnePlusFourApportionmentDetails = new List<CalcResultOnePlusFourApportionmentDetail>
                {
                    new CalcResultOnePlusFourApportionmentDetail
                    {
                        Name = Fixture.Create<string>(),
                        EnglandTotal = 40M,
                        ScotlandTotal = 20M,
                        WalesTotal = 20M,
                        NorthernIrelandTotal = 20M,
                        Total = "100%",
                        EnglandDisposalTotal = "40%",
                        ScotlandDisposalTotal = "20%",
                        WalesDisposalTotal = "20%",
                        NorthernIrelandDisposalTotal = "20%"
                    }
                }
            };
            var results = builder.Construct(resultsRequestDto, apportionment);
            results.Wait();
            var result = results.Result;

            Assert.IsNotNull(result);

            Assert.AreEqual("Parameters - Comms Costs", result.Name);

            var onePlusFourApp = result.CalcResultCommsCostOnePlusFourApportionment;
            Assert.IsNotNull(onePlusFourApp);
            Assert.AreEqual(2, onePlusFourApp.Count());
            var headerApp = onePlusFourApp.First();
            Assert.IsTrue(string.IsNullOrEmpty(headerApp.Name));

            Assert.AreEqual("England", headerApp.England);
            Assert.AreEqual("Wales", headerApp.Wales);
            Assert.AreEqual("Northern Ireland", headerApp.NorthernIreland);
            Assert.AreEqual("Scotland", headerApp.Scotland);

            Assert.AreEqual("Total", headerApp.Total);

            var dataApp = result.CalcResultCommsCostOnePlusFourApportionment.Last();
            Assert.IsNotNull(dataApp);

            Assert.AreEqual("1 + 4 Apportionment %s", dataApp.Name );
            Assert.AreEqual("40%", dataApp.England);
            Assert.AreEqual("20%", dataApp.Wales);
            Assert.AreEqual("20%", dataApp.NorthernIreland);
            Assert.AreEqual("20%", dataApp.Scotland);
            Assert.AreEqual("100%", dataApp.Total);


            var materialCosts = result.CalcResultCommsCostCommsCostByMaterial.ToList();
            Assert.IsNotNull(materialCosts);
            Assert.AreEqual(10, materialCosts.Count);

            var materialHeader = materialCosts.First();

            Assert.IsNotNull(materialHeader);

            Assert.AreEqual("2a Comms Costs - by Material", materialHeader.Name);
            Assert.AreEqual("England", materialHeader.England);
            Assert.AreEqual("Wales", materialHeader.Wales);
            Assert.AreEqual("Scotland", materialHeader.Scotland);
            Assert.AreEqual("Northern Ireland", materialHeader.NorthernIreland);
            Assert.AreEqual("Total", materialHeader.Total);
            Assert.AreEqual("Producer Reported Household Packaging Waste Tonnage",
                materialHeader.ProducerReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual("Reported Public Bin Tonnage", materialHeader.ReportedPublicBinTonnage);
            Assert.AreEqual("Household Drinks Containers", materialHeader.HouseholdDrinksContainers);
            Assert.AreEqual("Late Reporting Tonnage", materialHeader.LateReportingTonnage);
            Assert.AreEqual("Producer Reported Household Packaging Waste Tonnage + Late Reporting Tonnage + Report Public Bin Tonnage + Household Drinks Containers",
                materialHeader.ProducerReportedHouseholdPlusLateReportingTonnage);
            Assert.AreEqual("Comms Cost - by Material Price Per Tonne",
                materialHeader.CommsCostByMaterialPricePerTonne);

            var aluminiumCost = materialCosts[1];
            Assert.AreEqual("Aluminium", aluminiumCost.Name);
            Assert.AreEqual("£4.00", aluminiumCost.England);
            Assert.AreEqual("£2.00", aluminiumCost.Wales);
            Assert.AreEqual("£2.00", aluminiumCost.Scotland);
            Assert.AreEqual("£2.00", aluminiumCost.NorthernIreland);
            Assert.AreEqual("£10.00", aluminiumCost.Total);
            Assert.AreEqual("1000.000",
                aluminiumCost.ProducerReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual("10.000", aluminiumCost.LateReportingTonnage);
            Assert.AreEqual("1010.000",
                aluminiumCost.ProducerReportedHouseholdPlusLateReportingTonnage);
            Assert.AreEqual("0.0099",
                aluminiumCost.CommsCostByMaterialPricePerTonne);


            var fibreCompositeCost = materialCosts[2];
            Assert.AreEqual("Fibre composite", fibreCompositeCost.Name);
            Assert.AreEqual("£4.00", fibreCompositeCost.England);
            Assert.AreEqual("£2.00", fibreCompositeCost.Wales);
            Assert.AreEqual("£2.00", fibreCompositeCost.Scotland);
            Assert.AreEqual("£2.00", fibreCompositeCost.NorthernIreland);
            Assert.AreEqual("£10.00", fibreCompositeCost.Total);
            Assert.AreEqual("2000.000",
                fibreCompositeCost.ProducerReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual("10.000", fibreCompositeCost.LateReportingTonnage);
            Assert.AreEqual("2010.000",
                fibreCompositeCost.ProducerReportedHouseholdPlusLateReportingTonnage);
            Assert.AreEqual("0.0050",
                fibreCompositeCost.CommsCostByMaterialPricePerTonne);

            var totalMaterialCost = materialCosts.Last();
            Assert.AreEqual("Total", totalMaterialCost.Name);
            Assert.AreEqual("£32.00", totalMaterialCost.England);
            Assert.AreEqual("£16.00", totalMaterialCost.Wales);
            Assert.AreEqual("£16.00", totalMaterialCost.Scotland);
            Assert.AreEqual("£16.00", totalMaterialCost.NorthernIreland);
            Assert.AreEqual("£80.00", totalMaterialCost.Total);
            Assert.AreEqual("36000.000",
                totalMaterialCost.ProducerReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual("80.000", totalMaterialCost.LateReportingTonnage);
            Assert.AreEqual("36080.000",
                totalMaterialCost.ProducerReportedHouseholdPlusLateReportingTonnage);
            Assert.IsTrue(string.IsNullOrEmpty(totalMaterialCost.CommsCostByMaterialPricePerTonne));
        }

        [TestMethod]
        public async Task GetProducerReportedMaterials_ShouldReturnValidMaterials()
        {
            // Arrange
            SeedDatabase(dbContext);
            var runId = 1;

            // Act
            var result = await builder.GetProducerReportedMaterials(dbContext, runId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);
            Assert.IsTrue(result.Any(r => r.PackagingType == PackagingTypes.Household));
            Assert.IsTrue(result.Any(r => r.PackagingType == PackagingTypes.PublicBin));
            Assert.IsTrue(result.Any(r => r.PackagingType == PackagingTypes.HouseholdDrinksContainers && r.MaterialId == 3));
            Assert.IsTrue(result.Any(r => r.MaterialId == 1));
            Assert.IsTrue(result.Any(r => r.MaterialId == 2));
            Assert.IsTrue(result.Any(r => r.MaterialId == 3));
            Assert.IsTrue(result.Any(r => r.PackagingTonnage == 100));
            Assert.IsTrue(result.Any(r => r.PackagingTonnage == 200));
            Assert.IsTrue(result.Any(r => r.PackagingTonnage == 300));
            Assert.IsTrue(result.Any(r => r.PackagingType == PackagingTypes.Household));
            Assert.IsTrue(result.Any(r => r.PackagingType == PackagingTypes.PublicBin));
            Assert.IsTrue(result.Any(r => r.PackagingType == PackagingTypes.HouseholdDrinksContainers && r.MaterialId == 3));
        }

        private void SeedDatabase(ApplicationDBContext context)
        {
            var run = new CalculatorRun { Id = 1 , Financial_Year ="2024-25" , Name ="CalculatorRunTest1" };
            context.CalculatorRuns.Add(run);

            var producerDetail = new ProducerDetail { Id = 1, CalculatorRunId = 1 };
            context.ProducerDetail.Add(producerDetail);

            var materials = new List<Material>
        {
            new Material { Id = 1, Name ="Plastic", Code = MaterialCodes.Plastic },
            new Material { Id = 2, Name ="Steel",Code = MaterialCodes.Steel },
            new Material { Id = 3, Name ="Glass",Code = MaterialCodes.Glass }
        };
            context.Material.AddRange(materials);

            var producerReportedMaterials = new List<ProducerReportedMaterial>
        {
            new ProducerReportedMaterial { Id = 1, ProducerDetailId = 1, MaterialId = 1, PackagingType = PackagingTypes.Household, PackagingTonnage = 100 },
            new ProducerReportedMaterial { Id = 2, ProducerDetailId = 1, MaterialId = 2, PackagingType = PackagingTypes.PublicBin, PackagingTonnage = 200 },
            new ProducerReportedMaterial { Id = 3, ProducerDetailId = 1, MaterialId = 3, PackagingType = PackagingTypes.HouseholdDrinksContainers, PackagingTonnage = 300 }
        };
            context.ProducerReportedMaterial.AddRange(producerReportedMaterials);

            context.SaveChanges();
        }

        private void CreateProducerDetail()
        {
            var producerNames = new string[]
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
                    CalculatorRunId = 1,
                });
            }

            dbContext.SaveChanges();

            for (int producerDetailId = 1; producerDetailId <= 10; producerDetailId++)
            {
                for (int materialId = 1; materialId < 9; materialId++)
                {
                    dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial
                    {
                        MaterialId = materialId,
                        ProducerDetailId = producerDetailId,
                        PackagingType = "HH",
                        PackagingTonnage = (materialId * 100)
                    });
                }
            }
            dbContext.SaveChanges();
        }

        private void CreateDefaultTemplate()
        {
            dbContext.DefaultParameterTemplateMasterList.RemoveRange(
                dbContext.DefaultParameterTemplateMasterList.ToList());
            dbContext.SaveChanges();

            var materialDictionary = new Dictionary<string, string>
            {
                { "AL", "Aluminium" },
                { "FC", "Fibre composite" },
                { "GL", "Glass" },
                { "PC", "Paper or card" },
                { "PL", "Plastic" },
                { "ST", "Steel" },
                { "WD", "Wood" },
                { "OT", "Other materials" }
            };

            var parameterTypes = new string[] { "Communication costs by material", "Late reporting tonnage" };
            foreach (var material in materialDictionary.Values)
            {
                dbContext.DefaultParameterTemplateMasterList.Add(new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = Guid.NewGuid().ToString(),
                    ParameterCategory = material,
                    ParameterType = parameterTypes[0]
                });
                dbContext.DefaultParameterTemplateMasterList.Add(new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = Guid.NewGuid().ToString(),
                    ParameterCategory = material,
                    ParameterType = parameterTypes[1]
                });
            }

            var countries = new[]
            {
                "England",
                "Northern Ireland",
                "Scotland",
                "United Kingdom",
                "Wales"
            };

            foreach (var country in countries)
            {
                dbContext.DefaultParameterTemplateMasterList.Add(new DefaultParameterTemplateMaster
                {
                    ParameterUniqueReferenceId = Guid.NewGuid().ToString(),
                    ParameterCategory = country,
                    ParameterType = "Communication costs by country"
                });
            }

            dbContext.SaveChanges();
        }

        private void CreateNewRun()
        {
            var run = new CalculatorRun
            {
                CalculatorRunClassificationId = (int)RunClassification.RUNNING,
                Name = "Test Run",
                Financial_Year = "2024-25",
                CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                CreatedBy = "Test User",
                DefaultParameterSettingMasterId = 1
            };
            dbContext.CalculatorRuns.Add(run);
            dbContext.SaveChanges();
        }

        private void CreateDefaultParameters()
        {
            var templateMasterList = dbContext.DefaultParameterTemplateMasterList.ToList();

            var defaultMaster = new DefaultParameterSettingMaster
            {
                ParameterYear = "2024-25"
            };

            dbContext.DefaultParameterSettings.Add(defaultMaster);
            dbContext.SaveChanges();

            foreach (var templateMaster in templateMasterList)
            {
                var defaultDetail = new DefaultParameterSettingDetail
                {
                    ParameterUniqueReferenceId = templateMaster.ParameterUniqueReferenceId,
                    ParameterValue = GetValue(templateMaster),
                    DefaultParameterSettingMasterId = 1,
                    DefaultParameterSettingMaster = defaultMaster,
                };
                dbContext.DefaultParameterSettingDetail.Add(defaultDetail);
            }
            dbContext.SaveChanges();
        }

        private static decimal GetValue(DefaultParameterTemplateMaster templateMaster)
        {
            if (templateMaster.ParameterType == "Communication costs by material")
            {
                switch (templateMaster.ParameterCategory)
                {
                    case "England":
                        return 40M;
                    case "Northern Ireland":
                        return 10M;
                    case "Scotland":
                        return 20M;
                    case "Wales":
                        return 30M;
                }
            }

            return 10;
        }

        private void CreateMaterials()
        {
            var materialDictionary = new Dictionary<string, string>();
            materialDictionary.Add("AL", "Aluminium");
            materialDictionary.Add("FC", "Fibre composite");
            materialDictionary.Add("GL", "Glass");
            materialDictionary.Add("PC", "Paper or card");
            materialDictionary.Add("PL", "Plastic");
            materialDictionary.Add("ST", "Steel");
            materialDictionary.Add("WD", "Wood");
            materialDictionary.Add("OT", "Other materials");

            foreach (var materialKv in materialDictionary)
            {
                dbContext.Material.Add(new Material
                {
                    Name = materialKv.Value,
                    Code = materialKv.Key,
                    Description = "Some"
                });
            }

            dbContext.SaveChanges();
        }
    }
}
