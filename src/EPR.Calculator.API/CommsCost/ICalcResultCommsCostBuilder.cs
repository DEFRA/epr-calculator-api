namespace EPR.Calculator.API.CommsCost
{
    public interface ICalcResultCommsCostBuilder
    {
        CalcResultCommsCost Construct(int runId);
    }
}