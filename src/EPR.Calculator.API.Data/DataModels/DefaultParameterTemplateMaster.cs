namespace EPR.Calculator.API.Data.DataModels;

public class DefaultParameterTemplateMaster
{
    public required string ParameterUniqueReferenceId { get; set; }
    public required string ParameterType { get; set; }
    public required string ParameterCategory { get; set; }
    public decimal ValidRangeFrom { get; set; }
    public decimal ValidRangeTo { get; set; }
}

public enum ParameterUnit
{
    Unknown = 0,
    Date,
    Currency,
    Percentage,
    Factor,
    Tonnage
}

public static class DefaultParameterTemplateMasterExtensions
{
    extension(DefaultParameterTemplateMaster master)
    {
        public ParameterUnit Unit => master.GetUnit();

        private ParameterUnit GetUnit()
        {
            if (IsDate(master))
                return ParameterUnit.Date;

            if (IsPercentage(master))
                return ParameterUnit.Percentage;

            if (IsCurrency(master))
                return ParameterUnit.Currency;

            if (IsFactor(master))
                return ParameterUnit.Factor;

            if (IsTonnage(master))
                return ParameterUnit.Tonnage;

            return ParameterUnit.Unknown;

            static bool IsDate(DefaultParameterTemplateMaster master) =>
                master.ParameterCategory.Contains("date", StringComparison.OrdinalIgnoreCase);

            static bool IsPercentage(DefaultParameterTemplateMaster master) =>
                master.ParameterCategory.Contains("percent", StringComparison.OrdinalIgnoreCase)
                || master.ParameterType.Contains("percent", StringComparison.OrdinalIgnoreCase);

            static bool IsCurrency(DefaultParameterTemplateMaster master) =>
                master.ParameterType.Contains("costs", StringComparison.OrdinalIgnoreCase)
                || master.ParameterCategory.Contains("amount", StringComparison.OrdinalIgnoreCase);

            static bool IsFactor(DefaultParameterTemplateMaster master) =>
                master.ParameterType.Contains("factor", StringComparison.OrdinalIgnoreCase)
                && master.ParameterCategory.Contains("factor", StringComparison.OrdinalIgnoreCase);

            static bool IsTonnage(DefaultParameterTemplateMaster master) =>
                master.ParameterType.Contains("tonnage", StringComparison.OrdinalIgnoreCase)
                && !IsPercentage(master)
                && !IsCurrency(master);
        }
    }
}
