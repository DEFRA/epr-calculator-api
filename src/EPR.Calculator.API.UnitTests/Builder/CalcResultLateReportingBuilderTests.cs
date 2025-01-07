using EPR.Calculator.API.Builder;
using EPR.Calculator.API.Builder.LateReportingTonnages;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EPR.Calculator.API.UnitTests.Builder
{
    [TestClass]
    public class CalcResultLateReportingBuilderTest
    {
        public required CalcResultLateReportingBuilder builder;
        protected ApplicationDBContext? dbContext;

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
            dbContext.DefaultParameterSettingDetail.RemoveRange(dbContext.DefaultParameterSettingDetail);
            dbContext.CalculatorRuns.RemoveRange(dbContext.CalculatorRuns);
            dbContext.SaveChanges();

            var calculatorRuns = new List<CalculatorRun>
            {
                new() { Id = 1,
                        DefaultParameterSettingMasterId = 1,
                        CalculatorRunClassificationId = (int)RunClassification.RUNNING,
                        Name = "Test Run",
                        Financial_Year = "2024-25",
                        CreatedAt = new DateTime(2024, 8, 28, 10, 12, 30, DateTimeKind.Utc),
                        CreatedBy = "Test User",
                        LapcapDataMasterId = 2
                      }
            };

            var defaultParameterSettings = new List<DefaultParameterSettingMaster>
            {
                new() { Id = 1 }
            };

            var defaultParameterTemplateMasterList = new List<DefaultParameterTemplateMaster>
            {
                new() { ParameterUniqueReferenceId = "1", ParameterType = "Late Reporting Tonnage", ParameterCategory = "Aluminium" },
                new() { ParameterUniqueReferenceId = "2", ParameterType = "Late Reporting Tonnage", ParameterCategory = "Fibre composite" }
            };

            var defaultParameterSettingDetails = new List<DefaultParameterSettingDetail>
            {
                new()
                {
                    DefaultParameterSettingMasterId = 1,
                    ParameterUniqueReferenceId = "1",
                    ParameterValue = 100,
                    DefaultParameterSettingMaster = defaultParameterSettings[0],
                },
                new()
                {
                    DefaultParameterSettingMasterId = 1,
                    ParameterUniqueReferenceId = "2",
                    ParameterValue = 200,
                    DefaultParameterSettingMaster = defaultParameterSettings[0],
                }
            };

            dbContext.CalculatorRuns.AddRange(calculatorRuns);
            dbContext.DefaultParameterSettings.AddRange(defaultParameterSettings);
            dbContext.DefaultParameterSettingDetail.AddRange(defaultParameterSettingDetails);
            dbContext.DefaultParameterTemplateMasterList.AddRange(defaultParameterTemplateMasterList);
            dbContext.SaveChanges();

            builder = new CalcResultLateReportingBuilder(dbContext);
        }

        public ApplicationDBContext? GetDbContext()
        {
            return dbContext;
        }

        [TestMethod]
        public void Construct_ShouldReturnCorrectResults()
        {
            var requestDto = new CalcResultsRequestDto { RunId = 1 };

            var results = builder.Construct(requestDto);
            results.Wait();
            var result = results.Result;

            Assert.IsNotNull(result);
            Assert.AreEqual(CalcResultLateReportingBuilder.LateReportingHeader, result.Name);
            Assert.AreEqual(CalcResultLateReportingBuilder.MaterialHeading, result.MaterialHeading);
            Assert.AreEqual(CalcResultLateReportingBuilder.TonnageHeading, result.TonnageHeading);
            Assert.AreEqual(3, result.CalcResultLateReportingTonnageDetails?.Count());

            var material1 = result.CalcResultLateReportingTonnageDetails?.SingleOrDefault(x => x.Name == "Aluminium");
            Assert.IsNotNull(material1);
            Assert.AreEqual(100.000M, material1.TotalLateReportingTonnage);

            var material2 = result.CalcResultLateReportingTonnageDetails?.SingleOrDefault(x => x.Name == "Fibre composite");
            Assert.IsNotNull(material2);
            Assert.AreEqual(200.000M, material2.TotalLateReportingTonnage);

            var total = result.CalcResultLateReportingTonnageDetails?.SingleOrDefault(x => x.Name == CalcResultLateReportingBuilder.Total);
            Assert.IsNotNull(total);
            Assert.AreEqual(300.000M, total.TotalLateReportingTonnage);
        }
    }
}