namespace EPR.Calculator.API.BackgroundService.Features.BillingRuns.Constants;

public static class BillingConstants
{
    public static class Suggestion
    {
        public const string Initial = "INITIAL";
        public const string Delta = "DELTA";
        public const string Rebill = "REBILL";
        public const string Cancel = "CANCEL";
    }

    public static class Action
    {
        public const string Accepted = "Accepted";
        public const string Rejected = "Rejected";
    }
}
