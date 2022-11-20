using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenApi.Generator.CSharp.Tracking;

namespace OpenApi.Generator.CSharp.Generators
{
    public abstract class StaticCodeFileGenerator : CodeGenerator
    {
        protected readonly FileArtifactTracker FileArtifactTracker;
        protected readonly Logger Logger;
        protected readonly Options Options;

        public StaticCodeFileGenerator(FileArtifactTracker fileArtifactTracker, Options options, Logger logger) =>
            (FileArtifactTracker, Options, Logger) =
            (fileArtifactTracker, options, logger);

        protected IEnumerable<string> GetAllFiles()
        {
            var templateKeys = Assembly
                .GetExecutingAssembly()
                .GetManifestResourceNames()
                .ToList();

            if (Options.TemplateDirectory != null && Directory.Exists(Options.TemplateDirectory))
                foreach (var directory in Directory.EnumerateDirectories(Options.TemplateDirectory, "*",
                    SearchOption.TopDirectoryOnly))
                    foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
                    {
                        var templatePath = Path.GetRelativePath(Options.TemplateDirectory, file);
                        if (templateKeys.Any(t =>
                            !string.Equals(templatePath, t, StringComparison.InvariantCultureIgnoreCase)))
                            templateKeys.Add(templatePath);
                    }
            return templateKeys;
        }

        protected IEnumerable<string> GetAdditionalFiles()
        {
            var templateKeys = GetAllFiles();
            var blackList = new List<string>
                {"Api.cst", "Configuration.cst", "LicenseHeader.lic", "Model.cst", "ModelClass.cst", "ModelClassEnum.cst", "Summary.cst"};
            return templateKeys.Where(t => !blackList.Contains(t, StringComparer.InvariantCultureIgnoreCase));
        }

        protected async Task CopyFileAndReplaceNamespace(string templatePath, CancellationToken cancellationToken = default)
        {
            var stream = GetGeneralStream(templatePath);
            using var sourceStream = new StreamReader(stream);
            var content = await sourceStream.ReadToEndAsync();

            var @namespace = Path.GetDirectoryName(templatePath)!.Replace('\\', '.').Replace('/', '.');
            var result = System.Text.RegularExpressions.Regex
                .Replace(content, @"namespace ([0-9A-Za-z.:]*)(.*)", $@"namespace {Options.SDKNamespace}{@namespace}$2");

            var destinationFile = await Write(result, templatePath, sourceStream.CurrentEncoding, cancellationToken);
            FileArtifactTracker.TrackFile(destinationFile);

            Logger.LogInformation($"Processed C# file: \"{templatePath}\"");
        }

        protected async Task CopyFileAsync(string templatePath, string destinationDirectory, CancellationToken cancellationToken = default)
        {
            var destinationPath = Path.Combine(destinationDirectory, templatePath);
            await RenameFileAsync(templatePath, destinationPath, cancellationToken);
        }

        protected async Task RenameFileAsync(string templatePath, string destinationPath, CancellationToken cancellationToken = default)
        {
            var destinationFolder = Path.GetDirectoryName(destinationPath);
            Directory.CreateDirectory(destinationFolder);

            var sourceStream = GetGeneralStream(templatePath);
            await using var destinationStream = new FileStream(destinationPath,
                Options.Force ? FileMode.Create : FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
            await sourceStream.CopyToAsync(destinationStream, cancellationToken);
            FileArtifactTracker.TrackFile(destinationPath);

            Logger.LogInformation($"Copied file verbatim: \"{templatePath}\"");
        }

        protected async Task<string> Write(string result, string templatePath, Encoding currentEncoding, CancellationToken cancellationToken)
        {
            var destinationFolder = Path.Combine(Options.OutputDirectory, Path.GetDirectoryName(templatePath)!);
            Directory.CreateDirectory(destinationFolder);
            var destinationFile = Path.Combine(Options.OutputDirectory, templatePath);
            await using var destinationStream = new FileStream(destinationFile,
                Options.Force ? FileMode.Create : FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096,
                FileOptions.Asynchronous);
            await using var textStream = new StreamWriter(destinationStream, currentEncoding);
            await textStream.WriteAsync(new ReadOnlyMemory<char>(result.ToCharArray()), cancellationToken);

            return destinationFile;
        }

        protected Stream GetGeneralStream(string templatePath)
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(templatePath);
            if (stream != null && stream.CanRead)
                return stream;

            if (Options.TemplateDirectory == null)
                throw new FileNotFoundException($"Couldn't find \"{templatePath}\"");
            return new FileStream(Path.Combine(Options.TemplateDirectory, templatePath), FileMode.Open, FileAccess.Read, FileShare.Read, 4096,
                FileOptions.Asynchronous | FileOptions.SequentialScan);
        }

    }
}