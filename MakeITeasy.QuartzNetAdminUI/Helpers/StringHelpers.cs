using System.Collections.Generic;

namespace MakeITeasy.QuartzNetAdminUI.Helpers
{
    public static class StringHelpers
    {
        private static readonly string DynamicStringPrefix = "%%";

        public static string ReplacePercent(string input, Dictionary<string, string> values, int startIndex = 0)
        {
            var index1 = input.IndexOf(DynamicStringPrefix, startIndex);

            if (index1 > startIndex)
            {
                var index2 = input.IndexOf(DynamicStringPrefix, index1 + 1);
                if (index2 > index1)
                {
                    var output = input.Substring(0, index1);
                    output += values[input.Substring(index1 + DynamicStringPrefix.Length, index2 - index1 - DynamicStringPrefix.Length)];
                    output += input[(index2 + DynamicStringPrefix.Length)..];

                    return ReplacePercent(output, values, index1);
                }
            }

            return input;
        }
    }
}