using System.Text;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Enums;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.CancelledProducers
{
    public class CalcResultCancelledProducersExporter : ICalcResultCancelledProducersExporter
    {

        public void Export(CalcResultCancelledProducersResponse calcResultCancelledProducers, StringBuilder csvContent)
        {
            // Add empty lines
            csvContent.AppendLine();
            csvContent.AppendLine();

            // Add headers
            PrepareCancelledProducersHeader(calcResultCancelledProducers, csvContent);
            PrepareCancelledProducersValues(calcResultCancelledProducers, csvContent);
        }

        private static void PrepareCancelledProducersHeader(CalcResultCancelledProducersResponse response, StringBuilder csvContent)
        {
            // Add cancelled producers header
            csvContent.AppendLine(CsvSanitiser.SanitiseData(response.TitleHeader!));

            // Add sub header
            WriteCancelledProducersSecondaryHeaders(csvContent);

            // Add column header
            WriteCancelledProducersColumnHeaders(csvContent);
            csvContent.AppendLine();
        }

        private static void PrepareCancelledProducersValues(CalcResultCancelledProducersResponse response, StringBuilder csvContent)
        {
            foreach (var CancelledProducer in response.CancelledProducers)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.ProducerId));
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.ProducerOrSubsidiaryNameValue));
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.TradingNameValue));
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.LastTonnage?.AluminiumValue));
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.LastTonnage?.FibreCompositeValue));
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.LastTonnage?.GlassValue));
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.LastTonnage?.PaperOrCardValue));
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.LastTonnage?.PlasticValue));
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.LastTonnage?.SteelValue));
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.LastTonnage?.WoodValue));
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.LastTonnage?.OtherMaterialsValue));
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.LatestInvoice?.CurrentYearInvoicedTotalToDateValue, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.LatestInvoice?.RunNumberValue));
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.LatestInvoice?.RunNameValue));
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.LatestInvoice?.BillingInstructionIdValue));
                csvContent.AppendLine();
            }

        }
        private static void WriteCancelledProducersSecondaryHeaders(StringBuilder csvContent)
        {
            var headers = new Dictionary<int, string>
            {
                { CommonConstants.LastTonnageSubHeaderIndex, CommonConstants.LastTonnage },
                { CommonConstants.LatestInvoiceSubHeaderIndex, CommonConstants.LatestInvoice }
            };

            var maxColumnSize = headers.Keys.Max() + 1;
            var headerRows = new string[maxColumnSize];

            foreach (var header in headers)
            {
                headerRows[header.Key] = CsvSanitiser.SanitiseData(header.Value);
            }

            var headerRow = string.Join(CommonConstants.CsvFileDelimiter, headerRows);
            csvContent.AppendLine(headerRow);
        }

        private static void WriteCancelledProducersColumnHeaders(StringBuilder csvContent)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.ProducerId));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.ProducerOrSubsidiaryName));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.TradingName));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.Aluminium));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.FibreComposite));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.Glass));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.PaperOrCard));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.Plastic));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.Steel));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.Wood));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.OtherMaterials));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.CurrentYearInvoicedTotalToDate));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.RunNumber));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.RunName));
            csvContent.Append(CsvSanitiser.SanitiseData(CommonConstants.BillingInstructionId));
        }
    }
}