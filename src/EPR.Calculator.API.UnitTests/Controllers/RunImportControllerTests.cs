using EPR.Calculator.API.Services.RunImport;
using EPR.Calculator.API.Controllers;
using EPR.Calculator.API.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EPR.Calculator.API.UnitTests.Controllers;

[TestClass]
public class RunImportControllerTests
{
    private readonly Mock<IRunImportService> runImportService = new();

    [TestMethod]
    public async Task Import_IsNotFound_WhenTheFeatureIsOff()
    {
        var result = await Controller(runImport: false).Import(Zip(), checkOnly: false, CancellationToken.None);

        result.ShouldBeOfType<NotFoundResult>();
        runImportService.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task Import_ReturnsEachRunsOutcome()
    {
        IReadOnlyList<RunImportOutcome> outcomes = [new(10, RunImportService.Skipped), new(51, RunImportService.Imported)];
        runImportService.Setup(s => s.ImportAsync(It.IsAny<Stream>(), true, It.IsAny<CancellationToken>())).ReturnsAsync(outcomes);

        var result = await Controller().Import(Zip(), checkOnly: true, CancellationToken.None);

        result.ShouldBeOfType<OkObjectResult>().Value.ShouldBe(outcomes);
    }

    [TestMethod]
    public async Task Import_IsABadRequest_WithTheReason_WhenTheZipCantBeImported()
    {
        runImportService
            .Setup(s => s.ImportAsync(It.IsAny<Stream>(), false, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new RunImportPackageException("Run 84 has a billing file but no results file"));

        var result = await Controller().Import(Zip(), checkOnly: false, CancellationToken.None);

        result.ShouldBeOfType<BadRequestObjectResult>().Value.ShouldBe("Run 84 has a billing file but no results file");
    }

    private RunImportController Controller(bool runImport = true) =>
        new(runImportService.Object, Microsoft.Extensions.Options.Options.Create(new FeatureFlagOptions { RunImport = runImport }));

    private static FormFile Zip()
    {
        var content = new MemoryStream([1, 2, 3]);
        return new FormFile(content, 0, content.Length, "runsZip", "runs.zip");
    }
}
