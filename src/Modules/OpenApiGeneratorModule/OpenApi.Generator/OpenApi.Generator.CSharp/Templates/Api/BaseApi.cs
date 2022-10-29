using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Templates.Api;

using Client.Request;
using Configuration;
using Types;

public abstract partial class BaseApi<TApi> : RequestBuilderState where TApi : BaseApi<TApi>
{
    protected BaseApi(IConnection connection, ILogger<TApi> logger) : base(connection, logger)
    { }

    protected virtual async Task<TResult> Execute<TResult>(HttpMethod httpMethod, UriMethod relativePath, Func<IRequestBuilder, Task<IRequestBuilder>> requestBuilder, CancellationToken cancellationToken) =>
        await Execute(httpMethod, relativePath, requestBuilder, cancellationToken).ConfigureAwait(false);
}

public abstract partial class BaseApi
{
    protected readonly HttpRequestFunc HttpRequestFunc;
    protected BaseApi(HttpRequestFunc httpRequestFunc) =>
        HttpRequestFunc = httpRequestFunc;

    protected virtual async Task<T> Execute<T>(HttpMethod httpMethod, UriMethod relativePath, Func<IRequestBuilder, Task<IRequestBuilder>> func, CancellationToken cancellationToken) =>
        (T)await HttpRequestFunc(httpMethod, relativePath, func, cancellationToken).ConfigureAwait(false);
}
