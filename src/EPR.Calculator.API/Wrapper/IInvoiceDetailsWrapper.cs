using EPR.Calculator.API.Data.DataModels;

namespace EPR.Calculator.API.Wrapper
{
    public interface IInvoiceDetailsWrapper
    {
        Task<int> ExecuteSqlAsync(FormattableString sql, CancellationToken cancellationToken);
    }
}
