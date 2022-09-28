using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GK.WebLib.Types
{
    public class JsonModel : IJsonModel
    {
        [JsonExtensionData]
        public Dictionary<string, object> ___extensionData { get; set; } = new Dictionary<string, object>();
    }

    public class JsonModel<T> : JsonModel, IJsonModel<T>
        where T : IJsonModel
    {
        public Identifier<T> Id { get; set; }
    }
}