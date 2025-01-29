using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EPR.Calculator.API.Services
{
    public class CommandTimeoutService : ICommandTimeoutService
    {
        private IConfiguration Configuration { get; init; }

        public CommandTimeoutService()
            => this.Configuration = new ConfigurationBuilder().Build();

        public CommandTimeoutService(IConfiguration configuration)
            : this() => this.Configuration = configuration;

        public void SetCommandTimeout(DatabaseFacade database, string key)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key, nameof(key));

            var commandTimeout = this.Configuration
                .GetSection("Timeouts")
                .GetValue<double>(key);
            if (commandTimeout > 0)
            {
                database.SetCommandTimeout(TimeSpan.FromMinutes(commandTimeout));
            }
        }
    }
}
