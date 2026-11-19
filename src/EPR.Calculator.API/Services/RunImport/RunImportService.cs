using EPR.Calculator.API.Data;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Services.RunImport;

public sealed record RunImportOutcome(int RunId, string Outcome);

public interface IRunImportService
{
    /// <summary>
    ///     Imports a zip's runs in run ID order, each in its own transaction, skipping runs already stored. With
    ///     <paramref name="checkOnly" />, it imports them all and then rolls everything back.
    /// </summary>
    /// <exception cref="RunImportPackageException">The zip is missing a file, or one of its runs can't be imported.</exception>
    Task<IReadOnlyList<RunImportOutcome>> ImportAsync(Stream zip, bool checkOnly, CancellationToken cancellationToken);
}

public class RunImportService(ApplicationDBContext dbContext, IRunImporter runImporter) : IRunImportService
{
    public const string Imported = "Imported";
    public const string Skipped = "Skipped: already stored";
    public const string Checked = "Checked";

    public async Task<IReadOnlyList<RunImportOutcome>> ImportAsync(Stream zip, bool checkOnly, CancellationToken cancellationToken)
    {
        var runs = RunImportPackage.Read(zip);
        var outcomes = new List<RunImportOutcome>();

        await using var check = checkOnly ? await dbContext.Database.BeginTransactionAsync(cancellationToken) : null;

        foreach (var run in runs)
        {
            if (await dbContext.CalculatorRuns.AnyAsync(r => r.Id == run.RunId, cancellationToken))
            {
                outcomes.Add(new RunImportOutcome(run.RunId, Skipped));
                continue;
            }

            try
            {
                await runImporter.ImportAsync(run.Files, run.Details, cancellationToken);
            }
            catch (FormatException ex)
            {
                dbContext.ChangeTracker.Clear();
                var done = outcomes.Count == 0 ? "" : $" Runs {string.Join(", ", outcomes.Select(o => o.RunId))} were {(checkOnly ? "checked" : "imported or already stored")}; nothing after run {run.RunId} was tried.";
                throw new RunImportPackageException($"Run {run.RunId} can't be imported: {ex.Message}.{done}");
            }

            // Each run's rows are saved; letting go of them keeps memory down across a zip of large runs.
            dbContext.ChangeTracker.Clear();
            outcomes.Add(new RunImportOutcome(run.RunId, checkOnly ? Checked : Imported));
        }

        return outcomes;
    }
}
