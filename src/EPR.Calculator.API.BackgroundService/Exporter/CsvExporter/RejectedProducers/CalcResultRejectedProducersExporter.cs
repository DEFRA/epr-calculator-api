using System.Text;
using EPR.Calculator.API.BackgroundService.Constants;
using EPR.Calculator.API.BackgroundService.Enums;
using EPR.Calculator.API.BackgroundService.Misc;
using EPR.Calculator.API.BackgroundService.Models;

namespace EPR.Calculator.API.BackgroundService.Exporter.CsvExporter.RejectedProducers
{
    public class CalcResultRejectedProducersExporter : ICalcResultRejectedProducersExporter
    {
        public void Export(IEnumerable<CalcResultRejectedProducer> rejectedProducers, StringBuilder csvContent)
        {
            // Add empty lines
            csvContent.AppendLine();
            csvContent.AppendLine();

            // Add headers
            WriteHeader(csvContent);

            // Add data
            WriteData(rejectedProducers, csvContent);
        }

        private static void WriteHeader(StringBuilder csvContent)
        {
            csvContent.Append(CsvSanitiser.SanitiseData(CalcResultRejectedProducersHeader.RejectedReport));
            csvContent.AppendLine();
            csvContent.AppendLine();
            csvContent.Append(CsvSanitiser.SanitiseData(CalcResultRejectedProducersHeader.ProducerId));
            csvContent.Append(CsvSanitiser.SanitiseData(CalcResultRejectedProducersHeader.ProducerName));
            csvContent.Append(CsvSanitiser.SanitiseData(CalcResultRejectedProducersHeader.TradingName));
            csvContent.Append(CsvSanitiser.SanitiseData(CalcResultRejectedProducersHeader.SuggestedBillingInstruction));
            csvContent.Append(CsvSanitiser.SanitiseData(CalcResultRejectedProducersHeader.SuggestedInvoiceAmount));
            csvContent.Append(CsvSanitiser.SanitiseData(CalcResultRejectedProducersHeader.InstructionConfirmedDate));
            csvContent.Append(CsvSanitiser.SanitiseData(CalcResultRejectedProducersHeader.InstructionConfirmedBy));
            csvContent.Append(CsvSanitiser.SanitiseData(CalcResultRejectedProducersHeader.ReasonForReject));
            csvContent.AppendLine();
        }

        private static void WriteData(IEnumerable<CalcResultRejectedProducer> rejectedProducers, StringBuilder csvContent)
        {
            foreach (var producer in rejectedProducers)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerId));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.ProducerName));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.TradingName));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.SuggestedBillingInstruction));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.SuggestedInvoiceAmount, DecimalPlaces.Two, null, true));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.InstructionConfirmedDate?.ToString(CalculationResults.DateFormatWithSeconds) ?? string.Empty));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.InstructionConfirmedBy));
                csvContent.Append(CsvSanitiser.SanitiseData(producer.ReasonForRejection));
                csvContent.AppendLine();
            }
        }
    }
}
