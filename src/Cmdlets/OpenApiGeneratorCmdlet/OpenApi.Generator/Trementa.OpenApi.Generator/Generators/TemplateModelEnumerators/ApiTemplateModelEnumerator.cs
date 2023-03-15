using Microsoft.OpenApi.Models;

namespace Trementa.OpenApi.Generator.Generators.TemplateModelEnumerators;
using IO;
using Models;

public class ApiTemplateModelEnumerator
{
    public IEnumerable<ApiTemplateModel> EnumerateApiByTag(OpenApiDocument openApiDocument)
    {
        var tags = openApiDocument.Tags.Select(t => t.Name);
        var paths = openApiDocument.Paths;

        var allTags = paths
            .SelectMany(p => p.Value.Operations.Values)
            .SelectMany(v => v.Tags)
            .Select(t => t.Name)
            .Union(tags)
            .Distinct();

        // Each tag corresponds to a class
        foreach (var tag in allTags)
        {
            var grouping = paths
                .Where(p => p.Value.Operations.Any(o => o.Value.Tags.Count(t => t.Name.Equals(tag)) > 0));
            if (grouping.Any())
            {
                var className = Normalize(tag);
                var model = grouping.Select(g => (g.Key, g.Value));
                yield return CreateApiClassTemplate(className, model);
            }
        }

        ApiTemplateModel CreateApiClassTemplate(string typeName, IEnumerable<(string Key, OpenApiPathItem Value)> model)
             => new ApiTemplateModel(new(typeName), model);
    }

    string Normalize(string path)
    {
        if (path.StartsWith('/'))
            path = path.Substring(1);

        return path;
    }
}