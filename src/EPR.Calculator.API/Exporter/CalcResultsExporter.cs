using EPR.Calculator.API.Constants;
using EPR.Calculator.API.Data.Migrations;
using EPR.Calculator.API.Models;
using EPR.Calculator.API.Services;
using EPR.Calculator.API.Utils;
using System.Text;
using System.Threading.Channels;

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
        private const string RPDFileORG = "RPD File - ORG";
        private const string RPDFilePOM = "RPD File - POM";
        private const string LapcapFile = "LAPCAP File";
        private const string ParametersFile = "Parameters File";

        public CalcResultsExporter(IBlobStorageService blobStorageService)
        {
            _blobStorageService = blobStorageService;
        }
        public void Export(CalcResult results)
        {
            var csvContent = new StringBuilder();
            LoadCalcResultDetail(results, csvContent);
            if (results?.CalcResultLapcapData != null)
            {
                PrepareLapcapData(results.CalcResultLapcapData, csvContent);
            }

            if (results.CalcResultLateReportingTonnageData != null)
            {
                PrepareLateReportingData(results.CalcResultLateReportingTonnageData, csvContent);
            }

            csvContent.AppendLine();
            csvContent.AppendLine(results.CalcResultCommsCostReportDetail.ToString());

            if (results?.CalcResultParameterOtherCost != null)
            {
                PrepareOtherCosts(results.CalcResultParameterOtherCost, csvContent);
            }
            

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

        private static void PrepareOtherCosts(CalcResultParameterOtherCost otherCost, StringBuilder csvContent)
        {
            csvContent.AppendLine();
            csvContent.AppendLine();

            csvContent.AppendLine(otherCost.Name);

            var saOperatinCosts = otherCost.SaOperatingCost.OrderBy(x => x.OrderId);

            foreach (var saOperatingCost in saOperatinCosts)
            {
                csvContent.Append($"{CsvSanitiser.SanitiseData(saOperatingCost.Name)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(saOperatingCost.England)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(saOperatingCost.Wales)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(saOperatingCost.Scotland)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(saOperatingCost.NorthernIreland)},");
                csvContent.AppendLine($"{CsvSanitiser.SanitiseData(saOperatingCost.Total)}");
            }
            csvContent.AppendLine();

            var laDataPreps = otherCost.Details.OrderBy(x => x.OrderId);

            foreach (var laDataPrep in laDataPreps)
            {
                csvContent.Append($"{CsvSanitiser.SanitiseData(laDataPrep.Name)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(laDataPrep.England)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(laDataPrep.Wales)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(laDataPrep.Scotland)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(laDataPrep.NorthernIreland)},");
                csvContent.AppendLine($"{CsvSanitiser.SanitiseData(laDataPrep.Total)}");
            }
            csvContent.AppendLine();
            var schemeCost = otherCost.SchemeSetupCost;
            csvContent.Append($"{CsvSanitiser.SanitiseData(schemeCost.Name)},");
            csvContent.Append($"{CsvSanitiser.SanitiseData(schemeCost.England)},");
            csvContent.Append($"{CsvSanitiser.SanitiseData(schemeCost.Wales)},");
            csvContent.Append($"{CsvSanitiser.SanitiseData(schemeCost.Scotland)},");
            csvContent.Append($"{CsvSanitiser.SanitiseData(schemeCost.NorthernIreland)},");
            csvContent.AppendLine($"{CsvSanitiser.SanitiseData(schemeCost.Total)}");

            csvContent.AppendLine();
            csvContent.Append($"{CsvSanitiser.SanitiseData(otherCost.BadDebtProvision.Key)},");
            csvContent.AppendLine($"{CsvSanitiser.SanitiseData(otherCost.BadDebtProvision.Value)}");

            csvContent.AppendLine();
            var materiality = otherCost.Materiality;
            foreach (var material in materiality)
            {
                csvContent.Append($"{CsvSanitiser.SanitiseData(material.SevenMateriality)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(material.Amount)},");
                csvContent.AppendLine($"{CsvSanitiser.SanitiseData(material.Percentage)}");
            }
        }

        private static void LoadCalcResultDetail(CalcResult results, StringBuilder csvContent)
        {
            AppendCsvLine(csvContent, RunName, results.CalcResultDetail.RunName);
            AppendCsvLine(csvContent, RunId, results.CalcResultDetail.RunId.ToString());
            AppendCsvLine(csvContent, RunDate, results.CalcResultDetail.RunDate.ToString(CalculationResults.DateFormat));
            AppendCsvLine(csvContent, Runby, results.CalcResultDetail.RunBy);
            AppendCsvLine(csvContent, FinancialYear, results.CalcResultDetail.FinancialYear);
            AppendRPDFileInfo(csvContent, RPDFileORG, RPDFilePOM, results.CalcResultDetail.RpdFileORG, results.CalcResultDetail.RpdFilePOM);
            AppendFileInfo(csvContent, LapcapFile, results.CalcResultDetail.LapcapFile);
            AppendFileInfo(csvContent, ParametersFile, results.CalcResultDetail.ParametersFile);
        }

        private static void AppendRPDFileInfo(StringBuilder csvContent, string rPDFileORG, string rPDFilePOM, string rpdFileORGValue, string rpdFilePOMValue)
        {
            csvContent.AppendLine($"{rPDFileORG},{CsvSanitiser.SanitiseData(rpdFileORGValue)},{rPDFilePOM},{CsvSanitiser.SanitiseData(rpdFilePOMValue)}");
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
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.EnglandDisposalCost)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.WalesDisposalCost)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.ScotlandDisposalCost)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.NorthernIrelandDisposalCost)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.TotalDisposalCost)}\"");
                csvContent.AppendLine();
            }
        }

        private static void PrepareLateReportingData(CalcResultLateReportingTonnage calcResultLateReportingData, StringBuilder csvContent)
        {
            csvContent.AppendLine();
            csvContent.AppendLine();

            csvContent.AppendLine(calcResultLateReportingData.Name);
            csvContent.Append($"{calcResultLateReportingData.MaterialHeading},");
            csvContent.Append(calcResultLateReportingData.TonnageHeading);
            csvContent.AppendLine();

            foreach (var lateReportingData in calcResultLateReportingData.CalcResultLateReportingTonnageDetails)
            {
                csvContent.Append($"{CsvSanitiser.SanitiseData(lateReportingData.Name)},");
                csvContent.Append(CsvSanitiser.SanitiseData(lateReportingData.TotalLateReportingTonnage));
                csvContent.AppendLine();
            }
        }
    }
}
