using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Trementa.OpenApi.Generator.Tracking.Observers
{
    public class LicenseObserver : IObserver<FileInfo>
    {
        const string LicenseHeaderFileName = "LicenseHeader.lic";
        readonly string[] skipLicenseWhenFileStartsWith = new[] { "/*", Environment.NewLine };
        bool StartsWithExcludingTag(string text) => Array.TrueForAll(skipLicenseWhenFileStartsWith, value => text.StartsWith(value));

        protected readonly Logger Logger;
        protected readonly Options Options;

        public LicenseObserver(Logger<LicenseObserver> logger, Options options)
        {
            Logger = logger;
            Options = options;
        }

        private string license = string.Empty;
        protected string License => license ??= $"/*{Environment.NewLine}{GetLicenseFile()}{Environment.NewLine}*/{Environment.NewLine}";

        string GetLicenseFile() =>
            new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(LicenseHeaderFileName)!).ReadToEnd();

        public void OnCompleted()
            => Logger.LogDebug("LicenseObserver completed");

        public void OnError(Exception error)
            => Logger.LogError(error, "Error when creating file");

        public async void OnNext(FileInfo value)
        {
            if (!value.Name.EndsWith(Options.CsCompiled))
            {
                Logger.LogDebug($"Not adding license header to {value.Name}, skipping");
                return;
            }

            if (!value.Exists)
                Logger.LogError($"""Couldn't add license text to "{value.Name}", file didn't exist""");

            await InsertLicenseTextOrSkip(value);
            Logger.LogDebug($"Added license text to \"{value.Name}\"");
        }

        async Task InsertLicenseTextOrSkip(FileInfo file)
        {
            var text = await File.ReadAllTextAsync(file.FullName);
            if (StartsWithExcludingTag(text))
                return;

            await File.WriteAllTextAsync(file.FullName, $"{License}{text}");
        }
    }
}
