using System.Text;
using Microsoft.OpenApi.Models;
using RazorLight;
using RazorLight.Internal;

namespace Trementa.OpenApi.Generator.CSharp;
using CSharp.SyntaxProviders;
using Microsoft.CodeAnalysis;
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

    protected string GetNamespace(string postfix = "")
        => Options.GenNamespace[^1] == '.' ? 
           Options.GenNamespace + postfix : 
           Options.GenNamespace + "." + postfix;

    protected string GetClassName(string postfix = "")
        => Model.TypeName.GetTitleCase() + postfix;

    protected IEnumerable<string> GetUsings(params string[] bases)
    {
        foreach(var b in bases)
            yield return $"{Options.SDKNamespace}{b}";
    }

    protected virtual string GetOperationId(string operationId)
        => operationId;

    public IEnumerable<(string Path, OpenApiPathItem PathItem)> GetPaths()
    {
        foreach(var model in Model.Model)
            yield return (model.Key, model.Value);
    }
}

public static class OpenApiOperationExtensions
{
    public static string GetReturnType(this OpenApiResponses openApiResponses)
    {
        try
        {
            var successfulCodes = openApiResponses.Where(r => r.Key[0] == '2' && r.Value.Content.Any());
            if (successfulCodes.Count() == 0)
                return "Result";

            if (successfulCodes.Count() == 1)
            {
                var type = successfulCodes.First();
                return TypeResolver.GetCompileTimeType((type.Key, type.Value.Content.First().Value.Schema)).Type;
            }

            return "Result<>";
        }
        catch (Exception ex)
        {
            return "___unknown_type_name" + $"/* {ex.ToString()} */";
        }
    }

    public static IEnumerable<FunctionArgument> GetOperationArguments(this OpenApiOperation operation)
    {
        foreach (var parameter in operation.Parameters)
        {
            yield return new FunctionArgument {
                Description = parameter.Description,
                In = ArgumentLocation.Request,
                IsRequired = parameter.Required,
                Name = parameter.Name,
                Schema = parameter.Schema,
            };
        }
    }

    // https://swagger.io/docs/specification/describing-request-body/multipart-requests/
    public static IEnumerable<RequestBodyParameter> GetRequestBodyParameters(this OpenApiOperation operation)
    {
        if (operation.RequestBody == null)
            yield break;

        foreach (var request in operation.RequestBody.Content)
        {
            var mediaType = request.Key;
            var content = request.Value;
            yield return new RequestBodyParameter {
                In = ArgumentLocation.Body,
                Schema = content.Schema,
                MediaType = mediaType
            };
        }
    }

    public static FunctionArgument? JoinRequestTypes(this IEnumerable<RequestBodyParameter> requestBodyParameters)
    {
        if(requestBodyParameters.Any())
        {
            return new FunctionArgument {
                Name = "body",
                Type = "RequestBody<>"
            };
        }

        return null;
    }

    public static string GetRequestBodyMediaType(this OpenApiOperation operation) =>
        operation.RequestBody.Content!.Select(k => k.Key).First();

    public static IEnumerable<ResponseParameter> GetResponseTypes(this OpenApiOperation operation)
    {
        foreach (var response in operation.Responses)
        {
            var value = response.Value.Content?.FirstOrDefault().Value;
            if (value != null)
            {
                yield return new ResponseParameter {
                    Description = response.Value.Description,
                    Type = TypeResolver.GetCompileTimeType(("", value.Schema)).Type,
                    StatusCode = response.Key,
                    MediaType = response.Value.Content!.First().Key,
                    IsNullable = value.Schema?.Nullable ?? true
                };
            }
            else
            {
                yield return new ResponseParameter {
                    Description = response.Value.Description,
                    Type = "Void",
                    StatusCode = response.Key,
                    IsNullable = false,
                    MediaType = string.Empty
                };
            }
        }
    }

    // Edge behavior: If not declared, i.e. no $ref, then add declaration to class
    public static string GetFunctionParameterType(this OpenApiSchema schema)
    {
        foreach(var type in schema.AllOf)
        { }

        foreach (var type in schema.OneOf)
        { }

        foreach(var type in schema.AnyOf)
        { }

        foreach(var type in schema.Not)
        { }

        var baseType = schema.Type switch {
            "number" when schema.Format == "float" => "float",
            "number" when schema.Format == "double" => "double",
            "number" when NotDefined(schema.Format) => "decimal",
            "integer" when schema.Format == "byte" => "byte",
            "integer" when schema.Format == "int16" => "short",
            "integer" when schema.Format == "int32" => "int",
            "integer" when schema.Format == "int64" => "long",
            "integer" when NotDefined(schema.Format) => "BigInteger",
            "string" when Defined(schema.Enum) => GetEnum(schema.Enum),
            "string" when schema.Format == "date" => "Date",
            "string" when schema.Format == "date-time" => "DateTime",
            "string" when schema.Format == "password" => "Password",
            "string" when schema.Format == "byte" => "Base64EncodedString",
            "string" when schema.Format == "binary" => "Stream",
            "string" when schema.Format == "email" => "Email",
            "string" when schema.Format == "uuid" => "Guid",
            "string" when schema.Format == "uri" => "Uri",
            "string" when schema.Format == "hostname" => "HostName",
            "string" when schema.Format == "ipv4" => "IpV4",
            "string" when schema.Format == "ipv6" => "IpV6",
            "boolean" => "bool",
            "array" => GetArray(schema.Items),
            "object" => GetComposite(schema.Properties),
            _ => "Unknown"
        };

        bool NotDefined(string? val)
        => string.IsNullOrWhiteSpace(val);
        bool Defined(object? val)
        => val != null;

        string GetEnum(dynamic properties) => "enum";

        string GetArray(OpenApiSchema items)
            => "[]";

        string GetComposite(dynamic properties) => "[]";

        //schema.AllOf();
        //schema.AnyOf();
        //schema.OneOf();
        //schema.Not;

//        if(schema.Nullable && schema.Default && schema.Required)




        return baseType;
    }
}
