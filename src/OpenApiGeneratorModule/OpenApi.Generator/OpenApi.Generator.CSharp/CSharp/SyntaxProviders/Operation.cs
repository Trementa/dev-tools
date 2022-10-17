using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;
using OpenApi.Generator.CSharp.Utility;
#nullable enable

namespace OpenApi.Generator.CSharp.SyntaxProviders;

public static partial class Extensions
{
    public static string FormatArguments(this IEnumerable<FunctionArgument> arguments)
        => string.Join(", ", ArgumentsToString(arguments));

    static IEnumerable<string> ArgumentsToString(IEnumerable<FunctionArgument> arguments)
    {
        foreach (var argument in arguments)
            yield return argument.ToString();
    }
}

public record Operation(OperationType OperationType, OpenApiOperation ApiOperation)
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
            .OrderBy(e => e.IsRequired);

        IEnumerable<IEnumerable<FunctionArgument>> GetFunctionArguments()
        {
            yield return GetParameters();
            yield return GetRequestBodyParameters();
        }

        (string OriginalName, string ScrubbedName) GetValidVariableName(string originalName)
        {
            if (CSharpIdentifiers.TryParseRawIdentifier(originalName, out var scrubbedFunctionParameterName))
            {
                return (originalName, scrubbedFunctionParameterName);
            }
            else
            {
                // Not a valid C# variable name, randomize valid
                scrubbedFunctionParameterName = "ABC";
                return (originalName, scrubbedFunctionParameterName);
            }
        }

        IEnumerable<FunctionArgument> GetParameters()
        {
            foreach (var parameter in ApiOperation.Parameters)
            {
                var validVariableName = GetValidVariableName(parameter.Name);
                yield return new FunctionArgument
                {
                    Type = TypeResolver.GetCompileTimeType((parameter.Name, parameter.Schema)).Type,
                    OriginalName = validVariableName.OriginalName,
                    Name = validVariableName.ScrubbedName,
                    IsNullable = parameter.Schema.Nullable,
                    IsRequired = parameter.Required,
                    Description = parameter.Description,
                    FunctionArgumentEnum = parameter.In switch
                    {
                        ParameterLocation.Cookie => FunctionArgumentEnum.Cookie,
                        ParameterLocation.Header => FunctionArgumentEnum.Header,
                        ParameterLocation.Path => FunctionArgumentEnum.Path,
                        ParameterLocation.Query => FunctionArgumentEnum.Query,
                        _ => FunctionArgumentEnum.Unknown
                    }
                };
            }
        }


        IEnumerable<FunctionArgument> GetRequestBodyParameters()
        {
            foreach (var requestParam in GetRequestBodyParameters())
            {
                var validVariableName = GetValidVariableName(requestParam.Name);
                yield return new FunctionArgument
                {
                    Type = requestParam.Type,
                    OriginalName = validVariableName.OriginalName,
                    Name = validVariableName.ScrubbedName,
                    IsNullable = true, //requestParam.IsNullable,
                    IsRequired = false,
                    FunctionArgumentEnum = FunctionArgumentEnum.Request
                };
            }
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
                    operationName = $"Unknown operation id ({OperationType}, {ApiOperation}";
            }
        }
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

    public IEnumerable<ResponseParameter> GetResponseTypes((OperationType, OpenApiOperation) model)
    {
        foreach (var response in model.Item2.Responses)
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


    /*
                var path = GetOperationPath(pathInstance);
    @:@(GetOperationSummary(operation, "        ", operationName))
            @:public virtual async Task<@(returnType)> @(operationName)(@(FormatArguments(arguments)))
            @:    => await Execute<@returnType>(HttpMethod.@(operationType), "@(path)",
            @:        async rb => await rb
                    foreach (var parameter in GetParameters(operation))
                    {
                        var parameterType = parameter.In switch
                        {
                            ParameterLocation.Query => "Query",
                            ParameterLocation.Header => "Header",
                            ParameterLocation.Path => "Path",
                            ParameterLocation.Cookie => "Cookie",
                            var param => param.ToString()
                        };

            @:        .Add@(parameterType)Parameter("(@(parameter.OriginalName))", @(parameter.Name))
                    }

                    var requestParameters = GetRequestBodyParameters(operation);
                    if(requestParameters.Count() > 1)
                    {
            @:        .SetRequestBodyMediaType("@(GetRequestBodyMediaType(operation))")
                    }
                    foreach (var requestParameter in requestParameters)
                    {
            @:        .AddRequestBody(nameof(@(requestParameter.Name)), @(requestParameter.Name), "@(requestParameter.MediaType)")
                    }
                    foreach(var response in GetResponseTypes(operation))
                    {
                        if(string.IsNullOrWhiteSpace(response.MediaType))
                        {
            @:        .AddResponseMap<@response.Type>("@response.StatusCode")
                        }
                        else
                        {
            @:        .AddResponseMap<@response.Type>("@response.StatusCode", "@response.MediaType")
                        }
                    }


        */





}

