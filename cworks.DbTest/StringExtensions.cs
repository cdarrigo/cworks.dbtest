using System;
using System.Linq;

namespace cworks.DbTest
{
    internal static class StringExtensions
    {
        public static bool StartsWithAny(this string input, params string[] targets)
        {
            return targets.Any(input.StartsWith);
        }
        public static bool StartsWithAnyCi(this string input, params string[] targets)
        {
            return targets.Any(i=>input.StartsWith(i,StringComparison.InvariantCultureIgnoreCase));
        }
    }
}