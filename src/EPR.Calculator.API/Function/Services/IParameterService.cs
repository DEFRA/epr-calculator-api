namespace EPR.Calculator.Service.Function.Services
{
    public interface IParameterService
    {
        public Task<IReadOnlyDictionary<string, decimal>> GetDefaultParameters(int runId);
    }
}
