using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using Trementa.OpenApi.Generator.Tracking;

namespace Trementa.OpenApi.Generator.Generators.CodeGenerators
{
    public class TemplateCodeDumpGenerator : StaticCodeFileGenerator
    {
        public TemplateCodeDumpGenerator(
            FileArtifactTracker fileArtifactTracker,
            Options options,
            Logger<TemplateCodeDumpGenerator> logger) : base(fileArtifactTracker, options, logger)
        { }

        public override async Task Generate(OpenApiDocument document, CancellationToken cancellationToken = default)
        {
            using var disposable = Logger.BeginScope("DumpingTemplateFiles");
            var destinationPath = Path.Combine(Options.OutputDirectory, "Templates");
            Logger.LogInformation($"Dumping all templates into folder \"{destinationPath}\"");
            foreach (var file in GetAllFiles())
                await CopyFileAsync(file, destinationPath, cancellationToken);
        }
    }
}
