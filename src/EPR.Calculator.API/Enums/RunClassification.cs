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
        [Description(RunClassificationDescStatus.TESTRUN)]
        TEST_RUN = RunClassificationStatusIds.TESTRUNID,

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
        [Description(RunClassificationDescStatus.INITIALRUNCOMPLETED)]
        INITIAL_RUN_COMPLETED = RunClassificationStatusIds.INITIALRUNCOMPLETEDID,

        /// <summary>
        /// INITIAL RUN.
        /// </summary>
        [Description(RunClassificationDescStatus.INITIALRUN)]
        INITIAL_RUN = RunClassificationStatusIds.INITIALRUNID,

        /// <summary>
        /// INTERIM RE-CALCULATION RUN.
        /// </summary>
        [Description(RunClassificationDescStatus.INTERIMRECALCULATIONRUN)]
        INTERIM_RECALCULATION_RUN = RunClassificationStatusIds.INTERIMRECALCULATIONRUNID,

        /// <summary>
        /// FINAL RUN.
        /// </summary>
        [Description(RunClassificationDescStatus.FINALRUN)]
        FINAL_RUN = RunClassificationStatusIds.FINALRUNID,

        /// <summary>
        /// FINAL RE-CALCULATION RUN.
        /// </summary>
        [Description(RunClassificationDescStatus.FINALRECALCULATIONRUN)]
        FINAL_RECALCULATION_RUN = RunClassificationStatusIds.FINALRECALCULATIONRUNID,

        /// <summary>
        /// INTERIM RE-CALCULATION RUN COMPLETED.
        /// </summary>
        [Description(RunClassificationDescStatus.INTERMRECALCULATIONRUNCOMP)]
        INTERIM_RECALCULATION_RUN_COMPLETED = RunClassificationStatusIds.INTERMRECALCULATIONRUNCOMPID,

        /// <summary>
        /// FINAL RE-CALCULATION RUN COMPLETED.
        /// </summary>
        [Description(RunClassificationDescStatus.FINALRECALCULATIONRUNCOMP)]
        FINAL_RECALCULATION_RUN_COMPLETED = RunClassificationStatusIds.FINALRECALCULATIONRUNCOMPID,

        /// <summary>
        /// FINAL RUN COMPLETED.
        /// </summary>
        [Description(RunClassificationDescStatus.FINALRUNCOMPLETED)]
        FINAL_RUN_COMPLETED = RunClassificationStatusIds.FINALRUNCOMPLETEDID,
    }
}