using System.Text;
using Microsoft.OpenApi.Models;
using RazorLight;
using RazorLight.Internal;

namespace Trementa.OpenApi.Generator.CSharp;
using CSharp.SyntaxProviders;
using OpenApi.Generator;
using Trementa.OpenApi.Generator.Generators.Models;
using Trementa.OpenApi.Generator.Razor;

public abstract class ApiTemplatePage : TemplatePage<ApiTemplateModel>
{
#nullable disable

    [RazorInject]
    protected RazorCodeGenerator RazorCodeGenerator { get; set; }

    [RazorInject]
    protected Logger<ApiTemplatePage> Logger { get; set; }

    [RazorInject]
    protected Options Options { get; set; }

#nullable enable

    protected ApiTemplatePage() =>
        DisableEncoding = true;

    protected string GetNamespace()
        => Options.GenNamespace;

    protected string GetClassName()
        => Model.TypeName;
    //protected string GetClassName((string ClassName, IEnumerable<(string, OpenApiPathItem)> Operations) model, params string[] apppends)
    //        => new StringBuilder(model.ClassName).AppendJoin(string.Empty, apppends).ToString();

    //protected string GetClassName((string, OpenApiSchema) model) =>
    //    Path.GetFileName(model.Item1);

    protected string GetUsings(string indentation, params string[] bases) =>
        string.Join(Environment.NewLine, bases.Select(b => $"{indentation}using {Options.SDKNamespace}{b};"));


    protected string GetBaseClass((string ClassName, IEnumerable<(string, OpenApiPathItem)> Operations) model)
    {
        if (Options.IncludeTraceId)
            return $"AlternativeBaseApi";

        return $"BaseApi<${model.ClassName}>";
    }

    //protected string GetEnumBaseType(OpenApiSchema schema)
    //{
    //    if (schema.Enum.Count > 0)
    //    {
    //        if (!string.IsNullOrWhiteSpace(schema.Format))
    //            return schema.Format;

    //        return "int";
    //    }

    //    return schema.Type;
    //}

    //protected IEnumerable<ApiPath> GetPathes((string ClassName, IEnumerable<(string, OpenApiPathItem)> pathes) model) =>
    //    model.pathes.Select(p => new ApiPath(p.Item1, p.Item2, Options));

    //public string TemplateKey =>
    //    PageContext.ExecutingPageKey.StartsWith('/') ? PageContext.ExecutingPageKey.Substring(1) : PageContext.ExecutingPageKey;

    //public string TemplateDirectory => Path.GetDirectoryName(TemplateKey) ?? "";
    //public string TemplatePath => TemplateKey;
    //public string TemplateFileName => Path.GetFileName(TemplateKey);
    //public string TemplateName => Path.GetFileNameWithoutExtension(TemplateKey);
}
