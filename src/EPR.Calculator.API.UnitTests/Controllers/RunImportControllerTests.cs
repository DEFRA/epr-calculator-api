using EPR.Calculator.API.BackgroundService.Features.RunImport;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.DataTypes;
using EPR.Calculator.API.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.API.UnitTests.Controllers;

[TestClass]
public class RunImportControllerTests : InMemoryApplicationDbContext
{
    private const int RunId = 987654;
    private const string FileName = "987654-Some Run_Billing File_202510061011_AUTHORISED.csv";

    private readonly Mock<IRunImporter> runImporter = new();

    [TestCleanup]
    public void TestCleanup()
    {
        DbContext.CalculatorRuns.RemoveRange(DbContext.CalculatorRuns.Where(r => r.Id == RunId));
        DbContext.SaveChanges();
    }

    [TestMethod]
    public async Task Import_IsNotFound_WhenTheFeatureIsOff()
    {
        var result = await Controller(runImport: false).Import(File(FileName), null, null, CancellationToken.None);

        result.ShouldBeOfType<NotFoundResult>();
        runImporter.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task Import_IsABadRequest_WhenTheFileNameIsNotABillingFile()
    {
        var result = await Controller().Import(File("results.csv"), null, null, CancellationToken.None);

        result.ShouldBeOfType<BadRequestObjectResult>();
        runImporter.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task Import_IsAConflict_WhenTheRunAlreadyExists()
    {
        DbContext.CalculatorRuns.Add(new CalculatorRun { Id = RunId, Name = "Existing", RelativeYear = new RelativeYear(2025) });
        await DbContext.SaveChangesAsync();

        var result = await Controller().Import(File(FileName), null, null, CancellationToken.None);

        result.ShouldBeOfType<ConflictObjectResult>();
        runImporter.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task Import_ImportsTheFile_AndReturnsTheRunId()
    {
        runImporter
            .Setup(i => i.ImportAsync(It.IsAny<TextReader>(), FileName, It.Is<RunImportDetails>(d => d.Classification == RunClassification.Recalculation && d.BillingFileCreatedBy == "Someone"), It.IsAny<CancellationToken>()))
            .ReturnsAsync(RunId);

        var result = await Controller().Import(File(FileName), RunClassification.Recalculation, "Someone", CancellationToken.None);

        result.ShouldBeOfType<ObjectResult>().StatusCode.ShouldBe(StatusCodes.Status201Created);
        runImporter.VerifyAll();
    }

    [TestMethod]
    public async Task Import_IsABadRequest_WhenTheFileCannotBeRead()
    {
        runImporter
            .Setup(i => i.ImportAsync(It.IsAny<TextReader>(), FileName, It.IsAny<RunImportDetails>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FormatException("No row starting 'LAPCAP File'"));

        var result = await Controller().Import(File(FileName), null, null, CancellationToken.None);

        result.ShouldBeOfType<BadRequestObjectResult>().Value.ShouldBe("No row starting 'LAPCAP File'");
    }

    private RunImportController Controller(bool runImport = true) =>
        new(runImporter.Object, DbContext, Microsoft.Extensions.Options.Options.Create(new FeatureFlagOptions { RunImport = runImport }));

    private static FormFile File(string fileName)
    {
        var content = new MemoryStream("Run Name,Some Run"u8.ToArray());
        return new FormFile(content, 0, content.Length, "billingFile", fileName);
    }
}
