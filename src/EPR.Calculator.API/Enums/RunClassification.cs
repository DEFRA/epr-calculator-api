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
        [Description("PLAY")]
        PLAY = 4,

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
    }
}
