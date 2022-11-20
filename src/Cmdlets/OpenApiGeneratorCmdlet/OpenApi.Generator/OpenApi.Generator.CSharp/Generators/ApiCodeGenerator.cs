using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;

namespace OpenApi.Generator.CSharp.Generators
{
    public class ApiCodeGenerator : CodeGenerator
    {
        protected readonly RazorCodeGenerator RazorCodeGenerator;
        //protected readonly ApiTemplateEnumerable ApiTemplateEnumerable;
        protected readonly Logger Logger;

        public ApiCodeGenerator(RazorCodeGenerator razorCodeGenerator, /*ApiTemplateEnumerable apiTemplateEnumerable,*/ Logger<ApiCodeGenerator> logger)
        {
            RazorCodeGenerator = razorCodeGenerator;
            //ApiTemplateEnumerable = apiTemplateEnumerable;
            Logger = logger;
        }

        public override async Task Generate(OpenApiDocument doc, CancellationToken cancellationToken = default)
        {
            await GenerateApi(doc.Tags.Select(t => t.Name), doc.Paths, cancellationToken);
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

        string Normalize(string path)
        {
            if (path.StartsWith('/'))
                path = path.Substring(1);

            return path;
        }
    }
}
