using System.Collections;
using System.Diagnostics.CodeAnalysis;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.Builder.Summary.TonnageVsAllProducer;

public static class TonnageVsAllProducerUtil
{
    public static decimal GetPercentageofProducerReportedTonnagevsAllProducersTotal(IReadOnlyList<ProducerDetail> producers, IReadOnlyList<TotalPackagingTonnagePerRun> totalPackagingTonnage)
    {
        // PERF: route through the index so the grand total is computed once and each per-producer lookup is O(1).
        var index = AsIndex(totalPackagingTonnage);
        return producers.Sum(producer => GetPercentageofProducerReportedTonnagevsAllProducers(producer, index));
    }

    public static decimal GetPercentageofProducerReportedTonnagevsAllProducers(ProducerDetail producer, IReadOnlyList<TotalPackagingTonnagePerRun> totalPackagingTonnage) =>
        GetPercentageofProducerReportedTonnagevsAllProducers(producer, AsIndex(totalPackagingTonnage));

    // PERF: Fast path used by the summary builder, where the caller already has a precomputed index.
    private static decimal GetPercentageofProducerReportedTonnagevsAllProducers(ProducerDetail producer, TotalPackagingTonnageIndex index)
    {
        if (index.GrandTotal <= 0)
            return 0;

        return index.TryGetTonnage(producer.ProducerId, producer.SubsidiaryId, out var tonnage)
            ? tonnage / index.GrandTotal * 100
            : 0;
    }

    private static TotalPackagingTonnageIndex AsIndex(IReadOnlyList<TotalPackagingTonnagePerRun> totalPackagingTonnage) => totalPackagingTonnage as TotalPackagingTonnageIndex ?? new TotalPackagingTonnageIndex(totalPackagingTonnage);
}

/// <summary>
///     Wraps a <see cref="TotalPackagingTonnagePerRun" /> collection with a (ProducerId, SubsidiaryId)-keyed
///     dictionary and a precomputed grand total, so per-producer percentage lookups become O(1).
///     Implements <see cref="IReadOnlyList{T}" /> so callers that expect the original sequence keep working.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class TotalPackagingTonnageIndex : IReadOnlyList<TotalPackagingTonnagePerRun>
{
    private readonly IReadOnlyList<TotalPackagingTonnagePerRun> items;
    private readonly Dictionary<(int ProducerId, string? SubsidiaryId), decimal> tonnageByProducerSubsidiary;

    public TotalPackagingTonnageIndex(IReadOnlyList<TotalPackagingTonnagePerRun> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        items = source;
        tonnageByProducerSubsidiary = new Dictionary<(int, string?), decimal>(items.Count);

        decimal grandTotal = 0;

        foreach (var item in items)
        {
            grandTotal += item.TotalPackagingTonnage;

            // Preserve `FirstOrDefault` semantics: keep the first record encountered per key.
            tonnageByProducerSubsidiary.TryAdd((item.ProducerId, item.SubsidiaryId), item.TotalPackagingTonnage);
        }

        GrandTotal = grandTotal;
    }

    public decimal GrandTotal { get; }

    public int Count => items.Count;

    public TotalPackagingTonnagePerRun this[int index] => items[index];

    public IEnumerator<TotalPackagingTonnagePerRun> GetEnumerator() =>
        items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    public bool TryGetTonnage(int producerId, string? subsidiaryId, out decimal tonnage) =>
        tonnageByProducerSubsidiary.TryGetValue((producerId, subsidiaryId), out tonnage);
}
