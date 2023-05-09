using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;
using Trementa.OpenApi.Generator;
using Trementa.OpenApi.Generator.CSharp.SyntaxProviders;
using Trementa.OpenApi.Generator.CSharp.Utility;

#nullable enable

namespace OpenApi.Generator.CSharp.SyntaxProviders;

public static partial class Extensions
{
    public static string Format(this IEnumerable<FunctionArgument> arguments)
        => string.Join(", ", ArgumentsToString(arguments));

    static IEnumerable<string> ArgumentsToString(IEnumerable<FunctionArgument> arguments)
    {
        foreach (var argument in arguments)
            yield return argument.ToString();
    }

    public static (string OriginalName, string ScrubbedName) GetValidVariableName(this string originalName, char defaultReplacementCharacter = '_', params (char Match, string Replacement)[] matchAndReplaceMap)
        => (originalName, CSharpIdentifiers.CreateValidIdentifier(originalName, defaultReplacementCharacter, matchAndReplaceMap));
}

public record Operation(OperationType OperationType, OpenApiOperation ApiOperation, Options Options)
{
    public string GetReturnType()
    {
        try
        {
            var successfulCodes = ApiOperation.Responses.Where(r => r.Key[0] == '2' && r.Value.Content.Any());
            if (successfulCodes.Count() == 0)
                return "Void";

            if (successfulCodes.Count() == 1)
            {
                var type = successfulCodes.First();
                return TypeResolver.GetCompileTimeType((type.Key, type.Value.Content.First().Value.Schema)).Type;
            }

            // Multiple possible return values
            return "dynamic";
        }
        catch (Exception ex)
        {
            return "___unknown_type_name" + $"/* {ex.ToString()} */";
        }
    }

    public IEnumerable<FunctionArgument> GetFunctionArguments()
    {
        return GetFunctionArguments()
            .SelectMany(e => e)
            .Append(new FunctionArgument { Type = "CancellationToken", Name = "cancellationToken", IsNullable = false, IsRequired = false })
            .OrderByDescending(e => e.IsRequired);

        IEnumerable<IEnumerable<FunctionArgument>> GetFunctionArguments()
        {
            yield return GetParameters();
            yield return GetRequestBodyArguments();
        }

        IEnumerable<FunctionArgument> GetRequestBodyArguments()
        {
            foreach (var requestParam in GetRequestBodyParameters())
            {
                var validVariableName = requestParam.Name.GetValidVariableName();
                yield return new FunctionArgument {
                    Type = requestParam.Type,
                    OriginalName = validVariableName.OriginalName,
                    Name = validVariableName.ScrubbedName,
                    IsNullable = true, //requestParam.IsNullable,
                    IsRequired = false,
                    In = ArgumentLocation.Request
                };
            }
        }
    }

    public IEnumerable<FunctionArgument> GetParameters()
    {
        foreach (var parameter in ApiOperation.Parameters)
        {
            if (Options.ExcludeAPIParams.Contains(parameter.Name))
                continue;

            var validVariableName = parameter.Name.GetValidVariableName();
            yield return new FunctionArgument {
                Type = TypeResolver.GetCompileTimeType((parameter.Name, parameter.Schema)).Type,
                OriginalName = validVariableName.OriginalName,
                Name = validVariableName.ScrubbedName,
                IsNullable = parameter.Schema.Nullable,
                IsRequired = parameter.Required,
                Description = parameter.Description,
                In = parameter.In switch {
                    ParameterLocation.Cookie =>ArgumentLocation.Cookie,
                    ParameterLocation.Header =>ArgumentLocation.Header,
                    ParameterLocation.Path => ArgumentLocation.Path,
                    ParameterLocation.Query => ArgumentLocation.Query,
                    _ => ArgumentLocation.Unknown
                }
            };
        }
    }

    public string GetOperationName(string? fallbackOpName = null)
    {
        var operationName = ApiOperation.OperationId;

        if (string.IsNullOrWhiteSpace(operationName))
        {
            if (fallbackOpName != null)
                operationName = fallbackOpName;
            else
            {
                operationName = ApiOperation.Tags?.FirstOrDefault()?.Name;
                if (!string.IsNullOrWhiteSpace(operationName))
                    operationName = $"{OperationType}{operationName}";
                else
                    operationName = $"UnknownOperationId{OperationType}{ApiOperation}";
            }
        }

        operationName = operationName.ToPascalCase();
        if (Options.UseHTTPverbs)
            operationName = GetOperationType() + operationName;

        return operationName.ToPascalCase();
    }

