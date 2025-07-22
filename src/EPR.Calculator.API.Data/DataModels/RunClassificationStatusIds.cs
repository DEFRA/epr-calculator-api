namespace EPR.Calculator.API.Data.DataModels
{
    public static class RunClassificationStatusIds
    {
        public const int INTHEQUEUEID = 1;
        public const int RUNNINGID = 2;
        public const int UNCLASSIFIEDID = 3;

        public const int TEST_RUNID = 4;
        public const int ERRORID = 5;
        public const int DELETEDID = 6;

        public const int INITIAL_RUN_COMPLETEDID = 7;
        public const int INITIAL_RUNID = 8;
        public const int INTERIM_RECALCULATION_RUNID = 9;

        public const int FINAL_RUNID = 10;
        public const int FINAL_RECALCULATION_RUNID = 11;

        public const int INTERM_RECALCULATION_RUN_COMPID = 12;
        public const int FINAL_RECALCULATION_RUN_COMPID = 13;
        public const int FINAL_RUN_COMPLETEDID = 14;
    }
}