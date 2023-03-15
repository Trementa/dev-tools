using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OpenApi.Library.Client.Request;

public interface IConnection
{
    Uri BaseUri { get; }
    HttpClient HttpClient { get; }
    Task<IRequestBuilder> ConfigureRequest(IRequestBuilder requestBuilder, CancellationToken cancellationToken)
        => requestBuilder.GetAwaiter();
}
