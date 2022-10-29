using System;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;

namespace OpenApi.Generator.CSharp.SyntaxProviders;

public static partial class Extensions
{
    public static string GetPropertyName(this KeyValuePair<string, OpenApiSchema> property)
        => property.Key.ToPascalCase();

    public static (string Type, string ContentType) GetApiType(this KeyValuePair<string, OpenApiSchema> type) =>
        TypeResolver.GetCompileTimeType((type.Key, type.Value));

    public static string GetTypeName(this KeyValuePair<string, OpenApiSchema> type) =>
        $"{GetApiType(type).Type}{(type.Value.Nullable ? "?" : "")}";

    public static string NormalizePath(this string path)
        => path[0].Equals('/') ? path.Substring(1, path.Length - 1) : path;
}
