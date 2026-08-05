using System.Text.Json.Serialization;

namespace EPR.Calculator.Service.Function.Models
{
    public record CalcResultOnePlusFourApportionmentDetailJson
    {
        [JsonPropertyName("england")]        
        public string England { get; set; } = string.Empty;

        [JsonPropertyName("wales")]
        public string Wales { get; set; } = string.Empty;

        [JsonPropertyName("scotland")]
        public string Scotland { get; set; } = string.Empty;

        [JsonPropertyName("northernIreland")]
        public string NorthernIreland { get; set; } = string.Empty;

        [JsonPropertyName("total")]
        public string Total { get; set; } = string.Empty;
    }
}