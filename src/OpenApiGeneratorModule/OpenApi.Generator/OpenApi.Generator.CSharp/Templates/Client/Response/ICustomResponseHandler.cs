using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace GK.WebLib.Client.Response;

public interface ICustomResponseHandler
{
    Task<dynamic> HandleResponse(HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken);
}

public interface ICustomResponseHandler<T> : ICustomResponseHandler
{
    new Task<T> HandleResponse(HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken);
}
