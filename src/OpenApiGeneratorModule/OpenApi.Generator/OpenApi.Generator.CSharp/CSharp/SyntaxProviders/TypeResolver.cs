using System;
using Microsoft.OpenApi.Models;

namespace OpenApi.Generator.CSharp.SyntaxProviders;

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

            return GetWellKnownTypeName(type.Schema?.Type switch
            {
                "string" when string.IsNullOrWhiteSpace(type.Schema.Format) => ("string", "text/plain"),
                "string" => ($"string+{type.Schema.Format}", "text/plain"),
                "date" => ($"string+date", "text/plain"),
                "number" when string.IsNullOrWhiteSpace(type.Schema.Format) => ("number", "text/plain"),
                "number" => (type.Schema.Format, "text/plain"),
                "integer" when string.IsNullOrWhiteSpace(type.Schema.Format) => ("integer", "text/plain"),
                "integer" => ($"integer+{type.Schema.Format}", "text/plain"),
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

    static (string Type, string ContentType) GetWellKnownTypeName((string Type, string ContentType) typeName) =>
        (typeName.Type switch
        {
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
            "number" => "decimal",
            "integer" => "int",
            "integer+byte" => "byte",
            "integer+int16" => "short",
            "integer+int32" => "int",
            "integer+int64" => "long",
            _ => typeName.Type.Replace("string+", "").Replace("integer+", "")
        }, typeName.ContentType);
}