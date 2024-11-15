using EPR.Calculator.API.Models;

namespace EPR.Calculator.API.Builder.CommsCost
{
    public interface ICalcResultCommsCostBuilder
    {
        CalcResultCommsCost Construct(int runId, CalcResultOnePlusFourApportionment apportionment);
    }
}