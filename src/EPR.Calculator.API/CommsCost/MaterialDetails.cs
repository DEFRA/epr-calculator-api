namespace EPR.Calculator.API.CommsCost
{
    /// <summary>
    /// For use in database selects, to only retrieve the values neccesary for generating the CommsCost report.
    /// </summary>
    public record MaterialDetails
    {
        public required int Id { get; init; }

        public required string Name { get; init; }

        public required decimal TotalValue { get; init; }

        public required decimal LateReportingTonnage { get; init; }

        public required decimal ProdRepHoPaWaT { get; init; }
    }
}
