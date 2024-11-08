namespace EPR.Calculator.API.CommsCost
{
    public interface ICommsCostReportBuilder
    {
        CommsCostReport Construct(int runId);
    }
}