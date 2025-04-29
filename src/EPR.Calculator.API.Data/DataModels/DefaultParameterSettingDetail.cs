namespace EPR.Calculator.API.Data.DataModels
{
    public class DefaultParameterSettingDetail
    {
        public int Id { get; set; }

        public int DefaultParameterSettingMasterId { get; set; }

        public DefaultParameterSettingMaster? DefaultParameterSettingMaster { get; set; }

        public required string ParameterUniqueReferenceId { get; set; }

        public DefaultParameterTemplateMaster? ParameterUniqueReference { get; set; }

        public decimal ParameterValue { get; set; }
    }
}
