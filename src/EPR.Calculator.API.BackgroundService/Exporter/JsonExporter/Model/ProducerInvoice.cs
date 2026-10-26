using System.Globalization;
using System.Text.Json.Serialization;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.Exporter.JsonExporter.Model;

public record ProducerInvoice
{
    [JsonPropertyName("instruction")]
    public required string Instruction { get; init; }

    [JsonPropertyName("suggestedAmount")]
    public required string? SuggestedAmount { get; init; }

    [JsonPropertyName("invoicedToDate")]
    public required string? InvoicedToDate { get; init; }

    public static ProducerInvoice From(BillingInstruction? billing)
    {
        if (billing == null)
            return new() { Instruction = "", SuggestedAmount = null, InvoicedToDate = null };

        return new()
        {
            Instruction     = billing.SuggestedBillingInstruction,
            SuggestedAmount = billing.SuggestedInvoiceAmount?.ToString("F2", CultureInfo.InvariantCulture),
            InvoicedToDate  = billing.CurrentYearInvoiceTotalToDate?.ToString("F2", CultureInfo.InvariantCulture),
        };
    }
}
