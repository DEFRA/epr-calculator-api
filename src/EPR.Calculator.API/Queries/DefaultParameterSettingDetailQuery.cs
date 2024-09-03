namespace EPR.Calculator.API.Queries
{
    public class DefaultParameterSettingDetailQuery
    {
        public DefaultParameterSettingDetailQuery(string parameterYear) 
        {
            this.ParameterYear = parameterYear;
        }
        public string ParameterYear { get; set; }
    }
}
