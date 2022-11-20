using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Templates.Client.Serializer;
using Extensions;

public class JsonSerializer : ISerializer
{
    public static readonly JsonSerializer Instance = new JsonSerializer();

    protected static JsonSerializerOptions JsonSerializerOptions { get; } = CreateJsonSerializerOptions();

    public async ValueTask<object> DeserializeAsync(Stream utf8Json, Type returnType,
        CancellationToken cancellationToken = default) =>
        await System.Text.Json.JsonSerializer.DeserializeAsync(utf8Json, returnType, JsonSerializerOptions,
            cancellationToken);

    public async Task SerializeAsync(Stream utf8Json, object value,
        CancellationToken cancellationToken = default) =>
        await System.Text.Json.JsonSerializer.SerializeAsync(utf8Json, value, value.GetType(),
            JsonSerializerOptions, cancellationToken);

    public T Deserialize<T>(string json) =>
        System.Text.Json.JsonSerializer.Deserialize<T>(json, JsonSerializerOptions);

    public string Serialize<T>(T data) =>
        System.Text.Json.JsonSerializer.Serialize(data, JsonSerializerOptions);

    static JsonSerializerOptions CreateJsonSerializerOptions()
    {
        var jsonSerializerOptions = new JsonSerializerOptions { IgnoreNullValues = false, Converters = { new JsonStringEnumConverter() } };
        foreach (var jsonConverterType in AssemblyExtensions.ScanAssemblyForType<JsonSerializer, JsonConverter>())
            jsonSerializerOptions.Converters.Add(Activator.CreateInstance(jsonConverterType, true) as JsonConverter);
        return jsonSerializerOptions;
    }
}
