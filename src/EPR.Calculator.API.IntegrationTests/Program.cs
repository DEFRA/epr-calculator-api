using EPR.Calculator.API.IntegrationTests;

if (args.Contains("--performance"))
{
    var runsArg = args.SkipWhile(a => a != "--runs").Skip(1).FirstOrDefault();
    var numberOfRuns = runsArg is not null && int.TryParse(runsArg, out var parsed) ? parsed : 5;

    await BaseIntegrationTest.InitializeAsync();
    Directory.SetCurrentDirectory(AppContext.BaseDirectory);

    try
    {
        await CalculatorRunPerformanceTests.RunAsync(numberOfRuns);
    }
    finally
    {
        await BaseIntegrationTest.CleanupAsync();
    }
}
