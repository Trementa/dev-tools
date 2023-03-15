using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Readers.Exceptions;
using Microsoft.OpenApi.Readers.Interface;
using Microsoft.OpenApi.Services;
using Microsoft.OpenApi.Validations;
using Microsoft.OpenApi.Writers;
using Trementa.OpenApi.Generator.Generators.CodeGenerators;

namespace Trementa.OpenApi.Generator;

public class Compiler
{
    protected readonly Logger Logger;
    protected readonly Options Options;
    protected readonly IEnumerable<CodeGenerator> CodeGenerators;

    public Compiler(
        Logger<Compiler> logger,
        Options options,
        IEnumerable<CodeGenerator> codeGenerators) =>
        (Options, CodeGenerators, Logger) =
        (options, codeGenerators, logger);

    public async Task ExecuteCommands(CancellationToken cancellationToken = default)
    {
        try
        {
            await Compile(cancellationToken);
        }
        catch (Exception e)
        {
            Logger.LogError(e, "An error occured");
            if (Options.ContinueOnError)
                return;
            throw;
        }
    }

    protected async Task Compile(CancellationToken cancellationToken)
    {
        Logger.LogInformation($"Compiling {Options.SourceFile} into {Options.TemplateLanguage}");

        var document = await ReadDocument(new DefinitionSource(Options.SourceFile), cancellationToken);
        var tasks = new List<Task>();

        foreach (var generator in CodeGenerators)
            tasks.Add(generator.Generate(document, cancellationToken));

        await Task.WhenAll(tasks.ToArray());
    }

    protected async Task<OpenApiDocument> ReadDocument(DefinitionSource definitionSource, CancellationToken cancellationToken)
    {
        var source = await definitionSource.ReadAsync(cancellationToken);
        var apiStream = new OpenApiStreamReader(new OpenApiReaderSettings {
            ReferenceResolution = ReferenceResolutionSetting.ResolveLocalReferences,
            RuleSet = ValidationRuleSet.GetDefaultRuleSet()
        });

        var result = await apiStream.ReadAsync(source, cancellationToken)!;
        return VerifyDocument(result);
    }

    protected OpenApiDocument VerifyDocument(ReadResult readResult)
    {
        Logger.LogError(readResult.OpenApiDiagnostic.Errors);

        if (readResult.OpenApiDiagnostic.SpecificationVersion != Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0)
        {
            Logger.LogError(
                $"OpenAPI version {readResult.OpenApiDiagnostic.SpecificationVersion} is not supported");

            if(Options.ContinueOnError || Options.ConvertToV3)
            {
                Logger.LogInformation("Updating to OpenApi 3.0");

                var memoryStream = new MemoryStream();
                var textWriter = new StreamWriter(memoryStream);
                var openApiWriter = new OpenApiYamlWriter(textWriter);
                readResult.OpenApiDocument.SerializeAsV3(openApiWriter);

                if (Logger.IsEnabled(Logger.LogLevel.Trace))
                {
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    var reader = new StreamReader(memoryStream);
                    var transformedDocument = reader.ReadToEnd();
                    Logger.LogError("Document transformed to OpenApi v3.0.\nDocument:\n{transformedDocument}", transformedDocument);
                }
            }
            else
                throw new OpenApiUnsupportedSpecVersionException(readResult.OpenApiDiagnostic.SpecificationVersion.ToString());
        }

        return readResult.OpenApiDocument;
    }
}

public class StreamLoader : IStreamLoader
{
    protected readonly Uri BaseUri;

    public StreamLoader(Uri baseUri)
        => BaseUri = baseUri;

    public Stream Load(Uri uri)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        var client = new HttpClient();
        using var response = client.Send(request, HttpCompletionOption.ResponseContentRead);
        var content = response.Content.ReadAsStream();

        var text = new StreamReader(content, System.Text.Encoding.UTF8).ReadToEnd();
        text += "openapi: \"3.0.0\"\r\n";

        var array = Encoding.UTF8.GetBytes(text);
        return new MemoryStream(array);
    }

    public async Task<Stream> LoadAsync(Uri uri)
    {
        uri = new Uri(BaseUri, uri);
        var client = new HttpClient();

        var text = await client.GetStringAsync(uri);

        text = @"openapi: ""3.0.0""
info:
  version: 1.0.0
" + text;
        return new MemoryStream(Encoding.UTF8.GetBytes(text));
//        return await client.GetStreamAsync(uri);
    }
}

public class ResourceLoaderFixed : IStreamLoader
{
    private readonly string _mainFilePath;
    private readonly HttpClient _httpClient = new HttpClient();

    public ResourceLoaderFixed(string mainFilePath)
    {
        _mainFilePath = mainFilePath;
    }

    public Stream Load(Uri uri)
    {
        return LoadAsync(uri).GetAwaiter().GetResult();
    }

    public async Task<Stream> LoadAsync(Uri uri)
    {
        try
        {
            var uriScheme = uri.Scheme;

            switch (uriScheme)
            {
                case "file":
                    return File.OpenRead(uri.AbsolutePath);
                case "http":
                case "https":
                    return await _httpClient.GetStreamAsync(uri);
                default:
                    throw new ArgumentException("Unsupported scheme");
            }
        }
        catch (InvalidOperationException) // it seems that it's path to local file without 'file://' prefix
        {
            string originalAbsolutePath = uri.OriginalString;
            string compositeAbsolutePath = Path.Combine(_mainFilePath, uri.OriginalString);

            string finalFilePath;
            if (File.Exists(originalAbsolutePath))
            {
                finalFilePath = originalAbsolutePath;
            }
            else if (File.Exists(compositeAbsolutePath))
            {
                finalFilePath = compositeAbsolutePath;
            }
            else
            {
                throw new Exception($"Can't to resolve reference to {originalAbsolutePath}");
            }

            return File.OpenRead(finalFilePath);
        }

    }
}

public class ApiDocument
{
    protected readonly OpenApiDocument OpenApiDocument;

    public ApiDocument(OpenApiDocument openApiDocument) 
        => OpenApiDocument = openApiDocument;

    public IEnumerable<OpenApiTag> GetAllTags()
        => OpenApiDocument.Tags;

    public OpenApiPaths GetAllPaths()
        => OpenApiDocument.Paths;

    public OpenApiComponents GetComponentSchemas()
        => OpenApiDocument.Components;

    public IList<OpenApiSecurityRequirement> GetSecurityRequirements()
        => OpenApiDocument.SecurityRequirements;
}
