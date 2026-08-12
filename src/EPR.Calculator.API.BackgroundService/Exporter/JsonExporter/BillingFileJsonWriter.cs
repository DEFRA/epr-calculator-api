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
        var materials = (await materialService.GetMaterials())
                            .Select(m => m.Code switch
                            {
                                "PC" => m with { Name = "Paper or card" },
                                "FC" => m with { Name = "Fibre composite" },
                                "OT" => m with { Name = "Other materials" },
                                _ => m
                            }).ToImmutableList(); //Maintain previous capitalisation 

        var billingFileContent = BillingFileJson.From(runContext, calcResult, materials);

        return JsonSerializer.Serialize(billingFileContent, JsonSerializerOptions);
    }
}
