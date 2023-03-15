using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenApi.Library.Types
{
    public interface IJsonModel
    {
        [JsonExtensionData]
        Dictionary<string, object> ___extensionData { get; set; }
    }

    public interface IJsonModel<T> : IJsonModel
        where T : IJsonModel
    {
        Identifier<T> Id { get; set; }
    }
}
