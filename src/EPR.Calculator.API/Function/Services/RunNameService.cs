using EPR.Calculator.API.Data;
using Microsoft.EntityFrameworkCore;

namespace EPR.Calculator.Service.Function.Services
{
    public interface IRunNameService
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="runId">runId is number.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<string> GetRunNameAsync(int runId);
    }

    /// <summary>
    /// Service to fetch the run name from the database.
    /// </summary>
    public class RunNameService : IRunNameService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RunNameService"/> class.
        /// </summary>
        /// <param name="context">The context object.</param>
        public RunNameService(ApplicationDBContext context)
        {
            Context = context;
        }

        private ApplicationDBContext Context { get; init; }

        /// <summary>
        /// Gets the run name for the specified run ID.
        /// </summary>
        /// <param name="runId">The run ID.</param>
        /// <returns>The run name.</returns>
        public async Task<string> GetRunNameAsync(int runId)
        {
            var run = await Context.CalculatorRuns
                .SingleOrDefaultAsync(r => r.Id == runId);

            if (run == null)
            {
                throw new KeyNotFoundException($"Calculator run with id {runId} not found");
            }

            if (string.IsNullOrEmpty(run.Name))
            {
                throw new ArgumentNullException($"Run name not found for the run id {runId}");
            }

            return run.Name;
        }
    }
}
