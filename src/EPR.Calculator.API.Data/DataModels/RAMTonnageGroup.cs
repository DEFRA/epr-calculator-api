namespace EPR.Calculator.API.Data.DataModels
{
    public record RamTonnageGroup
    {
        public decimal? Red { get; init; }
        public decimal? Amber { get; init; }
        public decimal? Green { get; init; }
        public decimal? Total { get; init; }
        public static RamTonnageGroup Empty => new RamTonnageGroup();
    }
}
