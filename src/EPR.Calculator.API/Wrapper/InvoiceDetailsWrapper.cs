using EPR.Calculator.API.Data;
using EPR.Calculator.API.Data.DataModels;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.API.Wrapper
{
    public class InvoiceDetailsWrapper : IInvoiceDetailsWrapper
    {
        private readonly ApplicationDBContext context;

        public InvoiceDetailsWrapper(ApplicationDBContext context)
        {
            this.context = context;
        }

        public async Task<int> ExecuteSqlAsync(FormattableString sql, CancellationToken cancellationToken)
            => await this.context.Database.ExecuteSqlAsync(sql, cancellationToken);
    }
}
