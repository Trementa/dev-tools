using Microsoft.OpenApi.Models;

namespace Trementa.OpenApi.Generator.Generators.CodeGenerators;
using IO;
using Models;

public abstract class CodeGenerator
{
    public abstract Task Generate(OpenApiDocument document, CancellationToken cancellationToken = default);

    protected string CreateFileName(TemplateName templateName, TypeName typeName, string postfix) =>
        System.IO.Path.Combine(templateName, string.Concat(typeName, postfix));
}
