using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using EPR.Calculator.API.BackgroundService.Features.Common;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.Enums;
using EPR.Calculator.API.Options;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EPR.Calculator.API.UnitTests.Controllers
{
    [TestClass]
    public class CalculatorNewControllerTests
    {
        private const int CalculatorRunId = 1;

        private ApplicationDBContext context = null!;
        private CalculatorNewController controller = null!;
        private Mock<IFileExportService> fileExportServiceMock = null!;
        private Mock<IBlobStorageService> blobStorageMock = null!;
        private Mock<IStorageUploadService> storageUploadServiceMock = null!;

        [TestInitialize]
        public void Setup()
        {
            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: "PayCal")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;
            context = new ApplicationDBContext(dbContextOptions);
            context.Database.EnsureCreated();

            fileExportServiceMock = new Mock<IFileExportService>();
            fileExportServiceMock
                .Setup(x => x.Export(It.IsAny<int>(), It.IsAny<RunType>(), It.IsAny<FileExportType>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FileExportResult.Exported([], "billing.json"));

            blobStorageMock = new Mock<IBlobStorageService>();
            storageUploadServiceMock = new Mock<IStorageUploadService>();

            controller = new CalculatorNewController(
                context,
                Mock.Of<ICalculatorRunStatusDataValidator>(),
                Mock.Of<IInvoiceDetailsService>(),
                Mock.Of<ILogger<CalculatorNewController>>(),
                Mock.Of<ICalculationRunService>(),
                fileExportServiceMock.Object,
                blobStorageMock.Object,
                storageUploadServiceMock.Object,
                Microsoft.Extensions.Options.Options.Create(new BlobStorageOptions
                {
                    ConnectionString = "UseDevelopmentStorage=true",
                    ResultFileCsvContainer = "result-csv",
                    BillingFileCsvContainer = "billing-csv",
                    FssContainer = "fss",
                }),
                Microsoft.Extensions.Options.Options.Create(new FeatureFlagOptions
                {
                    UploadFssBillingFileToBlobStorage = true,
                }))
            {
                ControllerContext = CreateAuthenticatedControllerContext(),
            };

            context.CalculatorRuns.Add(new CalculatorRun
            {
                Id = CalculatorRunId,
                CalculatorRunClassificationId = (int)RunClassification.INITIAL_RUN,
                RelativeYear = new RelativeYear(2024),
                Name = "Test calculator run",
            });
            context.SaveChanges();
        }

        [TestCleanup]
        public void CleanUp()
        {
            context.Database.EnsureDeleted();
        }

        [TestMethod]
        public async Task PrepareBillingFileSendToFSS_Returns_Accepted_When_Successful()
        {
            // Arrange
            AddBillingFileMetadata(CalculatorRunId);

            // Act
            var result = await controller.PrepareBillingFileSendToFSS(CalculatorRunId, CancellationToken.None) as StatusCodeResult;

            // Assert
            result.ShouldNotBeNull();
            result.StatusCode.ShouldBe(StatusCodes.Status202Accepted);
        }

        [TestMethod]
        public async Task PrepareBillingFileSendToFSS_UploadsExportedContent_ToFssContainer()
        {
            // Arrange
            var content = "billing-content"u8.ToArray();
            fileExportServiceMock
                .Setup(x => x.Export(CalculatorRunId, RunType.Billing, FileExportType.Json, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FileExportResult.Exported(content, "billing.json"));
            AddBillingFileMetadata(CalculatorRunId);

            // Act
            var result = await controller.PrepareBillingFileSendToFSS(CalculatorRunId, CancellationToken.None) as StatusCodeResult;

            // Assert
            result.ShouldNotBeNull();
            result.StatusCode.ShouldBe(StatusCodes.Status202Accepted);
            storageUploadServiceMock.Verify(
                x => x.UploadFileContentAsync(
                    It.Is<IStorageUploadService.Request>(r =>
                        r.Content == content &&
                        r.ContainerName == "fss" &&
                        r.Overwrite),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [TestMethod]
        public async Task PrepareBillingFileSendToFSS_ReturnsUnprocessableEntity_WhenExportNotFound()
        {
            // Arrange
            fileExportServiceMock
                .Setup(x => x.Export(CalculatorRunId, RunType.Billing, FileExportType.Json, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FileExportResult.NotFound());
            AddBillingFileMetadata(CalculatorRunId);

            // Act
            var result = await controller.PrepareBillingFileSendToFSS(CalculatorRunId, CancellationToken.None) as ObjectResult;

            // Assert
            result.ShouldNotBeNull();
            result.StatusCode.ShouldBe(StatusCodes.Status422UnprocessableEntity);
            result.Value.ShouldBe(string.Format("Unable to export billing file for run {0}", CalculatorRunId));
            storageUploadServiceMock.Verify(
                x => x.UploadFileContentAsync(It.IsAny<IStorageUploadService.Request>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [TestMethod]
        public async Task PrepareBillingFileSendToFSS_Legacy_ReturnsAccepted_WhenMoveSucceeds()
        {
            // Arrange
            fileExportServiceMock
                .Setup(x => x.Export(CalculatorRunId, RunType.Billing, FileExportType.Json, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FileExportResult.Legacy());
            blobStorageMock
                .Setup(x => x.MoveBillingJsonToFss("test.json", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            AddBillingFileMetadata(CalculatorRunId);

            // Act
            var result = await controller.PrepareBillingFileSendToFSS(CalculatorRunId, CancellationToken.None) as StatusCodeResult;

            // Assert
            result.ShouldNotBeNull();
            result.StatusCode.ShouldBe(StatusCodes.Status202Accepted);
            blobStorageMock.Verify(x => x.MoveBillingJsonToFss("test.json", It.IsAny<CancellationToken>()), Times.Once);
            storageUploadServiceMock.Verify(
                x => x.UploadFileContentAsync(It.IsAny<IStorageUploadService.Request>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [TestMethod]
        public async Task PrepareBillingFileSendToFSS_Legacy_ReturnsUnprocessableEntity_WhenMoveFails()
        {
            // Arrange
            fileExportServiceMock
                .Setup(x => x.Export(CalculatorRunId, RunType.Billing, FileExportType.Json, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FileExportResult.Legacy());
            blobStorageMock
                .Setup(x => x.MoveBillingJsonToFss("test.json", It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            AddBillingFileMetadata(CalculatorRunId);

            // Act
            var result = await controller.PrepareBillingFileSendToFSS(CalculatorRunId, CancellationToken.None) as ObjectResult;

            // Assert
            result.ShouldNotBeNull();
            result.StatusCode.ShouldBe(StatusCodes.Status422UnprocessableEntity);
            result.Value.ShouldBe(string.Format("Unable to export billing file for run {0}", CalculatorRunId));
        }

        [TestMethod]
        public async Task PrepareBillingFileSendToFSS_Returns_UnprocessableEntity_When_BillingFile_Outdated()
        {
            // Arrange

            context.ProducerResultFileSuggestedBillingInstruction.Add(new ProducerResultFileSuggestedBillingInstruction
            {
                CalculatorRunId = CalculatorRunId,
                LastModifiedAcceptReject = DateTime.UtcNow.AddDays(1),
                SuggestedBillingInstruction = "ignored",
            });
            AddBillingFileMetadata(CalculatorRunId);

            // Act
            var result = await controller.PrepareBillingFileSendToFSS(CalculatorRunId, CancellationToken.None) as ObjectResult;

            // Assert
            result.ShouldNotBeNull();
            result.StatusCode.ShouldBe(StatusCodes.Status422UnprocessableEntity);
            result.Value.ShouldBe(CommonResources.BillingFileOutdated);
        }

        [TestMethod]
        public async Task PrepareBillingFileSendToFSS_Returns_UnprocessableEntity_When_Run_Not_Found()
        {
            // Act
            var result = await controller.PrepareBillingFileSendToFSS(-1, CancellationToken.None) as ObjectResult;

            // Assert
            result.ShouldNotBeNull();
            result.StatusCode.ShouldBe(StatusCodes.Status422UnprocessableEntity);
            result.Value.ShouldBe(string.Format(CommonResources.UnableToFindRun, -1));
        }

        [TestMethod]
        [DataRow(RunClassification.INITIAL_RUN, RunClassification.INITIAL_RUN_COMPLETED)]
        [DataRow(RunClassification.INTERIM_RECALCULATION_RUN, RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED)]
        [DataRow(RunClassification.FINAL_RECALCULATION_RUN, RunClassification.FINAL_RECALCULATION_RUN_COMPLETED)]
        [DataRow(RunClassification.FINAL_RUN, RunClassification.FINAL_RUN_COMPLETED)]
        public async Task PrepareBillingFileSendToFSS_Updates_Classification_To_Completed(
            RunClassification initialValue,
            RunClassification expectedNewValue)
        {
            // Arrange
            var calculatorRun = context.CalculatorRuns.Single(run => run.Id == CalculatorRunId);
            calculatorRun.CalculatorRunClassificationId = (int)initialValue;

            AddBillingFileMetadata(CalculatorRunId);

            // Act
            var result = (IStatusCodeActionResult)await controller.PrepareBillingFileSendToFSS(CalculatorRunId, CancellationToken.None);
            var newClassification = (RunClassification)context.CalculatorRuns
                .Single(run => run.Id == CalculatorRunId).CalculatorRunClassificationId;

            // Assert
            result.StatusCode.ShouldBe((int)HttpStatusCode.Accepted);
            newClassification.ShouldBe(expectedNewValue);
        }

        [TestMethod]
        public async Task PrepareBillingFileSendToFSS_SkipsExportAndUpload_WhenFeatureFlagDisabled()
        {
            // Arrange
            var calculatorRun = context.CalculatorRuns.Single(run => run.Id == CalculatorRunId);
            calculatorRun.CalculatorRunClassificationId = (int)RunClassification.INITIAL_RUN;

            AddBillingFileMetadata(CalculatorRunId);

            var fileExportServiceMock = new Mock<IFileExportService>();
            var storageUploadServiceMock = new Mock<IStorageUploadService>();

            var controllerWithFlagDisabled = new CalculatorNewController(
                context,
                Mock.Of<ICalculatorRunStatusDataValidator>(),
                Mock.Of<IInvoiceDetailsService>(),
                Mock.Of<ILogger<CalculatorNewController>>(),
                Mock.Of<ICalculationRunService>(),
                fileExportServiceMock.Object,
                Mock.Of<IBlobStorageService>(),
                storageUploadServiceMock.Object,
                Microsoft.Extensions.Options.Options.Create(new BlobStorageOptions
                {
                    ConnectionString = "UseDevelopmentStorage=true",
                    ResultFileCsvContainer = "result-csv",
                    BillingFileCsvContainer = "billing-csv",
                    FssContainer = "fss",
                }),
                Microsoft.Extensions.Options.Options.Create(new FeatureFlagOptions
                {
                    UploadFssBillingFileToBlobStorage = false,
                }))
            {
                ControllerContext = CreateAuthenticatedControllerContext(),
            };

            // Act
            var result = (IStatusCodeActionResult)await controllerWithFlagDisabled.PrepareBillingFileSendToFSS(CalculatorRunId, CancellationToken.None);
            var newClassification = (RunClassification)context.CalculatorRuns
                .Single(run => run.Id == CalculatorRunId).CalculatorRunClassificationId;

            // Assert
            result.StatusCode.ShouldBe((int)HttpStatusCode.Accepted);
            newClassification.ShouldBe(RunClassification.INITIAL_RUN_COMPLETED);
            fileExportServiceMock.Verify(
                x => x.Export(It.IsAny<int>(), It.IsAny<RunType>(), It.IsAny<FileExportType>(), It.IsAny<CancellationToken>()),
                Times.Never);
            storageUploadServiceMock.Verify(
                x => x.UploadFileContentAsync(It.IsAny<IStorageUploadService.Request>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [TestMethod]
        [DataRow(RunClassification.RUNNING)]
        [DataRow(RunClassification.TEST_RUN)]
        [DataRow(RunClassification.DELETED)]
        [DataRow(RunClassification.INTHEQUEUE)]
        public async Task PrepareBillingFileSendToFSS_Returns_UnprocessableEntity_When_Classification_Invalid(RunClassification initialValue)
        {
            // Arrange
            var calculatorRun = context.CalculatorRuns.Single(run => run.Id == CalculatorRunId);
            calculatorRun.CalculatorRunClassificationId = (int)initialValue;
            AddBillingFileMetadata(CalculatorRunId);

            // Act
            var result = (ObjectResult)await controller.PrepareBillingFileSendToFSS(CalculatorRunId, CancellationToken.None);
            var newClassification = (RunClassification)context.CalculatorRuns
                .Single(run => run.Id == CalculatorRunId).CalculatorRunClassificationId;

            // Assert
            result.StatusCode.ShouldBe((int)HttpStatusCode.UnprocessableContent);
            newClassification.ShouldBe(initialValue);
            result.Value.ShouldBe(string.Format(CommonResources.UnableToChangeStatusToCompleted, initialValue));
        }

        private void AddBillingFileMetadata(int calculatorRunId)
        {
            context.CalculatorRunBillingFileMetadata.Add(new CalculatorRunBillingFileMetadata
            {
                BillingCsvFileName = "test.csv",
                BillingJsonFileName = "test.json",
                BillingFileCreatedBy = "Test user",
                BillingFileCreatedDate = DateTime.UtcNow,
                CalculatorRunId = calculatorRunId,
            });
            context.SaveChanges();
        }

        private static ControllerContext CreateAuthenticatedControllerContext()
        {
            var identity = new GenericIdentity("TestUser");
            identity.AddClaim(new Claim("name", "TestUser"));
            var principal = new ClaimsPrincipal(identity);

            return new ControllerContext { HttpContext = new DefaultHttpContext { User = principal } };
        }
    }
}
