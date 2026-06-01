using System.Text.Json.Serialization;

namespace EPR.Calculator.Service.Function.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RunType
{
    Unknown = 0,
    Calculator,
    Billing
}
