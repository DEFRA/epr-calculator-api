using EPR.Calculator.API.Builder.CommsCost;
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
            if (results?.CalcResultLapcapData != null)
            {
                PrepareLapcapData(results.CalcResultLapcapData, csvContent);
            }

            if (results.CalcResultLateReportingTonnageData != null)
            {
                PrepareLateReportingData(results.CalcResultLateReportingTonnageData, csvContent);
            }            

            csvContent.AppendLine();
            // csvContent.AppendLine(results.CalcResultCommsCostReportDetail.ToString());

            if (results?.CalcResultParameterOtherCost != null)
            {
                PrepareOtherCosts(results.CalcResultParameterOtherCost, csvContent);
            }

            if (results?.CalcResultOnePlusFourApportionment != null)
            {
                PrepareOnePluseFourApportionment(results.CalcResultOnePlusFourApportionment, csvContent);
            }

            csvContent.AppendLine();

            if (results?.CalcResultCommsCostReportDetail != null)
            {
                PrepareCommsCost(results.CalcResultCommsCostReportDetail, csvContent);
            }

            if (results?.CalcResultLaDisposalCostData != null)
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

        private void PrepareCommsCost(CalcResultCommsCost communicationCost, StringBuilder csvContent)
        {
            csvContent.AppendLine();
            csvContent.AppendLine();
            csvContent.AppendLine(communicationCost.Name);

            var onePlusFourApportionments = communicationCost.CalcResultCommsCostOnePlusFourApportionment;

            foreach (var onePlusFourApportionment in onePlusFourApportionments)
            {
                csvContent.Append($"{CsvSanitiser.SanitiseData(onePlusFourApportionment.Name)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(onePlusFourApportionment.England)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(onePlusFourApportionment.Wales)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(onePlusFourApportionment.Scotland)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(onePlusFourApportionment.NorthernIreland)},");
                csvContent.AppendLine($"{CsvSanitiser.SanitiseData(onePlusFourApportionment.Total)}");
            }
            csvContent.AppendLine();
            var commsCostByMaterials = communicationCost.CalcResultCommsCostCommsCostByMaterial;

            foreach (var commsCostByMaterial in commsCostByMaterials)
            {
                csvContent.Append($"{CsvSanitiser.SanitiseData(commsCostByMaterial.Name)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(commsCostByMaterial.England)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(commsCostByMaterial.Wales)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(commsCostByMaterial.Scotland)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(commsCostByMaterial.NorthernIreland)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(commsCostByMaterial.Total)},");
                csvContent.Append(
                    $"{CsvSanitiser.SanitiseData(commsCostByMaterial.ProducerReportedHouseholdPackagingWasteTonnage)},");
                csvContent.Append(
                    $"{CsvSanitiser.SanitiseData(commsCostByMaterial.LateReportingTonnage)},");
                csvContent.Append(
                    $"{CsvSanitiser.SanitiseData(commsCostByMaterial.ProducerReportedHouseholdPlusLateReportingTonnage)},");
                csvContent.AppendLine(
                    $"{CsvSanitiser.SanitiseData(commsCostByMaterial.CommsCostByMaterialPricePerTonne)}");
            }

            csvContent.AppendLine();
            var countryList = communicationCost.CommsCostByCountry;
            foreach (var country in countryList)
            {
                csvContent.Append($"{CsvSanitiser.SanitiseData(country.Name)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(country.England)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(country.Wales)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(country.Scotland)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(country.NorthernIreland)},");
                csvContent.AppendLine($"{CsvSanitiser.SanitiseData(country.Total)}");
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
        private static void PrepareOnePluseFourApportionment(CalcResultOnePlusFourApportionment calcResult1Plus4Apportionment, StringBuilder csvContent)
        {
            csvContent.AppendLine();
            csvContent.AppendLine();

            csvContent.AppendLine(calcResult1Plus4Apportionment.Name);
            var lapcapDataDetails = calcResult1Plus4Apportionment.CalcResultOnePlusFourApportionmentDetails.OrderBy(x => x.OrderId);

            foreach (var lapcapData in lapcapDataDetails)
            {
                csvContent.Append($"{CsvSanitiser.SanitiseData(lapcapData.Name)},");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.EnglandDisposalTotal)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.WalesDisposalTotal)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.ScotlandDisposalTotal)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.NorthernIrelandDisposalTotal)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.Total)}\",");
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
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.Total)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.ProducerReportedHouseholdPackagingWasteTonnage)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.LateReportingTonnage)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.ProducerReportedHouseholdTonnagePlusLateReportingTonnage)}\",");
                csvContent.Append($"\"{CsvSanitiser.SanitiseData(lapcapData.DisposalCostPricePerTonne)}\",");
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
                csvContent.Append($"{CsvSanitiser.SanitiseData(producer.ProducerId)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(producer.SubsidiaryId)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(producer.ProducerName)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(producer.Level)},");

                foreach (var disposalFee in producer.ProducerDisposalFeesByMaterial)
                {
                    csvContent.Append($"{CsvSanitiser.SanitiseData(disposalFee.Value.HouseholdPackagingWasteTonnage)},");
                    csvContent.Append($"{CsvSanitiser.SanitiseData(disposalFee.Value.ManagedConsumerWasteTonnage)},");
                    csvContent.Append($"{CsvSanitiser.SanitiseData(disposalFee.Value.NetReportedTonnage)},");
                    csvContent.Append($"{CsvSanitiser.SanitiseData(disposalFee.Value.PricePerTonne)},");
                    csvContent.Append($"{CsvSanitiser.SanitiseData(disposalFee.Value.ProducerDisposalFee)},");
                    csvContent.Append($"{CsvSanitiser.SanitiseData(disposalFee.Value.BadDebtProvision)},");
                    csvContent.Append($"{CsvSanitiser.SanitiseData(disposalFee.Value.ProducerDisposalFeeWithBadDebtProvision)},");
                    csvContent.Append($"{CsvSanitiser.SanitiseData(disposalFee.Value.EnglandWithBadDebtProvision)},");
                    csvContent.Append($"{CsvSanitiser.SanitiseData(disposalFee.Value.WalesWithBadDebtProvision)},");
                    csvContent.Append($"{CsvSanitiser.SanitiseData(disposalFee.Value.ScotlandWithBadDebtProvision)},");
                    csvContent.Append($"{CsvSanitiser.SanitiseData(disposalFee.Value.NorthernIrelandWithBadDebtProvision)},");
                }

                csvContent.Append($"{CsvSanitiser.SanitiseData(producer.TotalProducerDisposalFee)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(producer.BadDebtProvision)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(producer.TotalProducerDisposalFeeWithBadDebtProvision)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(producer.EnglandTotal)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(producer.WalesTotal)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(producer.ScotlandTotal)},");
                csvContent.Append($"{CsvSanitiser.SanitiseData(producer.NorthernIrelandTotal)},");

                csvContent.AppendLine();
            }
        }

        private static void PrepareSummaryDataHeader(CalcResultSummary resultSummary, StringBuilder csvContent)
        {
            // Add result summary header
            csvContent.AppendLine(CsvSanitiser.SanitiseData(resultSummary.ResultSummaryHeader.Name));

            // Add producer disposal fees header
            for (var i = 0; i < resultSummary.ProducerDisposalFeesHeader.ColumnIndex; i++)
            {
                csvContent.Append(",");
            }
            csvContent.AppendLine(CsvSanitiser.SanitiseData(resultSummary.ProducerDisposalFeesHeader.Name));

            // Add material breakdown header
            var indexCounter = 0;
            foreach (var item in resultSummary.MaterialBreakdownHeaders)
            {
                for (var i = indexCounter; i < item.ColumnIndex; i++)
                {
                    csvContent.Append(",");
                }
                csvContent.Append(CsvSanitiser.SanitiseData(item.Name));
                indexCounter = item.ColumnIndex;
            }
            csvContent.AppendLine();

            // Add column header
            foreach (var item in resultSummary.ColumnHeaders)
            {
                csvContent.Append($"{CsvSanitiser.SanitiseData(item)},");
            }
            csvContent.AppendLine();
        }
    }
}
