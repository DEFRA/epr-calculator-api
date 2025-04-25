namespace EPR.Calculator.API.Data.DataModels
{
    public class LapcapDataDetail
    {
        public int Id { get; set; }

        public int LapcapDataMasterId { get; set; }

        public required string UniqueReference { get; set; }

        public decimal TotalCost { get; set; }

        public required LapcapDataMaster LapcapDataMaster { get; set; }

        public virtual LapcapDataTemplateMaster? LapcapDataTemplateMaster { get; set; }
    }
}
