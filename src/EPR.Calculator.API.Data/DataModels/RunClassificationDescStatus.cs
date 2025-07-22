namespace EPR.Calculator.API.Data.DataModels
{
    public static class RunClassificationDescStatus
    {
        public const string INTHEQUEUE = "IN THE QUEUE";
        public const string RUNNING = "RUNNING";
        public const string UNCLASSIFIED = "UNCLASSIFIED";

        public const string TEST_RUN = "TEST RUN";
        public const string ERROR = "ERROR";
        public const string DELETED = "DELETED";

        public const string INITIAL_RUN_COMPLETED = "INITIAL RUN COMPLETED";
        public const string INITIAL_RUN = "INITIAL RUN";
        public const string INTERIM_RECALCULATION_RUN = "INTERIM RE-CALCULATION RUN";

        public const string FINAL_RUN = "FINAL RUN";
        public const string FINAL_RECALCULATION_RUN = "FINAL RE-CALCULATION RUN";

        public const string INTERM_RECALCULATION_RUN_COMP = "INTERIM RE-CALCULATION RUN COMPLETED";
        public const string FINAL_RECALCULATION_RUN_COMP = "FINAL RE-CALCULATION RUN COMPLETED";
        public const string FINAL_RUN_COMPLETED = "FINAL RUN COMPLETED";
    }
}