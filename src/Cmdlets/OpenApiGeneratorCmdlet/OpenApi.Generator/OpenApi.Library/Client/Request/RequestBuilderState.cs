using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OpenApi.Library.Client.Request;

using Types;

public abstract class RequestBuilderState
{
    protected RequestBuilderState(IConnection connection, ILogger logger) =>
        (Connection, Logger) = (connection, logger);

    protected IConnection Connection { get; }
    protected ILogger Logger { get; }
    protected HttpClient HttpClient => Connection.HttpClient;
    protected Uri BaseUri => Connection.BaseUri;

    protected async virtual Task<dynamic> Execute(HttpMethod httpMethod, UriMethod relativePath, Func<IRequestBuilder, Task<IRequestBuilder>> requestBuilder, CancellationToken cancellationToken)
    {
        var request = await requestBuilder(await ConfigureRequest(RequestBuilder.Create(httpMethod, BaseUri, relativePath), cancellationToken)).ConfigureAwait(false);
        Logger.LogDebug("Request: {request}", request);

        using var httpRequest = await request.Build(cancellationToken).ConfigureAwait(false);
        using var httpResponse = await HttpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false);
        Logger.LogTrace("Response: {httpResponse}", httpResponse);

        var result = await request.HandleResponse(httpResponse, cancellationToken).ConfigureAwait(false);
        Logger.LogDebug("Result: {result}", result as object);
        return result;
    }

    protected async virtual Task<IRequestBuilder> ConfigureRequest(RequestBuilder requestBuilder, CancellationToken cancellationToken) =>
        await Connection.ConfigureRequest(requestBuilder, cancellationToken);
}
