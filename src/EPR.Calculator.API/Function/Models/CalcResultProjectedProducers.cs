namespace EPR.Calculator.Service.Function.Models
{
    public record ProjectedProducersHeader
    {
        required public string Name { get; init; }
        public int? ColumnIndex { get; init; }
    }

    public record ProjectedProducersHeaders {
        public required ProjectedProducersHeader TitleHeader { get; init; }
        public required IEnumerable<ProjectedProducersHeader> MaterialBreakdownHeaders { get; init; }
        public required IEnumerable<ProjectedProducersHeader> ColumnHeaders { get; init; }
    }

    public abstract record ICalcResultProjectedProducer
    {
        public required int ProducerId { get; init; }
        public string? SubsidiaryId { get; init; }
        public required string Level { get; init; }
        public required string SubmissionPeriodCode { get; init; }
        public bool IsSubtotal { get; init; }
        public abstract IEnumerable<KeyValuePair<string, CalcResultProjectedProducerMaterialTonnage>> ProjectedTonnageByMaterial { get; }
    }

    public record CalcResultH2ProjectedProducer : ICalcResultProjectedProducer
    {
        public IReadOnlyDictionary<string, CalcResultH2ProjectedProducerMaterialTonnage> H2ProjectedTonnageByMaterial { get; init; }
            = new Dictionary<string, CalcResultH2ProjectedProducerMaterialTonnage>();

        public override IEnumerable<KeyValuePair<string, CalcResultProjectedProducerMaterialTonnage>> ProjectedTonnageByMaterial =>
            H2ProjectedTonnageByMaterial.Select(kv => new KeyValuePair<string, CalcResultProjectedProducerMaterialTonnage>(kv.Key, kv.Value));
    }

    public record CalcResultH1ProjectedProducer : ICalcResultProjectedProducer
    {
        public IReadOnlyDictionary<string, CalcResultH1ProjectedProducerMaterialTonnage> H1ProjectedTonnageByMaterial { get; init; }
            = new Dictionary<string, CalcResultH1ProjectedProducerMaterialTonnage>();
        public override IEnumerable<KeyValuePair<string, CalcResultProjectedProducerMaterialTonnage>> ProjectedTonnageByMaterial =>
            H1ProjectedTonnageByMaterial.Select(kv => new KeyValuePair<string, CalcResultProjectedProducerMaterialTonnage>(kv.Key, kv.Value));
    }

    public record CalcResultProjectedProducers
    {
        public IImmutableList<CalcResultH2ProjectedProducer>? H2ProjectedProducers { get; init; }
        public IImmutableList<CalcResultH1ProjectedProducer>? H1ProjectedProducers { get; init; }
    }
}
