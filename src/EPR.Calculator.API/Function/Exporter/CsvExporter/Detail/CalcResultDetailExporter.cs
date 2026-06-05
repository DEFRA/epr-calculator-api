using System.Text;
using EPR.Calculator.Service.Function.Constants;
using EPR.Calculator.Service.Function.Misc;
using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter.Detail
{
    public interface ICalcResultDetailExporter
    {
        void Export(CalcResultDetail calcResultDetail, StringBuilder stringBuilder);
    }

    public class CalcResultDetailExporter : ICalcResultDetailExporter
    {
        private const string RunName = "Run Name";
        private const string RunId = "Run Id";
        private const string RunDate = "Run Date";
        private const string Runby = "Run by";
        private const string FinancialYear = "Financial Year";
        private const string RPDFileORG = "RPD File - ORG";
        private const string RPDFilePOM = "RPD File - POM";
        private const string LapcapFile = "LAPCAP File";
        private const string ParametersFile = "Parameters File";
        private const string CountryApportionmentFile = "Country Apportionment File";

        public void Export(CalcResultDetail calcResultDetail, StringBuilder stringBuilder)
        {
            AppendCsvLine(stringBuilder, RunName, calcResultDetail.RunName);
            AppendCsvLine(stringBuilder, RunId, calcResultDetail.RunId.ToString());
            AppendCsvLine(stringBuilder, RunDate, calcResultDetail.RunDate.ToString(CalculationResults.DateFormat));
            AppendCsvLine(stringBuilder, Runby, calcResultDetail.RunBy);
            AppendCsvLine(stringBuilder, FinancialYear, calcResultDetail.RelativeYear.ToFinancialYear());
            AppendRpdFileInfo(stringBuilder, RPDFileORG, RPDFilePOM, calcResultDetail.RpdFileORG, calcResultDetail.RpdFilePOM);
            AppendFileInfo(stringBuilder, LapcapFile, calcResultDetail.LapcapFile);
            AppendFileInfo(stringBuilder, ParametersFile, calcResultDetail.ParametersFile);
            AppendFileInfo(stringBuilder, CountryApportionmentFile, calcResultDetail.CountryApportionmentFile);
        }

        private static void AppendRpdFileInfo(StringBuilder csvContent, string rPDFileORG, string rPDFilePOM, string rpdFileORGValue, string rpdFilePOMValue)
        {
            csvContent.AppendLine($"{rPDFileORG},{CsvSanitiser.SanitiseData(rpdFileORGValue)},{rPDFilePOM},{CsvSanitiser.SanitiseData(rpdFilePOMValue)}");
        }

        public static void AppendFileInfo(StringBuilder csvContent, string label, string filePath)
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
    }
}
