using EPR.Calculator.Service.Function.Enums;

namespace EPR.Calculator.Service.Function.Services
{
    public interface IClassificationService
    {
        public Task UpdateRunClassification(int runId, RunClassification runClassification);
    }
}
