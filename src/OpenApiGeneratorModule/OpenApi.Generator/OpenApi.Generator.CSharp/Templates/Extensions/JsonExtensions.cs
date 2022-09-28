using GK.WebLib.Types;

namespace GK.WebLib.Extensions
{
    public static class JsonExtensions
    {
        public static string ToJson<T>(this T me) where T : IJsonModel =>
            Client.Serializer.JsonSerializer.Instance.Serialize(me);

        public static T FromJson<T>(this string me) where T : IJsonModel =>
            Client.Serializer.JsonSerializer.Instance.Deserialize<T>(me);
    }
}
