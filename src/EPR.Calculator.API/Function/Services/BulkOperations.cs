using System.Diagnostics.CodeAnalysis;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Services;

public interface IBulkOperations
{
    Task BulkInsertAsync<T>(DbContext dbContext, IEnumerable<T> entities, CancellationToken cancellationToken = default)
        where T : class;

    Task BulkInsertAsync<T>(DbContext dbContext, IEnumerable<T> entities, Action<BulkConfig> bulkAction, CancellationToken cancellationToken = default)
        where T : class;
}

/// <summary>
///     Abstraction over <see cref="EFCore.BulkExtensions" /> to allow bulk operations to be verified in unit tests.
/// </summary>
[ExcludeFromCodeCoverage]
public class BulkOperationsWrapper
    : IBulkOperations
{
    public Task BulkInsertAsync<T>(DbContext dbContext, IEnumerable<T> entities, CancellationToken cancellationToken = default)
        where T : class
    {
        return dbContext.BulkInsertAsync(entities, cancellationToken: cancellationToken);
    }

    public Task BulkInsertAsync<T>(DbContext dbContext, IEnumerable<T> entities, Action<BulkConfig> bulkAction, CancellationToken cancellationToken = default)
        where T : class
    {
        return dbContext.BulkInsertAsync(entities, bulkAction, cancellationToken: cancellationToken);
    }
}
