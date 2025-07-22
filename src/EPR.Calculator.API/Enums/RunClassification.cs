using EnumsNET;
using EPR.Calculator.API.Data.DataModels;
using System.ComponentModel;

namespace EPR.Calculator.API.Enums
{
    public enum RunClassification
    {
        /// <summary>
        /// In the Queue.
        /// </summary>
        [Description(RunClassificationDescStatus.INTHEQUEUE)]
        INTHEQUEUE = RunClassificationStatusIds.INTHEQUEUEID,

        /// <summary>
        /// Running.
        /// </summary>
        [Description(RunClassificationDescStatus.RUNNING)]
        RUNNING = RunClassificationStatusIds.RUNNINGID,

        /// <summary>
        /// Unclassified.
        /// </summary>
        [Description(RunClassificationDescStatus.UNCLASSIFIED)]
        UNCLASSIFIED = RunClassificationStatusIds.UNCLASSIFIEDID,

        /// <summary>
        /// Play.
        /// </summary>
        [Description(RunClassificationDescStatus.TEST_RUN)]
        TEST_RUN = RunClassificationStatusIds.TEST_RUNID,

        /// <summary>
        /// Error.
        /// </summary>
        [Description(RunClassificationDescStatus.ERROR)]
        ERROR = RunClassificationStatusIds.ERRORID,

        /// <summary>
        /// Deleted.
        /// </summary>
        [Description(RunClassificationDescStatus.DELETED)]
        DELETED = RunClassificationStatusIds.DELETEDID,

        /// <summary>
        /// INITIAL RUN COMPLETED.
        /// </summary>
        [Description(RunClassificationDescStatus.INITIAL_RUN_COMPLETED)]
        INITIAL_RUN_COMPLETED = RunClassificationStatusIds.INITIAL_RUN_COMPLETEDID,

        /// <summary>
        /// INITIAL RUN.
        /// </summary>
        [Description(RunClassificationDescStatus.INITIAL_RUN)]
        INITIAL_RUN = RunClassificationStatusIds.INITIAL_RUNID,

        /// <summary>
        /// INTERIM RE-CALCULATION RUN.
        /// </summary>
        [Description(RunClassificationDescStatus.INTERIM_RECALCULATION_RUN)]
        INTERIM_RECALCULATION_RUN = RunClassificationStatusIds.INTERIM_RECALCULATION_RUNID,

        /// <summary>
        /// FINAL RUN.
        /// </summary>
        [Description(RunClassificationDescStatus.FINAL_RUN)]
        FINAL_RUN = RunClassificationStatusIds.FINAL_RUNID,

        /// <summary>
        /// FINAL RE-CALCULATION RUN.
        /// </summary>
        [Description(RunClassificationDescStatus.FINAL_RECALCULATION_RUN)]
        FINAL_RECALCULATION_RUN = RunClassificationStatusIds.FINAL_RECALCULATION_RUNID,

        /// <summary>
        /// INTERIM RE-CALCULATION RUN COMPLETED.
        /// </summary>
        [Description(RunClassificationDescStatus.INTERM_RECALCULATION_RUN_COMP)]
        INTERIM_RECALCULATION_RUN_COMPLETED = RunClassificationStatusIds.INTERM_RECALCULATION_RUN_COMPID,

        /// <summary>
        /// FINAL RE-CALCULATION RUN COMPLETED.
        /// </summary>
        [Description(RunClassificationDescStatus.FINAL_RECALCULATION_RUN_COMP)]
        FINAL_RECALCULATION_RUN_COMPLETED = RunClassificationStatusIds.FINAL_RECALCULATION_RUN_COMPID,

        /// <summary>
        /// FINAL RUN COMPLETED.
        /// </summary>
        [Description(RunClassificationDescStatus.FINAL_RUN_COMPLETED)]
        FINAL_RUN_COMPLETED = RunClassificationStatusIds.FINAL_RUN_COMPLETEDID,
    }
}