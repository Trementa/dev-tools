using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;
using OpenApi.Generator.CSharp.IO;

namespace OpenApi.Generator.CSharp.Generators;

public class ApiTemplateEnumerable : IEnumerable<Template>
{
    public ApiTemplateEnumerable(OpenApiDocument doc) =>
        OpenApiDocument = doc;

    protected readonly OpenApiDocument OpenApiDocument;

    public IEnumerator<Template> GetEnumerator()
    {
        var tags = OpenApiDocument.Tags.Select(t => t.Name);
        var paths = OpenApiDocument.Paths;

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
                var model = (className, grouping.Select(g => (g.Key, g.Value)));
                yield return Create(className, model);
            }
        }

        ApiTemplate<TModel> Create<TModel>(string typeName, TModel model)
             => new ApiTemplate<TModel>(new(typeName), model);
    }

    string Normalize(string path)
    {
        if (path.StartsWith('/'))
            path = path.Substring(1);

        return path;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}