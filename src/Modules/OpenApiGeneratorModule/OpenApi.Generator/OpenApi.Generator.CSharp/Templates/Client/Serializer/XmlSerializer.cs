using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GK.WebLib.Client.Serializer;

public class XmlSerializer : ISerializer
{
    public ValueTask<object> DeserializeAsync(Stream utf8Json, Type returnType, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task SerializeAsync(Stream utf8Json, object value, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public T Deserialize<T>(string json)
    {
        throw new NotImplementedException();
    }

    public string Serialize<T>(T data) => throw new NotImplementedException();
}
