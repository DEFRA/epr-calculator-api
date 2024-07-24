using System.Reflection.Metadata.Ecma335;

namespace api.Dtos
{
    public class CreateDefaultParameterSettingDto
    {
        public string ParameterYear { get; set; }
        public IEnumerable<SchemeParameterTemplateValue> SchemeParameterTemplateValues { get; set; }
    }
}
