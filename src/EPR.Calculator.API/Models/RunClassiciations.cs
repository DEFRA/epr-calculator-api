using System.ComponentModel;

namespace EPR.Calculator.API.Models
{
    public enum RunClassiciations
    {
        [Description("IN THE QUEUE")]
        INTHEQUEUE = 1,
        [Description("RUNNING")]
        RUNNING,
        [Description("UNCLASSIFIED")]
        UNCLASSIFIED,
        [Description("PLAY")]
        PLAY,
        [Description("ERROR")]
        ERROR
    }
}