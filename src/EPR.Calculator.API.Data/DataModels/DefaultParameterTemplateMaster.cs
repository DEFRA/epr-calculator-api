namespace EPR.Calculator.API.Data.DataModels
{
    public class DefaultParameterTemplateMaster
    {
        public required string ParameterUniqueReferenceId { get; set; }

        public required string ParameterType { get; set; }

        public required string ParameterCategory { get; set; }

        public decimal ValidRangeFrom { get; set; }

        public decimal ValidRangeTo { get; set; }
    }
}
