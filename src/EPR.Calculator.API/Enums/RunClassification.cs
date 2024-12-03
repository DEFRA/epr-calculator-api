using System.ComponentModel;

namespace EPR.Calculator.API.Enums
{
    public enum RunClassification
    {
        [Description("IN THE QUEUE")]
        INTHEQUEUE = 1,

        [Description("RUNNING")]
        RUNNING = 2,

        [Description("UNCLASSIFIED")]
        UNCLASSIFIED = 3,

        [Description("PLAY")]
        PLAY = 4,

        [Description("ERROR")]
        ERROR = 5,

        [Description("DELETED")]
        DELETED = 6
    }
}
