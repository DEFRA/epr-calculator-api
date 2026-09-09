using System.Reflection;
using EPR.Calculator.API.App;
using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns;
using EPR.Calculator.API.BackgroundService.Features.CalculatorRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

if (args.Length != 2 || !int.TryParse(args[1], out var runId))
{
    Console.Error.WriteLine("Usage: dotnet run -- <reprocess|retranspose> <runId>");
    return 1;
}

var command = args[0];

if (command is not ("reprocess" or "retranspose"))
{
    Console.Error.WriteLine($"Unknown command '{command}'. Expected 'reprocess' or 'retranspose'.");
    return 1;
}

var configuration = new ConfigurationBuilder()
    .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
    .AddEnvironmentVariables()
    .Build();

var services = new ServiceCollection()
    .AddSingleton<IConfiguration>(configuration)
    .AddPayCalDatabase()
    .AddPayCalServices()
    .AddPayCalBackgroundServices();

await using var provider = services.BuildServiceProvider();
using var scope = provider.CreateScope();

return command switch
{
    "reprocess" => await Reprocess(scope.ServiceProvider, runId),
    "retranspose" => await Retranspose(scope.ServiceProvider, runId),
    _ => throw new InvalidOperationException($"Unreachable: {command}")
};

static async Task<int> Reprocess(IServiceProvider services, int runId)
{
    Console.WriteLine($"About to reprocess run {runId} - this WRITES result rows and will fail/duplicate if any already exist for this run.");
    if (!Confirm())
        return 1;

    var reprocessor = services.GetRequiredService<IHistoricalCalculatorRunReprocessor>();
    var result = await reprocessor.ReprocessAsync(runId, CancellationToken.None);

    Console.WriteLine($"Done. Result: {result}");
    return 0;
}

static async Task<int> Retranspose(IServiceProvider services, int runId)
{
    Console.WriteLine($"About to DELETE producer_detail/producer_reported_material for run {runId} and regenerate them from the run's historical calculator_run_organization_data_detail/calculator_run_pom_data_detail snapshot.");
    if (!Confirm())
        return 1;

    var dbContext = services.GetRequiredService<ApplicationDBContext>();
    var parameterService = services.GetRequiredService<IParameterService>();
    var transposer = services.GetRequiredService<IProducerDataTransposer>();
    var timeProvider = services.GetRequiredService<TimeProvider>();

    var run = await dbContext.CalculatorRuns.AsNoTracking().SingleAsync(r => r.Id == runId);

    var runContext = new CalculatorRunContext
    {
        RunId = run.Id,
        RunName = run.Name.Trim(),
        ProcessingStartedAt = timeProvider.GetUtcNow(),
        RelativeYear = run.RelativeYear,
        User = run.CreatedBy,
        DefaultParameters = await parameterService.GetDefaultParameters(runId)
    };

    var reportedMaterialDeleted = await dbContext.ProducerReportedMaterial
        .Where(x => x.ProducerDetail.CalculatorRunId == runId)
        .ExecuteDeleteAsync();

    var producerDetailDeleted = await dbContext.ProducerDetail
        .Where(x => x.CalculatorRunId == runId)
        .ExecuteDeleteAsync();

    Console.WriteLine($"Deleted {producerDetailDeleted} producer_detail, {reportedMaterialDeleted} producer_reported_material");

    await transposer.Transpose(runContext, CancellationToken.None);

    Console.WriteLine("Retranspose complete.");
    return 0;
}

static bool Confirm()
{
    Console.Write("Type 'yes' to continue: ");
    if (Console.ReadLine()?.Trim() == "yes")
        return true;

    Console.WriteLine("Aborted.");
    return false;
}
