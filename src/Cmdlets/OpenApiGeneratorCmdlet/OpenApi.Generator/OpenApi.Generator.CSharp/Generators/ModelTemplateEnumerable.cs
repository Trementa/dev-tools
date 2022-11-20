using System.Collections;
using System.Collections.Generic;
using Microsoft.OpenApi.Models;
using OpenApi.Generator.CSharp.IO;

namespace OpenApi.Generator.CSharp.Generators;

public class ModelTemplateEnumerable : IEnumerable<Template>
{
    public ModelTemplateEnumerable(OpenApiDocument doc) =>
        OpenApiDocument = doc;

    protected readonly OpenApiDocument OpenApiDocument;

    public IEnumerator<Template> GetEnumerator()
    {
        var components = OpenApiDocument.Components;
        foreach (var modelSchema in components.Schemas)
        {
            var className = modelSchema.Key.ToPascalCase();
            var model = (className, modelSchema.Value);
            yield return Create(className, model);
        }

        ModelTemplate<TModel> Create<TModel>(string typeName, TModel model)
            => new ModelTemplate<TModel>(new(typeName), model);

    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
