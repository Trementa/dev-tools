using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GK.WebLib.Client.Serializer
{
    public interface ISerializer
    {
        ValueTask<object> DeserializeAsync(Stream data, Type returnType, CancellationToken cancellationToken = default);
        Task SerializeAsync(Stream data, object value, CancellationToken cancellationToken = default);
        T Deserialize<T>(string data);

        string Serialize<T>(T data);
    }
}