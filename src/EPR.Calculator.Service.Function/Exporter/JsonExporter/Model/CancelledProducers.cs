using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Converter;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.Service.Function.JsonExporter.Model;

public record CancelledProducers
{
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("cancelledProducerTonnageInvoice")]
    public IEnumerable<CancelledProducerTonnageInvoice>? CancelledProducerTonnageInvoices { get; init; }

    public static CancelledProducers From(IReadOnlyList<CalcResultCancelledProducer> calcResultCancelledProducers)
    {
        IEnumerable<CancelledProducerTonnageInvoice> GetCancelledProducerTonnageInvoice()
        {
            var cancelledProducerTonnageInvoices = new List<CancelledProducerTonnageInvoice>();

            foreach (var producer in calcResultCancelledProducers)
            {
                int runNumber = 0;
                if (!string.IsNullOrWhiteSpace(producer.LatestInvoice?.RunNumber))
                {
                    _ = int.TryParse(producer.LatestInvoice.RunNumber, out runNumber);
                }

                cancelledProducerTonnageInvoices.Add(CancelledProducerTonnageInvoice.From(runNumber, producer));
            }

            return cancelledProducerTonnageInvoices;
        }

        if (!calcResultCancelledProducers.Any())
        {
            return new CancelledProducers
            {
                Name = CalcResultCancelledProducersHeader.CancelledProducers,
                CancelledProducerTonnageInvoices =Array.Empty<CancelledProducerTonnageInvoice>()
            };
        }

        return new CancelledProducers
        {
            Name = CalcResultCancelledProducersHeader.CancelledProducers,
            CancelledProducerTonnageInvoices = GetCancelledProducerTonnageInvoice()
        };
    }
}

public record CancelledProducerTonnageInvoice
{
    [JsonPropertyName("producerId")]
    public required int ProducerId { get; init; }

    [JsonPropertyName("subsidiaryId")]
    public string? SubsidiaryId { get; init; }

    [JsonPropertyName("producerName")]
    public required string ProducerName { get; init; }

    [JsonPropertyName("tradingName")]
    public required string TradingName { get; init; }

    [JsonPropertyName("lastProducerTonnages")]
    public required IEnumerable<LastProducerTonnages> LastProducerTonnages { get; init; }

    [JsonPropertyName("lastInvoicedTotal")]
    [JsonConverter(typeof(CurrencyConverter))]
    public required decimal LastInvoicedTotal { get; init; }

    [JsonPropertyName("runNumber")]
    public required int RunNumber { get; init; }

    [JsonPropertyName("runName")]
    public required string RunName { get; init; }

    [JsonPropertyName("billingInstructionID")]
    public required string BillingInstructionID { get; init; }

    public static CancelledProducerTonnageInvoice From(int runNumber, CalcResultCancelledProducer producer)
    {
        IEnumerable<LastProducerTonnages> GetLastProducerTonnages(LastTonnage lastTonnage)
        {
            var lastProducerTonnagesList = new List<LastProducerTonnages>();

            lastProducerTonnagesList.AddRange([
                new LastProducerTonnages
                {
                    MaterialName = MaterialNames.Aluminium,
                    LastTonnage  = lastTonnage.Aluminium ?? CommonConstants.DefaultMinValue,
                },
                new LastProducerTonnages
                {
                    MaterialName = MaterialNames.FibreComposite,
                    LastTonnage  = lastTonnage.FibreComposite ?? CommonConstants.DefaultMinValue,
                },
                new LastProducerTonnages
                {
                    MaterialName = MaterialNames.Glass,
                    LastTonnage  = lastTonnage.Glass ?? CommonConstants.DefaultMinValue,
                },
                new LastProducerTonnages
                {
                    MaterialName = MaterialNames.PaperOrCard,
                    LastTonnage  = lastTonnage.PaperOrCard ?? CommonConstants.DefaultMinValue,
                },
                new LastProducerTonnages
                {
                    MaterialName = MaterialNames.Plastic,
                    LastTonnage  = lastTonnage.Plastic ?? CommonConstants.DefaultMinValue,
                },
                new LastProducerTonnages
                {
                    MaterialName = MaterialNames.Steel,
                    LastTonnage  = lastTonnage.Steel ?? CommonConstants.DefaultMinValue,
                },
                new LastProducerTonnages
                {
                    MaterialName = MaterialNames.Wood,
                    LastTonnage  = lastTonnage.Wood ?? CommonConstants.DefaultMinValue,
                },
                new LastProducerTonnages
                {
                    MaterialName = MaterialNames.OtherMaterials,
                    LastTonnage  = lastTonnage.OtherMaterials ?? CommonConstants.DefaultMinValue,
                }
            ]);

            return lastProducerTonnagesList;
        }

        return new CancelledProducerTonnageInvoice
        {
            ProducerId           = producer.ProducerId,
            SubsidiaryId         = producer.SubsidiaryId ?? string.Empty,
            ProducerName         = producer.ProducerOrSubsidiaryName ?? string.Empty,
            TradingName          = producer.TradingName ?? string.Empty,
            LastProducerTonnages = GetLastProducerTonnages(producer.LastTonnage!),
            RunNumber            = runNumber,
            RunName              = producer.LatestInvoice?.RunName ?? string.Empty,
            BillingInstructionID = producer.LatestInvoice?.BillingInstructionId ?? string.Empty,
            LastInvoicedTotal    = producer.LatestInvoice?.CurrentYearInvoicedTotalToDate ?? 0m
        };
    }
}

public record LastProducerTonnages
{
    [JsonPropertyName("materialName")]
    public required string MaterialName { get; init; }

    [JsonPropertyName("lastTonnage")]
    public required decimal LastTonnage { get; init; }
}
