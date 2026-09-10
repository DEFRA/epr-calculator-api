using EPR.Calculator.API.IntegrationTests;

if (args.Contains("--performance"))
{
    await BaseIntegrationTest.InitializeAsync();
    Directory.SetCurrentDirectory(AppContext.BaseDirectory);

    try
    {
        await CalculatorRunPerformanceTests.RunAsync();
    }
    finally
    {
        await BaseIntegrationTest.CleanupAsync();
    }
}
