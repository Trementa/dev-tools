using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Validations;
using OpenApi.Generator.CSharp.Generators;

namespace OpenApi.Generator
{
    public class Compiler
    {
        protected readonly Logger Logger;
        protected readonly Options Options;
        protected readonly IEnumerable<CodeGenerator> CodeGenerators;

        public Compiler(
            Logger<Compiler> logger,
            Options options,
            IEnumerable<CodeGenerator> codeGenerators) =>
            (Options, CodeGenerators, Logger) =
            (options, codeGenerators, logger);

        public async Task ExecuteCommands(CancellationToken cancellationToken = default)
        {
            try
            {
                await Compile(cancellationToken);
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

            var document = await ParseDocument(new DefinitionSource(Options.SourceFile), cancellationToken);
            var tasks = new List<Task>();

            foreach(var generator in CodeGenerators)
                tasks.Add(generator.Generate(document, cancellationToken));

            await Task.WhenAll(tasks.ToArray());
        }

        protected async Task<OpenApiDocument> ParseDocument(DefinitionSource definitionSource, CancellationToken cancellationToken)
        {
            var source = await definitionSource.Read(cancellationToken);
            var document = new OpenApiStreamReader(new OpenApiReaderSettings {
                ReferenceResolution = ReferenceResolutionSetting.ResolveLocalReferences,
                RuleSet = ValidationRuleSet.GetDefaultRuleSet()
            }).Read(source, out var result);

            Logger.LogError(result.Errors);

            if (result.SpecificationVersion != Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0)
            {
                Logger.LogWarning(
                    $"The document has version {result.SpecificationVersion} which is not the recommended");
                Logger.LogInformation("Updating to OpenApi 3.0");
                return await ParseDocument(await definitionSource.Update(cancellationToken), cancellationToken);
            }

            return document;
        }
    }
}
