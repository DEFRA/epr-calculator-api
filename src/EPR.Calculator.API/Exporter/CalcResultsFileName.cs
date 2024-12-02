namespace EPR.Calculator.API.Exporter
{
    using EPR.Calculator.API.Data;

    /// <summary>
    /// Builds the file name for the calculator results file.
    /// </summary>
    public class CalcResultsFileName
    {
        /// <summary>
        /// The file extension to append to the file name.
        /// </summary>
        public const string FileExtension = "csv";

        /// <summary>
        /// The maximum number of characters to from the run name to include in the file name.
        /// If the run name is longer than this, it will be truncated.
        /// </summary>
        public const int MaxRunNameLength = 30;

        private string Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CalcResultsFileName"/> class.
        /// </summary>
        /// <param name="runId">The calculator run ID.</param>
        /// <param name="runName">The calculator run name.</param>
        /// <param name="timeStamp">The date when the report is generated.</param>
        public CalcResultsFileName(int runId, string runName, DateTime timeStamp)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(runName);
            var truncatedRunName = string.Join(string.Empty, runName.Take(MaxRunNameLength));
            var name = $"{runId}-{truncatedRunName}_Results File_{timeStamp:yyyyMMdd}";
            Value = Path.ChangeExtension(name, FileExtension);
        }

        /// <inheritdoc/>
        public override string ToString() => Value;

        /// <summary>
        /// Implicitly converts the <paramref name="calcResultsFileName"/> object to a string.
        /// </summary>
        /// <param name="calcResultsFileName"></param>
        public static implicit operator string(CalcResultsFileName calcResultsFileName)
            => calcResultsFileName.ToString();

        /// <summary>
        /// Generates a file name for the results file, by retrieving the needed values
        /// from the database.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="runId">The run ID.</param>
        /// <returns></returns>
        public static CalcResultsFileName FromDatabase(ApplicationDBContext context, int runId)
        {
            var runDetails = context.CalculatorRuns
                .Where(run => run.Id == runId)
                .Select(run => new { run.Name, run.CreatedAt}).Single();

            return new CalcResultsFileName(runId, runDetails.Name, runDetails.CreatedAt);
        }
    }
}
