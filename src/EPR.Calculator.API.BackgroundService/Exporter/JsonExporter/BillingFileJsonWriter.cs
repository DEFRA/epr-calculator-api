using System.Text.Encodings.Web;
using System.Text.Json;
using EPR.Calculator.API.BackgroundService.Converter;
using EPR.Calculator.API.BackgroundService.Features.BillingRuns.Contexts;
using EPR.Calculator.API.BackgroundService.JsonExporter.Model;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.BackgroundService.Services;

namespace EPR.Calculator.API.BackgroundService.Exporter.JsonExporter;

public interface IBillingFileJsonWriter
{
    Task<string> WriteToString(BillingRunContext runContext, CalcResult calcResult);
}

public class BillingFileJsonWriter(IMaterialService materialService)
    : IBillingFileJsonWriter
{
    private const int DecimalPrecision = 3;

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new DecimalPrecisionConverter(DecimalPrecision) }
    };

    public async Task<string> WriteToString(BillingRunContext runContext, CalcResult calcResult)
    {
        var materials = await materialService.GetMaterials();
        var billingFileContent = BillingFileJson.From(runContext, calcResult, materials);

        return JsonSerializer.Serialize(billingFileContent, JsonSerializerOptions);
    }
}
