using System;
using System.IO;
using System.Threading.Tasks;
using RazorLight;

namespace OpenApi.Generator
{
    public class RazorCodeGenerator
    {
        protected readonly IRazorLightEngine RazorLightEngine;
        protected readonly Options Options;
        protected readonly Logger Logger;
        protected readonly FileArtifactTracker FileArtifactTracker;
        protected readonly ErrorTracker ErrorTracker;

        public RazorCodeGenerator(IRazorLightEngine razorLightEngine, Options options, Logger<RazorCodeGenerator> logger, FileArtifactTracker fileArtifactTracker, ErrorTracker errorTracker) =>
            (RazorLightEngine, Options, Logger, FileArtifactTracker, ErrorTracker)
            = (razorLightEngine, options, logger, fileArtifactTracker, errorTracker);

        public async Task GenerateAndSaveCode<TModel>(string templatePath, TModel model)
        {
            var file = Path.GetFileName(templatePath);
            var path = Path.GetDirectoryName(templatePath);
            var template = Path.GetFileNameWithoutExtension(file);
            var destinationPath = Path.Combine(path!, $"{template}.cs");
            await GenerateAndSaveCode(templatePath, destinationPath, model);
        }

        public async Task GenerateAndSaveCode<TModel>(string templateName, string destinationFile, TModel model)
        {
            var destinationFileInfo = new FileInfo(Path.Combine(Options.OutputDirectory, destinationFile))!;

            try
            {
                var content = await RazorLightEngine.CompileRenderAsync(templateName, model);
                Save(destinationFileInfo, content);
                FileArtifactTracker.TrackFile(destinationFileInfo);
                Logger.LogInformation("Generated C# file: {generatedFile}\n\tUsing template: {template}", destinationFileInfo.Name, templateName);
            }
            catch(Exception ex)
            {
                ErrorTracker.ReportError(ex, $"Failed to process template {templateName} with destination {destinationFileInfo.FullName}");
                if (!Options.ContinueOnError)
                    throw;
            }
        }

        protected void Save(FileInfo destinationFile, string content)
        {
            destinationFile!.Directory.Create();
            File.WriteAllText(destinationFile.FullName, content);
        }
    }
}
