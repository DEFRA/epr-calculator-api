namespace EPR.Calculator.Service.Function.Constants
{
    /// <summary>
    /// The keys used to identify the timeout policies when retrieving them from the config file.
    /// </summary>
    public static class TimeoutPolicies
    {
        public const string RpdStatus = "RpdStatus";

        public const string PrepareCalcResults = "PrepareCalcResults";

        public const string Transpose = "Transpose";

        public static string[] AllPolicies => [RpdStatus, PrepareCalcResults, Transpose];

    }
}
