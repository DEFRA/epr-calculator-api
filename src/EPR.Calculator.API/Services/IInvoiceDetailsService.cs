namespace EPR.Calculator.Service.Function.Services
{
    public interface IInvoiceDetailsService
    {
        Task<int> InsertInvoiceDetailsAtProducerLevel(int runId, DateTime instructionConfirmedDate, string instructionConfirmedBy, CancellationToken cancellationToken);
    }
}