namespace OpenApi.Library.Extensions;

using OpenApi.Library.Client.Serializer;
using Types;

public static class JsonExtensions
{
    public static string ToJson<T>(this T me) where T : IJsonModel =>
        JsonSerializer.Instance.Serialize(me);

    public static T FromJson<T>(this string me) where T : IJsonModel =>
        JsonSerializer.Instance.Deserialize<T>(me);
}
