using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using SharpYaml.Tokens;

namespace OpenApi.Generator.CSharp.IO;
public record Persistance(Logger<Persistance> Logger, Options Options)
{
    public void Save(GeneratedCode generatedCode)
    {

    }



}


public record struct TemplateType(string Name);
public record struct TypeName(string Name);

public abstract record Template(TemplateType TemplateType, TypeName TypeName);
public abstract record Template<TModel>(TemplateType TemplateType, TypeName TypeName, TModel Model) : Template(TemplateType, TypeName);

public record ApiTemplate<TModel>(TypeName TypeName, TModel Model) : Template<TModel>(new TemplateType("Api"), TypeName, Model);
public record ModelTemplate<TModel>(TypeName TypeName, TModel Model) : Template<TModel>(new TemplateType("Model"), TypeName, Model);


public static class A
{
    public static ApiTemplate<TModel> Create<TModel>(TypeName typeName, TModel model)
        => new ApiTemplate<TModel>(typeName, model);
}

public record struct GeneratedCode(string Code);

public class ApiGenerator : IAsyncEnumerable<Template>
{
    public ApiGenerator(OpenApiDocument doc) =>
        OpenApiDocument = doc;

    protected readonly OpenApiDocument OpenApiDocument;

    public async IAsyncEnumerator<Template> GetAsyncEnumerator(CancellationToken cancellationToken = default)
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
            cancellationToken.ThrowIfCancellationRequested();

            var grouping = paths
                .Where(p => p.Value.Operations.Any(o => o.Value.Tags.Count(t => t.Name.Equals(tag)) > 0));
            if (grouping.Any())
            {
                var className = Normalize(tag);
                var fileName = CreateFileName("Api", className, "Api.cs");
                (string className, IEnumerable<(string Key, OpenApiPathItem Value)>) model = (className, grouping.Select(g => (g.Key, g.Value)));

                yield return new ApiTemplate<>(new TypeName(className), model);
                yield return A.Create(new TypeName(className), model);
            }
        }
    }



    string CreateFileName(string folder, string typeName, string postfix) =>
        System.IO.Path.Combine(folder, string.Concat(typeName, postfix));


    string Normalize(string path)
    {
        if (path.StartsWith('/'))
            path = path.Substring(1);

        return path;
    }

}