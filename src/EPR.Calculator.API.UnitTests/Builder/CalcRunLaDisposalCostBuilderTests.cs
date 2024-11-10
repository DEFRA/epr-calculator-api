namespace EPR.Calculator.API.UnitTests.Builder
{
    using System;
    using EPR.Calculator.API.Builder.LaDisposalCost;
    using EPR.Calculator.API.Constants;
    using EPR.Calculator.API.Data;
    using EPR.Calculator.API.Data.DataModels;
    using EPR.Calculator.API.Dtos;
    using EPR.Calculator.API.Enums;
    using EPR.Calculator.API.Models;
    using EPR.Calculator.API.Tests.Controllers;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class CalcRunLaDisposalCostBuilderTests
    {
        private CalcRunLaDisposalCostBuilder builder;
        private ApplicationDBContext dbContext;       

        [TestInitialize]
        public void DataSetup()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                                    .UseInMemoryDatabase(databaseName: "PayCal")
                                    .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

            dbContext = new ApplicationDBContext(dbContextOptions);
            dbContext.Database.EnsureCreated();
            dbContext.DefaultParameterTemplateMasterList.RemoveRange(dbContext.DefaultParameterTemplateMasterList);
            dbContext.SaveChanges();
            dbContext.DefaultParameterTemplateMasterList.AddRange(BaseControllerTest.GetDefaultParameterTemplateMasterData().ToList());
            // dbContext.LapcapDataTemplateMaster.AddRange(BaseControllerTest.GetLapcapTemplateMasterData().ToList());
            dbContext.SaveChanges();

            builder = new CalcRunLaDisposalCostBuilder(dbContext);           
        }


        [TestMethod]
        public void ConstructTest_For_Aluminium()
        {
            const string aluminium = "Aluminium";
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

            var producer = new ProducerDetail { CalculatorRunId = 2, ProducerId = 1, ProducerName = "Producer Name", CalculatorRun = run };

            dbContext.ProducerReportedMaterial.Add(new ProducerReportedMaterial { Material = material, PackagingTonnage = 1000.00m, PackagingType = "CW", MaterialId = 2, ProducerDetail = producer });         
            

            dbContext.SaveChanges();

            var resultsDto = new CalcResultsRequestDto { RunId = 2 };
            var calcResult = new CalcResult
            {
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
                        } }
                },
                 CalcResultLateReportingTonnageData = new CalcResultLateReportingTonnage { 
                     Name = "Late Reporting Tonnage",                     
                     CalcResultLateReportingTonnageDetails = new[]
                     {
                         new CalcResultLateReportingTonnageDetail()
                         {
                              Name = "Aluminium",
                               TotalLateReportingTonnage = 8000.00m
                         }
                     }               
                 
                 }

            };



            var lapcapDisposalCostResults = builder.Construct(resultsDto, calcResult);

            Assert.IsNotNull(lapcapDisposalCostResults);
            Assert.AreEqual(lapcapDisposalCostResults.Name, CommonConstants.LADisposalCostData);
            Assert.AreEqual(lapcapDisposalCostResults?.CalcResultLaDisposalCostDetails?.Count(), 2);

            var headerRow = lapcapDisposalCostResults?.CalcResultLaDisposalCostDetails?.Single(x => x.OrderId == 1);
            Assert.IsNotNull(headerRow);
            Assert.AreEqual(CommonConstants.Material, headerRow.Name);
            Assert.AreEqual(CommonConstants.England, headerRow.England);
            Assert.AreEqual(CommonConstants.Wales, headerRow.Wales);
            Assert.AreEqual(CommonConstants.Scotland, headerRow.Scotland);
            Assert.AreEqual(CommonConstants.NorthernIreland, headerRow.NorthernIreland);
            Assert.AreEqual(CommonConstants.Total, headerRow.Total);
            Assert.AreEqual(CommonConstants.ProducerReportedHouseholdPackagingWasteTonnage, headerRow.ProducerReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual(CommonConstants.LateReportingTonnage, headerRow.LateReportingTonnage);
            Assert.AreEqual(CommonConstants.ProduceLateTonnage, headerRow.ProducerReportedHouseholdTonnagePlusLateReportingTonnage);
            Assert.AreEqual(CommonConstants.DisposalCostPricePerTonne, headerRow.DisposalCostPricePerTonne);


            var aluminiumRow = lapcapDisposalCostResults?.CalcResultLaDisposalCostDetails?.Single(x => x.Name == aluminium);
            Assert.IsNotNull(aluminiumRow);
            Assert.AreEqual(aluminium, aluminiumRow.Name);
            Assert.AreEqual("£100.00", aluminiumRow.England);
            Assert.AreEqual("£200.00", aluminiumRow.Wales);
            Assert.AreEqual("£300.00", aluminiumRow.Scotland);
            Assert.AreEqual("£400.00", aluminiumRow.NorthernIreland);
            Assert.AreEqual("£1000.00", aluminiumRow.Total);
            Assert.AreEqual("1000.00", aluminiumRow.ProducerReportedHouseholdPackagingWasteTonnage);
            Assert.AreEqual("8000.00", aluminiumRow.LateReportingTonnage);
            Assert.AreEqual("9000.00", aluminiumRow.ProducerReportedHouseholdTonnagePlusLateReportingTonnage);
            Assert.AreEqual("£0.11", aluminiumRow.DisposalCostPricePerTonne);
        }
    }
}