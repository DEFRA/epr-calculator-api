using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Converter;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public record CancelledProducers
    {
        [JsonPropertyName("name")]
        public string? Name { get; init; }

        [JsonPropertyName("cancelledProducerTonnageInvoice")]
        public IEnumerable<CancelledProducerTonnageInvoice>? CancelledProducerTonnageInvoice { get; init; }

        public static CancelledProducers From(CalcResultCancelledProducersResponse calcResultCancelledProducersResponse)
        {
            IEnumerable<CancelledProducerTonnageInvoice> GetCancelledProducerTonnageInvoice(CalcResultCancelledProducersResponse calcResultCancelledProducersResponse)
            {
                var cancelledProducerTonnageInvoices = new List<CancelledProducerTonnageInvoice>();

                foreach (var producer in calcResultCancelledProducersResponse.CancelledProducers)
                {
                    int runNumber = 0;
                    if (!string.IsNullOrWhiteSpace(producer.LatestInvoice?.RunNumberValue))
                    {
                        _ = int.TryParse(producer.LatestInvoice!.RunNumberValue, out runNumber);
                    }

                    cancelledProducerTonnageInvoices.Add(JsonExporter.CancelledProducerTonnageInvoice.From(runNumber, producer));
                }

                return cancelledProducerTonnageInvoices;
            }

            if (!calcResultCancelledProducersResponse.CancelledProducers.Any())
            {
                return new CancelledProducers
                {
                    Name = CommonConstants.CancelledProducers,
                    CancelledProducerTonnageInvoice = Array.Empty<CancelledProducerTonnageInvoice>()
                };
            }

            return new CancelledProducers
            {
                Name = CommonConstants.CancelledProducers,
                CancelledProducerTonnageInvoice = GetCancelledProducerTonnageInvoice(calcResultCancelledProducersResponse)
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
        
        public static CancelledProducerTonnageInvoice From(int runNumber, CalcResultCancelledProducersDto producer)
        {
            IEnumerable<LastProducerTonnages> GetLastProducerTonnages(LastTonnage lastTonnage)
            {
                var lastProducerTonnagesList = new List<LastProducerTonnages>();

                lastProducerTonnagesList.AddRange([
                    new LastProducerTonnages
                    {
                        MaterialName = lastTonnage.Aluminium_Header ?? MaterialNames.Aluminium,
                        LastTonnage = lastTonnage.AluminiumValue ?? CommonConstants.DefaultMinValue,
                    },
                    new LastProducerTonnages
                    {
                        MaterialName = lastTonnage.FibreComposite_Header ?? MaterialNames.FibreComposite,
                        LastTonnage = lastTonnage.FibreCompositeValue ?? CommonConstants.DefaultMinValue,
                    },
                    new LastProducerTonnages
                    {
                        MaterialName = lastTonnage.Glass_Header ?? MaterialNames.Glass,
                        LastTonnage = lastTonnage.GlassValue ?? CommonConstants.DefaultMinValue,
                    },
                    new LastProducerTonnages
                    {
                        MaterialName = lastTonnage.PaperOrCard_Header ?? MaterialNames.PaperOrCard,
                        LastTonnage = lastTonnage.PaperOrCardValue ?? CommonConstants.DefaultMinValue,
                    },
                    new LastProducerTonnages
                    {
                        MaterialName = lastTonnage.Plastic_Header ?? MaterialNames.Plastic,
                        LastTonnage = lastTonnage.PlasticValue ?? CommonConstants.DefaultMinValue,
                    },
                    new LastProducerTonnages
                    {
                        MaterialName = lastTonnage.Steel_Header ?? MaterialNames.Steel,
                        LastTonnage = lastTonnage.SteelValue ?? CommonConstants.DefaultMinValue,
                    },
                    new LastProducerTonnages
                    {
                        MaterialName = lastTonnage.Wood_Header ?? MaterialNames.Wood,
                        LastTonnage = lastTonnage.WoodValue ?? CommonConstants.DefaultMinValue,
                    },
                    new LastProducerTonnages
                    {
                        MaterialName = lastTonnage.OtherMaterials_Header ?? MaterialNames.OtherMaterials,
                        LastTonnage = lastTonnage.OtherMaterialsValue ?? CommonConstants.DefaultMinValue,
                    }
                ]);

                return lastProducerTonnagesList;
            }

            return new CancelledProducerTonnageInvoice
            {
                ProducerId = producer.ProducerId,
                SubsidiaryId = producer.SubsidiaryIdValue ?? string.Empty,
                ProducerName = producer.ProducerOrSubsidiaryNameValue ?? string.Empty,
                TradingName = producer.TradingNameValue ?? string.Empty,
                LastProducerTonnages = GetLastProducerTonnages(producer.LastTonnage!),

                RunNumber = runNumber,
                RunName = producer.LatestInvoice?.RunNameValue ?? string.Empty,
                BillingInstructionID = producer.LatestInvoice?.BillingInstructionIdValue ?? string.Empty,
                LastInvoicedTotal = producer.LatestInvoice?.CurrentYearInvoicedTotalToDateValue ?? 0m
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
}
