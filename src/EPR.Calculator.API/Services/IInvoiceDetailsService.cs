namespace EPR.Calculator.Service.Function.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IInvoiceDetailsService
    {
        Task<int> InsertInvoiceDetailsAtProducerLevel(int runId, DateTime instructionConfirmedDate, string instructionConfirmedBy, CancellationToken cancellationToken);
    }
}