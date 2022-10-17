using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace OpenApi.Generator
{
    public class CSharpCodeGenerator
    {
        protected readonly RazorCodeGenerator RazorCodeGenerator;
        protected readonly Logger Logger;

        public CSharpCodeGenerator(RazorCodeGenerator razorViewToStringRenderer, Logger<CSharpCodeGenerator> logger)
        {
            RazorCodeGenerator = razorViewToStringRenderer;
            Logger = logger;
        }

        public async Task GenerateApi(OpenApiDocument doc, CancellationToken cancellationToken = default)
        {
            await GenerateApi(doc.Tags.Select(t => t.Name), doc.Paths, cancellationToken);
        }

        public async Task GenerateModel(OpenApiDocument doc, CancellationToken cancellationToken = default)
        {
            await GenerateModels(doc.Components, cancellationToken);
        }

        protected async Task GenerateApi(IEnumerable<string> tags, OpenApiPaths paths, CancellationToken cancellationToken)
        {
            var allTags = paths
                .SelectMany(p => p.Value.Operations.Values)
                .SelectMany(v => v.Tags)
                .Select(t => t.Name)
                .Union(tags)
                .Distinct();

            // Each tag corresponds to a class
            foreach (var tag in allTags)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var grouping = paths
                    .Where(p => p.Value.Operations.Any(o => o.Value.Tags.Count(t => t.Name.Equals(tag)) > 0));
                if (grouping.Any())
                {
                    var className = Normalize(tag);
                    var fileName = CreateFileName("Api", className, "Api.cs");
                    await RazorCodeGenerator.GenerateAndSaveCode("Api", fileName,
                        (className, grouping.Select(g => (g.Key, g.Value))));
                }
            }
        }

        string CreateFileName(string folder, string typeName, string prefix) =>
            Path.Combine(folder, string.Concat(typeName, prefix));

        protected async Task GenerateModels(OpenApiComponents components, CancellationToken cancellationToken)
        {
            foreach (var model in components.Schemas)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var modelName = model.Key.ToPascalCase();
                var fileName = CreateFileName("Model", modelName, ".cs");
                await RazorCodeGenerator.GenerateAndSaveCode("Model", fileName,
                    (modelName, model.Value));
            }
        }

        string Normalize(string path)
        {
            if (path.StartsWith('/'))
                path = path.Substring(1);

            return path;
        }
    }
}
