using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GK.WebLib.Types;
using Microsoft.Extensions.Logging;

namespace GK.WebLib.Client.Request
{
    public abstract class RequestBuilderState
    {
        protected HttpClient HttpClient { get; }
        protected Uri BaseUri { get; }
        protected ILogger Logger { get; }

        protected RequestBuilderState(Uri baseUri, ILogger logger, HttpClient httpClient) =>
            (BaseUri, Logger, HttpClient) =
            (baseUri, logger, httpClient);

        protected async virtual Task<dynamic> Execute(HttpMethod httpMethod, UriMethod relativePath, Func<IRequestBuilder, Task<IRequestBuilder>> requestBuilder, CancellationToken cancellationToken)
        {
            var request = await requestBuilder(RequestBuilder.Create(httpMethod, BaseUri, relativePath)).ConfigureAwait(false);
#if DEBUG
            Logger.LogDebug(request.ToString());
#endif
            using var httpRequest = await request.Build(cancellationToken).ConfigureAwait(false);
            using var httpResponse = await HttpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false);
            return await request.HandleResponse(httpResponse, cancellationToken).ConfigureAwait(false);
        }
    }
}
