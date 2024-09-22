using Microsoft.Extensions.Configuration;

namespace EPR.Calculator.API.Common
{
    public static class ConfigurationHelper
    {
        private static IConfiguration AppSetting { get; }

        static ConfigurationHelper()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // or specify the correct path if needed
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            AppSetting = builder.Build();
        }

        public static string GetSetting(string key)
        {
            return AppSetting[key];
        }
    }
}
