using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using EPR.Calculator.API.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EPR.Calculator.Service.Function.Services {
    public interface ICalculatorRunOrgData { Task LoadOrgDataForCalcRun(int runId, RelativeYear relativeYear, string createdBy, CancellationToken cancellationToken); }

    public class CalculatorRunOrgData : ICalculatorRunOrgData
    {
        private readonly ApplicationDBContext _context;
        public CalculatorRunOrgData(ApplicationDBContext context) { _context = context; }

        public async Task LoadOrgDataForCalcRun(int runId, RelativeYear relativeYear, string createdBy, CancellationToken cancellationToken)
        {
            var now = DateTime.Now;

            var oldOrgMaster = await _context.CalculatorRunOrganisationDataMaster
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (oldOrgMaster != null)
            {
                oldOrgMaster.EffectiveTo = now;
                await _context.SaveChangesAsync(cancellationToken);
            }

            var newMaster = new CalculatorRunOrganisationDataMaster
            {
                RelativeYear = relativeYear,
                CreatedAt = now,
                CreatedBy = createdBy,
                EffectiveFrom = now
            };

            _context.CalculatorRunOrganisationDataMaster.Add(newMaster);
            await _context.SaveChangesAsync(cancellationToken);

            // Bulk insert via raw SQL for performance (server side - no loading of entities into memory)
            var insertTable = _context.Model.FindEntityType(typeof(CalculatorRunOrganisationDataDetail))!;
            var selectTable = _context.Model.FindEntityType(typeof(OrganisationData))!;
            var tableId = StoreObjectIdentifier.Table(insertTable.GetTableName()!, insertTable.GetSchema());
            var columnNames = insertTable.GetProperties()
                .Where(p => !string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase))
                .Select(p => p.GetColumnName(tableId));

            #pragma warning disable EF1002, S2077 // Table and column names come from EF Core metadata, not user input
            await _context.Database.ExecuteSqlRawAsync($@"
                INSERT INTO {insertTable.GetTableName()} ({string.Join(", ", columnNames)})
                SELECT {newMaster.Id}, {string.Join(", ", columnNames.Skip(1))}
                FROM {selectTable.GetTableName()};",
                cancellationToken
            );
            #pragma warning restore EF1002, S2077

            var calculatorRun = await _context.CalculatorRuns
                .FirstAsync(x => x.Id == runId, cancellationToken);

            calculatorRun.CalculatorRunOrganisationDataMasterId = newMaster.Id;

            await _context.SaveChangesAsync(cancellationToken);
        }

    }
}
