﻿using Azure.Messaging.ServiceBus;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.Calculator.API.UnitTests
{
    [TestClass]
    public class PutCalculatorRunStatusTest
    {
        private ApplicationDBContext context;
        private Mock<IConfiguration> mockConfig;
        private Mock<IAzureClientFactory<ServiceBusClient>> mockServiceBusFactory;
        private Mock<IStorageService> mockStorageService;

        [TestInitialize]
        public void SetUp()
        {
            this.mockStorageService = new Mock<IStorageService>();
            this.mockConfig = new Mock<IConfiguration>();
            this.mockServiceBusFactory = new Mock<IAzureClientFactory<ServiceBusClient>>();
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "PayCal")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            this.context = new ApplicationDBContext(dbContextOptions);
            this.context.Database.EnsureCreated();
        }

        [TestCleanup]
        public void CleanUp()
        {
            this.context.Database.EnsureDeleted();
        }

        [TestMethod]
        public void PutCalculatorRunStatusTest_422()
        {
            var controller =
                new CalculatorController(this.context, this.mockConfig.Object, this.mockServiceBusFactory.Object, this.mockStorageService.Object);
            var runId = 999;
            var result = controller.PutCalculatorRunStatus(new CalculatorRunStatusUpdateDto
                { ClassificationId = 6, RunId = runId }) as ObjectResult;
            Assert.IsNotNull(result);

            Assert.AreEqual(422, result.StatusCode);
            Assert.AreEqual($"Unable to find Run Id {runId}", result.Value);
        }

        [TestMethod]
        public void PutCalculatorRunStatusTest_Invalid_Classification_Id()
        {
            var runId = 1;
            var invalidClassificationId = 10;
            var date = DateTime.Now;
            this.context.CalculatorRuns.Add(new CalculatorRun
            {
                Name = "Calc RunName",
                CalculatorRunClassificationId = 2,
                CreatedAt = date,
                CreatedBy = "User23",
                LapcapDataMasterId = 1,
                DefaultParameterSettingMasterId = 1,
                Financial_Year = "2024-25"
            });
            this.context.SaveChanges();

            var controller =
                new CalculatorController(this.context, this.mockConfig.Object, this.mockServiceBusFactory.Object,
                    this.mockStorageService.Object);

            var result = controller.PutCalculatorRunStatus(new CalculatorRunStatusUpdateDto
                { ClassificationId = invalidClassificationId, RunId = runId }) as ObjectResult;
            Assert.IsNotNull(result);

            Assert.AreEqual(422, result.StatusCode);
            Assert.AreEqual($"Unable to find Classification Id {invalidClassificationId}", result.Value);
        }

        [TestMethod]
        public void PutCalculatorRunStatusTest_Valid_Run_Classification_Id()
        {
            var runId = 1;
            var validClassificationId = 5;
            var date = DateTime.Now;
            this.context.CalculatorRuns.Add(new CalculatorRun
            {
                Name = "Calc RunName",
                CalculatorRunClassificationId = 2,
                CreatedAt = date,
                CreatedBy = "User23",
                LapcapDataMasterId = 1,
                DefaultParameterSettingMasterId = 1,
                Financial_Year = "2024-25"
            });
            this.context.SaveChanges();

            var controller =
                new CalculatorController(this.context, this.mockConfig.Object, this.mockServiceBusFactory.Object,
                    this.mockStorageService.Object);

            var result = controller.PutCalculatorRunStatus(new CalculatorRunStatusUpdateDto
                { ClassificationId = validClassificationId, RunId = runId }) as StatusCodeResult;
            Assert.IsNotNull(result);
            Assert.AreEqual(201, result.StatusCode);

            var run = this.context.CalculatorRuns.Single(x => x.Id == 1);
            Assert.IsNotNull(run);

            Assert.AreEqual(5, run.CalculatorRunClassificationId);
        }

        [TestMethod]
        public void PutCalculatorRunStatusTest_Unable_To_Change_Classification_Id()
        {
            var runId = 1;
            var classificationId = 5;
            var date = DateTime.Now;
            this.context.CalculatorRuns.Add(new CalculatorRun
            {
                Name = "Calc RunName",
                CalculatorRunClassificationId = classificationId,
                CreatedAt = date,
                CreatedBy = "User23",
                LapcapDataMasterId = 1,
                DefaultParameterSettingMasterId = 1,
                Financial_Year = "2024-25"
            });
            this.context.SaveChanges();

            var controller =
                new CalculatorController(this.context, this.mockConfig.Object, this.mockServiceBusFactory.Object,
                    this.mockStorageService.Object);

            var result = controller.PutCalculatorRunStatus(new CalculatorRunStatusUpdateDto
                { ClassificationId = classificationId, RunId = runId }) as ObjectResult;
            Assert.IsNotNull(result);

            Assert.AreEqual(422, result.StatusCode);
            Assert.AreEqual($"RunId {runId} cannot be changed to classification {classificationId}", result.Value);
        }
    }
}
