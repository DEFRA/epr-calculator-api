using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPR.Calculator.API.Common
{
    public static class ConfigurationHelper
    {
        public static IConfiguration config;
        public static void Initialise(IConfiguration configuration)
        {
            config = configuration;
        }
    }
}
