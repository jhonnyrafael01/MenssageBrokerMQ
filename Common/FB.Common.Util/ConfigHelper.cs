using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FB.Common.Util
{
    public static class ConfigHelper
    {
        private static IConfiguration ConfigurationSettings => ConfigurationAppSettings.Configuration;

        public static string GetConfiguration(string key)
        {
            string a = null;

            if (ConfigurationSettings.GetSection("appSettings").GetSection(key).Value == null)
                a = ConfigurationSettings.GetSection("ConnectionStrings").GetSection(key).Value;
            else
                a = ConfigurationSettings.GetSection("appSettings").GetSection(key).Value;
            return a;
        }

        public static string QueueName
        {
            get
            {
                return GetConfiguration("QueueName");
            }
        }
    }
}
