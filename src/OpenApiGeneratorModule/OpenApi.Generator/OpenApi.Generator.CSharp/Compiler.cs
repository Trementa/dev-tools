using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Validations;

namespace OpenApi.Generator
{
    public class Compiler
    {
        protected readonly Options Options;
        protected readonly CodeGenAdditional CodeGenAdditional;
        protected readonly CSharpCodeGenerator CodeGenVisitor;
        protected readonly Logger Logger;
        protected readonly ErrorTracker ErrorTracker;
        protected readonly FileArtifactTracker FileArtifactTracker;

        public Compiler(
            FileArtifactTracker fileArtifactTracker,
            ErrorTracker errorTracker,
            Logger<Compiler> logger,
            Options options,
            CSharpCodeGenerator codeGenVisitor,
            CodeGenAdditional codeGenAdditional) =>
            (Options, CodeGenVisitor, Logger, FileArtifactTracker, ErrorTracker, CodeGenAdditional) =
            (options, codeGenVisitor, logger, fileArtifactTracker, errorTracker, codeGenAdditional);

        public async Task ExecuteCommands(CancellationToken cancellationToken = default)
        {
            try
            {
                if (Options.Generate)
                    await Compile(cancellationToken);
                if (Options.DumpTemplate)
                    await CodeGenAdditional.DumpTemplates(cancellationToken);

                if (!Options.Generate && !Options.DumpTemplate)
                    Logger.LogWarning("Nothing to do! No command supplied.");
            }
            catch (Exception e)
            {
                Logger.LogError(e, "An error occured");
                if (Options.ContinueOnError)
                    return;
                throw;
            }
        }

        protected async Task Compile(CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Compiling { Options.SourceFile } into { Options.TemplateLanguage }");

            using var fileLogger = Logger.BeginScope("Compilation");
            FileArtifactTracker.Subscribe(new LicenseObserver(Options, Logger));
            ErrorTracker.Subscribe(new ErrorObserver(Logger));
            var document = await ParseDocument(new DefinitionSource(Options.SourceFile), cancellationToken);
            var tasks = new List<Task>();
            if ((Options.OutputType & Options.OutputTypeEnum.SDK) == Options.OutputTypeEnum.SDK)
                tasks.Add(CodeGenAdditional.Execute(document, cancellationToken));

            if ((Options.OutputType & Options.OutputTypeEnum.Api) == Options.OutputTypeEnum.Api)
                tasks.Add(CodeGenVisitor.GenerateApi(document, cancellationToken));

            if ((Options.OutputType & Options.OutputTypeEnum.Model) == Options.OutputTypeEnum.Model)
                tasks.Add(CodeGenVisitor.GenerateModel(document, cancellationToken));

            await Task.WhenAll(tasks.ToArray());

            FileArtifactTracker.End();
            ErrorTracker.End();
        }

        protected async Task<OpenApiDocument> ParseDocument(DefinitionSource definitionSource, CancellationToken cancellationToken)
        {
            var source = await definitionSource.Read(cancellationToken);
            var document = new OpenApiStreamReader(new OpenApiReaderSettings {
                ReferenceResolution = ReferenceResolutionSetting.ResolveLocalReferences,
                RuleSet = ValidationRuleSet.GetDefaultRuleSet()
            }).Read(source, out var result);

            //var (document, result) = await Task.Run(() =>
            //{
            //    var source = definitionSource.Read();
            //    var doc = new OpenApiStreamReader(new OpenApiReaderSettings
            //    {
            //        ReferenceResolution = ReferenceResolutionSetting.ResolveLocalReferences,
            //        RuleSet = ValidationRuleSet.GetDefaultRuleSet()
            //    }).Read(source, out var context);
            //    return (doc, context);
            //}, cancellationToken);

            Logger.LogError(result.Errors);

            if (result.SpecificationVersion != Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0)
                //string.Compare(document?.Info?.Version, "3.0.1", StringComparison.Ordinal) == -1)
            {
                Logger.LogWarning(
                    $"The document has version {result.SpecificationVersion} which is not the recommended");

                if(result.SpecificationVersion == Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0)
                    Logger.LogInformation("Updating to OpenApi 3.0");

                if (result.SpecificationVersion == Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0)
                    return await ParseDocument(await definitionSource.Update(cancellationToken), cancellationToken);
            }

            return document;
        }
    }
}