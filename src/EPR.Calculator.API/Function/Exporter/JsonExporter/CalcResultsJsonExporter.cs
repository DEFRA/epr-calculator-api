using System.Text.Encodings.Web;
using System.Text.Json;
using EPR.Calculator.Service.Function.Converter;
using EPR.Calculator.Service.Function.Models;
using EPR.Calculator.Service.Function.Models.JsonExporter;

namespace EPR.Calculator.Service.Function.Exporter.JsonExporter
{
    public interface ICalcBillingJsonExporter
    {
        string Export(CalcResult results, IImmutableList<MaterialDetail> materials, IEnumerable<int> acceptedProducerIds);
    }
    public class CalcResultsJsonExporter : ICalcBillingJsonExporter
    {
        private const int DecimalPrecision = 3;

        public string Export(CalcResult results, IImmutableList<MaterialDetail> materials, IEnumerable<int> acceptedProducerIds)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results), "The results parameter cannot be null.");
            }

            var billingFileContent = BillingFileJson.From(results, acceptedProducerIds, materials);

            return JsonSerializer.Serialize(
                billingFileContent,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true,
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    Converters = { new DecimalPrecisionConverter(DecimalPrecision), },
                });
        }
    }
}
