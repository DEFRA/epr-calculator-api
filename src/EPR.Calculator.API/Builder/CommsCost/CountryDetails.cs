namespace EPR.Calculator.API.Builder.CommsCost
{
    /// <summary>
    /// For use in database selects, to only retrieve the values neccesary for generating the CommsCost report.
    /// </summary>
    public record CountryDetails
    {
        public required int Id { get; init; }

        public required string Name { get; init; }

        public required decimal Apportionment { get; init; }
    }
}
