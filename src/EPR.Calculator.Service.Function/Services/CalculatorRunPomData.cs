using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Features.CalculatorRun.Contexts;
using EPR.Calculator.Service.Function.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EPR.Calculator.Service.Function.Services;

public interface ICalculatorRunPomData
{
    Task LoadPomDataForCalcRun(CalculatorRunContext runContext, CancellationToken cancellationToken);
}

public class CalculatorRunPomData(
    ApplicationDBContext dbContext,
    ILogger<CalculatorRunPomData> logger)
    : ICalculatorRunPomData
{
    public async Task LoadPomDataForCalcRun(CalculatorRunContext runContext, CancellationToken cancellationToken) =>
        await logger.LogDuration(async () =>
        {
            var now = DateTime.Now;

            var oldPomMaster = await dbContext.CalculatorRunPomDataMaster
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (oldPomMaster != null)
                oldPomMaster.EffectiveTo = now;

            var newMaster = new CalculatorRunPomDataMaster
            {
                RelativeYear = runContext.RelativeYear,
                CreatedAt = now,
                CreatedBy = runContext.User,
                EffectiveFrom = now
            };

            dbContext.CalculatorRunPomDataMaster.Add(newMaster);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Bulk insert via raw SQL for performance (server side - no loading of entities into memory)
            var insertTable = dbContext.Model.FindEntityType(typeof(CalculatorRunPomDataDetail))!;
            var selectTable = dbContext.Model.FindEntityType(typeof(PomData))!;
            var tableId = StoreObjectIdentifier.Table(insertTable.GetTableName()!, insertTable.GetSchema());
            var columnNames = insertTable.GetProperties()
                .Where(p => !string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase))
                .Select(p => p.GetColumnName(tableId))
                .ToImmutableList();

            #pragma warning disable EF1002, S2077 // Table and column names come from EF Core metadata, not user input
            await dbContext.Database.ExecuteSqlRawAsync($@"
                INSERT INTO {insertTable.GetTableName()} ({string.Join(", ", columnNames)})
                SELECT {newMaster.Id}, {string.Join(", ", columnNames.Skip(1))}
                FROM {selectTable.GetTableName()};",
                cancellationToken
            );
            #pragma warning restore EF1002, S2077

            var calculatorRun = await dbContext.CalculatorRuns
                .FirstAsync(x => x.Id == runContext.RunId, cancellationToken);

            calculatorRun.CalculatorRunPomDataMasterId = newMaster.Id;

            await dbContext.SaveChangesAsync(cancellationToken);
        });
}
