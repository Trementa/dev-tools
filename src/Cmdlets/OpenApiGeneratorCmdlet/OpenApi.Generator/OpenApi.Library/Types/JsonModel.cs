using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OpenApi.Library.Types
{
    public record JsonModel : IJsonModel
    {
        [JsonExtensionData]
        public Dictionary<string, object> ___extensionData { get; set; } = new Dictionary<string, object>();
    }

    public record JsonModel<T> : JsonModel, IJsonModel<T>
        where T : IJsonModel
    {
        public Identifier<T> Id { get; set; }
    }
}