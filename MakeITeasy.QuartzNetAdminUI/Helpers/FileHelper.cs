using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MakeITeasy.QuartzNetAdminUI.Helpers
{
    public static class FileHelper
    {
        public readonly static Dictionary<string, string> ContentTypeByExtension = new()
        {
            { ".js", "text/javascript" },
            { ".css", "text/css" },
            { ".json", "application/json" }
        };

        public async static Task<string> GetResourceFileAsync(string fileName)
        {
            var assembly = typeof(QuartzNetUIMiddleware).Assembly;

            string assemblyName = assembly.GetName().Name;

            string assetsPath = $"{assemblyName}.assets.";

            string[] resourceNames = assembly.GetManifestResourceNames();

            string indexResourceFile = resourceNames.FirstOrDefault(x => x[assetsPath.Length..].Equals(fileName));

            if (!string.IsNullOrEmpty(indexResourceFile))
            {
                using (var stream = assembly.GetManifestResourceStream(indexResourceFile))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        return await reader.ReadToEndAsync();
                    }
                }
            }

            return string.Empty;
        }
    }
}
