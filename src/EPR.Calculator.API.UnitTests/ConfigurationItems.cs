using Microsoft.Extensions.Configuration;

namespace EPR.Calculator.API.UnitTests
{
    public static class ConfigurationItems
    {
        public static IConfiguration GetConfigurationValues()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Test.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables();

            IConfiguration config = builder.Build();

            return config;
        }
    }
}
