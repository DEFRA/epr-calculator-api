using System.ComponentModel;

namespace EPR.Calculator.API.Enums
{
    public enum RunClassification
    {
        /// <summary>
        /// In the Queue.
        /// </summary>
        [Description("IN THE QUEUE")]
        INTHEQUEUE = 1,

        /// <summary>
        /// Running.
        /// </summary>
        [Description("RUNNING")]
        RUNNING = 2,

        /// <summary>
        /// Unclassified.
        /// </summary>
        [Description("UNCLASSIFIED")]
        UNCLASSIFIED = 3,

        /// <summary>
        /// Play.
        /// </summary>
        [Description("TEST RUN")]
        TEST_RUN = 4,

        /// <summary>
        /// Error.
        /// </summary>
        [Description("ERROR")]
        ERROR = 5,

        /// <summary>
        /// Deleted.
        /// </summary>
        [Description("DELETED")]
        DELETED = 6,

        /// <summary>
        /// Deleted.
        /// </summary>
        [Description("INITIAL RUN COMPLETED")]
        INITIAL_RUN_COMPLETED = 7,

        /// <summary>
        /// Deleted.
        /// </summary>
        [Description("INITIAL RUN")]
        INITIAL_RUN = 8,

        /// <summary>
        /// Deleted.
        /// </summary>
        [Description("INTERIM RE-CALCULATION RUN")]
        INTERIM_RECALCULATION_RUN = 9,

        /// <summary>
        /// Deleted.
        /// </summary>
        [Description("FINAL RUN")]
        FINAL_RUN = 10,

        /// <summary>
        /// Deleted.
        /// </summary>
        [Description("FINAL RE-CALCULATION RUN")]
        FINAL_RECALCULATION_RUN = 11,
    }
}
