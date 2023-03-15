using RazorLight;

namespace Trementa.OpenApi.Generator.Razor;
using Generators.Models;
using Tracking;
using Trementa.OpenApi.Generator;

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

    public async Task GenerateAndSaveCode(ApiTemplateModel model, string destinationFile)
    {
        var destinationFileInfo = new FileInfo(System.IO.Path.Combine(Options.OutputDirectory, destinationFile))!;

        try
        {
            var content = await RazorLightEngine.CompileRenderAsync(model.TemplateName, model);
            Save(destinationFileInfo, content);
            FileArtifactTracker.TrackFile(destinationFileInfo);
            Logger.LogInformation("Generated C# file: {generatedFile}\n\tUsing template: {template}", destinationFileInfo.Name, model.TemplateName);
        }
        catch (Exception ex)
        {
            ErrorTracker.ReportError(ex, $"Failed to process template {model.TemplateName} with destination {destinationFileInfo.FullName}. Model: {model}");
            if (!Options.ContinueOnError)
                throw;
        }
    }

    protected void Save(FileInfo destinationFile, string content)
    {
        destinationFile!.Directory!.Create();
        File.WriteAllText(destinationFile.FullName, content);
    }
}
