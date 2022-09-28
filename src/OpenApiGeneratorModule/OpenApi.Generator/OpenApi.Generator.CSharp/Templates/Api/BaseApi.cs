using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GK.WebLib.Api
{
    using GK.WebLib.Client.Request;
    using GK.WebLib.Types;

    public abstract class BaseApi<T> : RequestBuilderState
    {
        protected BaseApi(Uri baseUri, ILogger<T> logger, HttpClient httpClient) : base(baseUri, logger, httpClient)
        { }

        protected virtual async Task<S> Execute<S>(HttpMethod httpMethod, UriMethod relativePath, Func<IRequestBuilder, Task<IRequestBuilder>> requestBuilder, CancellationToken cancellationToken) =>
            (S)await Execute(httpMethod, relativePath, requestBuilder, cancellationToken).ConfigureAwait(false);
    }

    public abstract partial class BaseApi
    {
        protected readonly HttpRequestFunc HttpRequestFunc;
        protected BaseApi(HttpRequestFunc httpRequestFunc) =>
            HttpRequestFunc = httpRequestFunc;

        protected virtual async Task<T> Execute<T>(HttpMethod httpMethod, UriMethod relativePath, Func<IRequestBuilder, Task<IRequestBuilder>> func, CancellationToken cancellationToken) =>
            (T)await HttpRequestFunc(httpMethod, relativePath, func, cancellationToken).ConfigureAwait(false);
    }
}
