using System.Text.RegularExpressions;
using Microsoft.OpenApi.Models;

namespace Trementa.OpenApi.Generator.CSharp.SyntaxProviders;

public static class TypeResolver
{
    public static (string Type, string ContentType) GetCompileTimeType((string Name, OpenApiSchema Schema) type)
    {
        if (!string.IsNullOrWhiteSpace(type.Schema?.Reference?.Id))
            return (type.Schema.Reference.Id.ToPascalCase(), "application/json");
        else
        {
            if (type.Schema?.Type == "array")
            {
                var tuple = ($"List<{GetCompileTimeType((type.Name, type.Schema.Items)).Type}>", GetCompileTimeType((type.Name, type.Schema.Items)).ContentType);
                var test = GetWellKnownTypeName(tuple);
            }

            return GetWellKnownTypeName(type.Schema?.Type switch {
                "string" when string.IsNullOrWhiteSpace(type.Schema.Format) => ("string", "text/plain"),
                "string" => ($"string+{type.Schema.Format}", "text/plain"),
                "date" => ($"string+date", "text/plain"),
                "number" when string.IsNullOrWhiteSpace(type.Schema.Format) => ("decimal", "text/plain"),
                "number" => ($"decimal+{type.Schema.Format}", "text/plain"),
                "integer" when string.IsNullOrWhiteSpace(type.Schema.Format) => ("int", "text/plain"),
                "integer" => ($"int+{type.Schema.Format}", "text/plain"),
                "boolean" => ("bool", "text/plain"),
                "array" => ($"List<{GetCompileTimeType((type.Name, type.Schema.Items)).Type}>", GetCompileTimeType((type.Name, type.Schema.Items)).ContentType),
                //"object" => ($"{type.Name.ToPascalCase()}Type", "application/json"), /// Inline type declaration
                "object" => ("object", "application/json"),
                "" => ("dynamic", "application/json"),
                _ => ("___unknown_type_name", "application/json"),
                //($"___unknown_type_name({type.Schema.Type})", "error/unknown")
            });
        }
    }

    static Regex compositePlusType = new("(.*)\\+.*");
    static (string Type, string ContentType) GetWellKnownTypeName((string Type, string ContentType) typeName) =>
        (typeName.Type switch {
            "string+date" => "Date",
            "string+date-time" => "DateTime",
            "string+password" => "Password",
            "string+byte" => "Base64EncodedString",
            "string+binary" => "Stream",
            "string+email" => "Email",
            "string+uuid" => "Guid",
            "string+uri" => "Uri",
            "string+hostname" => "HostName",
            "string+ipv4" => "IpV4",
            "string+ipv6" => "IpV6",
            "decimal+double" => "double",
            "decimal+float" => "float",
            "int+byte" => "byte",
            "int+int16" => "short",
            "int+int32" => "int",
            "int+int64" => "long",
            var composite when IsMatch(composite) => GetMatch(composite),
            var verbatim => verbatim
        }, typeName.ContentType);

    static bool IsMatch(string composite)
        => compositePlusType.IsMatch(composite);

    static string GetMatch(string composite)
        => compositePlusType.Match(composite).Groups[1].Value;
}
