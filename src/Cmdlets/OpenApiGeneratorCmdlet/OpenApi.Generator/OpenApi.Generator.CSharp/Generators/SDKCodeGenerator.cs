using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using OpenApi.Generator.CSharp.Tracking;

namespace OpenApi.Generator.CSharp.Generators
{
    public class SDKCodeGenerator : StaticCodeFileGenerator
    {
        protected readonly RazorCodeGenerator RazorCodeGenerator;

        public SDKCodeGenerator(
            FileArtifactTracker fileArtifactTracker,
            Options options,
            RazorCodeGenerator razorCodeGenerator,
            Logger<SDKCodeGenerator> logger) : base(fileArtifactTracker, options, logger) =>
            RazorCodeGenerator = razorCodeGenerator;

        public override async Task Generate(OpenApiDocument document, CancellationToken cancellationToken)
        {
            foreach (var templatePath in GetAdditionalFiles())
            {
                Logger.LogInformation($"Processing template {templatePath}");

                if (templatePath.EndsWith(Options.CsTemplate))
                    await RazorCodeGenerator.GenerateAndSaveCode(templatePath, document);
                else if (templatePath.EndsWith(Options.CsCompiled))
                    await CopyFileAndReplaceNamespace(templatePath, cancellationToken);
                else if (templatePath.EndsWith(Options.CsProject))
                {
                    var destinationPath = Path.Combine(Options.OutputDirectory, $"{Options.GenNamespace}csproj");
                    await RenameFileAsync(templatePath, destinationPath, cancellationToken);
                }
                else
                    await CopyFileAsync(templatePath, Options.OutputDirectory, cancellationToken);
            }
        }
    }
}
