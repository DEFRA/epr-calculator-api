using EPR.Calculator.API.Models;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Utils;
using System;
using System.Text;

namespace EPR.Calculator.API.Exporter
{
    public class CalcResultsExporter : ICalcResultsExporter<CalcResult>
    {
        private readonly IBlobStorageService _blobStorageService;
        public CalcResultsExporter(IBlobStorageService blobStorageService)
        {
            _blobStorageService = blobStorageService;
        }
        public void Export(CalcResult results)
        {
            var csvContent = new StringBuilder();
            AppendCsvLine(csvContent, "Run Name", results.CalcResultDetail.RunName);
            AppendCsvLine(csvContent, "Run Id", results.CalcResultDetail.RunId.ToString());
            AppendCsvLine(csvContent, "Run Date", results.CalcResultDetail.RunDate.ToString("dd/MM/yyyy HH:mm"));
            AppendCsvLine(csvContent, "Run by", results.CalcResultDetail.RunBy);
            AppendCsvLine(csvContent, "Financial Year", results.CalcResultDetail.FinancialYear);
            AppendFileInfo(csvContent, "Lapcap File", results.CalcResultDetail.LapcapFile);
            AppendFileInfo(csvContent, "Parameters File", results.CalcResultDetail.ParametersFile);
            var fileName = GetResultFileName(results.CalcResultDetail.RunId);
            try
            {
                _blobStorageService.UploadResultFileContentAsync(fileName, csvContent);
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error writing file: {ex.Message}");
            }
        }

        private static void AppendFileInfo(StringBuilder csvContent, string label, string filePath)
        {
            var fileParts = filePath.Split(',');
            if (fileParts.Length >= 3)
            {
                string fileName = CsvSanitiser.SanitiseData(fileParts[0]);
                string date = CsvSanitiser.SanitiseData(fileParts[1]);
                string user = CsvSanitiser.SanitiseData(fileParts[2]);
                csvContent.AppendLine($"{label},{fileName},{date},{user}");
            }
        }

        private static void AppendCsvLine(StringBuilder csvContent, string label, string value)
        {
            csvContent.AppendLine($"{label},{CsvSanitiser.SanitiseData(value)}");
        }
        private static string GetResultFileName(int runId)
        {
            return $"{runId}-{DateTime.Now:yyyy-MM-dd-HHmm}";
        }
    }
}
