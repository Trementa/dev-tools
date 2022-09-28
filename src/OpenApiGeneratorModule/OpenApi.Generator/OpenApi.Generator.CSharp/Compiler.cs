using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
        protected readonly ILogger Logger;
        protected readonly ErrorTracker ErrorTracker;
        protected readonly FileArtifactTracker FileArtifactTracker;

        public Compiler(
            FileArtifactTracker fileArtifactTracker,
            ErrorTracker errorTracker,
            ILogger<Compiler> logger,
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
                if(!Options.Quiet)
                    Logger.LogError(e, "An error occured");
                if (Options.ContinueOnError)
                    return;
                throw;
            }
        }

        protected async Task Compile(CancellationToken cancellationToken)
        {
            if(!Options.Quiet)
                Logger.LogInformation($"Compiling { Options.SourceFile } into { Options.TemplateLanguage }");

            using var fileLogger = Logger.BeginScope("Compilation");
            FileArtifactTracker.Subscribe(new LicenseObserver(Options, Logger));
            ErrorTracker.Subscribe(new ErrorObserver(Options, Logger));
            var document = await ParseDocument(new DefinitionSource(Options.SourceFile), cancellationToken);
            var tasks = new List<Task>();
            if ((Options.OutputType & Options.OutputTypeEnum.SDK) == Options.OutputTypeEnum.SDK)
                tasks.Add(CodeGenAdditional.Execute(document, cancellationToken));

            if ((Options.OutputType & Options.OutputTypeEnum.Api) == Options.OutputTypeEnum.Api)
                tasks.Add(CodeGenVisitor.GenerateApi(document, cancellationToken));

            if ((Options.OutputType & Options.OutputTypeEnum.Model) == Options.OutputTypeEnum.Model)
                tasks.Add(CodeGenVisitor.GenerateModel(document, cancellationToken));

            Task.WaitAll(tasks.ToArray());

            FileArtifactTracker.End();
            ErrorTracker.End();
        }

        protected async Task<OpenApiDocument> ParseDocument(DefinitionSource definitionSource, CancellationToken cancellationToken)
        {
            var (document, result) = await Task.Run(async () =>
            {
                var doc = new OpenApiStreamReader(new OpenApiReaderSettings
                {
                    ReferenceResolution = ReferenceResolutionSetting.ResolveLocalReferences,
                    RuleSet = ValidationRuleSet.GetDefaultRuleSet()
                }).Read(await definitionSource.Read(cancellationToken), out var context);
                return (doc, context);
            }, cancellationToken);

            if (!Options.Quiet)
                foreach (var error in result.Errors)
                    Logger.LogError(error.ToString());

            if (string.Compare(document?.Info?.Version, "3.0.1", StringComparison.Ordinal) == -1)
            {
                if (!Options.Quiet)
                {
                    Logger.LogWarning(
                        $"The document has version {document?.Info?.Version} which is less than the recommended");
                    Logger.LogInformation("Updating to OpenApi 3.0");
                }

                return await ParseDocument(await definitionSource.Update(cancellationToken), cancellationToken);
            }

            return document;
        }
    }
}