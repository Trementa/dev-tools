﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using RazorLight;
using RazorLight.Internal;

namespace OpenApi.Generator.CSharp;
using CSharp.SyntaxProviders;

public abstract class CodeTemplate : CodeTemplate<dynamic>
{ }

/// <summary>
/// TODO:
/// Improve base class for code generation template
/// and reduce dependency on Extension methods.
/// </summary>
/// <typeparam name="TModel"></typeparam>
public abstract class CodeTemplate<TModel> : TemplatePage<TModel>
{
#nullable disable

    [RazorInject]
    protected RazorCodeGenerator RazorCodeGenerator { get; set; }

    [RazorInject]
    protected Logger<CodeTemplate<TModel>> Logger { get; set; }

    [RazorInject]
    protected Options Options { get; set; }

#nullable enable

    protected CodeTemplate() =>
        DisableEncoding = true;

    protected async Task Generate<T>(string templateName, string destinationFile, T model) =>
        await RazorCodeGenerator.GenerateAndSaveCode(
            Path.Combine(TemplateDirectory, templateName),
            Path.Combine(TemplateDirectory, destinationFile),
            model);

    protected string GetUsings(string indentation, params string[] bases) =>
        string.Join(Environment.NewLine, bases.Select(b => $"{ indentation }using {Options.SDKNamespace}{b};"));

    protected string GetNamespace()
    {
        var configuredRootNamespace = Options.GenNamespace;
        var relativeRootNamespace = TemplateDirectory?.Replace('\\', '.').Replace('/', '.');
        return $"{configuredRootNamespace}{relativeRootNamespace}";
    }

    protected string GetNamespace(string rootSpace)
    {
        var configuredRootNamespace = Options.GenNamespace;
        var relativeRootNamespace = TemplateDirectory?.Replace('\\', '.').Replace('/', '.');
        var rootNamespace = string.IsNullOrWhiteSpace(relativeRootNamespace) ? rootSpace : $"{relativeRootNamespace}.{rootSpace}";
        return $"{configuredRootNamespace}{rootNamespace}";
    }

    protected string GetNamespace(string rootSpace, string endWithPath)
    {
        var root = GetNamespace(rootSpace);
        var pathNamespace = Path.GetDirectoryName(endWithPath)?.Replace('\\', '.').Replace('/', '.');
        return string.IsNullOrWhiteSpace(pathNamespace) ? root : $"{root}.{pathNamespace}";
    }

    protected string GetSDKNamespace(string childSpace) =>
        $"{Options.SDKNamespace}{childSpace}";

    protected string GetClassName((string ClassName, IEnumerable<(string, OpenApiPathItem)> Operations) model, params string[] apppends)
            => new StringBuilder(model.ClassName).AppendJoin(string.Empty, apppends).ToString();

    protected string GetClassName((string, OpenApiSchema) model) =>
        Path.GetFileName(model.Item1);

    protected string GetBaseClass((string ClassName, IEnumerable<(string, OpenApiPathItem)> Operations) model)
    {
        if (Options.IncludeTraceId)
            return $"AlternativeBaseApi";

        return $"BaseApi<${model.ClassName}>";
    }

    protected string GetEnumBaseType(OpenApiSchema schema) => schema.Enum.Count > 0 ? schema.Format : schema.Type;

    protected IEnumerable<ApiPath> GetPathes((string ClassName, IEnumerable<(string, OpenApiPathItem)> pathes) model) =>
        model.pathes.Select(p => new ApiPath(p.Item1, p.Item2));

    public string TemplateKey =>
        PageContext.ExecutingPageKey.StartsWith('/') ? PageContext.ExecutingPageKey.Substring(1) : PageContext.ExecutingPageKey;

    public string TemplateDirectory => Path.GetDirectoryName(TemplateKey) ?? "";
    public string TemplatePath => TemplateKey;
    public string TemplateFileName => Path.GetFileName(TemplateKey);
    public string TemplateName => Path.GetFileNameWithoutExtension(TemplateKey);
}