namespace EPR.Calculator.API.Data.DataModels
{
    public static class RunClassificationDescStatus
    {
        public const string INTHEQUEUE = "IN THE QUEUE";
        public const string RUNNING = "RUNNING";
        public const string UNCLASSIFIED = "UNCLASSIFIED";

        public const string TESTRUN = "TEST RUN";
        public const string ERROR = "ERROR";
        public const string DELETED = "DELETED";

        public const string INITIALRUNCOMPLETED = "INITIAL RUN COMPLETED";
        public const string INITIALRUN = "INITIAL RUN";
        public const string INTERIMRECALCULATIONRUN = "INTERIM RE-CALCULATION RUN";

        public const string FINALRUN = "FINAL RUN";
        public const string FINALRECALCULATIONRUN = "FINAL RE-CALCULATION RUN";

        public const string INTERMRECALCULATIONRUNCOMP = "INTERIM RE-CALCULATION RUN COMPLETED";
        public const string FINALRECALCULATIONRUNCOMP = "FINAL RE-CALCULATION RUN COMPLETED";
        public const string FINALRUNCOMPLETED = "FINAL RUN COMPLETED";
    }
}