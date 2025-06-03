namespace EPR.Calculator.API.Services.Abstractions
{
    public interface IPrepareBillingFileService
    {
        Task<ServiceProcessResponseDto> PrepareBillingFileAsync(int calculatorRunId);
    }
}