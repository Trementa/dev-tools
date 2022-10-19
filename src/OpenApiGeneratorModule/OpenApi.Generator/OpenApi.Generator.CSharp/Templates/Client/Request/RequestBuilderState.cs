using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Templates.Client.Request;

using Configuration;
using Types;

public abstract class RequestBuilderState
{
    protected RequestBuilderState(IConnection connection, ILogger logger) =>
        (Connection, Logger) = (connection, logger);

    protected IConnection Connection { get; }
    protected HttpClient HttpClient => Connection.HttpClient;
    protected Uri BaseUri => Connection.BaseUri;
    protected ILogger Logger { get; }

    protected async virtual Task<dynamic> Execute(HttpMethod httpMethod, UriMethod relativePath, Func<IRequestBuilder, Task<IRequestBuilder>> requestBuilder, CancellationToken cancellationToken)
    {
        var request = await requestBuilder(await ConfigureRequest(RequestBuilder.Create(httpMethod, BaseUri, relativePath), cancellationToken)).ConfigureAwait(false);
#if DEBUG
        Logger.LogDebug(request.ToString());
#endif
        using var httpRequest = await request.Build(cancellationToken).ConfigureAwait(false);
        using var httpResponse = await HttpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false);
        return await request.HandleResponse(httpResponse, cancellationToken).ConfigureAwait(false);
    }

    protected async Task<IRequestBuilder> ConfigureRequest(RequestBuilder requestBuilder, CancellationToken cancellationToken) =>
        await Connection.ConfigureRequest(requestBuilder, cancellationToken);
}
