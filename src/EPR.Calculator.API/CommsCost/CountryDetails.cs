namespace EPR.Calculator.API.CommsCost
{
    /// <summary>
    /// For use in database selects, to only retrieve the values neccesary for generating the CommsCost report.
    /// </summary>
    internal struct CountryDetails
    {
        public int Id { get; init; }

        public string Name { get; init; }

        public decimal Apportionment { get; init; }
    }
}
