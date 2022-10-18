using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Drofus.WebLib.Configuration;
using Client.Request;

public interface IConnection
{
    Uri BaseUri { get; }
    HttpClient HttpClient { get; }
    Task<IRequestBuilder> ConfigureRequest(IRequestBuilder requestBuilder, CancellationToken cancellationToken)
        => Task.FromResult(requestBuilder);
}
