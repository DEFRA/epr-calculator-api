using System.Text.Json.Serialization;

namespace EPR.Calculator.Service.Function.Models.JsonExporter
{
    public class BillingFileJson
    {
        [JsonPropertyName("calcResultDetail")]
        public CalcResultDetailJson? CalcResultDetail { get; set; }

        [JsonPropertyName("calcResultLapcapData")]
        public object? CalcResultLapcapData { get; set; }

        [JsonPropertyName("calcResultLateReportingTonnageData")]
        public CalcResultLateReportingTonnageJson? CalcResultLateReportingTonnageData { get; set; }

        [JsonPropertyName("parametersOther")]
        public CalcResultParametersOtherJson? ParametersOther { get; set; }

        [JsonPropertyName("onePlusFourApportionment")]
        public CalcResultOnePlusFourApportionmentJson? OnePlusFourApportionment { get; set; }

        [JsonPropertyName("parametersCommsCost")]
        public CalcResultCommsCostJson? ParametersCommsCost { get; set; }

        [JsonPropertyName("calcResult2aCommsDataByMaterial")]
        public CalcResult2ACommsDataByMaterial? CalcResult2aCommsDataByMaterial { get; set; }

        [JsonPropertyName("calcResult2bCommsDataByUkWide")]
        public CalcResultCommsCostOnePlusFourApportionmentUKWide? CalcResult2bCommsDataByUkWide { get; set; }

        [JsonPropertyName("calcResult2cCommsDataByCountry")]
        public CalcResultCommsCostOnePlusFourApportionmentCountryWide? CalcResult2cCommsDataByCountry { get; set; }

        [JsonPropertyName("calcResultLaDisposalCostData")]
        public CalcResultLaDisposalCostDataJson? CalcResultLaDisposalCostData { get; set; }

        [JsonPropertyName("cancelledProducers")]
        public CancelledProducers? CancelledProducers { get; set; }

        [JsonPropertyName("scaleUpProducers")]
        public CalcResultScaledupProducersJson? ScaleUpProducers { get; set; }

        [JsonPropertyName("modulationResults")]
        public CalcResultModulationResults? ModulationResults { get; set; }

        [JsonPropertyName("calculationResults")]
        public object? CalculationResults { get; set; }

        public static BillingFileJson From(CalcResult results, IEnumerable<int> acceptedProducerIds, IImmutableList<MaterialDetail> materials)
        {
            return new BillingFileJson {
                CalcResultDetail                   = CalcResultDetailJson.From(results.CalcResultDetail),
                CalcResultLapcapData               = CalcResultLapcapDataJson.From(results.CalcResultLapcapData, materials),
                CalcResultLateReportingTonnageData = CalcResultLateReportingTonnageJson.From(results.CalcResultLateReportingTonnageData, materials),
                ParametersOther                    = CalcResultParametersOtherJson.From(results.CalcResultParameterOtherCost),
                OnePlusFourApportionment           = CalcResultOnePlusFourApportionmentJson.From(results.CalcResultOnePlusFourApportionment),
                ParametersCommsCost                = CalcResultCommsCostJson.From(results.CalcResultCommsCostReportDetail),
                CalcResult2aCommsDataByMaterial    = CalcResult2ACommsDataByMaterial.From(materials, results.CalcResultCommsCostReportDetail.ByMaterial, results.CalcResultCommsCostReportDetail.Total),
                CalcResult2bCommsDataByUkWide      = CalcResultCommsCostOnePlusFourApportionmentUKWide.From(results.CalcResultCommsCostReportDetail.CommsCostUkWide),
                CalcResult2cCommsDataByCountry     = CalcResultCommsCostOnePlusFourApportionmentCountryWide.From(results.CalcResultCommsCostReportDetail.CommsCostByCountry),
                CalcResultLaDisposalCostData       = CalcResultLaDisposalCostDataJson.From(results.CalcResultLaDisposalCostData.ByMaterial, results.CalcResultLaDisposalCostData.Total, materials, results.ApplyModulation),
                CancelledProducers                 = CancelledProducers.From(results.CalcResultCancelledProducers),
                ScaleUpProducers                   = CalcResultScaledupProducersJson.From(results.CalcResultScaledupProducers, acceptedProducerIds, materials),
                ModulationResults                  = results.ApplyModulation && results.CalcResultModulation is not null ? CalcResultModulationResults.From(results.CalcResultModulation) : null,
                CalculationResults                 = CalculationResultsJson.From(results, acceptedProducerIds, materials)
            };
        }
    }
}
