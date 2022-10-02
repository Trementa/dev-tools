using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using RazorLight;
using RazorLight.Internal;

namespace OpenApi.Generator
{
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

        protected string GetClassName((string ClassName, IEnumerable<(string, OpenApiPathItem)> Operations) model) =>
            model.ClassName;

        protected string GetClassName((string, OpenApiSchema) model) =>
            Path.GetFileName(model.Item1);

        protected string GetBaseClass((string ClassName, IEnumerable<(string, OpenApiPathItem)> Operations) model)
        {
            if (Options.IncludeTraceId)
                return $"AlternativeBaseApi";

            return "BaseApi";
        }
       

        protected string NormalizePath(string path) =>
            path[0].Equals('/') ? path.Substring(1, path.Length - 1) : path;

        protected string GetEnumBaseType(OpenApiSchema schema) => schema.Enum.Count > 0 ? schema.Format : schema.Type;

        protected (string Type, string ContentType) GetType((string Name, OpenApiSchema Schema) type)
        {
            if (!string.IsNullOrWhiteSpace(type.Schema?.Reference?.Id))
                return (type.Schema.Reference.Id.ToPascalCase(), "application/json");
            else
            {
                if (type.Schema?.Type == "array")
                {
                    var tuple = ($"List<{ GetType((type.Name, type.Schema.Items)).Type }>", GetType((type.Name, type.Schema.Items)).ContentType);
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
                    "array" => ($"List<{ GetType((type.Name, type.Schema.Items)).Type }>", GetType((type.Name, type.Schema.Items)).ContentType),
                    //"object" => ($"{type.Name.ToPascalCase()}Type", "application/json"), /// Inline type declaration
                    "object" => ("object", "application/json"),
                    "" => ("dynamic", "application/json"),
                    _ => ("___unknown_type_name", "application/json"),
                    //($"___unknown_type_name({type.Schema.Type})", "error/unknown")
                });
            }
        }

        protected (string Type, string ContentType) GetWellKnownTypeName((string Type, string ContentType) typeName) =>
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

        private (string Type, string ContentType) GetType(KeyValuePair<string, OpenApiSchema> type) =>
            GetType((type.Key, type.Value));

        protected string GetTypeName(KeyValuePair<string, OpenApiSchema> type) =>
            $"{GetType(type).Type}{(type.Value.Nullable ? "?" : "")}";

        protected string GetPropertyName(KeyValuePair<string, OpenApiSchema> property) =>
            property.Key.ToPascalCase();

        protected IEnumerable<(string path, OpenApiPathItem pathItem)> GetPathes((string ClassName, IEnumerable<(string, OpenApiPathItem)> pathes) model) =>
            model.pathes;

        protected IEnumerable<(OperationType, OpenApiOperation)> GetOperations((string path, OpenApiPathItem pathItem) model)
        {
            foreach (var keyPair in model.pathItem.Operations)
            {
                yield return (keyPair.Key, keyPair.Value);
            }
        }

        protected IEnumerable<ResponseParameter> GetResponseTypes((OperationType, OpenApiOperation) model)
        {
            foreach (var response in model.Item2.Responses)
            {
                var value = response.Value.Content?.FirstOrDefault().Value;
                if (value != null)
                {
                    yield return new ResponseParameter
                    {
                        Description = response.Value.Description,
                        Type = GetType(("", value.Schema)).Type,
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

        protected string GetReturnType((OperationType, OpenApiOperation) model)
        {
            try
            {
                var successfulCodes = model.Item2.Responses.Where(r => r.Key[0] == '2' && r.Value.Content.Any());
                if (successfulCodes.Count() == 0)
                    return "Void";

                if (successfulCodes.Count() == 1)
                {
                    var type = successfulCodes.First();
                    return GetType((type.Key, type.Value.Content.First().Value.Schema)).Type;
                }

                // Multiple possible return values
                return "dynamic";
            }
            catch(Exception ex)
            {
                return "___unknown_type_name" + $"/* { ex.ToString() } */";
            }
        }

        protected string GetRequestBodyMediaType((OperationType, OpenApiOperation) model) =>
            model.Item2.RequestBody.Content!.Select(k => k.Key).First();

        protected bool ExistsRequestBody((OperationType, OpenApiOperation) model) =>
            model.Item2.RequestBody?.Content != null;

        protected IEnumerable<RequestBodyParameter> GetRequestBodyParameters((OperationType, OpenApiOperation) model)
        {
            if (!ExistsRequestBody(model))
                return new List<RequestBodyParameter>();

            var content = model.Item2.RequestBody.Content.First();
            var schema = model.Item2.RequestBody.Content.First().Value.Schema;
            return GetRequestBodyParameters(content.Key, schema);
        }

        // https://swagger.io/docs/specification/describing-request-body/multipart-requests/
        private IEnumerable<RequestBodyParameter> GetRequestBodyParameters(string mediaType, OpenApiSchema schema)
        {
            if (schema.Type == "array")
            {
                if (schema.Items.Reference != null)
                {
                    yield return new RequestBodyParameter
                    {
                        Type = $"List<{GetType((schema.Items.Reference.Id, schema.Items)).Type}>",
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
                            Name = GetPropertyName(property),
                            IsNullable = true,
                            MediaType = "application/json"
                        };
                    }
                    else
                    {
                        var typeSignature = GetType(property);
                        yield return new RequestBodyParameter
                        {
                            Type = typeSignature.Type,
                            Name = GetPropertyName(property),
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
                    Type = GetType((schema.Title, schema)).Type,
                    Name = (string.IsNullOrWhiteSpace(schema.Title) ? "body" : schema.Title),
                    IsNullable = schema.Nullable,
                    MediaType = mediaType
                };
        }

        protected IEnumerable<OpenApiParameter> GetParameters((OperationType, OpenApiOperation) model) =>
            model.Item2.Parameters;

        protected IEnumerable<FunctionArgument> GetFunctionArguments((OperationType, OpenApiOperation) model)
        {
            foreach (var parameter in model.Item2.Parameters)
            {
                yield return new FunctionArgument
                {
                    Type = GetType((parameter.Name, parameter.Schema)).Type,
                    Name = parameter.Name,
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

            foreach (var requestParam in GetRequestBodyParameters(model))
            {
                yield return new FunctionArgument
                {
                    Type = requestParam.Type,
                    Name = requestParam.Name,
                    IsNullable = true, //requestParam.IsNullable,
                    IsRequired = false,
                    FunctionArgumentEnum = FunctionArgumentEnum.Request
                };
            }

            yield return new FunctionArgument { Type = "CancellationToken", Name = "cancellationToken", IsNullable = false, IsRequired = false };
        }

        protected string FormatArguments(IEnumerable<FunctionArgument> arguments) =>
            string.Join(", ", ArgumentsToString(arguments));

        private IEnumerable<string> ArgumentsToString(IEnumerable<FunctionArgument> arguments)
        {
            foreach (var argument in arguments)
            {
                yield return argument.ToString();
            }
        }

        protected string GetOperationName((OperationType, OpenApiOperation) operation, string? fallbackOpName = null)
        {
            var operationName = operation.Item2.OperationId;

            if (string.IsNullOrWhiteSpace(operationName))
            {
                if (fallbackOpName != null)
                    operationName = fallbackOpName;
                else
                {
                    operationName = operation.Item2.Tags?.FirstOrDefault()?.Name;
                    if (!string.IsNullOrWhiteSpace(operationName))
                        operationName = $"{ operation.Item1 }{ operationName }";
                    else
                        operationName = $"Unknown operation id ({ operation.Item1 }, { operation.Item2 }";
                }
            }
            return operationName.ToPascalCase();
        }

        protected string GetOperationSummary((OperationType, OpenApiOperation) model, string indentation, string? operationNameFallback = null) =>
            new OperationSummary
            {
                Summary = string.IsNullOrWhiteSpace(model.Item2.Summary) ? operationNameFallback : model.Item2.Summary,
                Arguments = GetFunctionArguments(model),
                ReturnType = GetReturnType(model)
            }.CreateComment(indentation);

        protected string[] GetPathAsSubPaths((string path, OpenApiPathItem pathItem) model) =>
            model.path.Split('/');

        protected OperationType GetOperationType((OperationType, OpenApiOperation) model) =>
            model.Item1;

        protected string GetOperationPath((string path, OpenApiPathItem pathItem) model) =>
            model.path.StartsWith('/') ? model.path.Substring(1) : model.path;

        public string TemplateKey =>
            PageContext.ExecutingPageKey.StartsWith('/') ? PageContext.ExecutingPageKey.Substring(1) : PageContext.ExecutingPageKey;

        public string TemplateDirectory => Path.GetDirectoryName(TemplateKey) ?? "";
        public string TemplatePath => TemplateKey;
        public string TemplateFileName => Path.GetFileName(TemplateKey);
        public string TemplateName => Path.GetFileNameWithoutExtension(TemplateKey);
    }

    public struct ResponseParameter
    {
        public string Type { get; set; }
        public string Description { get; set; }
        public string StatusCode { get; set; }
        public bool IsNullable { get; set; }
        public string MediaType { get; set; }
    }

    public struct RequestBodyParameter
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public bool IsNullable { get; set; }
        public string MediaType { get; set; }
    }

    public enum FunctionArgumentEnum { Query, Header, Path, Cookie, Request, Unknown };

    public struct FunctionArgument
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public bool IsNullable { get; set; }
        public bool IsRequired { get; set; }
        public string Description { get; set; }

        public FunctionArgumentEnum FunctionArgumentEnum { get; set; }

        public override string ToString()
        {
            if (IsNullable)
                return $"{ Type }? { Name }{ (IsRequired ? "" : " = default") }";
            return $"{ Type } { Name }{ (IsRequired ? "" : " = default") }";
        }
    }

    public struct OperationSummary
    {
        public string? Summary { get; set; }
        public IEnumerable<FunctionArgument> Arguments { get; set; }
        public string ReturnType { get; set; }

        public string CreateComment(string indentation)
        {
            var strB = new StringBuilder();
            indentation = $"{ indentation }/// ";
            strB.AppendLine($"{ indentation }<summary>");
            if (Summary != null)
                strB.Append(CreateMultiLine(indentation, Summary));
            strB.AppendLine($"{ indentation }</summary>");
            foreach (var argument in Arguments)
                strB.AppendLine($@"{ indentation }<param name=""{ argument.Name }"">{ argument.Description }</param>");
            strB.Append($"{ indentation }<returns>{ ReturnType }</returns>");
            return strB.ToString();
        }

        private string CreateMultiLine(string indentation, string text)
        {
            var strB = new StringBuilder();
            var lines = text.Split('\n');
            foreach (var line in lines)
                strB.AppendLine($"{ indentation }{ line }");
            return strB.ToString();
        }
    }
}