using System.Text;
using EPR.Calculator.API.BackgroundService.Constants;
using EPR.Calculator.API.BackgroundService.Enums;
using EPR.Calculator.API.BackgroundService.Misc;
using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.CancelledProducers
{
    public interface ICalcResultCancelledProducersExporter
    {
        public void Export(IReadOnlyList<CalcResultCancelledProducer> calcResultCancelledProducers, IImmutableList<MaterialDetail> materials, StringBuilder csvContent);
    }

    public class CalcResultCancelledProducersExporter : ICalcResultCancelledProducersExporter
    {
        [ActivityTrace]
        public void Export(IReadOnlyList<CalcResultCancelledProducer> calcResultCancelledProducers, IImmutableList<MaterialDetail> materials, StringBuilder csvContent)
        {
            // Add empty lines
            csvContent.AppendLine();
            csvContent.AppendLine();

            // Add headers
            PrepareCancelledProducersHeader(materials, csvContent);
            PrepareCancelledProducersValues(calcResultCancelledProducers, csvContent);
        }

        private static void PrepareCancelledProducersHeader(IImmutableList<MaterialDetail> materials, StringBuilder csvContent)
        {
            // Add cancelled producers header
            csvContent.AppendLine(CsvSanitiser.SanitiseData(CalcResultCancelledProducersHeader.CancelledProducers));

            // Add sub header
            WriteCancelledProducersSecondaryHeaders(csvContent);

            // Add column header
            WriteCancelledProducersColumnHeaders(materials, csvContent);
            csvContent.AppendLine();
        }

        private static void PrepareCancelledProducersValues(IReadOnlyList<CalcResultCancelledProducer> calcResultCancelledProducers, StringBuilder csvContent)
        {
            foreach (var CancelledProducer in calcResultCancelledProducers)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.ProducerId));
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.ProducerOrSubsidiaryName));
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.TradingName));
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.LastTonnage?.Aluminium));
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.LastTonnage?.FibreComposite));
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.LastTonnage?.Glass));
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.LastTonnage?.PaperOrCard));
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.LastTonnage?.Plastic));
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.LastTonnage?.Steel));
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.LastTonnage?.Wood));
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.LastTonnage?.OtherMaterials));
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.LatestInvoice?.CurrentYearInvoicedTotalToDate, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.LatestInvoice?.RunNumber));
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.LatestInvoice?.RunName));
                csvContent.Append(CsvSanitiser.SanitiseData(CancelledProducer.LatestInvoice?.BillingInstructionId));
                csvContent.AppendLine();
            }

        }
        private static void WriteCancelledProducersSecondaryHeaders(StringBuilder csvContent)
        {
            var headers = new Dictionary<int, string>
            {
                { CalcResultCancelledProducersHeader.LastTonnageSubHeaderIndex  , CalcResultCancelledProducersHeader.LastTonnage },
                { CalcResultCancelledProducersHeader.LatestInvoiceSubHeaderIndex, CalcResultCancelledProducersHeader.LatestInvoice }
            };

            var maxColumnSize = headers.Keys.Max() + 1;
            var headerRows = new string[maxColumnSize];

            foreach (var header in headers)
            {
                headerRows[header.Key] = header.Value;
            }

            var headerRow = string.Join("", headerRows.Select(x => CsvSanitiser.SanitiseData(x)));
            csvContent.AppendLine(headerRow);
        }

        private static void WriteCancelledProducersColumnHeaders(IImmutableList<MaterialDetail> materials, StringBuilder csvContent)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(CalcResultCancelledProducersHeader.ProducerId));
            csvContent.Append(CsvSanitiser.SanitiseData(CalcResultCancelledProducersHeader.ProducerOrSubsidiaryName));
            csvContent.Append(CsvSanitiser.SanitiseData(CalcResultCancelledProducersHeader.TradingName));
            foreach (var mat in materials.Select(m => m.Name))
            {
                csvContent.Append(CsvSanitiser.SanitiseData(mat));
            }
            csvContent.Append(CsvSanitiser.SanitiseData(CalcResultCancelledProducersHeader.CurrentYearInvoicedTotalToDate));
            csvContent.Append(CsvSanitiser.SanitiseData(CalcResultCancelledProducersHeader.RunNumber));
            csvContent.Append(CsvSanitiser.SanitiseData(CalcResultCancelledProducersHeader.RunName));
            csvContent.Append(CsvSanitiser.SanitiseData(CalcResultCancelledProducersHeader.BillingInstructionId));
        }
    }
}
