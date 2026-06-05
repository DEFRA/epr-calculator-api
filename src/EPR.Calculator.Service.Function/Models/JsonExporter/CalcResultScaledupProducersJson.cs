using System.Text.Json.Serialization;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Converter;
using EPR.Calculator.Service.Function.Features.BillingRun.Contexts;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public record CalcResultScaledupProducersJson
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("producerSubmissions")]
        public IEnumerable<ProducerSubmission>? ProducerSubmissions { get; set; }

        public static CalcResultScaledupProducersJson From(
            BillingRunContext runContext,
            CalcResultScaledupProducers calcResultScaledupProducers,
            IImmutableList<MaterialDetail> materials)
        {
            IEnumerable<ProducerSubmission> GetProducerSubmissions()
            {
                var producerSubmissions = new List<ProducerSubmission>();

                var filteredProducers = calcResultScaledupProducers?.ScaledupProducers?.Where(producer => runContext.AcceptedProducerIds.Contains(producer.ProducerId)) ?? [];

                foreach (var item in filteredProducers)
                {
                    int? level = null;
                    if (!string.IsNullOrWhiteSpace(item.Level) && int.TryParse(item.Level, out int result))
                    {
                        level = result;
                    }

                    producerSubmissions.Add(ProducerSubmission.From(level, item, materials));
                }

                return producerSubmissions;
            }

            if (calcResultScaledupProducers == null)
            {
                return new CalcResultScaledupProducersJson();
            }

            return new CalcResultScaledupProducersJson
            {
                Name = CalcResultScaledupProducerHeaders.ScaledupProducers,
                ProducerSubmissions = GetProducerSubmissions()
            };
        }
    }

    public record ProducerSubmission
    {
        [JsonPropertyName("producerId")]
        public int ProducerId { get; set; }

        [JsonPropertyName("subsidiaryId")]
        public string? SubsidiaryId { get; set; }

        [JsonPropertyName("producerName")]
        public string? ProducerName { get; set; }

        [JsonPropertyName("tradingName")]
        public string? TradingName { get; set; }

        [JsonPropertyName("level")]
        public int? Level { get; set; }

        [JsonPropertyName("submissionPeriodCode")]
        public string? SubmissionPeriodCode { get; set; }

        [JsonPropertyName("daysInSubmissionPeriod")]
        public int DaysInSubmissionPeriod { get; set; }

        [JsonPropertyName("daysInWholePeriod")]
        public int DaysInWholePeriod { get; set; }

        [JsonPropertyName("scaleUpFactor")]
        [JsonConverter(typeof(DecimalPrecisionTwelveConverter))]
        public decimal ScaleUpFactor { get; set; }

        [JsonPropertyName("materialBreakdown")]
        public required IEnumerable<MaterialBreakdown> MaterialBreakdown { get; set; }

        public static ProducerSubmission From(int? level, CalcResultScaledupProducer item, IImmutableList<MaterialDetail> materials)
        {
            IEnumerable<MaterialBreakdown> GetMaterialBreakdown(Dictionary<string, CalcResultScaledupProducerTonnage> producerTonnageByMaterial, IImmutableList<MaterialDetail> materials)
            {
                var materialBreakdown = new List<MaterialBreakdown>();

                foreach (var producerTonnage in producerTonnageByMaterial)
                {
                    var material = materials.Single(m => m.Code == producerTonnage.Key);

                    var breakdown = JsonExporter.MaterialBreakdown.From(material.Name, producerTonnage.Value);

                    if (producerTonnage.Key == MaterialCodes.Glass)
                    {
                        breakdown.HouseholdDrinksContainersTonnageGlass = producerTonnage.Value.HouseholdDrinksContainersTonnageGlass;
                        breakdown.ScaledUpHouseholdDrinksContainersTonnageGlass = producerTonnage.Value.ScaledupHouseholdDrinksContainersTonnageGlass;
                    }

                    materialBreakdown.Add(breakdown);
                }

                return materialBreakdown;
            }

            return new ProducerSubmission
            {
                ProducerId = item.ProducerId,
                SubsidiaryId = string.IsNullOrWhiteSpace(item.SubsidiaryId) ? null : item.SubsidiaryId,
                ProducerName = item.ProducerName,
                TradingName = string.IsNullOrWhiteSpace(item.TradingName) ? null : item.TradingName,
                Level = level,
                SubmissionPeriodCode = item.SubmissionPeriodCode,
                DaysInSubmissionPeriod = item.DaysInSubmissionPeriod,
                DaysInWholePeriod = item.DaysInWholePeriod,
                ScaleUpFactor = item.ScaleupFactor,
                MaterialBreakdown = GetMaterialBreakdown(item.ScaledupProducerTonnageByMaterial, materials)
            };
        }
    }

    public record MaterialBreakdown
    {
        [JsonPropertyName("materialName")]
        public string? MaterialName { get; set; }

        [JsonPropertyName("reportedHouseholdPackagingWasteTonnage")]
        public decimal ReportedHouseholdPackagingWasteTonnage { get; set; }

        [JsonPropertyName("reportedPublicBinTonnage")]
        public decimal ReportedPublicBinTonnage { get; set; }

        [JsonPropertyName("householdDrinksContainersTonnageGlass")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? HouseholdDrinksContainersTonnageGlass { get; set; }

        [JsonPropertyName("totalReportedTonnage")]
        public decimal TotalReportedTonnage { get; set; }

        [JsonPropertyName("reportedSelfManagedConsumerWasteTonnage")]
        public decimal ReportedSelfManagedConsumerWasteTonnage { get; set; }

        [JsonPropertyName("netReportedTonnage")]
        public decimal NetReportedTonnage { get; set; }

        [JsonPropertyName("scaledUpReportedHouseholdPackagingWasteTonnage")]
        public decimal ScaledUpReportedHouseholdPackagingWasteTonnage { get; set; }

        [JsonPropertyName("scaledUpReportedPublicBinTonnage")]
        public decimal ScaledUpReportedPublicBinTonnage { get; set; }

        [JsonPropertyName("scaledUpHouseholdDrinksContainersTonnageGlass")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public decimal? ScaledUpHouseholdDrinksContainersTonnageGlass { get; set; }

        [JsonPropertyName("scaledUpTotalReportedTonnage")]
        public decimal ScaledUpTotalReportedTonnage { get; set; }

        [JsonPropertyName("scaledUpReportedSelfManagedConsumerWasteTonnage")]
        public decimal ScaledUpReportedSelfManagedConsumerWasteTonnage { get; set; }

        [JsonPropertyName("scaledUpNetReportedTonnage")]
        public decimal ScaledUpNetReportedTonnage { get; set; }

        public static MaterialBreakdown From(string materialName, CalcResultScaledupProducerTonnage producerTonnage)
        {
            return new MaterialBreakdown
            {
                MaterialName = materialName,
                ReportedHouseholdPackagingWasteTonnage = producerTonnage.ReportedHouseholdPackagingWasteTonnage,
                ReportedPublicBinTonnage = producerTonnage.ReportedPublicBinTonnage,
                TotalReportedTonnage = producerTonnage.TotalReportedTonnage,
                ReportedSelfManagedConsumerWasteTonnage = producerTonnage.ReportedSelfManagedConsumerWasteTonnage,
                NetReportedTonnage = producerTonnage.NetReportedTonnage,
                ScaledUpReportedHouseholdPackagingWasteTonnage = producerTonnage.ScaledupReportedHouseholdPackagingWasteTonnage,
                ScaledUpReportedPublicBinTonnage = producerTonnage.ScaledupReportedPublicBinTonnage,
                ScaledUpTotalReportedTonnage = producerTonnage.ScaledupTotalReportedTonnage,
                ScaledUpReportedSelfManagedConsumerWasteTonnage = producerTonnage.ScaledupReportedSelfManagedConsumerWasteTonnage,
                ScaledUpNetReportedTonnage = producerTonnage.ScaledupNetReportedTonnage,
            };
        }
    }
}
