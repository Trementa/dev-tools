using Microsoft.Extensions.Configuration;
using System.Linq;

namespace OpenApi.Generator
{
    internal static class Utility
    {
        public static string PrettyPrint(this IConfiguration configuration) =>
            string.Join("\n\t",
                new[]
                {
                    "outputDirectory",
                    "templateDirectory",
                    "source",
                    "continueOnError",
                    "force",
                    "GenNamespace",
                    "SDKNamespace",
                    "quiet",
                    "generate",
                    "lang",
                    "generate",
                    "dumpTemplate",
                    "templateLanguage",
                    "outputType"
                }.Select(key => $"{key}: {configuration[key]}"));
    }
}
