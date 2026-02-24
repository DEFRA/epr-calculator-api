using System.Net;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Dtos;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Services.Abstractions;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EPR.Calculator.API.UnitTests.Controllers
{
    [TestClass]
    public class BillingFileControllerTests
    {
        private readonly BillingFileController billingFileControllerUnderTest;

        private readonly Mock<IBillingFileService> billingFileServiceMock;

        private readonly Mock<IStorageService> storageServiceMock;

        private readonly ApplicationDBContext context;

        public BillingFileControllerTests()
        {
            billingFileServiceMock = new Mock<IBillingFileService>();
            storageServiceMock = new Mock<IStorageService>();

            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "PayCal")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            context = new ApplicationDBContext(dbContextOptions);
            context.Database.EnsureCreated();

            billingFileControllerUnderTest = new BillingFileController(billingFileServiceMock.Object, storageServiceMock.Object, context);
        }

        [TestMethod]
        public async Task DownloadBillingFile_ReturnsBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            billingFileControllerUnderTest.ModelState.AddModelError("key", "error");

            // Act
            var result = await billingFileControllerUnderTest.DownloadCsvBillingFile(1);

            // Assert
            result.Should().BeOfType<BadRequest<IEnumerable<ModelError>>>();
        }

        [TestMethod]
        public async Task DownloadBillingFile_ReturnsNotFound_WhenNoMetadataFound()
        {
            // Arrange
            int runId = 9999; // Use a runId that does not exist in the in-memory DB

            // Act
            var result = await billingFileControllerUnderTest.DownloadCsvBillingFile(runId);

            // Assert
            result.Should().BeOfType<NotFound<string>>();
        }

        [TestMethod]
        public async Task DownloadBillingFile_ReturnsProblem_WhenStorageThrows()
        {
            // Arrange
            int runId = 123;
            var billingMeta = new CalculatorRunBillingFileMetadata()
            {
                CalculatorRunId = runId,
                BillingCsvFileName = "csvfile.json",
                BillingJsonFileName = "jsonfile.json",
                BillingFileCreatedBy = "user",
                BillingFileCreatedDate = DateTime.UtcNow,
            };

            var csvMeta = new CalculatorRunCsvFileMetadata()
            {
                FileName = "csvfile.json",
                BlobUri = "C:\\dev\\file.json",
                CalculatorRunId = runId,
            };
            context.CalculatorRunBillingFileMetadata.Add(billingMeta);
            context.CalculatorRunCsvFileMetadata.Add(csvMeta);
            context.SaveChanges();

            storageServiceMock
                .Setup(x => x.DownloadFile("csvfile.json", "C:\\dev\\file.json"))
                .ThrowsAsync(new Exception("fail"));

            // Act
            var result = await billingFileControllerUnderTest.DownloadCsvBillingFile(runId);

            // Assert
            result.Should().BeOfType<ProblemHttpResult>();

            // Tidy Up
            context.CalculatorRunBillingFileMetadata.Remove(billingMeta);
            context.CalculatorRunCsvFileMetadata.Remove(csvMeta);
            context.SaveChanges();
        }

        [TestMethod]
        public async Task DownloadBillingFile_ReturnsFileResult_WhenSuccess()
        {
            // Arrange
            int runId = 456;
            var billingMeta = new CalculatorRunBillingFileMetadata
            {
                CalculatorRunId = runId,
                BillingCsvFileName = "csvfile.json",
                BillingJsonFileName = "file2.json",
                BillingFileCreatedBy = "user",
                BillingFileCreatedDate = DateTime.UtcNow,
            };
            var csvMeta = new CalculatorRunCsvFileMetadata
            {
                FileName = "csvfile.json",
                BlobUri = "C:\\dev\\csvfile.json",
                CalculatorRunId = runId,
            };
            context.CalculatorRunBillingFileMetadata.Add(billingMeta);
            context.CalculatorRunCsvFileMetadata.Add(csvMeta);
            context.SaveChanges();

            var expectedResult = Mock.Of<IResult>();
            storageServiceMock
                .Setup(x => x.DownloadFile("csvfile.json", "C:\\dev\\csvfile.json"))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await billingFileControllerUnderTest.DownloadCsvBillingFile(runId);

            // Assert
            result.Should().BeSameAs(expectedResult);

            // Tidy Up
            context.CalculatorRunBillingFileMetadata.Remove(billingMeta);
            context.CalculatorRunCsvFileMetadata.Remove(csvMeta);
            context.SaveChanges();
        }
    }
}
