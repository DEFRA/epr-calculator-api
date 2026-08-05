using System.Text.Json.Serialization;

namespace EPR.Calculator.API.BackgroundService.Features.Common;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RunType
{
    Unknown = 0,
    Calculator,
    Billing
}