    public string GetOperationSummary(string indentation, string? operationNameFallback = null) =>
        new OperationSummary
        {
            Summary = string.IsNullOrWhiteSpace(ApiOperation.Summary) ? operationNameFallback : ApiOperation.Summary,
            Arguments = GetFunctionArguments(),
            ReturnType = GetReturnType()
        }.CreateComment(indentation);

    public OperationType GetOperationType() => OperationType;

    public IEnumerable<ResponseParameter> GetResponseTypes()
    {
        foreach (var response in ApiOperation.Responses)
        {
            var value = response.Value.Content?.FirstOrDefault().Value;
            if (value != null)
            {
                yield return new ResponseParameter
                {
                    Description = response.Value.Description,
                    Type = TypeResolver.GetCompileTimeType(("", value.Schema)).Type,
                    StatusCode = response.Key,
                    MediaType = response.Value.Content!.First().Key,
                    IsNullable = value.Schema?.Nullable ?? true
                };
            }
            else
            {
                yield return new ResponseParameter
                {
                    Description = response.Value.Description,
                    Type = "Void",
                    StatusCode = response.Key,
                    IsNullable = false,
                    MediaType = string.Empty
                };
            }
        }
    }

    public string GetReturnType((OperationType, OpenApiOperation) model)
    {
        try
        {
            var successfulCodes = model.Item2.Responses.Where(r => r.Key[0] == '2' && r.Value.Content.Any());
            if (successfulCodes.Count() == 0)
                return "Void";

            if (successfulCodes.Count() == 1)
            {
                var type = successfulCodes.First();
                return TypeResolver.GetCompileTimeType((type.Key, type.Value.Content.First().Value.Schema)).Type;
            }

            // Multiple possible return values
            return "dynamic";
        }
        catch (Exception ex)
        {
            return "___unknown_type_name" + $"/* {ex.ToString()} */";
        }
    }

    public string GetRequestBodyMediaType() =>
        ApiOperation.RequestBody.Content!.Select(k => k.Key).First();

    bool ExistsRequestBody() =>
        ApiOperation.RequestBody?.Content != null;

    public IEnumerable<RequestBodyParameter> GetRequestBodyParameters()
    {
        if (!ExistsRequestBody())
            return new List<RequestBodyParameter>();

        var content = ApiOperation.RequestBody.Content.First();
        var schema = ApiOperation.RequestBody.Content.First().Value.Schema;
        return GetRequestBodyParameters(content.Key, schema);
    }

    // https://swagger.io/docs/specification/describing-request-body/multipart-requests/
    IEnumerable<RequestBodyParameter> GetRequestBodyParameters(string mediaType, OpenApiSchema schema)
    {
        if (schema.Type == "array")
        {
            if (schema.Items.Reference != null)
            {
                yield return new RequestBodyParameter
                {
                    Type = $"List<{TypeResolver.GetCompileTimeType((schema.Items.Reference.Id, schema.Items)).Type}>",
                    Name = (string.IsNullOrWhiteSpace(schema.Items.Title) ? "body" : schema.Items.Title),

                    IsNullable = true,
                    MediaType = "application/json"
                };
            }
        }

        if (schema.Reference == null && schema.Properties != null)
        {
            // Composite type
            // Generate schema
            foreach (var property in schema.Properties)
            {
                if (property.Value.Type == "object")
                {
                    List<string> types = new List<string>();
                    foreach (var internalProperty in GetRequestBodyParameters(property.Key, property.Value))
                    {
                        types.Add($"{internalProperty.Type} {internalProperty.Name}");
                    }
                    var compositeType = string.Join(", ", types);
                    if (types.Count > 1)
                        compositeType = $"({compositeType})";

                    yield return new RequestBodyParameter
                    {
                        Type = compositeType,
                        Name = property.GetPropertyName(),
                        IsNullable = true,
                        MediaType = "application/json"
                    };
                }
                else
                {
                    var typeSignature = property.GetApiType();
                    yield return new RequestBodyParameter
                    {
                        Type = typeSignature.Type,
                        Name = property.GetPropertyName(),
                        IsNullable = true,
                        MediaType = typeSignature.ContentType
                    };
                }
            }
        }
        else
            // Single type
            yield return new RequestBodyParameter
            {
                Type = TypeResolver.GetCompileTimeType((schema.Title, schema)).Type,
                Name = (string.IsNullOrWhiteSpace(schema.Title) ? "body" : schema.Title),
                IsNullable = schema.Nullable,
                MediaType = mediaType
            };
    }
}
