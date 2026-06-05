using EPR.Calculator.API.Data;

namespace EPR.Calculator.Service.Function.Exporter.CsvExporter
{
    /// <summary>
    /// Builds the file name for the calculator results file.
    /// </summary>
    public class CalcResultsAndBillingFileName
    {
        /// <summary>
        /// The file extension to append to the file name.
        /// </summary>
        public const string CsvFileExtension = "csv";

        /// <summary>
        /// The file extension to append to the file name.
        /// </summary>
        public const string JsonFileExtension = "json";

        /// <summary>
        /// The maximum number of characters to from the run name to include in the file name.
        /// If the run name is longer than this, it will be truncated.
        /// </summary>
        public const int MaxRunNameLength = 30;

        private string Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CalcResultsAndBillingFileName"/> class.
        /// Only use it for CSV results files.
        /// </summary>
        /// <param name="runId">The calculator run ID.</param>
        /// <param name="runName">The calculator run name.</param>
        /// <param name="timeStamp">The date when the report is generated.</param>
        /// <param name="isDraftBillingFile">The boolean value for isDraftBillingFile</param>
        public CalcResultsAndBillingFileName(int runId,
            string runName,
            DateTime timeStamp,
            bool isDraftBillingFile = false)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(runName);
            var truncatedRunName = string.Join(string.Empty, runName.Take(MaxRunNameLength));
            var filePart = isDraftBillingFile ? "Billing" : "Results";
            var timeOrDatePart = isDraftBillingFile ? $"{timeStamp:yyyyMMddHHmm}" : $"{timeStamp:yyyyMMdd}";
            var name = $"{runId}-{truncatedRunName}_{filePart} File_{timeOrDatePart}";
            Value = Path.ChangeExtension(name, CsvFileExtension);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CalcResultsAndBillingFileName"/> class.
        /// Only use it for JSON billing files.
        /// </summary>
        public CalcResultsAndBillingFileName(int runId, bool isDraftBillingFile, bool isJson)
        {
            if (!isJson || !isDraftBillingFile)
            {
                throw new ArgumentException("This constructor is only for JSON billing files.");
            }
            var name = $"{runId}billing";
            Value = Path.ChangeExtension(name, JsonFileExtension);
        }

        /// <inheritdoc/>
        public override string ToString() => Value;

        /// <summary>
        /// Implicitly converts the <paramref name="calcResultsFileName"/> object to a string.
        /// </summary>
        /// <param name="calcResultsFileName"></param>
        public static implicit operator string(CalcResultsAndBillingFileName calcResultsFileName)
            => calcResultsFileName.ToString();

        /// <summary>
        /// Generates a file name for the results file, by retrieving the needed values
        /// from the database.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="runId">The run ID.</param>
        /// <returns></returns>
        public static CalcResultsAndBillingFileName FromDatabase(ApplicationDBContext context, int runId)
        {
            var runDetails = context.CalculatorRuns
                .Where(run => run.Id == runId)
                .Select(run => new { run.Name, run.CreatedAt }).Single();

            return new CalcResultsAndBillingFileName(runId, runDetails.Name ?? string.Empty, runDetails.CreatedAt);
        }
    }
}