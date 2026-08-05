namespace EPR.Calculator.Service.Function.Services
{
    public static class ObligationStates
    {
        public const string Obligated = "O";
        public const string NotObligated = "N";
        public const string Error = "E";

        public static bool IsObligated(string status)
        {
            return status == Obligated;
        }
    }
}