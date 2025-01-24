namespace EPR.Calculator.API.UnitTests.Builder
{
    using AutoFixture;
    using EPR.Calculator.API.Builder.LaDisposalCost;
    using EPR.Calculator.API.Constants;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.API.Dtos;
    using EPR.Calculator.API.Enums;
    using EPR.Calculator.API.Models;
    using EPR.Calculator.API.UnitTests.Controllers;
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
            this.Fixture = new Fixture();
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                                    .UseInMemoryDatabase(databaseName: "PayCal")
                                    .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

            dbContext = new ApplicationDBContext(dbContextOptions);
            dbContext.Database.EnsureCreated();
            dbContext.DefaultParameterTemplateMasterList.RemoveRange(dbContext.DefaultParameterTemplateMasterList);
            dbContext.SaveChanges();
            dbContext.DefaultParameterTemplateMasterList.AddRange(BaseControllerTest.GetDefaultParameterTemplateMasterData().ToList());
            dbContext.SaveChanges();

            builder = new CalcRunLaDisposalCostBuilder(dbContext);           
        }

        private Fixture Fixture { get; init; }

        [TestCleanup]
        public void TearDown()
        {
            dbContext?.Database.EnsureDeleted();
        }

        [TestMethod]
        public void ConstructTest_For_LA_DisposalCost()
        {
            const string aluminium = "Aluminium";
            const string plastic = "Plastic";
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

            var material = new Material() { Code = "AL", Name = "Aluminium", Description = "Aluminium" };
            var plasticMaterial = new Material() { Code = "PL", Name = "Plastic", Description = "Plastic" };

            var producer = new ProducerDetail { CalculatorRunId = 2, ProducerId = 1, ProducerName = "Producer Name", CalculatorRun = run };
            

            dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial { Material = material, PackagingTonnage = 1000.00m, PackagingType = "HH", MaterialId = 1, ProducerDetail = producer });
            dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial { Material = plasticMaterial, PackagingTonnage = 2000.00m, PackagingType = "HH", MaterialId = 2, ProducerDetail = producer });
            dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial { Material = plasticMaterial, PackagingTonnage = 2000.00m, PackagingType = "CW", MaterialId = 2, ProducerDetail = producer });

            dbContext.SaveChanges();

            var resultsDto = new CalcResultsRequestDto { RunId = 2 };
            var calcResult = new CalcResult
            {
                CalcResultParameterOtherCost = Fixture.Create<CalcResultParameterOtherCost>(),
                CalcResultDetail = new CalcResultDetail
                {
                    RunName = "TestValue1471524307",
                    RunId = 939003072,
                    RunDate = DateTime.UtcNow,
                    RunBy = "TestValue1268476870",
                    FinancialYear = "TestValue2118326334",
                    RpdFileORG = "TestValue1264803979",
                    RpdFilePOM = "TestValue10102480",
                    LapcapFile = "TestValue702897241",
                    ParametersFile = "TestValue1161721091"
                },
                CalcResultLapcapData = new CalcResultLapcapData
                {
                    Name = "LAPCAP Data",
                    CalcResultLapcapDataDetails = new[] {
                        new CalcResultLapcapDataDetails
                        {
                            Name = "Aluminium",
                            EnglandDisposalCost = "£100.00",
                            WalesDisposalCost = "£200.00",
                            ScotlandDisposalCost = "£300.00",
                            NorthernIrelandDisposalCost = "£400.00",
                            TotalDisposalCost = "£1000.00",
                            OrderId = 2
                        } ,
                    new CalcResultLapcapDataDetails
                    {
                        Name = "Plastic",
                        EnglandDisposalCost = "£200.00",
                        WalesDisposalCost = "£300.00",
                        ScotlandDisposalCost = "£400.00",
                        NorthernIrelandDisposalCost = "£500.00",
                        TotalDisposalCost = "£1400.00",
                        OrderId = 3
                    },
                        new CalcResultLapcapDataDetails
                    {
                        Name = "Total",
                        EnglandDisposalCost = "£300.00",
                        WalesDisposalCost = "£500.00",
                        ScotlandDisposalCost = "£700.00",
                        NorthernIrelandDisposalCost = "£900.00",
                        TotalDisposalCost = "£2400.00",
                        OrderId = 4
                    }
                }
            },
                 CalcResultLateReportingTonnageData = new CalcResultLateReportingTonnage { 
                     Name = "Late Reporting Tonnage",                     
                     CalcResultLateReportingTonnageDetails = new[]
                     {
                         new CalcResultLateReportingTonnageDetail()
                         {
                              Name = "Aluminium",
                               TotalLateReportingTonnage = 8000.00m
                         },
                          new CalcResultLateReportingTonnageDetail()
                         {
                              Name = "Plastic",
                               TotalLateReportingTonnage = 2000.00m
                         },
                           new CalcResultLateReportingTonnageDetail()
                         {
                              Name = "Total",
                               TotalLateReportingTonnage = 10000.00m
                         }
                     }               
                 
                 }

            };



            var results = builder.Construct(resultsDto, calcResult);
            results.Wait();
            var lapcapDisposalCostResults = results.Result;

            Assert.IsNotNull(lapcapDisposalCostResults);
            Assert.AreEqual(CommonConstants.LADisposalCostData, lapcapDisposalCostResults.Name);
            Assert.AreEqual(4, lapcapDisposalCostResults.CalcResultLaDisposalCostDetails?.Count());

            var headerRow = lapcapDisposalCostResults.CalcResultLaDisposalCostDetails?.Single(x => x.OrderId == 1);
            Assert.IsNotNull(headerRow);
            Assert.AreEqual(CommonConstants.Material, headerRow.Name);
            Assert.AreEqual(CommonConstants.England, headerRow.England);
            Assert.AreEqual(CommonConstants.Wales, headerRow.Wales);
            Assert.AreEqual(CommonConstants.Scotland, headerRow.Scotland);
            Assert.AreEqual(CommonConstants.NorthernIreland, headerRow.NorthernIreland);
            Assert.AreEqual(CommonConstants.Total, headerRow.Total);
            Assert.AreEqual(CommonConstants.ProducerReportedHouseholdPackagingWasteTonnage, headerRow.ProducerReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual(CommonConstants.LateReportingTonnage, headerRow.LateReportingTonnage);
            Assert.AreEqual(CommonConstants.ProduceLateTonnage, headerRow.ProducerReportedTotalTonnage);
            Assert.AreEqual(CommonConstants.DisposalCostPricePerTonne, headerRow.DisposalCostPricePerTonne);


            var aluminiumRow = lapcapDisposalCostResults.CalcResultLaDisposalCostDetails?.Single(x => x.Name == aluminium);
            Assert.IsNotNull(aluminiumRow);
            Assert.AreEqual(aluminium, aluminiumRow.Name);
            Assert.AreEqual("£100.00", aluminiumRow.England);
            Assert.AreEqual("£200.00", aluminiumRow.Wales);
            Assert.AreEqual("£300.00", aluminiumRow.Scotland);
            Assert.AreEqual("£400.00", aluminiumRow.NorthernIreland);
            Assert.AreEqual("£1000.00", aluminiumRow.Total);
            Assert.AreEqual("1000.00", aluminiumRow.ProducerReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual("8000.00", aluminiumRow.LateReportingTonnage);
            Assert.AreEqual("9000.00", aluminiumRow.ProducerReportedTotalTonnage);

            var plasticRow = lapcapDisposalCostResults.CalcResultLaDisposalCostDetails?.Single(x => x.Name == plastic);
            Assert.IsNotNull(plasticRow);
            Assert.AreEqual(plastic, plasticRow.Name);
            Assert.AreEqual("£200.00", plasticRow.England);
            Assert.AreEqual("£300.00", plasticRow.Wales);
            Assert.AreEqual("£400.00", plasticRow.Scotland);
            Assert.AreEqual("£500.00", plasticRow.NorthernIreland);
            Assert.AreEqual("£1400.00", plasticRow.Total);
            Assert.AreEqual("2000.00", plasticRow.ProducerReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual("2000.00", plasticRow.LateReportingTonnage);
            Assert.AreEqual("4000.00", plasticRow.ProducerReportedTotalTonnage);

            var totalRow = lapcapDisposalCostResults.CalcResultLaDisposalCostDetails?.Single(x => x.Name == "Total");
            Assert.IsNotNull(aluminiumRow);
            Assert.AreEqual("Total", totalRow?.Name);
            Assert.AreEqual("£300.00", totalRow?.England);
            Assert.AreEqual("£500.00", totalRow?.Wales);
            Assert.AreEqual("£700.00", totalRow?.Scotland);
            Assert.AreEqual("£900.00", totalRow?.NorthernIreland);
            Assert.AreEqual("£2400.00", totalRow?.Total);
            Assert.AreEqual("3000.00", totalRow?.ProducerReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual("10000.00", totalRow?.LateReportingTonnage);
            Assert.AreEqual("13000.00", totalRow?.ProducerReportedTotalTonnage);
            Assert.IsNull(totalRow?.DisposalCostPricePerTonne);
        }
    }
}