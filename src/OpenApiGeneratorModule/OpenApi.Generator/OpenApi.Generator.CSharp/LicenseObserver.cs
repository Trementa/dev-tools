using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace OpenApi.Generator
{
    public class LicenseObserver : IObserver<FileInfo>
    {
        const string LicenseHeaderFileName = "LicenseHeader.lic";
        readonly string[] skipLicenseWhenFileStartsWith = new[] { "/*", Environment.NewLine };
        bool StartsWithExcludingTag(string text) => Array.TrueForAll(skipLicenseWhenFileStartsWith, value => text.StartsWith(value));

        protected readonly ILogger Logger;
        protected readonly Options Options;

        public LicenseObserver(Options options, ILogger logger) =>
            (Options, Logger) =
            (options, logger);

        private string license = string.Empty;
        protected string License => license ??= $"/*{ Environment.NewLine }{ GetLicenseFile() }{ Environment.NewLine }*/{ Environment.NewLine }";

        string GetLicenseFile() =>
            new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(LicenseHeaderFileName)!).ReadToEnd();

        public void OnCompleted()
        {
            if (!Options.Quiet)
                Logger.LogInformation("LicenseObserver completed");
        }

        public void OnError(Exception error)
        {
            if (!Options.Quiet)
                Logger.LogInformation("Error when creating file");
        }

        public async void OnNext(FileInfo value)
        {
            if (!value.Name.EndsWith(Options.CsCompiled))
                return;

            if (!value.Exists)
            {
                if (!Options.Quiet)
                    Logger.LogError($"Couldn't add license text to \"{ value.Name }\", file didn't exist");
            }

            await InsertLicenseTextOrSkip(value);
            if (!Options.Quiet)
                Logger.LogInformation($"Added license text to \"{ value.Name }\"");
        }

        async Task InsertLicenseTextOrSkip(FileInfo file)
        {
            var text = await File.ReadAllTextAsync(file.FullName);
            if (StartsWithExcludingTag(text))
                return;

            await File.WriteAllTextAsync(file.FullName, $"{ License }{ text }");
        }
    }
}
