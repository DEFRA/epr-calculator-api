using System.Text;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.ProjectedProducers
{
    public interface ICalcResultProjectedProducersExporter
    {
        public void Export(CalcResultProjectedProducers calcResultProjectedProducers, IImmutableList<MaterialDetail> materials, StringBuilder stringBuilder);
    }

    public class CalcResultProjectedProducersExporter : ICalcResultProjectedProducersExporter
    {
        public void Export(CalcResultProjectedProducers calcResultProjectedProducers, IImmutableList<MaterialDetail> materials, StringBuilder stringBuilder)
        {
            var allH2 = calcResultProjectedProducers.H2ProjectedProducers ?? ImmutableList<CalcResultH2ProjectedProducer>.Empty;
            var allH1 = calcResultProjectedProducers.H1ProjectedProducers ?? ImmutableList<CalcResultH1ProjectedProducer>.Empty;
            var completeH1AndH2RamProducers = allH2
                .Cast<ICalcResultProjectedProducer>()
                .Concat(allH1)
                .GroupBy(p => p.ProducerId)
                .Where(g => g.All(p => p.ProjectedTonnageByMaterial.All(m => !m.Value.IsWithoutRamTonnage())))
                .Select(g => g.Key)
                .ToHashSet();

            var h2WhereModified = allH2.Where(p => !completeH1AndH2RamProducers.Contains(p.ProducerId)).ToImmutableList();
            var h1WhereModified = allH1.Where(p => !completeH1AndH2RamProducers.Contains(p.ProducerId)).ToImmutableList();

            // Add empty lines
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();

            // Add H2 headers
            PrepareProjectedProducersHeaders(H2ProjectedProducersExporterUtils.GetProjectedProducerHeaders(materials), stringBuilder);

            // Add H2 data
            if (h2WhereModified.Any())
            {
                H2ProjectedProducersExporterUtils.AppendProjectedProducers(h2WhereModified, stringBuilder);
            }
            else
            {
                stringBuilder.AppendLine(CsvSanitiser.SanitiseData(CalcResultProjectedProducersHeaders.NoProjectedProducers));
            }

            // Add empty lines
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();

            // Add H1 headers
            PrepareProjectedProducersHeaders(H1ProjectedProducersExporterUtils.GetProjectedProducerHeaders(materials), stringBuilder);

            // Add H1 data
            if (h1WhereModified.Any())
            {
                H1ProjectedProducersExporterUtils.AppendProjectedProducers(h1WhereModified, stringBuilder);
            }
            else
            {
                stringBuilder.AppendLine(CsvSanitiser.SanitiseData(CalcResultProjectedProducersHeaders.NoProjectedProducers));
            }
        }

        private void PrepareProjectedProducersHeaders(ProjectedProducersHeaders headers, StringBuilder csvContent)
        {
            // Add projected producers headers
            csvContent.AppendLine(CsvSanitiser.SanitiseData(headers.TitleHeader.Name));
            csvContent.AppendLine();

            // Add material breakdown headers
            WriteProjectedProducersSecondaryHeaders(headers.MaterialBreakdownHeaders.ToList(), csvContent);

            // Add column headers
            WriteProjectedProducersColumnHeaders(headers.ColumnHeaders, csvContent);
            csvContent.AppendLine();
        }

        private static void WriteProjectedProducersSecondaryHeaders(IReadOnlyCollection<ProjectedProducersHeader> headers, StringBuilder csvContent)
        {
            var maxColumnSize = headers.MaxBy(h => h.ColumnIndex ?? 0)?.ColumnIndex ?? throw new ArgumentException("No headers specified");

            var headerRows = new string[maxColumnSize];
            foreach (var item in headers.Where(h => h.ColumnIndex.HasValue))
            {
                headerRows[item.ColumnIndex!.Value - 1] = CsvSanitiser.SanitiseData(item.Name, false);
            }

            var headerRow = string.Join(CommonConstants.CsvFileDelimiter, headerRows);
            csvContent.AppendLine(headerRow);
        }

        private static void WriteProjectedProducersColumnHeaders(IEnumerable<ProjectedProducersHeader> columnHeaders, StringBuilder csvContent)
        {
            foreach (var item in columnHeaders)
            {
                csvContent.Append(CsvSanitiser.SanitiseData(item.Name));
            }
        }
    }
}
