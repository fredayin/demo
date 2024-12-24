using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Extensions
{
    public static class ConfigurationExtensions
    {
        public static string GetValueWithSuffix(this IConfiguration configuration, string key, string trailingValue)
        {
            return GetValueWithSuffix(configuration, key, "", trailingValue);
        }

        public static string GetValueWithSuffix(this IConfiguration configuration, string key, string defaultValue, string trailingValue)
        {
            var prefix = configuration[key];

            if (string.IsNullOrEmpty(prefix))
            {
                return defaultValue;
            }

            return prefix.EndsWith(trailingValue) ? prefix : prefix + trailingValue;
        }
    }
}
