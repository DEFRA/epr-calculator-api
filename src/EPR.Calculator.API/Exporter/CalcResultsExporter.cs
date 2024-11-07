using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Models;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Utils;
using System.Text;

namespace EPR.Calculator.API.Exporter
{
    public class CalcResultsExporter : ICalcResultsExporter<CalcResult>
    {
        private readonly IBlobStorageService _blobStorageService;
        private const string RunName = "Run Name";
        private const string RunId = "Run Id";
        private const string RunDate = "Run Date";
        private const string Runby = "Run by";
        private const string FinancialYear = "Financial Year";
        private const string LapcapFile = "Lapcap File";
        private const string ParametersFile = "Parameters File";

        public CalcResultsExporter(IBlobStorageService blobStorageService)
        {
            _blobStorageService = blobStorageService;
        }
        public void Export(CalcResult results)
        {
            var csvContent = new StringBuilder();
            LoadCalcResultDetail(results, csvContent);
            PrepareLapcapData(results.CalcResultLapcapData, csvContent);

            var fileName = GetResultFileName(results.CalcResultDetail.RunId);
            try
            {
                _blobStorageService.UploadResultFileContentAsync(fileName, csvContent);
            }
            catch (IOException ex)
            {
                throw new IOException($"File upload failed: {ex.Message}", ex);
            }
        }

        private static void LoadCalcResultDetail(CalcResult results, StringBuilder csvContent)
        {
            AppendCsvLine(csvContent, RunName, results.CalcResultDetail.RunName);
            AppendCsvLine(csvContent, RunId, results.CalcResultDetail.RunId.ToString());
            AppendCsvLine(csvContent, RunDate, results.CalcResultDetail.RunDate.ToString(CalculationResults.DateFormat));
            AppendCsvLine(csvContent, Runby, results.CalcResultDetail.RunBy);
            AppendCsvLine(csvContent, FinancialYear, results.CalcResultDetail.FinancialYear);
            AppendFileInfo(csvContent, LapcapFile, results.CalcResultDetail.LapcapFile);
            AppendFileInfo(csvContent, ParametersFile, results.CalcResultDetail.ParametersFile);
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
            return $"{runId}-{DateTime.Now:yyyy-MM-dd-HHmm}.csv";
        }

        private static void PrepareLapcapData(CalcResultLapcapData calcResultLapcapData, StringBuilder csvContent)
        {
            csvContent.AppendLine();
            csvContent.AppendLine();

            csvContent.AppendLine(calcResultLapcapData.Name);
            var lapcapDataDetails = calcResultLapcapData.CalcResultLapcapDataDetails.OrderBy(x => x.OrderId);

            foreach (var lapcapData in lapcapDataDetails)
            {
                csvContent.Append($"{CsvSanitiser.SanitiseData(lapcapData.Name)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(lapcapData.EnglandDisposalCost)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(lapcapData.WalesDisposalCost)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(lapcapData.ScotlandDisposalCost)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(lapcapData.NorthernIrelandDisposalCost)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(lapcapData.TotalDisposalCost)}");
                csvContent.AppendLine();
            }
        }
    }
}
