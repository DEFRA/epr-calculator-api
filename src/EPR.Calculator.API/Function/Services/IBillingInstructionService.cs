using EPR.Calculator.Service.Function.Models;

namespace EPR.Calculator.Service.Function.Services
{
    public interface IBillingInstructionService
    {
        public Task<bool> CreateBillingInstructions(CalcResult calcResult);
    }
}
