using System.ComponentModel;

namespace EPR.Calculator.API.Enums
{
    [Flags]
    internal enum RunClassificationStatus
    {
        /// <summary>
        /// In the Queue.
        /// </summary>
        INTHEQUEUE = RunClassification.INTHEQUEUE,

        /// <summary>
        /// Running.
        /// </summary>
        RUNNING = RunClassification.RUNNING,

        /// <summary>
        /// Unclassified.
        /// </summary>
        UNCLASSIFIED = RunClassification.UNCLASSIFIED,

        /// <summary>
        /// Test.
        /// </summary>
        TEST_RUN = RunClassification.TEST_RUN,

        /// <summary>
        /// Error.
        /// </summary>
        ERROR = RunClassification.ERROR,

        /// <summary>
        /// Deleted.
        /// </summary>
        DELETED = RunClassification.DELETED,

        /// <summary>
        /// INITIAL RUN.
        /// </summary>
        INITIAL_RUN = RunClassification.INITIAL_RUN | Designated,

        /// <summary>
        /// INITIAL RUN COMPLETED.
        /// </summary>
        INITIAL_RUN_COMPLETED = RunClassification.INITIAL_RUN_COMPLETED | Designated | Complete,

        /// <summary>
        /// INTERIM RE-CALCULATION RUN.
        /// </summary>
        INTERIM_RECALCULATION_RUN = RunClassification.INTERIM_RECALCULATION_RUN | Designated,

        /// <summary>
        /// INTERIM RE-CALCULATION RUN COMPLETED.
        /// </summary>
        INTERIM_RECALCULATION_RUN_COMPLETED = RunClassification.INTERIM_RECALCULATION_RUN_COMPLETED | Designated | Complete,

        /// <summary>
        /// FINAL RE-CALCULATION RUN.
        /// </summary>
        FINAL_RECALCULATION_RUN = RunClassification.FINAL_RECALCULATION_RUN | Designated,

        /// <summary>
        /// FINAL RE-CALCULATION RUN COMPLETED.
        /// </summary>
        FINAL_RECALCULATION_RUN_COMPLETED = RunClassification.FINAL_RECALCULATION_RUN_COMPLETED | Designated | Complete,

        /// <summary>
        /// FINAL RUN.
        /// </summary>
        FINAL_RUN = RunClassification.FINAL_RUN | Designated,

        /// <summary>
        /// FINAL RUN COMPLETED.
        /// </summary>
        FINAL_RUN_COMPLETED = RunClassification.FINAL_RUN_COMPLETED | Designated | Complete,

        /// <summary>
        /// The bit flag used to isolate the RunClassification value.
        /// </summary>
        Raw = 0b00111111,

        /// <summary>
        /// The bit flag used to isolated Completed statuses.
        /// </summary>
        Complete = 0b01000000,

        /// <summary>
        /// The bit flag used to isolated Designated statuses.
        /// </summary>
        Designated = 0b10000000,
    }
}
