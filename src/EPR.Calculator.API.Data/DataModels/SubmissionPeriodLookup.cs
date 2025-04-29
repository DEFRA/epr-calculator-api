namespace EPR.Calculator.API.Data.DataModels
{
    public class SubmissionPeriodLookup
    {
        public required string SubmissionPeriod { get; set; }

        public required string SubmissionPeriodDesc { get; set; }

        public required DateTime StartDate { get; set; }

        public required DateTime EndDate { get; set; }

        public required int DaysInSubmissionPeriod { get; set; }

        public required int DaysInWholePeriod { get; set; }

        public required decimal ScaleupFactor { get; set; }
    }
}
