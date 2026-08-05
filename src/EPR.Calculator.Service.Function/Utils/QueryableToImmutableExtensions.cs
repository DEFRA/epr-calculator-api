using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Utils;

/// <summary>
///     Provides extension methods for Entity Framework's <see cref="IQueryable{T}" /> to materialize the results to
///     various immutable collections.
/// </summary>
[ExcludeFromCodeCoverage]
public static class QueryableToImmutableExtensions
{
    public static async Task<ImmutableHashSet<T>> ToImmutableHashSetAsync<T>(this IQueryable<T> queryable, CancellationToken ct = default)
    {
        var builder = ImmutableHashSet.CreateBuilder<T>();

        await foreach (var dto in queryable.AsAsyncEnumerable().WithCancellation(ct))
            builder.Add(dto);

        return builder.ToImmutable();
    }

    public static Task<ImmutableList<T>> ToImmutableListAsync<T>(this IQueryable<T> queryable, CancellationToken ct = default)
    {
        return queryable.AsAsyncEnumerable().ToImmutableListAsync(ct);
    }

    public static async Task<ImmutableList<T>> ToImmutableListAsync<T>(this IAsyncEnumerable<T> asyncEnumerable, CancellationToken ct = default)
    {
        var builder = ImmutableList.CreateBuilder<T>();

        await foreach (var dto in asyncEnumerable.WithCancellation(ct))
            builder.Add(dto);

        return builder.ToImmutable();
    }

    public static Task<ImmutableDictionary<TKey, T>> ToImmutableDictionaryAsync<T, TKey>(this IQueryable<T> queryable,
        Func<T, TKey> keySelector,
        CancellationToken ct = default) where TKey : notnull
    {
        return queryable.ToImmutableDictionaryAsync(keySelector, null, ct);
    }

    public static Task<ImmutableDictionary<TKey, T>> ToImmutableDictionaryAsync<T, TKey>(this IQueryable<T> queryable,
        Func<T, TKey> keySelector,
        IEqualityComparer<TKey>? keyComparer,
        CancellationToken ct = default) where TKey : notnull
    {
        return queryable.ToImmutableDictionaryAsync(keySelector, t => t, keyComparer, ct);
    }

    public static async Task<ImmutableDictionary<TKey, TValue>> ToImmutableDictionaryAsync<T, TKey, TValue>(this IQueryable<T> queryable,
        Func<T, TKey> keySelector,
        Func<T, TValue> valueSelector,
        IEqualityComparer<TKey>? keyComparer = null,
        CancellationToken ct = default) where TKey : notnull
    {
        var builder = ImmutableDictionary.CreateBuilder<TKey, TValue>(keyComparer);

        await foreach (var entity in queryable.AsAsyncEnumerable().WithCancellation(ct))
            builder.Add(keySelector(entity), valueSelector(entity));

        return builder.ToImmutable();
    }
}
