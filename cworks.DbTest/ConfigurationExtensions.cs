using Microsoft.Extensions.Configuration;

namespace cworks.DbTest
{
    internal static class ConfigurationExtensions
    {
        public static bool ReadBool(this IConfiguration config, string key, bool defaultValue = false)
        {
            return (config[key] ?? defaultValue.ToString()).ToLower().Equals("true");
        }
        public static bool? ReadBoolNullable(this IConfiguration config, string key, bool? defaultValue = null)
        {
            var val = config[key];
            return string.IsNullOrEmpty(val) ? defaultValue : val.ToLower().Equals("true");
        }
    }
}