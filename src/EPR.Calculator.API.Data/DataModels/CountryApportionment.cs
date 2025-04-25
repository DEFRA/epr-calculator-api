namespace EPR.Calculator.API.Data.DataModels
{
    public class CountryApportionment
    {
        public int Id { get; set; }

        public decimal Apportionment { get; set; }

        public required int CountryId { get; set; }

        public required int CostTypeId { get; set; }

        public required int CalculatorRunId { get; set; }

        public virtual Country? Country { get; set; }

        public virtual CostType? CostType { get; set; }

        public virtual CalculatorRun? CalculatorRun { get; set; }
    }
}