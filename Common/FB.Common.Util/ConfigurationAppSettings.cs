using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FB.Common.Util
{
    public class ConfigurationAppSettings
    {
        private static IConfiguration _configuration;
        private static readonly object _lock = new object();

        public static void ConfigureSettings(IConfiguration configuration)
        {
            if (_configuration == null)
            {
                lock (_lock)
                {
                    if (_configuration == null)
                        _configuration = configuration;
                }
            }
        }

        public static IConfiguration Configuration => _configuration;
    }
}
