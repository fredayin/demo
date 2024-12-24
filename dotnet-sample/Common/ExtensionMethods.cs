using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Common
{
    public static class ExtensionMethods
    {
        public static string ToJson<T>(this T item)
        {
            return JsonSerializer.Serialize(item);
        }

        public static string RemoveHyphens(this Guid guid)
        {
            return guid.ToString("N");
        }

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
