using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.Service.Function.Features.CalculatorRun.Contexts;
using EPR.Calculator.Service.Function.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EPR.Calculator.Service.Function.Services;

public interface ICalculatorRunOrgData
{
    Task LoadOrgDataForCalcRun(CalculatorRunContext runContext, CancellationToken cancellationToken);
}

public class CalculatorRunOrgData(
    ApplicationDBContext dbContext,
    ILogger<CalculatorRunOrgData> logger)
    : ICalculatorRunOrgData
{
    public async Task LoadOrgDataForCalcRun(CalculatorRunContext runContext, CancellationToken cancellationToken) =>
        await logger.LogDuration(async () =>
        {
            var now = DateTime.Now;

            var oldOrgMaster = await dbContext.CalculatorRunOrganisationDataMaster
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (oldOrgMaster != null)
            {
                oldOrgMaster.EffectiveTo = now;
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            var newMaster = new CalculatorRunOrganisationDataMaster
            {
                RelativeYear = runContext.RelativeYear,
                CreatedAt = now,
                CreatedBy = runContext.User,
                EffectiveFrom = now
            };

            dbContext.CalculatorRunOrganisationDataMaster.Add(newMaster);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Bulk insert via raw SQL for performance (server side - no loading of entities into memory)
            var insertTable = dbContext.Model.FindEntityType(typeof(CalculatorRunOrganisationDataDetail))!;
            var selectTable = dbContext.Model.FindEntityType(typeof(OrganisationData))!;
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

            calculatorRun.CalculatorRunOrganisationDataMasterId = newMaster.Id;

            await dbContext.SaveChangesAsync(cancellationToken);
        });
}
