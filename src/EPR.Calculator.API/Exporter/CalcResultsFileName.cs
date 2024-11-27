namespace EPR.Calculator.API.Exporter
{
    /// <summary>
    /// Builds the file name for the calculator results file.
    /// </summary>
    public class CalcResultsFileName
    {
        public const string FileExtension = "csv";

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
    }
}
