namespace EPR.Calculator.API.Data.DataModels
{
    public class LapcapDataTemplateMaster
    {
        public required string UniqueReference { get; set; }

        public required string Country { get; set; }

        public required string Material { get; set; }

        public decimal TotalCostFrom { get; set; }

        public decimal TotalCostTo { get; set; }

        public ICollection<LapcapDataDetail> Details { get; } = new List<LapcapDataDetail>();
    }
}
