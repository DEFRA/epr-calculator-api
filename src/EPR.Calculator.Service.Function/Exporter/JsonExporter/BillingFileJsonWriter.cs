using System.Text.Encodings.Web;
using System.Text.Json;
using EPR.Calculator.Service.Function.Converter;
using EPR.Calculator.Service.Function.Features.BillingRun.Contexts;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;
using EPR.Calculator.Service.Function.Services;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter;

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
