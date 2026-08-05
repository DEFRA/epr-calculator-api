using EFCore.BulkExtensions;
using EPR.Calculator.Service.Function.Services;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.UnitTests.TestHelpers.Services;

/// <summary>
///     Skips doing anything fancy (due to InMemory db incompatibility) and just directly inserts the entities into the
///     database.
/// </summary>
public class TestBulkOps
    : IBulkOperations
{
    public async Task BulkInsertAsync<T>(DbContext dbContext, IEnumerable<T> entities, CancellationToken cancellationToken = default)
        where T : class
    {
        var dbSet = dbContext.Set<T>();
        await dbSet.AddRangeAsync(entities, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task BulkInsertAsync<T>(DbContext dbContext, IEnumerable<T> entities, Action<BulkConfig> bulkAction, CancellationToken cancellationToken = default) where T : class
    {
        return BulkInsertAsync(dbContext, entities, cancellationToken);
    }
}
