using System.Text.Encodings.Web;
using System.Text.Json;
using EPR.Calculator.API.BackgroundService.Converter;
using EPR.Calculator.API.BackgroundService.Exporter.JsonExporter.Model;
using EPR.Calculator.API.BackgroundService.Features.BillingRuns.Contexts;
using EPR.Calculator.API.BackgroundService.Models;
using EPR.Calculator.API.BackgroundService.Services;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.Exporter.JsonExporter;

public interface IBillingFileJsonWriter
{
    Task WriteTo(Stream stream, BillingRunContext runContext, CalcResult calcResult, IEnumerable<FeeDetail> producerFeeDetails, CancellationToken cancellationToken);
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

    public async Task WriteTo(Stream stream, BillingRunContext runContext, CalcResult calcResult, IEnumerable<FeeDetail> producerFeeDetails, CancellationToken cancellationToken)
    {
        var materials = (await materialService.GetMaterials())
                            .Select(m => m.Code switch
                            {
                                "PC" => m with { Name = "Paper or card" },
                                "FC" => m with { Name = "Fibre composite" },
                                "OT" => m with { Name = "Other materials" },
                                _ => m
                            }).ToImmutableList(); //Maintain previous capitalisation

        // producerFeeDetails is a deferred, streamed DB read; JsonSerializer pulls one producer at a
        // time and SerializeAsync flushes to the stream as it goes, so neither the fee graph nor the
        // serialized document is ever held whole in memory.
        var billingFileContent = BillingFileJson.From(runContext, calcResult, materials, producerFeeDetails);

        await JsonSerializer.SerializeAsync(stream, billingFileContent, JsonSerializerOptions, cancellationToken);
    }
}
