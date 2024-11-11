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
        private const string RPDFileORG = "RPD File - ORG";
        private const string RPDFilePOM = "RPD File - POM";
        private const string LapcapFile = "LAPCAP File";
        private const string ParametersFile = "Parameters File";
        private const string LaDisposalCostFile = "LA Disposal cost File";
        private const string CountryApportionmentFile = "Country Apportionment File";


        public CalcResultsExporter(IBlobStorageService blobStorageService)
        {
            _blobStorageService = blobStorageService;
        }
        public void Export(CalcResult results)
        {
            var csvContent = new StringBuilder();
            LoadCalcResultDetail(results, csvContent);
            if (results.CalcResultLapcapData != null)
            {
                PrepareLapcapData(results.CalcResultLapcapData, csvContent);
            }

            if (results.CalcResultLateReportingTonnageData != null)
            {
                PrepareLateReportingData(results.CalcResultLateReportingTonnageData, csvContent);
            }

            //Rekha Chnages
            if (results.CalcResultOnePlusFourApportionment != null)
            {
                PrepareOnePluseFourApportionment(results.CalcResultOnePlusFourApportionment, csvContent);
            }

            if (results.CalcResultLaDisposalCostData != null)
            {
                PrepareLaDisposalCostData(results.CalcResultLaDisposalCostData, csvContent);
            }

            if (results.CalcResultSummary != null)
            {
                PrepareSummaryData(results.CalcResultSummary, csvContent);
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
            AppendFileInfo(csvContent, CountryApportionmentFile, results.CalcResultDetail.CountryApportionmentFile);
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

        private static void PrepareLaDisposalCostData(CalcResultLaDisposalCostData calcResultLaDisposalCostData, StringBuilder csvContent)
        {
            csvContent.AppendLine();
            csvContent.AppendLine();

            csvContent.AppendLine(calcResultLaDisposalCostData.Name);
            var lapcapDataDetails = calcResultLaDisposalCostData.CalcResultLaDisposalCostDetails.OrderBy(x => x.OrderId);

            foreach (var lapcapData in lapcapDataDetails)
            {
                csvContent.Append($"{CsvSanitiser.SanitiseData(lapcapData.Name)},");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.England)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.Wales)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.Scotland)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.NorthernIreland)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.Total)}\"");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.ProducerReportedHouseholdPackagingWasteTonnage)}\"");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.LateReportingTonnage)}\"");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.ProducerReportedHouseholdTonnagePlusLateReportingTonnage)}\"");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.DisposalCostPricePerTonne)}\"");
                csvContent.AppendLine();
            }
        }

        private static void PrepareSummaryData(CalcResultSummary resultSummary, StringBuilder csvContent)
        {
            // Add empty lines
            csvContent.AppendLine();
            csvContent.AppendLine();

            // Add headers
            CalcResultsExporter.PrepareSummaryDataHeader(resultSummary, csvContent);

            // Add data
            foreach (var producer in resultSummary.ProducerDisposalFees)
            {
                csvContent.Append($"{producer.ProducerId},");
                csvContent.Append($"{producer.SubsidiaryId},");
                csvContent.Append($"{producer.ProducerName},");
                csvContent.Append($"{producer.Level},");

                foreach (var materialDisposalFees in producer.ProducerDisposalFeesByMaterial)
                {
                    foreach (var disposalFee in materialDisposalFees.Value)
                    {
                        csvContent.Append($"{disposalFee.HouseholdPackagingWasteTonnage},");
                        csvContent.Append($"{disposalFee.ManagedConsumerWasteTonnage},");
                        csvContent.Append($"{disposalFee.NetReportedTonnage},");
                        csvContent.Append($"{disposalFee.PricePerTonne},");
                        csvContent.Append($"{disposalFee.ProducerDisposalFee},");
                        csvContent.Append($"{disposalFee.BadDebtProvision},");
                        csvContent.Append($"{disposalFee.ProducerDisposalFeeWithBadDebtProvision},");
                        csvContent.Append($"{disposalFee.EnglandWithBadDebtProvision},");
                        csvContent.Append($"{disposalFee.WalesWithBadDebtProvision},");
                        csvContent.Append($"{disposalFee.ScotlandWithBadDebtProvision},");
                        csvContent.Append($"{disposalFee.NorthernIrelandWithBadDebtProvision},");
                    }
                }

                foreach (var materialDisposalFees in producer.TwoACommsCostByMaterial)
                {
                    foreach (var disposalFee in materialDisposalFees.Value)
                    {
                        csvContent.Append($"{disposalFee.HouseholdPackagingWasteTonnage},");
                        csvContent.Append($"{disposalFee.PriceperTonne},");
                        csvContent.Append($"{disposalFee.ProducerTotalCostWithoutBadDebtProvision},");
                        csvContent.Append($"{disposalFee.BadDebtProvision},");
                        csvContent.Append($"{disposalFee.ProducerTotalCostwithBadDebtProvision},");
                        csvContent.Append($"{disposalFee.EnglandWithBadDebtProvision},");
                        csvContent.Append($"{disposalFee.WalesWithBadDebtProvision},");
                        csvContent.Append($"{disposalFee.ScotlandWithBadDebtProvision},");
                        csvContent.Append($"{disposalFee.NorthernIrelandWithBadDebtProvision},");
                    }
                }
                csvContent.AppendLine();
            }
        }

        private static void PrepareSummaryDataHeader(CalcResultSummary resultSummary, StringBuilder csvContent)
        {
            // Add result summary header
            csvContent.AppendLine(resultSummary.ResultSummaryHeader.Name);

            // Add producer disposal fees header
            for (var i = 0; i < resultSummary.ProducerDisposalFeesHeader.ColumnIndex; i++)
            {
                csvContent.Append(",");
            }

            StringBuilder lineBuilder = new StringBuilder();
            lineBuilder.Append(resultSummary.ProducerDisposalFeesHeader.Name);

            for (int i = 0; i < 95; i++)
            {
                lineBuilder.Append(",");
            }

            lineBuilder.Append(resultSummary.TwoACommsCostHeader.Name);

            csvContent.AppendLine(lineBuilder.ToString());
            var indexCounter = 0;
            foreach (var item in resultSummary.MaterialBreakdownHeaders)
            {
                for (var i = indexCounter; i < item.ColumnIndex; i++)
                {
                    csvContent.Append(",");
                }
                csvContent.Append(item.Name);
                indexCounter = item.ColumnIndex;
            }
            csvContent.AppendLine();

            // Add column header
            foreach (var item in resultSummary.ColumnHeaders)
            {
                csvContent.Append($"{item},");
            }
            csvContent.AppendLine();
        }
        private static void PrepareOnePluseFourApportionment(CalcResultOnePlusFourApportionment calcResult1Plus4Apportionment, StringBuilder csvContent)
        {
            csvContent.AppendLine();
            csvContent.AppendLine();

            csvContent.AppendLine(calcResult1Plus4Apportionment.Name);
            var lapcapDataDetails = calcResult1Plus4Apportionment.CalcResultOnePlusFourApportionmentDetails.OrderBy(x => x.OrderId);

            foreach (var lapcapData in lapcapDataDetails)
            {
                csvContent.Append($"{CsvSanitiser.SanitiseData(lapcapData.Name)},");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.Total)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.EnglandDisposalTotal)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.WalesDisposalTotal)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.ScotlandDisposalTotal)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.NorthernIrelandDisposalTotal)}\",");
                csvContent.AppendLine();
            }
        }
    }
}
